using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.Common;
using System.Configuration;
using System.Data.SQLite;
using System.Data.SQLite.Generic;
using Perfor.Lib.Enums;
using Perfor.Lib.Extension;



namespace Perfor.Lib.Helpers
{
    public class SQLContext : IDisposable
    {
        /**
         *  @ 配置文件指定的数据库类型
         *  @ 在AppSettings配置节中指定
         * */
        private const string DbTypeKey = "DbType";
        private const string defaultConnectionString = "defaultConnectionString";
        private bool m_disposeing = false;

        /**
         * @ 默认构造函数，使用该函数必须在appconfig配置文件中指定connectonstring节
         * */
        public SQLContext()
        {
        }

        /**
         * @ 构造函数第一次重载，自动构造数据连接对象
         * @ dbType 数据库类型
         * @ connectionString 数据库连接字符串
         * */
        public SQLContext(SQLDataBaseType dbType, string connectionString)
        {
            this.dbType = dbType;
            this.connectionString = connectionString;
        }

        /**
         * @ 析构函数释放资源
         * */
        ~SQLContext()
        {
            Dispose(false);
        }

        /**
         * @ 初始化数据库连接
         * */
        public void EnSureConnection()
        {
            if (openedConnection)
            {
                connectionRequestCount++;
                return;
            }

            if (connectionString.IsNullOrEmpty())
            {
                ConnectionStringSettings conSett = ConfigurationManager.ConnectionStrings[defaultConnectionString];
                if (conSett == null)
                {
                    throw new ArgumentNullException("如果使用默认的SQLContext构造函数，必须在主程序配置文件中配置节点：connectionStrings，并添加一个add name为defaultConnectionString的数据库连接字符串");
                }
                connectionString = conSett.ConnectionString;
                if (connectionString.IsNullOrEmpty())
                {
                    throw new ArgumentNullException("connectionStrings在配置文件中已设置，但没有找到可用的连接字符串");
                }
                string dbtypekey = ConfigurationManager.AppSettings[DbTypeKey];
                if (dbtypekey.IsNotNullOrEmpty())
                {
                    Enum.TryParse<SQLDataBaseType>(dbtypekey, out dbType);
                }
            }
            GetDbConnection();
        }

        /**
         * @ 获取数据库链接对象
         * */
        protected DbConnection GetDbConnection()
        {
            switch (dbType)
            {
                case SQLDataBaseType.MSSQL:
                    conn = new SqlConnection(connectionString);
                    break;
                case SQLDataBaseType.ACCESS:
                    conn = new OleDbConnection(connectionString);
                    break;
                case SQLDataBaseType.MYSQL:
                    break;
                case SQLDataBaseType.ORACLE:
                    break;
                case SQLDataBaseType.SQLITE:
                    conn = new SQLiteConnection(connectionString);
                    break;
                default:
                    throw new ArgumentException("数据库类型不存在！");
            }
            return conn;
        }

        /**
         * @ 获取数据库执行命令对象
         * @ cmdType 该执行命令的类型
         * */
        public DbCommand GetDbCommand(CommandType cmdType = CommandType.Text)
        {
            switch (dbType)
            {
                case SQLDataBaseType.MSSQL:
                    ccmd = new SqlCommand("", (SqlConnection)DbConn);
                    break;
                case SQLDataBaseType.ACCESS:
                    ccmd = new OleDbCommand("", (OleDbConnection)DbConn);
                    break;
                case SQLDataBaseType.MYSQL:
                    break;
                case SQLDataBaseType.ORACLE:
                    break;
                case SQLDataBaseType.SQLITE:
                    ccmd = new SQLiteCommand((SQLiteConnection)DbConn);
                    break;
                default:
                    throw new ArgumentException("数据库类型不存在！");
            }
            ccmd.CommandType = cmdType;
            return ccmd;
        }

        /**
         * @ 获取数据库适配器对象
         * */
        protected DbDataAdapter GetDbAdapter()
        {
            switch (dbType)
            {
                case SQLDataBaseType.MSSQL:
                    adapter = new SqlDataAdapter((SqlCommand)ccmd);
                    break;
                case SQLDataBaseType.ACCESS:
                    adapter = new OleDbDataAdapter();
                    break;
                case SQLDataBaseType.MYSQL:
                    break;
                case SQLDataBaseType.ORACLE:
                    break;
                case SQLDataBaseType.SQLITE:
                    adapter = new SQLiteDataAdapter((SQLiteCommand)ccmd);
                    break;
                default:
                    throw new ArgumentException("数据库类型不存在！");
            }
            return adapter;
        }

        /**
         * @ 获取数据库参数对象
         * */
        public DbParameter GetDbParameter()
        {
            switch (dbType)
            {
                case SQLDataBaseType.MSSQL:
                    return new SqlParameter();
                case SQLDataBaseType.ACCESS:
                    return new OleDbParameter();
                case SQLDataBaseType.MYSQL:
                    break;
                case SQLDataBaseType.ORACLE:
                    break;
                case SQLDataBaseType.SQLITE:
                    return new SQLiteParameter();
                default:
                    throw new ArgumentException("数据库类型不存在！");
            }
            return null;
        }

        #region 事务执行
        /**
         *  @ 开始事务
         *  @ 该方法必须在DbConnection对象创建后和DbCommand执行前调用才可发挥作用
         * */
        public void TransBegin()
        {
            if (conn == null || conn.State == ConnectionState.Closed)
                throw new ArgumentNullException("不正确的事务对象设置，必须先初始化DbConection对象");

            if (ccmd == null)
                throw new ArgumentNullException("不正确的事务对象设置，必须先初始化DbCommand对象");

            // 必须是关闭状态才能开启事务，提交或者回滚都不能开启
            if (tranState == TranSactionState.Off)
            {
                ccmd.Transaction = conn.BeginTransaction();
                tranState = TranSactionState.Begin;
            }
        }

        /**
         *  @ 提交事务
         * */
        public void TransCommit()
        {
            if (ccmd != null && ccmd.Transaction != null)
            {
                tranState = TranSactionState.Commit;
                ccmd.Transaction.Commit();
                tranState = TranSactionState.Off;
            }
        }

        /**
         *  @ 回滚事务
         * */
        public void TransRollback()
        {
            if (ccmd != null && ccmd.Transaction != null)
            {
                tranState = TranSactionState.Rollback;
                ccmd.Transaction.Rollback();
                tranState = TranSactionState.Off;
            }
        }
        #endregion

        #region Properties
        private int connectionRequestCount = 0;
        /**
         * @ 请求创建连接次数
         * */
        public int ConnectionRequestCount
        {
            get { return connectionRequestCount; }
            set { connectionRequestCount = value; }
        }
        private bool openedConnection = false;
        /**
         * @ 数据库连接是否已经被打开
         * */
        public bool OpenedConnection
        {
            get { return openedConnection; }
            set { openedConnection = value; }
        }
        private DbConnection conn = null;
        /**
         * @ 当前数据库链接
         * */
        public DbConnection DbConn
        {
            get { return conn; }
            private set { conn = value; }
        }

        private DbCommand ccmd = null;
        /**
        * @DbCmd：当前数据库命令工具
        * */
        public DbCommand DbCmd
        {
            get
            {
                if (ccmd == null)
                {
                    GetDbCommand();
                }

                return ccmd;
            }
            private set { ccmd = value; }
        }

        private DbDataReader reader = null;
        /**
         * @ 数据流读取对象
         * */
        public DbDataReader DbReader { get { return reader; } set { reader = value; } }

        private DbDataAdapter adapter = null;
        /**
         * @ 数据库适配器对象
         * */
        public DbDataAdapter DbAdapter
        {
            get
            {
                if (adapter == null)
                {
                    GetDbAdapter();
                }

                return adapter;

            }
            set { adapter = value; }
        }

        private List<DbParameter> dbparas = null;
        /**
         * @ 参数管理对象
         * */
        public List<DbParameter> DbParas
        {
            get
            {
                if (dbparas == null)
                    dbparas = new List<DbParameter>();
                return dbparas;
            }
            set { dbparas = value; }
        }

        private string connectionString = string.Empty;
        /**
         * @ 数据库连接字符串
         * */
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        private SQLDataBaseType dbType = SQLDataBaseType.MSSQL;
        /**
         * @ 当前使用的数据库类型
         * @ 已默认使用 DataBaseType.MSSQL
         * */
        public SQLDataBaseType DbType
        {
            get { return dbType; }
            set { dbType = value; }
        }

        private TranSactionState tranState = TranSactionState.Off;
        /**
         * @ 当前事务执行状态
         * */
        public TranSactionState TranState
        {
            get { return tranState; }
        }
        #endregion

        /**
         * @ 实现接口
         * */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /**
         * @ 清理托管资源
         * */
        private void Dispose(bool disposeing)
        {
            if (!this.m_disposeing && disposeing)
            {
                Close();
                m_disposeing = true;
            }
        }

        /**
         * @ 清理资源
         * */
        public void Close()
        {
            try
            {
                if (reader != null)
                    reader.Close();
                if (conn != null && conn.State != ConnectionState.Closed)
                    conn.Close();
                if (ccmd != null)
                {
                    if (dbparas != null)
                        dbparas.Clear();
                    ccmd.Dispose();
                }
                if (adapter != null)
                    adapter.Dispose();
            }
            finally
            {
                ConnectionRequestCount--;
                OpenedConnection = false;
            }
        }
    }
}
