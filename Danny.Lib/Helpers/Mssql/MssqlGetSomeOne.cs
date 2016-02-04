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
     * @ 获取一些行，这个类是一个奇葩的存在，暂未重构
     * */
    public partial class MssqlGetSomeOne : SQLHelper
    {
        #region Identity
        /**
         * @ 默认构造函数
         * */
        public MssqlGetSomeOne()
            : base()
        {
            InitializeComponent(null);
        }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlGetSomeOne(string tableName)
            : this(tableName, "")
        {
        }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlGetSomeOne(string tableName, string sqlCmdText)
        {
            InitializeComponent(tableName, sqlCmdText);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlGetSomeOne(string tableName, SQLContext context)
            : base(context)
        {
            InitializeComponent(tableName);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlGetSomeOne(string tableName, SQLContext context, string sqlCmdText)
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
        }
        #endregion

        /**
         * @ 查询数据集，该方法仅支持对单表进行封装
         * @ 注意：如果是进行多表连接查询，不管查询的字段多少，都以T类型为标准
         * @ 如果希望返回多表查询的完整字段，请考虑使用 Select 非泛型方法
         * */
        public List<T> Select<T>() where T : class,new()
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
        public List<T> Select<T>(IEnumerable<string> fields, string leftJoin) where T : class,new()
        {
            List<T> dataList = new List<T>();
            try
            {
                Type type = typeof(T);
                if (TableName.IsNullOrEmpty())
                {
                    TableName = type.Name;
                }

                DbDataReader reader = DoReader(fields, leftJoin);
                if (reader == null)
                    return dataList;
                int len = fields.Count();
                List<string> queryName = new List<string>();
                for (int i = 0; i < len; i++)
                {
                    queryName.Add(reader.GetName(i).ToLower());
                }

                PropertyInfo[] pis = type.GetProperties();
                reader.Read();
                do
                {
                    T result = new T();

                    for (int i = 0; i < pis.Length; i++)
                    {
                        PropertyInfo pi = pis[i];
                        if (queryName.Contains<string>(pi.Name.ToLower()) == false)
                            continue;

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
         * */
        public List<SQLDataResult> Select(IEnumerable<string> fields, string leftJoin)
        {
            List<SQLDataResult> list = new List<SQLDataResult>();
            try
            {
                DbDataReader reader = DoReader(fields, leftJoin);
                if (reader == null || reader.HasRows == false)
                    return list;
                reader.Read();
                do
                {
                    SQLDataResult result = new SQLDataResult();
                    int len = fields.Count();

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
         * */
        private DbDataReader DoReader(IEnumerable<string> fields, string leftJoin)
        {
            this.fields = fields;
            this.leftJoin = leftJoin;
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

            if (leftJoin.IsNotNullOrEmpty() && tableAlias.IsNullOrEmpty())
                throw new ArgumentNullException("当存在多表连接查询时，必须指定主表的别名，即属性TableAlias的值");

            if (fields.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置要查询的字段fields");

            string alias = string.Empty;
            if (tableAlias.IsNotNullOrEmpty())
            {
                alias = string.Format("AS {0}", tableAlias);
            }
            string whereString = GetCondition();
            SQLCmdText = string.Format(@"SELECT {0} FROM {1} {2} {3} {4} {5} {6}", fields.ToJoin(), TableName, alias, leftJoin, whereString, orderBy, groupBy);

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
         * @ 设置排序
         * @ field 排序字段，如 ID DESC,Name ASC
         * */
        public void SetOrderBy(string field)
        {
            orderBy = string.Format(" ORDER BY {0} ", field);
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

        private IEnumerable<string> fields = null;
        /**
         * @ 要查询的字段名称
         * */
        public List<string> Fields
        {
            get
            {
                if (fields == null)
                    fields = new List<string>();

                return fields.ToList();
            }
            set { fields = value; }
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
        #endregion
    }
}
