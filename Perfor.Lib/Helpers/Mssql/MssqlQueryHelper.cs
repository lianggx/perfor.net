using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Perfor.Lib.Extension;
using Perfor.Lib.Enums;
using System.Data.Common;
using System.Reflection;
using Perfor.Lib.Reflection;

namespace Perfor.Lib.Helpers.Mssql
{
    /**
    @ Mssql 查询帮助基类
    */
    public abstract class MssqlQueryHelper : SQLHelper
    {
        #region Identity
        /**
         * @context 数据库上下文对象
         * */
        protected MssqlQueryHelper(SQLContext context)
            : base(context)
        {
        }

        /**
         * @ 默认构造函数
         * */
        protected MssqlQueryHelper()
            : base()
        {
            InitializeComponent(null);
        }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        protected MssqlQueryHelper(string tableName)
            : this(tableName, "")
        {
        }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        protected MssqlQueryHelper(string tableName, string sqlCmdText)
        {
            InitializeComponent(tableName, sqlCmdText);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        protected MssqlQueryHelper(string tableName, SQLContext context)
            : base(context)
        {
            InitializeComponent(tableName);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        protected MssqlQueryHelper(string tableName, SQLContext context, string sqlCmdText)
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
         * @ 获取 SQLDataResult 列表
         * */
        protected List<SQLDataResult> GetDataResult(DbDataReader reader)
        {
            List<SQLDataResult> list = new List<SQLDataResult>();
            try
            {
                if (reader == null || reader.HasRows == false)
                    return list;
                do
                {
                    SQLDataResult result = new SQLDataResult();
                    int len = fields.Count();

                    for (int i = 0; i < len; i++)
                    {
                        result.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    list.Add(result);
                } while (reader.Read());
            }
            finally
            {
                Dispose(false);
            }

            return list;
        }

        /**
        * @ 查询数据集，该方法仅支持对单表进行封装，泛型方法第一次重载
        * @ reader
        * */
        public List<T> GetDataResult<T>(DbDataReader reader) where T : class, new()
        {
            List<T> dataList = new List<T>();
            if (reader == null)
                return null;

            try
            {
                DynamicManager<T> dm = new DynamicManager<T>();
                dm.InitDynamicObject(reader);
                do
                {
                    T returnObj = dm.CreateObject(reader);
                    dataList.Add(returnObj);
                } while (reader.Read());
                /*
                Type type = typeof(T);
                if (TableName.IsNullOrEmpty())
                {
                    TableName = type.Name;
                }

                int len = fields.Count();
                List<string> queryName = new List<string>();
                for (int i = 0; i < len; i++)
                {
                    queryName.Add(reader.GetName(i).ToLower());
                }
                PropertyInfo[] pis = type.GetProperties();
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
                 * */
            }
            finally
            {
                Dispose(false);
            }
            return dataList;
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

                    if (dire.IsEnum<SQLExpression.Order, string>())
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

        private List<string> fields = null;
        /**
         * @ 要查询的字段名称
         * */
        public List<string> Fields
        {
            get
            {
                if (fields == null)
                    fields = new List<string>();

                return fields;
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

        private string primaryKey = string.Empty;
        /**
         * @ 主表的主键
         * */
        public string PrimaryKey
        {
            get { return primaryKey; }
            set { primaryKey = value; }
        }
        #endregion
    }
}
