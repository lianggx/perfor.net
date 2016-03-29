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
     * @ 获取一些行，这个类是一个奇葩的存在，暂未重构
     * */
    public partial class MssqlGetSomeOne : MssqlQueryHelper
    {
        #region Identity
        /**
         * @ 默认构造函数
         * */
        public MssqlGetSomeOne() : base() { }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlGetSomeOne(string tableName) : base(tableName, "") { }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlGetSomeOne(string tableName, string sqlCmdText) : base(tableName, sqlCmdText) { }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlGetSomeOne(string tableName, SQLContext context) : base(tableName, context) { }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlGetSomeOne(string tableName, SQLContext context, string sqlCmdText) : base(tableName, context, sqlCmdText) { }
        #endregion

        /**
         * @ 查询数据集，该方法仅支持对单表进行封装
         * @ 注意：如果是进行多表连接查询，不管查询的字段多少，都以T类型为标准
         * @ 如果希望返回多表查询的完整字段，请考虑使用 Select 非泛型方法
         * */
        public List<T> Select<T>() where T : class, new()
        {
            List<T> dataList = new List<T>();
            // 检查是否设置了主键            
            Type type = typeof(T);
            PropertyInfo[] piArray = type.GetProperties();
            string[] fields = new string[piArray.Length];
            for (int i = 0; i < piArray.Length; i++)
            {
                fields[i] = piArray[i].Name;
            }
            TableName = type.Name;
            dataList = Select<T>(fields, "");

            return dataList;
        }

        /**
         * @ 查询数据集，该方法仅支持对单表进行封装，泛型方法第一次重载
         * @ 注意：如果是进行多表连接查询，不管查询的字段多少，都以T类型为标准
         * @ 如果希望返回多表查询的完整字段，请考虑使用 Select 非泛型方法
         * @ primaryKey 表的主键
         * @ fields 要查询的字段名称
         * @ leftJoin 连接查询的语句
         * */
        public List<T> Select<T>(IEnumerable<string> fields, string leftJoin) where T : class, new()
        {
            DbDataReader reader = DoReader(fields, leftJoin);
            reader.Read();
            List<T> dataList = GetDataResult<T>(reader);
            return dataList;
        }

        /**
         * @ 查询数据集
         * @ primaryKey 查询主表的主键字段名称
         * @ fields 要查询的字段名称
         * @ leftJoin 左连接查询语句
         * */
        public List<SQLDataResult> Select(IEnumerable<string> fields, string leftJoin)
        {
            DbDataReader reader = DoReader(fields, leftJoin);
            reader.Read();
            List<SQLDataResult> list = GetDataResult(reader);

            return list;
        }

        /**
         * @ 读取数据
         * @ primaryKey 查询主表的主键字段名称
         * @ fields 要查询的字段名称
         * @ leftJoin 左连接查询语句
         * */
        private DbDataReader DoReader(IEnumerable<string> fields, string leftJoin)
        {
            this.Fields = fields.ToList();
            this.LeftJoin = leftJoin;
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

            string alias = string.Empty;
            if (TableAlias.IsNotNullOrEmpty())
            {
                alias = string.Format("AS {0}", TableAlias);
            }
            string whereString = GetCondition();
            SQLCmdText = string.Format(@"SELECT {0} FROM {1} {2} {3} {4} {5} {6}", Fields.ToJoin(), TableName, alias, LeftJoin, whereString, OrderBy, GroupBy);

            Succeed = true;
            return Succeed;
        }

        #region Propeties
        #endregion
    }
}
