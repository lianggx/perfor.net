using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Reflection;

namespace Perfor.Lib.Cacheing
{
    /**
     * @ 自定义全集缓存管理类
     * */
    public partial class GLCacheExpiration
    {
        /**
         * @ 默认缓存对象
         * */
        public static ObjectCache Cache
        {
            get
            {
                return MemoryCache.Default;
            }
        }

        /**
         * @ 获取缓存值
         * @ key 名称
         * */
        public static Object GetValue(string key)
        {
            return Cache.Get(key);
        }

        /**
         * @ 设置缓存值
         * @ key 名称
         * @ value 要缓存的值
         * */
        public static void SetValue(string key, object value)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.NotRemovable;
            SetValue(key, value, policy);

        }

        /**
         * @ 设置缓存值
         * @ key 名称
         * @ value 要缓存的值
         * @ policy 缓存策略
         * */
        public static void SetValue(string key, Object value, CacheItemPolicy policy)
        {
            Cache.Set(key, value, policy);
        }

        /**
         * @ 创建单个对象的SQL类型的缓存策略，该方法适用于单个字段类型对象
         * @ T 要创建的缓存对象类型，仅限单个类型
         * @ key 缓存的键
         * @ dbConnectionStr 数据库连接字符串
         * @ cmdText sql语句，必须包含架构信息，并且不能使用“*”号代替要查询的字段
         * @ offset 缓存过期时间
         * @ sourceChange 数据源更改通知事件
         * @ priority 缓存逐出的优先级
         * */
        public static CacheItem CreateSQLCache(string key, string dbConnectionStr, string cmdText, DateTimeOffset offset, EventHandler sourceChange, CacheItemPriority priority = CacheItemPriority.Default)
        {
            SqlCacheExpiration sqlCache = GetSqlCache(key, dbConnectionStr, cmdText, offset, sourceChange);
            CacheItem cacheItem = null;
            CacheItemPolicy policy = null;
            cacheItem = sqlCache.CreateSQLCacheScalar(key, cmdText, offset, out policy, priority);

            Cache.Set(cacheItem, policy);
            return cacheItem;
        }

        /**
         * @ 创建SQL类型的缓存策略
         * @ T 要创建的缓存对象类型，仅限单个类型
         * @ key 缓存的键
         * @ dbConnectionStr 数据库连接字符串
         * @ cmdText sql语句，必须包含架构信息，并且不能使用“*”号代替要查询的字段
         * @ offset 缓存过期时间
         * @ sourceChange 数据源更改通知事件
         * @ cacheType 缓存类型
         * @ priority 缓存逐出的优先级
         * */
        public static CacheItem CreateSQLCache<T>(string key, string dbConnectionStr, string cmdText, DateTimeOffset offset, EventHandler sourceChange, SqlCacheOption cacheType, CacheItemPriority priority = CacheItemPriority.Default) where T : class, new()
        {
            SqlCacheExpiration sqlCache = GetSqlCache(key, dbConnectionStr, cmdText, offset, sourceChange);
            CacheItem cacheItem = null;
            CacheItemPolicy policy = null;
            switch (cacheType)
            {
                case SqlCacheOption.Single:
                    cacheItem = sqlCache.CreateSQLCacheSingle<T>(key, cmdText, offset, out policy, priority);
                    break;
                case SqlCacheOption.List:
                    cacheItem = sqlCache.CreateSQLCacheList<T>(key, cmdText, offset, out policy, priority);
                    break;
            }
            Cache.Set(cacheItem, policy);
            return cacheItem;
        }
        /**
         * @ 创建 SqlCacheExpiration 对象
         * @ T 要创建的缓存对象类型，仅限单个类型
         * @ key 缓存的键
         * @ dbConnectionStr 数据库连接字符串
         * @ cmdText sql语句，必须包含架构信息，并且不能使用“*”号代替要查询的字段
         * @ offset 缓存过期时间
         * @ sourceChange 数据源更改通知事件
         * */

        private static SqlCacheExpiration GetSqlCache(string key, string dbConnectionStr, string cmdText, DateTimeOffset offset, EventHandler sourceChange)
        {
            if (string.IsNullOrEmpty(dbConnectionStr))
            {
                throw new ArgumentNullException("dbConnectionStr不能为空");
            }
            if (string.IsNullOrEmpty(cmdText))
            {
                throw new ArgumentNullException("cmdText不能为空");
            }
            SqlCacheExpiration sqlCache = new SqlCacheExpiration(dbConnectionStr);
            sqlCache.SourceChange += delegate (object sender, EventArgs e)
            {
                if (sourceChange != null)
                {
                    sourceChange(sender, e);
                }
            };

            return sqlCache;
        }

        /**
         * @ 创建文件系统类型的缓存
         * @ key 缓存的键
         * @ filePath 文件路径
         * @ offset 缓存有效时间
         * @ priority 逐出缓存优先级策略
         * */
        public static string CreateFileCache(string key, string filePath, DateTimeOffset offset, CacheItemPriority priority = CacheItemPriority.Default)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key不能为空");
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath不能为空");
            }
            bool isNotExists = File.Exists(filePath);
            if (isNotExists)
            {
                throw new ArgumentNullException(string.Format("指定文件：{0}不存在！", filePath));
            }
            object values = GLCacheExpiration.Cache.Get(key);
            if (values == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                List<string> fp = new List<string>() { filePath };

                HostFileChangeMonitor monitor = new HostFileChangeMonitor(fp);
                policy.AbsoluteExpiration = offset;
                policy.Priority = priority;
                policy.ChangeMonitors.Add(monitor);
                values = File.ReadAllText(filePath);
                CacheItem chitem = new CacheItem(key, values);
                Cache.AddOrGetExisting(chitem, policy);
            }
            return values.ToString();
        }

        /**
         * @ 创建目录类型的缓存，主要用于监控文件数量
         * @ key 缓存的键
         * @ path 路径，请确保路径存在
         * @ offset 缓存有效时间
         * @ searchPattern 搜索匹配关键字，默认：*
         * @ searchOption 文件搜索选项，默认TopDirectoryOnly
         * @ direcotryOption 目录缓存选项，默认FileAmountOnly
         * @ priority 逐出缓存优先级策略
         * */
        public static string CreateDirectoryCache(string key, string path, DateTimeOffset offset, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly, DirectoryOption direcotryOption = DirectoryOption.FileAmountOnly, CacheItemPriority priority = CacheItemPriority.Default)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key不能为空");
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path不能为空");
            }
            bool isNotExists = Directory.Exists(path);
            if (isNotExists)
            {
                throw new ArgumentNullException(string.Format("指定目录：{0}不存在！", path));
            }

            object values = GLCacheExpiration.Cache.Get(key);
            if (values == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                List<string> fp = new List<string>() { path };

                HostFileChangeMonitor monitor = new HostFileChangeMonitor(fp);
                policy.AbsoluteExpiration = offset;
                policy.Priority = priority;
                policy.ChangeMonitors.Add(monitor);
                string[] files = Directory.GetFiles(path, searchPattern, searchOption);

                if (direcotryOption == DirectoryOption.FileAmountOnly)
                {
                    values = files == null ? 0 : files.Length;
                }
                else if (direcotryOption == DirectoryOption.FileNameArray)
                {
                    values = files;
                }
                CacheItem chitem = new CacheItem(key, values);
                Cache.AddOrGetExisting(chitem, policy);
            }

            return values.ToString();
        }
    }
}
