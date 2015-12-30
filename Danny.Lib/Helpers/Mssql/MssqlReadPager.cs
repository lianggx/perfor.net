using Danny.Lib.Enums;
using Danny.Lib.Extension;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Danny.Lib.Helpers.Mssql
{
    /**
     * @ 分页查询
     * */
    public partial class MssqlReadPager : SQLHelper
    {
        #region Identity
        /**
 * @ 默认构造函数
 * */
        public MssqlReadPager()
            : base()
        {
            InitializeComponent(null);
        }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlReadPager(string tableName)
            : this(tableName, "")
        {
        }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlReadPager(string tableName, string sqlCmdText)
        {
            InitializeComponent(tableName, sqlCmdText);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlReadPager(string tableName, SQLContext context)
            : base(context)
        {
            InitializeComponent(tableName);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlReadPager(string tableName, SQLContext context, string sqlCmdText)
            : base(context)
        {
            InitializeComponent(tableName, sqlCmdText);
        }

        /**
         * @ 初始化内部数据
         * */
        private void InitializeComponent(string tableName, string sqlCmdText = null)
        {
            SetTableName(tableName);
            this.SQLCmdText = SQLCmdText;
            this.condition = new List<SQLCondition>();
        }
        #endregion

        /**
         * @ 查询数据集，该方法仅支持对单表进行封装
         * @ 注意：如果是进行多表连接查询，不管查询的字段多少，都以T类型为标准
         * @ 如果希望返回多表查询的完整字段，请考虑使用 Select 非泛型方法
         * @ page 页码
         * @ size 页大小
         * */
        public List<T> Select<T>(int page, int size, out int rowCount) where T : class,new()
        {
            rowCount = 0;
            List<T> dataList = new List<T>(size);
            // 检查是否设置了主键            
            Type type = typeof(T);
            PropertyInfo[] piArray = type.GetProperties();

            if (primaryKey.IsNullOrEmpty())
            {
                int len = piArray.Length;
                if (len == 0)
                    throw new ArgumentException("没有可以使用的删除条件");

                PropertyInfo pi = null;
                for (int i = 0; i < len; i++)
                {
                    if (CheckPrimaryKey(piArray[i]) == false) continue;

                    pi = piArray[i];
                    break;

                }
                if (pi == null)
                    throw new ArgumentException(string.Format("必须指定PrimaryKey属性或者对实体类 {0} 进行实体键 LdfSQLEntityKey 的配置，且只能出现一次，请勿使用复合主键", type.Name));

                primaryKey = pi.Name;
            }
            string[] fields = new string[piArray.Length];
            for (int i = 0; i < piArray.Length; i++)
            {
                fields[i] = piArray[i].Name;
            }

            dataList = Select<T>(primaryKey, fields, "", page, size, out rowCount);

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
        public List<T> Select<T>(string primaryKey, string[] fields, string leftJoin, int page, int size, out int rowCount) where T : class,new()
        {
            rowCount = 0;
            List<T> dataList = new List<T>(size);
            try
            {
                Type type = typeof(T);
                if (TableName.IsNullOrEmpty())
                {
                    TableName = type.Name;
                }
                if (orderBy.IsNullOrEmpty())
                    SetOrderBy(primaryKey, SQLExpression.Order.ASC);
                PropertyInfo[] pis = type.GetProperties();

                DbDataReader reader = DoReader(primaryKey, fields, leftJoin, page, size);
                if (reader == null)
                    return null;

                reader.Read();
                rowCount = reader["DataCount"].ObjToInt();
                do
                {
                    T result = new T();
                    int len = fields.Length;
                    for (int i = 0; i < pis.Length; i++)
                    {
                        PropertyInfo pi = pis[i];
                        object rValue = reader[pi.Name];
                        if (rValue != null)
                            pi.SetValue(result, rValue, null);
                    }
                    dataList.Add(result);
                } while (Context.DbReader.Read());

            }
            finally
            {
                Dispose(false);
            }
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
        public List<SQLDataResult> Select(string primaryKey, string[] fields, string leftJoin, int page, int size, out int rowCount)
        {
            rowCount = 0;
            List<SQLDataResult> list = new List<SQLDataResult>();
            try
            {
                DbDataReader reader = DoReader(primaryKey, fields, leftJoin, page, size);
                if (reader == null || reader.HasRows == false)
                    return null;
                reader.Read();
                rowCount = reader["DataCount"].ObjToInt();
                do
                {
                    SQLDataResult result = new SQLDataResult();
                    int len = fields.Length;

                    for (int i = 0; i < len; i++)
                    {
                        result.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    list.Add(result);
                } while (Context.DbReader.Read());

            }
            finally
            {
                Dispose(false);
            }

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
        private DbDataReader DoReader(string primaryKey, string[] fields, string leftJoin, int page, int size)
        {
            this.primaryKey = primaryKey;
            this.fields = fields;
            this.leftJoin = leftJoin;
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
         * @ 给条件列表增加一个左括号（
         * */
        public void AddBracketLeft()
        {
            condition.Add(new BracketLeft());
        }

        /**
         * @ 给条件列表增加一个右括号）
         * */
        public void AddBracketRight()
        {
            condition.Add(new BracketRight());
        }

        /**
         * @ 增加一个条件
         * @ field 字段名称
         * @ value 值，支持空值
         * */
        public void AddWhere(string field, object value)
        {
            AddWhere(field, SQLExpression.ExprOperator.Eq, value);
        }

        /**
        * @ 增加一个条件
        * @ field 字段名称
        * @ expr 运算符
        * @ value 值，支持空值
        * */
        public void AddWhere(string field, SQLExpression.ExprOperator expr, object value)
        {
            SQLCondition lsc = new SQLCondition(field, value, expr);
            condition.Add(lsc);
        }

        /**
      * @ 增加一个条件
      * @ field 字段名称
      * @ expr 运算符
      * @ value 值，支持空值
      * @ joinType 连接上一个条件的运算位
      * */
        public void AddWhere(string field, SQLExpression.ExprOperator expr, object value, SQLExpression.JoinType joinType)
        {
            condition.Add(new SQLCondition(field, value, expr, joinType));
        }

        /**
         * @ 获取条件列表的表现形式
         * */
        private string GetCondition()
        {
            if (condition.Count == 0)
                return "";

            StringBuilder whereString = new StringBuilder();
            for (int i = 0; i < condition.Count; i++)
            {
                var item = condition[i];
                if (item.GetType() == typeof(SQLCondition))
                {
                    whereString.Append(GetCondition(i, item));
                }
                else if (item.GetType() == typeof(BracketLeft))
                {

                    if (i + 1 == condition.Count)
                    {
                        throw new ArgumentException("最后一个参数不能是左括号（");
                    }
                    i++;
                    item = condition[i];
                    // 因为是第一个括号连接条件，不需要连接符号，所以第一个参数传递0
                    string leftWhere = string.Format("{0} ({1}", item.JoinType.GetEnumString(), GetCondition(0, item));
                    whereString.Append(leftWhere);
                }
                else if (item.GetType() == typeof(BracketRight))
                {
                    whereString.Append(")");
                }
            }

            string ws = string.Format("WHERE {0}", whereString.ToString().ToTrimSpace());

            return ws;
        }

        private string GetCondition(int index, SQLCondition item)
        {
            string jt = index == 0 ? "" : item.JoinType.GetEnumString();
            // 构造参数化形式
            DbParameter par = AddParameter(Guid.NewGuid().ToString("N"), item.Value);
            if ((item.Expression & (SQLExpression.ExprOperator.Like | SQLExpression.ExprOperator.NotLike)) == item.Expression)
                return string.Format("{0} {1} {2} '%{3}%' ", jt, item.Name, item.Expression.GetEnumString(), par.ParameterName);
            else
                return string.Format("{0} {1} {2} {3} ", jt, item.Name, item.Expression.GetEnumString(), par.ParameterName);
        }

        /**
         * @ 初始化SQL命令，准备执行
         * */
        protected override bool InitSQLWithCmdText()
        {
            if (TableName.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置属性TableName的值，该值为查询的主表名称");

            if (primaryKey.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置属性PrimaryKey的值，该值主表的主键字段");

            if (leftJoin.IsNotNull() && tableAlias.IsNullOrEmpty())
                throw new ArgumentNullException("当存在多表连接查询时，必须指定主表的别名，即属性TableAlias的值");

            if (fields.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置要查询的字段fields");

            if (pageSize < 1)
                throw new ArgumentNullException("必须设置属性pageSize的值，且该值必须大于0");

            if (orderBy.IsNullOrEmpty())
                throw new ArgumentNullException("必须调用SetOrderBy方法进行设置排序字段");

            string alias = string.Empty;
            string pk = string.Empty;
            if (tableAlias.IsNotNull())
            {
                alias = string.Format("AS {0}", tableAlias);
                pk = string.Format("{0}.{1}", tableAlias, primaryKey);
            }
            else
            {
                pk = primaryKey;
            }
            string whereString = GetCondition();
            string tempTableName = string.Format("{0}{1}", "A", Guid.NewGuid().ToString("N"));
            // 如果没有条件，对全表进行统计行数
            string sysSql = string.Format(@"dbcc updateusage(0,{0}) with no_infomsgs
SELECT @DataCount =SUM (CASE WHEN (index_id < 2) THEN row_count ELSE 0 END) FROM sys.dm_db_partition_stats
 WHERE object_id = object_id('{0}')", TableName);
            // 按条件查询
            string mSql = string.Format(@"SELECT @DataCount=COUNT(1) FROM {0} {1} {2} {3}", TableName,alias,leftJoin, whereString);
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
            ) {7} {5}", fields.ToJoin(), TableName, alias, leftJoin, primaryKey, orderBy, whereString, groupBy, pageIndex, pageSize, pageIndex * pageSize, tempTableName, pk, whereString.IsNullOrEmpty() ? sysSql : mSql);


            Succeed = true;
            return Succeed;
        }

        /**
         * @ 设置排序
         * @ tableFields 表字段集合，形如url参数key：数据库真实字段名称
         * @ orderby 客户端传回要排序的参数，形如：key,order，key即名称，order用int表示，具体参考LdfSQLExpression.Order，0：ASC，1：DESC
         * @ separator 客户端排序参数的分隔符号
         * @ defaultField 默认排序字段
         * @ defaultDirection 默认排序方式
         * */
        public string SetOrderBy(Dictionary<string, string> tableFields, string orderby, char separator, string defaultField, SQLExpression.Order defaultDirection = SQLExpression.Order.DESC)
        {
            if (defaultField.IsNullOrEmpty())
                throw new ArgumentException("必须设置默认排序字段");

            string orderField = defaultField;
            string[] orderValue = orderby.Split(separator);
            if (orderValue.IsNotNullAndEq(2))
            {
                string okey = orderValue[0];
                if (tableFields.ContainsKey(okey))
                {
                    defaultField = tableFields[okey];
                    string dire = orderValue[1];

                    if (dire.GetType().IsEnum<SQLExpression.Order>())
                    {
                        defaultDirection = dire.ToEnum<SQLExpression.Order>();
                    }
                }
            }

            SetOrderBy(defaultField, defaultDirection);

            return orderField;
        }

        /**
         * @ 设置排序
         * @ field 排序字段名称
         * @ order 指定排序方式
         * */
        public void SetOrderBy(string field, SQLExpression.Order direction)
        {
            orderBy = string.Format(" ORDER BY {0} {1}", field, direction);
        }

        /**
         * @ 设置分组字段
         * */
        public void SetGroupBy(params string[] fields)
        {
            if (fields.IsNullOrEmpty())
                return;
            groupBy = string.Format(" GROUP BY {0} ", fields.ToJoin(","));
        }

        #region Propeties

        private string tableAlias = string.Empty;
        /**
         * @ 主表的别名
         * */
        public string TableAlias
        {
            get { return tableAlias; }
            set { tableAlias = value; }
        }

        private string leftJoin = string.Empty;
        /**
         * @ 左连接语句
         * */
        public string LeftJoin
        {
            get { return leftJoin == null ? "" : leftJoin; }
            set { leftJoin = value; }
        }

        private string[] fields = null;
        /**
         * @ 要查询的字段名称
         * */
        public string[] Fields
        {
            get { return fields; }
            set { fields = value; }
        }


        private string primaryKey = string.Empty;
        /**
         * @ 主表的主键
         * */
        public string PrimaryKey
        {
            get { return primaryKey; }
            set { primaryKey = value; }
        }
        private string groupBy = string.Empty;
        /**
         * @ 分组字段，形如：groupby xx,xx,xx
         * */
        public string GroupBy
        {
            get { return groupBy; }
            set { groupBy = value; }
        }

        private string orderBy = string.Empty;
        /**
       * @ 排序字段，形如：orderby xx asc,xxx desc
       * */
        public string OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; }
        }

        List<SQLCondition> condition = null;
        /**
         * @ WHERE参数列表
         * */
        public List<SQLCondition> Conditions { get; set; }

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
