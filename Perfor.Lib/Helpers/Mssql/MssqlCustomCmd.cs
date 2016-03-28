using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Helpers.Mssql
{
    /**
     * @ 自定义SQL命令执行，完全开放和自由的语法
     * */
    public class MssqlCustomCmd : SQLHelper
    {
        #region Identity
        /**
 * @ 默认构造函数
 * */
        public MssqlCustomCmd()
            : base()
        {
            InitializeComponent(null);
        }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlCustomCmd(string sqlCmdText)
        {
            InitializeComponent(sqlCmdText);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlCustomCmd(SQLContext context)
            : base(context)
        {

        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlCustomCmd(SQLContext context, string sqlCmdText)
            : base(context)
        {
            InitializeComponent(sqlCmdText);
        }

        /**
         * @ 初始化内部数据
         * */
        private void InitializeComponent(string sqlCmdText)
        {
            this.SQLCmdText = SQLCmdText;
        }
        #endregion

        /**
         * @ 返回执行命令所影响的行数
         * */
        public new int ExecuteNonQuery()
        {
            int result = base.ExecuteNonQuery();
            Dispose(false);

            return result;
        }

        /**
         * @ 返回一个查询后的可读数据流
         * */
        public new DbDataReader ExecuteReader()
        {
            return base.ExecuteReader();
        }

        /**
         * @ 返回一个结果集合
         * */
        public List<SQLDataResult> ExecuteToDataResult()
        {
            List<SQLDataResult> dataList = null;
            try
            {
                base.ExecuteReader();
                DbDataReader reader = Context.DbReader;
                if (reader.HasRows == false)
                {
                    return null;
                }
                reader.Read();
                do
                {
                    SQLDataResult result = new SQLDataResult();
                    int len = reader.FieldCount;

                    for (int i = 0; i < len; i++)
                    {
                        result.Add(reader.GetName(i), reader.GetValue(i));
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
         * @ 返回执行结果后的首行首列（如果有结果）
         * */
        public new object ExecuteScalar()
        {
            object result = base.ExecuteScalar();
            Dispose(false);
            return result;
        }

        /**
         * @ 添加SQL命令对应的参数
         * */
        public void AddParams(string name, object value)
        {
            base.AddParameter(name, value);
        }

        /**
         * @ 所有初始化SQL语句的逻辑都可以写在这里
         * */
        protected override bool InitSQLWithCmdText()
        {
            throw new NotImplementedException("自定义命令不应该调用该方法");
        }

        #region Properties
        /**
         * @ 当前命令中的条件
         * */
        public new List<DbParameter> Parameters
        {
            get { return Context.DbParas; }
        }
        #endregion
    }
}
