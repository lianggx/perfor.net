using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Perfor.Lib.Extension;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using Perfor.Lib.Common;

namespace Perfor.Lib.Helpers.Mssql
{
    /**
     * @ 添加数据的入口
     * */
    public partial class MssqlInsert : SQLHelper, SQLIDataSave
    {
        /**
         * @ 默认构造函数
         * */
        public MssqlInsert() : base() { }

        /**
         *  @ 构造函数第一次重载
         *  @ tableName ：要插入的表名
         * */
        public MssqlInsert(string tableName)
        {
            SetTableName(tableName);
        }

        /**
         *  @ 构造函数第二次重载
         *  @ tableName ：要插入的表名
         *  @ context 数据库上下文对象
         * */
        public MssqlInsert(string tableName, SQLContext context)
            : base(context)
        {
            SetTableName(tableName);
        }

        /**
         * @ 初始化SQL插入语句，fields和values数组长度必须一致
         * @ fields 要插入的字段名称
         * @ values 字段对应的值 
         * */
        public void InsertObject(string[] fields, object[] values)
        {
            AddObject(fields, values);
        }

        /**
          * @ 初始化SQL插入语句，该方法使用反射，性能有所降低，请酌情使用
          * @ fields 要插入的字段名称
          * @ values 字段对应的值 
          * */
        public void InsertObject<T>(T obj) where T : class,new()
        {
            AddObject<T>(obj);
        }

        /**
         * @ 使用SqlBulkCopy进行大量数据插入，可支持100万级
         * @ 注意：该方法仅支持MSSQL数据库
         * @ 调用该方法后，不需要再另行调用SaveChange方法
         * @ table 要插入的表数据
         * */
        public bool InsertObject(DataTable table)
        {
            if (table == null && table.Rows.Count == 0)
                return Succeed;
            if (TableName.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置属性TableName，即目标数据库表名");

            Context.EnSureConnection();
            OpenConnection();
            SqlConnection conn = (SqlConnection)Context.DbConn;
            SqlBulkCopy bulkCopy = new SqlBulkCopy(conn);
            bulkCopy.DestinationTableName = TableName;
            bulkCopy.BatchSize = table.Rows.Count;
            try
            {
                OpenConnection();
                bulkCopy.WriteToServer(table);
                Succeed = true;
                bulkOrTvbs = true;
            }
            finally
            {
                Dispose(false);
                if (bulkCopy != null)
                    bulkCopy.Close();
            }
            return Succeed;
        }

        /**
         * @ 使用TVBs进行大量数据插入，可支持100万级
         * @ 注意：该方法仅支持MSSQL 2008+数据库
         * @ 调用该方法后，不需要再另行调用SaveChange方法
         * @ table 要插入的表数据
         * */
        public bool InsertObjectOnTVBs(DataTable table)
        {
            if (table == null && table.Rows.Count == 0)
                return Succeed;
            if (TableName.IsNullOrEmpty())
                throw new ArgumentNullException("必须设置属性TableName，即目标数据库表名");

            string paraName = Guid.NewGuid().ToString("N");
            StringBuilder fieldBuilder = new StringBuilder("");
            int len = table.Columns.Count;
            for (int i = 0; i < len; i++)
            {
                DataColumn item = table.Columns[i];
                fieldBuilder.Append(item.ColumnName);
                if (i + 1 < len)
                    fieldBuilder.Append(",");
            }
            SQLCmdText = string.Format("INSERT INTO {0} ({1})  SELECT {1} FROM @{2}", TableName, fieldBuilder.ToString(), paraName);

            SqlParameter tvbparam = new SqlParameter("@" + paraName, table);
            tvbparam.SqlDbType = SqlDbType.Structured;
            //表值参数的名字叫BulkUdt
            tvbparam.TypeName = "dbo.BulkUdt";
            try
            {
                Context.EnSureConnection();
                DbCommand cmd = Context.DbCmd;
                cmd.CommandText = SQLCmdText;
                cmd.Parameters.Add(tvbparam);
                OpenConnection();
                Succeed = cmd.ExecuteNonQuery() > 0;
                bulkOrTvbs = true;
            }
            finally
            {
                Dispose(false);
            }
            return Succeed;
        }

        /**
         * @ 初始化数据库命令
         * */
        protected override bool InitSQLWithCmdText()
        {
            if (TableName.IsNullOrEmpty())
            {
                throw new ArgumentException("插入目标数据库表名：tablename不能为空！");
            }
            StringBuilder cmdText = new StringBuilder();

            foreach (var item in Parameters)
            {
                StringBuilder insertBuilder = new StringBuilder(" INSERT");
                StringBuilder valuesBuilder = new StringBuilder(" VALUES(");
                insertBuilder.AppendFormat(" {0}(", TableName);
                List<string> fields = item.Fields;
                List<object> values = item.Values;
                int len = fields.Count;
                for (int i = 0; i < len; i++)
                {
                    string field = fields[i];
                    insertBuilder.AppendFormat(" [{0}]", field);
                    DbParameter para = AddParameter(field, values[i]);
                    valuesBuilder.AppendFormat("{0}", para.ParameterName);
                    if (i + 1 < len)
                    {
                        insertBuilder.Append(",");
                        valuesBuilder.Append(",");
                    }
                }
                valuesBuilder.Append(")");
                insertBuilder.Append(")");

                cmdText.AppendLine(string.Format("{0} {1}", insertBuilder.ToString(), valuesBuilder.ToString()));
            }
            SQLCmdText = cmdText.ToString().ToTrimSpace();
            return SQLCmdText.IsNotNullOrEmpty();
        }

        /**
         * @ 提交保存
         * */
        public bool SaveChange()
        {
            Succeed = false;
            if (bulkOrTvbs)
            {
                // 开关复位
                bulkOrTvbs = false;
                return Succeed;
            }
            int result = 0;
            if (InitSQLWithCmdText())
            {
                result = ExecuteNonQuery();
            }
            // 清空数据
            Parameters.Clear();
            Succeed= result > 0;

            return Succeed;
        }

        #region Properties
        // 当前是否执行了bulk或者tvbs的插入数据方式，设置此开关，防止重复调用SaveChange
        private bool bulkOrTvbs = false;
        #endregion
    }
}
