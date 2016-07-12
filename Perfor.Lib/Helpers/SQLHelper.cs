using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.Common;
using System.Reflection;
using Perfor.Lib.Extension;
using Perfor.Lib.Enums;
using Perfor.Lib.Common;

namespace Perfor.Lib.Helpers
{
    [Serializable]
    /**
     *  @ 数据操作基类
     * */
    public abstract class SQLHelper : IDisposable
    {
        #region Identity
        private bool m_disposeing = false;
        /**
         *  @ 默认构造函数
         * */
        protected SQLHelper()
        {
            this.context = new SQLContext();
        }

        /**
         * @context 数据库上下文对象
         * */
        protected SQLHelper(SQLContext context)
        {
            this.context = context;
        }
        /**
         *  @ 默认构造函数
         *  @connectionString 数据库连接字符串
         * */
        protected SQLHelper(SQLDataBaseType dbType, string connectionString)
            : this(new SQLContext(dbType, connectionString))
        {

        }
        #endregion

        /**
         *  @ 对连接对象执行 SQL 语句。
         * */
        protected int ExecuteNonQuery()
        {
            int result = 0;
            SetParas();
            result = context.DbCmd.ExecuteNonQuery();
            return result;
        }

        /**
		 *  @ 对连接对象执行 SQL 语句。
         *  @ 注意：使用该方法必须手动调用Close方法
		 * */
        protected DbDataReader ExecuteReader()
        {
            SetParas();
            context.DbReader = context.DbCmd.ExecuteReader();
            return context.DbReader;
        }

        /**
		 *  @ 对连接对象执行 SQL 语句。
		 * */
        protected object ExecuteScalar()
        {
            object result = null;
            SetParas();
            result = context.DbCmd.ExecuteScalar();
            return result;
        }

        /**
		 *  @ 对连接对象执行 SQL 语句。
         *  @ tableName 数据库表名称
         *  @ page 页码
         *  @ size 页大小
		 * */
        protected DataSet ExecuteToFill(string tableName, int page = 0, int size = 20)
        {
            DataSet result = new DataSet("newSet");
            context.DbAdapter.SelectCommand = context.DbCmd;
            SetParas();
            int startion = page * size;
            context.DbAdapter.Fill(result, startion, size, tableName);
            return result;
        }

        /**
          * @ 将对象加入待处理列表，方便稍后进行的批量新增或者更新操作
         *  @ fields和values数组长度必须一致
          * @ fields 要插入的字段名称
          * @ values 字段对应的值 
         *  @ primaryKeyIndex 实体键在fields参数列表中的索引
          * */
        protected SQLParameter AddObject(IEnumerable<string> fields, IEnumerable<object> values, int primaryKeyIndex = -1)
        {
            if (fields.IsNullOrEmpty())
                throw new ArgumentNullException("fields 参数不能为空");
            if (values.IsNullOrEmpty())
                throw new ArgumentNullException("values 参数不能为空");
            if (fields.Count() != values.Count())
                throw new ArgumentException("字段名称和值的数量必须一致");
            if (primaryKeyIndex == -1)
                throw new ArgumentException("必须指定参数primaryKeyIndex的值");

            SQLParameter item = new SQLParameter(fields, values, primaryKeyIndex);
            item.TableName = tablename;
            parameters.Add(item);

            return item;
        }

        /**
          * @ 初始化SQL插入语句，该方法使用反射，性能有所降低，请酌情使用
          * @ fields 要插入的字段名称
          * @ values 字段对应的值 
          * */
        protected bool AddObject<T>(T obj, SQLOption option = SQLOption.NORMAL) where T : class, new()
        {
            if (obj == null)
                throw new ArgumentNullException("obj对象不能为空");

            if (TableName.IsNullOrEmpty())
            {
                TableName = obj.GetType().Name;
            }

            PropertyInfo[] piArray = obj.GetType().GetProperties();
            int len = piArray.Length;
            if (len == 0)
                return false;

            int pkCount = 0;
            SQLParameter par = new SQLParameter();
            List<string> fields = new List<string>(len);
            List<object> values = new List<object>(len);
            for (int i = 0; i < len; i++)
            {
                PropertyInfo pi = piArray[i];
                if (CheckPrimaryKey(pi))
                {
                    pkCount++;
                    par.PrimaryKeyIndex = i;
                }

                if (pkCount > 1)
                {
                    throw new ArgumentException("实体类属性中对实体键 LdfSQLEntityKey 的配置只能出现一次，请勿使用复合主键");
                }

                if (pkCount == 0 && option == (option & (SQLOption.UPDATE | SQLOption.DELETE)))
                {
                    throw new ArgumentException("必须对实体类进行实体键 LdfSQLEntityKey 的配置，且只能出现一次，请勿使用复合主键");
                }
                fields.Add(pi.Name);
                values.Add(pi.GetValue(obj, null));
            }
            par.Fields = fields;
            par.Values = values;
            Parameters.Add(par);


            return Succeed;
        }

        /**
         * @ 检查属性是否配置了LdfSQLEntityKey特性 PrimaryKey=true
         * @ pi 要检查的属性
         * */
        protected bool CheckPrimaryKey(PropertyInfo pi)
        {
            object[] eks = pi.GetCustomAttributes(typeof(SQLEntityKey), false);
            if (eks != null && eks.Length > 0)
            {
                SQLEntityKey entityKey = (SQLEntityKey)eks[0];
                if (entityKey.PrimaryKey)
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * @ 初始化数据库命令
         * */
        protected virtual bool InitSQLWithCmdText()
        {
            throw new NotImplementedException("该方法必须由继承者实现具体的业务逻辑");
        }

        /**
         * @ 设置要查询的表名
         * */
        protected void SetTableName(string tableName)
        {
            this.tablename = tableName;
        }

        #region AddWhere
        /**
         * @ 给条件列表增加一个左括号（
         * */
        public void AddBracketLeft()
        {
            Conditions.Add(new BracketLeft());
        }

        /**
         * @ 给条件列表增加一个右括号）
         * */
        public void AddBracketRight()
        {
            Conditions.Add(new BracketRight());
        }

        /**
         * @ 增加一个条件
         * @ field 字段名称
         * @ value 值，支持空值
         * */
        public SQLHelper AddWhere(string field, object value)
        {
            return AddWhere(field, SQLExpression.ExprOperator.Eq, value);
        }

        /**
        * @ 增加一个条件
        * @ field 字段名称
        * @ expr 运算符
        * @ value 值，支持空值
        * */
        public SQLHelper AddWhere(string field, SQLExpression.ExprOperator expr, object value)
        {
            SQLCondition lsc = new SQLCondition(field, value, expr);
            Conditions.Add(lsc);
            return this;
        }

        /**
      * @ 增加一个条件
      * @ field 字段名称
      * @ expr 运算符
      * @ value 值，支持空值
      * @ joinType 连接上一个条件的运算位
      * */
        public SQLHelper AddWhere(string field, SQLExpression.ExprOperator expr, object value, SQLExpression.JoinType joinType)
        {
            Conditions.Add(new SQLCondition(field, value, expr, joinType));

            return this;
        }

        /**
         * @ 获取条件列表的表现形式
         * */
        protected string GetCondition()
        {
            if (Conditions.Count == 0)
                return "";

            StringBuilder whereString = new StringBuilder();
            for (int i = 0; i < Conditions.Count; i++)
            {
                var item = Conditions[i];
                if (item.GetType() == typeof(SQLCondition))
                {
                    whereString.Append(GetCondition(i, item));
                }
                else if (item.GetType() == typeof(BracketLeft))
                {

                    if (i + 1 == Conditions.Count)
                    {
                        throw new ArgumentException("最后一个参数不能是左括号（");
                    }
                    i++;
                    item = Conditions[i];
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

        protected string GetCondition(int index, SQLCondition item)
        {
            string jt = index == 0 ? "" : item.JoinType.GetEnumString();
            // 构造参数化形式
            DbParameter par = AddParameter(item.Name, item.Value);
            if ((item.Expression & (SQLExpression.ExprOperator.Like | SQLExpression.ExprOperator.NotLike)) == item.Expression)
                return string.Format("{0} {1} {2} '%{3}%' ", jt, item.Name, item.Expression.GetEnumString(), par.ParameterName);
            else
                return string.Format("{0} {1} {2} {3} ", jt, item.Name, item.Expression.GetEnumString(), par.ParameterName);
        }
        #endregion

        #region 参数设置
        /**
         * @ 添加泛型参数
         * @ name 参数名称
         * @ value 参数值
         * */
        protected DbParameter AddParameter<T>(string name, T value)
        {
            return AddParameter(name, value, ParameterDirection.Input);
        }

        /**
         * @ 添加参数到参数列表
         * @ name 参数名称
         * @ value 参数值
         * @ direction 参数方向
         * */
        protected DbParameter AddParameter(string name, object value, ParameterDirection direction)
        {
            DbParameter dp = context.GetDbParameter();
            if (name.IndexOf('@') < 0)
            {
                name = string.Format("@{0}_{1}", name.Trim(), Guid.NewGuid().ToString("N"));
            }
            dp.ParameterName = name;
            if (value == null)
                value = DBNull.Value;
            dp.Value = value;
            dp.Direction = direction;
            context.DbParas.Add(dp);

            return dp;
        }

        /**
         * @ 设置参数
         * */
        protected void SetParas()
        {
            context.EnSureConnection();
            SetParas(context.DbCmd);
        }

        /**
		 * @ 设置参数，调用该方法请确保ccmd参数的连接已经建立
         * @ ccmd 执行SQL语句命令对象
		 * */
        protected void SetParas(DbCommand ccmd)
        {
            ccmd.CommandText = SQLCmdText;
            ccmd.Parameters.Clear();
            if (context.DbParas != null && context.DbParas.Count > 0)
            {
                foreach (var p in context.DbParas)
                {
                    ccmd.Parameters.Add(p);
                }
            }
            OpenConnection();
        }

        /**
         * @ 打开数据库连接
         * */
        protected void OpenConnection()
        {
            if (context.DbConn.State == ConnectionState.Closed)
                context.DbConn.Open();
            else if (context.DbConn.State == ConnectionState.Broken)
            {
                // 连接被中断，尝试重新打开
                context.DbConn.Close();
                context.DbConn.Open();
            }

            context.OpenedConnection = true;
        }
        #endregion

        #region 事务执行
        /**
         *  @ 开始事务
         *  @ 该方法必须在DbConnection对象创建后和DbCommand执行前调用才可发挥作用
         * */
        protected void TranBegin()
        {
            context.TransBegin();
        }

        /**
         *  @ 提交事务
         * */
        protected void TranCommit()
        {
            context.TransCommit();
        }

        /**
         *  @ 回滚事务
         * */
        protected void TranRollback()
        {
            context.TransRollback();
        }
        #endregion

        #region 资源释放

        /**
         * @ 关闭数据流对象，关闭数据库连接，重新丢回连接池
         * */
        protected virtual void Dispose(bool disposeing)
        {
            if (!this.m_disposeing && disposeing)
            {
                context.Close();
                this.m_disposeing = true;
            }
        }

        /**
         * @ 释放资源
         * @ 实现 IDispose接口的方法
         * */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Properties
        private SQLContext context = null;
        /**
         * @ 数据库上下文对象
         * */
        public SQLContext Context
        {
            get { return context; }
            set { context = value; }
        }

        private string sqlCmdText = string.Empty;
        /**
         * @ 要执行的SQL命令
         * */
        public string SQLCmdText
        {
            get
            {
                //sqlCmdText = Utilities.DetectSQLInjection(sqlCmdText);
                return sqlCmdText;
            }
            set { sqlCmdText = value; }
        }

        private string tablename = string.Empty;
        /**
         *  @ 重写父类的属性
         * */
        public virtual string TableName
        {
            get
            {
                return tablename;
            }
            set
            {
                tablename = value;
            }
        }

        private bool succeed = false;
        /**
         *  @ 处理结果
         * */
        public bool Succeed
        {
            get { return succeed; }
            set { succeed = value; }
        }

        private List<SQLParameter> parameters = null;
        /**
         * @ 参数列表
         * */
        public List<SQLParameter> Parameters
        {
            get
            {
                if (parameters == null)
                    parameters = new List<SQLParameter>();

                return parameters;
            }
        }

        private List<SQLCondition> condition = null;
        /**
         * @ WHERE参数列表
         * */
        public List<SQLCondition> Conditions
        {
            get
            {
                if (condition == null)
                    condition = new List<SQLCondition>();

                return condition;
            }
            set { condition = value; }
        }
        #endregion
    }
}
