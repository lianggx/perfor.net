using Perfor.Lib.Enums;
using Perfor.Lib.Extension;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;
using Perfor.Lib.Reflection;

namespace Perfor.Lib.Helpers.Mssql
{
    /**
     * @ 分页查询
     * */
    public partial class MssqlReadPager : MssqlQueryHelper
    {
        #region Identity
        /**
 * @ 默认构造函数
 * */
        public MssqlReadPager() : base() { }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlReadPager(string tableName) : base(tableName, "") { }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlReadPager(string tableName, string sqlCmdText) : base(tableName, sqlCmdText) { }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlReadPager(string tableName, SQLContext context) : base(tableName, context) { }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlReadPager(string tableName, SQLContext context, string sqlCmdText) : base(tableName, context, sqlCmdText) { }
        #endregion

        /**
         * @ 查询数据集，该方法仅支持对单表进行封装
         * @ 注意：如果是进行多表连接查询，不管查询的字段多少，都以T类型为标准
         * @ 如果希望返回多表查询的完整字段，请考虑使用 Select 非泛型方法
         * @ page 页码
         * @ size 页大小
         * */
        public List<T> Select<T>(int page, int size, out int rowCount) where T : class, new()
        {
            rowCount = 0;
            List<T> dataList = new List<T>(size);
            // 检查是否设置了主键            
            Type type = typeof(T);
            PropertyInfo[] piArray = type.GetProperties();

            string[] fields = new string[piArray.Length];
            for (int i = 0; i < piArray.Length; i++)
            {
                fields[i] = piArray[i].Name;
                if (CheckPrimaryKey(piArray[i]))
                {
                    PrimaryKey = fields[i];
                }
            }
            TableName = type.Name;
            dataList = Select<T>(fields, "", page, size, out rowCount);

            return dataList;
        }

        /**
         * @ 查询数据集，该方法仅支持对单表进行封装，泛型方法第一次重载
         * @ 注意：如果是进行多表连接查询，不管查询的字段多少，都以T类型为标准
         * @ 如果希望返回多表查询的完整字段，请考虑使用 Select 非泛型方法
         * @ primaryKey 表的主键
         * @ fields 要查询的字段名称
         * @ leftJoin 连接查询的语句
         * @ page 页码
         * @ size 页大小
         * */
        public List<T> Select<T>(IEnumerable<string> fields, string leftJoin, int page, int size, out int rowCount) where T : class, new()
        {
            DbDataReader reader = DoReader(fields, leftJoin, page, size);
            reader.Read();
            rowCount = reader["DataCount"].ObjToInt();
            List<T> dataList = GetDataResult<T>(reader);
            return dataList;
        }

        /**
         * @ 查询数据集
         * @ primaryKey 查询主表的主键字段名称
         * @ fields 要查询的字段名称
         * @ leftJoin 左连接查询语句
         * @ page 页码
         * @ size 页大小
         * */
        public List<SQLDataResult> Select(IEnumerable<string> fields, string leftJoin, int page, int size, out int rowCount)
        {
            rowCount = 0;
            List<SQLDataResult> list = new List<SQLDataResult>();
            DbDataReader reader = DoReader(fields, leftJoin, page, size);
            if (reader == null || reader.HasRows == false)
                return list;

            reader.Read();
            rowCount = reader["DataCount"].ObjToInt();
            list = GetDataResult(reader);

            return list;
        }

        /**
         * @ 读取数据
         * @ primaryKey 查询主表的主键字段名称
         * @ fields 要查询的字段名称
         * @ leftJoin 左连接查询语句
         * @ page 页码
         * @ size 页大小
         * */
        private DbDataReader DoReader(IEnumerable<string> fields, string leftJoin, int page, int size)
        {
            this.Fields = fields.ToList();
            this.LeftJoin = leftJoin;
            this.pageIndex = page;
            this.pageSize = size;
            if (InitSQLWithCmdText() == false)
                return null;

            ExecuteReader();
            DbDataReader reader = Context.DbReader;
            if (reader.HasRows == false)
            {
                return null;
            }

            return reader;
        }

        /**
         * @ 初始化SQL命令，准备执行
         * */
        protected override bool InitSQLWithCmdText()
        {
            if (TableName.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置属性TableName的值，该值为查询的主表名称");

            if (LeftJoin.IsNotNullOrEmpty() && TableAlias.IsNullOrEmpty())
                throw new ArgumentNullException("当存在多表连接查询时，必须指定主表的别名，即属性TableAlias的值");

            if (Fields.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置要查询的字段fields");

            if (PrimaryKey.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置属性PrimaryKey的值为查询主表的主键名称");

            if (pageSize < 1)
                throw new ArgumentNullException("必须设置属性pageSize的值，且该值必须大于0");

            if (OrderBy.IsNullOrEmpty())
                throw new ArgumentNullException("必须调用SetOrderBy方法进行设置排序字段");

            string alias = string.Empty;
            string pk = string.Empty;
            if (TableAlias.IsNotNullOrEmpty())
            {
                alias = string.Format("AS {0}", TableAlias);
                pk = string.Format("{0}.{1}", TableAlias, PrimaryKey);
            }
            else
            {
                pk = PrimaryKey;
            }
            string whereString = GetCondition();
            string tempTableName = string.Format("{0}{1}", "A", Guid.NewGuid().ToString("N"));
            // 如果没有条件，对全表进行统计行数
            string sysSql = string.Format(@"dbcc updateusage(0,{0}) with no_infomsgs
SELECT @DataCount =SUM (CASE WHEN (index_id < 2) THEN row_count ELSE 0 END) FROM sys.dm_db_partition_stats
 WHERE object_id = object_id('{0}')", TableName);
            // 按条件查询
            string mSql = string.Format(@"SELECT @DataCount=COUNT(1) FROM {0} {1} {2} {3}", TableName, alias, LeftJoin, whereString);
            SQLCmdText = string.Format(@"DECLARE @DataCount int
{13}
        SELECT {0},@DataCount as DataCount FROM {1} {2} 
            {3} 
            WHERE {12} IN 
            (
                SELECT {4} FROM 
                    (
                        SELECT TOP {9} {12},ROW_NUMBER() OVER({5}) AS R_NO FROM {1} {2} {3} {6} {5}
                    ){11} WHERE R_NO BETWEEN {8} AND {10}
            ) {7} {5}", Fields.ToJoin(), TableName, alias, LeftJoin, PrimaryKey, OrderBy, whereString, GroupBy, pageIndex, pageSize, pageIndex * pageSize, tempTableName, pk, whereString.IsNullOrEmpty() ? sysSql : mSql);


            Succeed = true;
            return Succeed;
        }

        #region Propeties

        /**
         * @ 页码
         * */
        private int pageIndex = 0;
        public int PageIndex
        {
            get
            {
                if (pageIndex < 1)
                    pageIndex = 0;
                else
                    pageIndex--;
                return pageIndex;
            }
            set { pageIndex = value; }
        }
        /**
         * @ 页大小
         * */
        private int pageSize = 0;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value; }
        }
        #endregion
    }
}
