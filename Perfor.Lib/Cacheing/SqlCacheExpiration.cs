using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;

using Perfor.Lib.Extension;
using Perfor.Lib.Common;

namespace Perfor.Lib.Cacheing
{
    /**
     * @ SQL缓存管理类
     * */
    public class SqlCacheExpiration : IDisposable
    {
        #region Identity
        private bool m_disposeing = false;
        private string dbConnectionString = string.Empty;
        public event EventHandler SourceChange;
        private SqlChangeMonitor monitor = null;
        private SqlDependency dency = null;
        private CacheItem cacheItem = null;

        public SqlCacheExpiration() { }

        ~SqlCacheExpiration()
        {
            Dispose(false);
        }
        #endregion

        /**
        * @ 构造函数
        * @ dbString 数据库连接字符串
        * */
        public SqlCacheExpiration(string dbString)
        {
            if (string.IsNullOrEmpty(dbString))
            {
                throw new ArgumentNullException("dbConnectionStr不能为空");
            }
            dbConnectionString = dbString;
            SqlDependency.Start(dbConnectionString);
        }

        /**
         * @ 创建单个对象的SQL类型的缓存策略，该方法适用于单个字段类型对象
         * @ T 要创建的缓存对象类型，仅限单个类型
         * @ key 缓存的键
         * @ cmdText sql语句，必须包含架构信息，并且不能使用“*”号代替要查询的字段
         * @ offset 缓存过期时间
         * @ policy 缓存策略对象
         * @ sourceChange 数据源更改通知事件
         * @ priority 缓存逐出的优先级
         * */
        public CacheItem CreateSQLCacheSingle<T>(string key, string cmdText, DateTimeOffset offset, out CacheItemPolicy policy, CacheItemPriority priority = CacheItemPriority.Default) where T : class, new()
        {
            T t = new T();
            policy = new CacheItemPolicy();

            List<T> resultList = SelectData<T>(cmdText, out dency);
            if (resultList.Count > 0)
                t = resultList[0];
            monitor = new SqlChangeMonitor(dency);
            policy.AbsoluteExpiration = offset;
            policy.ChangeMonitors.Add(monitor);
            cacheItem = new CacheItem(key);
            cacheItem.Value = t;

            return cacheItem;
        }

        /**
         * @ 创建单个对象的SQL类型的缓存策略，该方法适用于单个字段类型对象
         * @ T 要创建的缓存对象类型，仅限单个类型
         * @ key 缓存的键
         * @ cmdText sql语句，必须包含架构信息，并且不能使用“*”号代替要查询的字段
         * @ offset 缓存过期时间
         * @ policy 缓存策略对象
         * @ sourceChange 数据源更改通知事件
         * @ priority 缓存逐出的优先级
         * */
        public CacheItem CreateSQLCacheScalar(string key, string cmdText, DateTimeOffset offset, out CacheItemPolicy policy, CacheItemPriority priority = CacheItemPriority.Default)
        {
            policy = new CacheItemPolicy();
            object result = SelectScalar<object>(cmdText, out dency);
            monitor = new SqlChangeMonitor(dency);
            policy.AbsoluteExpiration = offset;
            policy.ChangeMonitors.Add(monitor);
            cacheItem = new CacheItem(key);
            cacheItem.Value = result;
            return cacheItem;
        }

        /**
        * @ 创建单个对象的SQL类型的缓存策略，该方法适用于单个字段类型对象
        * @ T 要创建的缓存对象类型，仅限单个类型
        * @ key 缓存的键
        * @ cmdText sql语句，必须包含架构信息，并且不能使用“*”号代替要查询的字段
        * @ offset 缓存过期时间
        * @ policy 缓存策略对象
        * @ sourceChange 数据源更改通知事件
        * @ priority 缓存逐出的优先级
        * */
        public CacheItem CreateSQLCacheList<T>(string key, string cmdText, DateTimeOffset offset, out CacheItemPolicy policy, CacheItemPriority priority = CacheItemPriority.Default)
            where T : class, new()
        {
            policy = new CacheItemPolicy();
            List<T> resultList = SelectData<T>(cmdText, out dency);
            monitor = new SqlChangeMonitor(dency);
            policy.AbsoluteExpiration = offset;
            policy.ChangeMonitors.Add(monitor);
            cacheItem = new CacheItem(key);
            cacheItem.Value = resultList;
            return cacheItem;
        }

        /**
         * @ 读取数据列表
         * @ T 数据类型
         * @ cmdText sql语句
         * @ dency 依赖对象
         * */
        private List<T> SelectData<T>(string cmdText, out SqlDependency dency) where T : class, new()
        {
            if (cmdText.IsNullOrEmpty())
            {
                throw new ArgumentNullException("cmdText不能为空");
            }

            List<T> resultList = new List<T>();
            SqlCommand sqlCmd = GetSqlCmd(cmdText, out dency);
            using (SqlDataReader reader = sqlCmd.ExecuteReader())
            {
                #region 读取数据

                if (reader.Read())
                {
                    string[] fields = new string[reader.FieldCount];
                    for (int i = 0; i < fields.Length; i++)
                    {
                        fields[i] = reader.GetName(i);
                    }
                    do
                    {
                        T t = new T();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            PropertyInfo[] pros = t.GetType().GetProperties();
                            PropertyInfo perInfo = pros.FirstOrDefault(f => f.Name == fields[i]);
                            object v_obj = reader[fields[i]];
                            if (perInfo != null && v_obj != DBNull.Value)
                            {
                                perInfo.SetValue(t, v_obj, null);
                            }
                        }
                        resultList.Add(t);
                    } while (reader.Read());
                }
                #endregion
            }

            return resultList;
        }

        /**
         * @ 读取首列首行数据
         * @ T 数据类型
         * @ cmdText sql语句
         * @ dency 依赖对象
         * */
        private T SelectScalar<T>(string cmdText, out SqlDependency dency)
        {
            T result;
            using (SqlCommand sqlCmd = GetSqlCmd(cmdText, out dency))
            {
                result = (T)sqlCmd.ExecuteScalar();
            }

            return result;
        }

        /**
         * @ 创建 SqlCommand 对象
         * @
         **/
        protected SqlCommand GetSqlCmd(string cmdText, out SqlDependency dency)
        {
            SqlConnection conn = new SqlConnection(dbConnectionString);
            SqlCommand sqlCmd = new SqlCommand(cmdText, conn);
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            dency = new SqlDependency(sqlCmd);
            dency.OnChange += delegate(object sender, SqlNotificationEventArgs e)
            {
                if (e.Type == SqlNotificationType.Change && SourceChange != null)
                {
                    SourceChange(sender, e);
                }
                else if (e.Type == SqlNotificationType.Subscribe)
                {
                    throw new ArgumentException("请检查sql查询语句是否包含架构信息，并确保查询字段不使用*号");
                }

            };

            return sqlCmd;
        }

        /*
         * @ 接口实现
         * */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /**
         * @ 清理本机资源
         * */
        private void Dispose(bool disposing)
        {
            if (!this.m_disposeing && disposing)
            {
                if (monitor != null)
                    monitor.Dispose();

                m_disposeing = true;
            }
        }
    }
}
