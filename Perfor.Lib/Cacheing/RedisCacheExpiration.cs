using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using Perfor.Lib.Extension;
using ServiceStack.CacheAccess;

namespace Perfor.Lib.Cacheing
{
    /**
     * @ 分布式缓存代理
     * */
    public partial class RedisCacheExpiration : ObjectCache
    {
        #region Identity
        private static PooledRedisClientManager redisPool = null;
        /**
         * @ 构造函数
         * @ writeHost 写入数据主机列表
         * @ readHost 读取数据主机列表
         * @ config 连接池的配置
         * @ initalDb 默认连接的数据库，如果config没有指定defaultDb，initalDb 默认为 0，有可能连接不上
         * @ poolSizeMultiplier pool大小的跨度 ，计算公式：db * multiplier，如10个数据库，mutiplier为2，则最终pool的大小为：10*2
         * @ poolTimeOutSeconds pool超时时间 ，秒
         * */
        public RedisCacheExpiration(IEnumerable<string> writeHost, IEnumerable<string> readHost, RedisClientManagerConfig config = null, int initalDb = 0, int? poolSizeMultiplier = 10, int? poolTimeOutSeconds = 2)
        {
            if (writeHost.IsNullOrEmpty() || readHost.IsNullOrEmpty())
                throw new ArgumentNullException("必须指定参数：writeHost和readHost的值");
            if (config == null)
            {
                config = new RedisClientManagerConfig();
            }
            redisPool = new PooledRedisClientManager(writeHost, readHost, config, initalDb, poolSizeMultiplier, poolTimeOutSeconds);
            if (!config.AutoStart)
            {
                redisPool.Start();
            }
        }
        #endregion

        #region override parent
        /**
         * @ 实现父类的方法
         * @ key 缓存键，存在则覆盖值
         * @ value 缓存值
         * @ policy 缓存策略，仅过期时间有效
         * @ regionName 缓存中的一个可用来添加缓存项的命名区域（如果实现了区域）
         * */
        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            CacheItem cheItem = new CacheItem(key, value, regionName);
            return AddOrGetExisting(cheItem, policy);
        }

        /**
         * @ 实现父类的方法
         * @ value 缓存项
         * @ policy 缓存策略，仅过期时间有效
         * */
        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            Set(value, policy);
            return value;
        }
        /**
         * @ 实现父类的方法
         * @ key 缓存键，存在则覆盖值
         * @ value 缓存值
         * @ absoluteExpiration 缓存过期时间
         * @ regionName 缓存中的一个可用来添加缓存项的命名区域（如果实现了区域）
         * */
        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            CacheItem cheItem = new CacheItem(key, value, regionName);
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = absoluteExpiration;

            return AddOrGetExisting(cheItem, policy);
        }

        /**
         * @ 检查 key 是否存在于缓存中
         * */
        public override bool Contains(string key, string regionName = null)
        {
            bool succeed = false;
            using (IRedisClient client = redisPool.GetReadOnlyClient())
            {
                succeed = client.ContainsKey(key);
            }
            return succeed;
        }

        /**
         * @ 缓存监视器，未实现
         * */
        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException("分布式缓存暂时无法支持监视");
        }

        /**
         * @ 指示缓存提供的一组功能
         * */
        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get
            {
                return DefaultCacheCapabilities.OutOfProcessProvider;
            }
        }

        /**
         * @ 获取缓存的值
         * @ key 缓存的 key
         * @ regionName 缓存中的一个可用来添加缓存项的命名区域（如果实现了区域）
         * */
        public override object Get(string key, string regionName = null)
        {
            object val = null;
            using (IRedisClient client = redisPool.GetReadOnlyClient())
            {
                val = client.Get<object>(key);
            }
            return val;
        }

        /**
         * @ 获取缓存值，并包装成 CacheItem，缓存值存在于 CacheItem.Value成员中
         * @ key 缓存的 key
         * @ regionName 缓存中的一个可用来添加缓存项的命名区域（如果实现了区域）
         * */
        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            CacheItem cacheItem = new CacheItem(key);
            cacheItem.Value = Get(key, regionName);

            return cacheItem;
        }

        /**
         * @ 获取已缓存项的数量
         * */
        public override long GetCount(string regionName = null)
        {
            int retryCount = 0;
            using (IRedisClient client = redisPool.GetReadOnlyClient())
            {
                retryCount = client.RetryCount;
            }
            return retryCount;
        }

        /**
         * @ 分布式暂时不提供迭代缓存功能
         * */
        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException("分布式暂时不提供迭代缓存功能");
        }

        /**
         * @ 获取一组缓存的值
         * @ keys 一组缓存 key
         * @ regionName 缓存中的一个可用来添加缓存项的命名区域（如果实现了区域）
         * */
        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            if (keys.IsNullOrEmpty())
                return null;
            IDictionary<string, object> dict = new Dictionary<string, object>();
            using (IRedisClient client = redisPool.GetReadOnlyClient())
            {
                foreach (var key in keys)
                {
                    object val = client.Get<object>(key);
                    dict.Add(key, val);
                }
            }
            return dict;
        }

        /**
         * @ 缓存提供者的名称
         * */
        public override string Name
        {
            get { return this.ToString(); }
        }

        /**
         * @ 移除指定的缓存项
         * @ regionName 缓存中的一个可用来添加缓存项的命名区域（如果实现了区域）
         * */
        public override object Remove(string key, string regionName = null)
        {
            object val = null;
            using (IRedisClient client = redisPool.GetClient())
            {
                val = client.Remove(key);
            }
            return val;
        }

        /**
         * @ 实现父类的方法
         * @ key 缓存键，存在则覆盖值
         * @ value 缓存值
         * @ policy 缓存策略，仅过期时间有效
         * @ regionName 缓存中的一个可用来添加缓存项的命名区域（如果实现了区域）
         * */
        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            CacheItem item = new CacheItem(key, value, regionName);
            Set(item, policy);
        }

        /**
       * @ 实现父类的方法
       * @ value 缓存项
       * @ policy 缓存策略，仅过期时间有效
       * */
        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            using (IRedisClient client = redisPool.GetClient())
            {
                if (policy == null || policy.AbsoluteExpiration.Equals(null))
                    client.Set(item.Key, item.Value);
                else
                    client.Set(item.Key, item.Value, policy.AbsoluteExpiration.DateTime);
            }
        }

        /**
        * @ 实现父类的方法
        * @ key 缓存键，存在则覆盖值
        * @ value 缓存值
        * @ absoluteExpiration 缓存过期时间
        * @ regionName 缓存中的一个可用来添加缓存项的命名区域（如果实现了区域）
        * */
        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = absoluteExpiration;
            Set(key, value, policy, regionName);
        }

        /**
         * @ 索引，或者或者设置指定缓存 key 的 值
         * */
        public override object this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value, DateTimeOffset.MaxValue);
            }
        }
        #endregion

        #region Method self
        /// <summary>
        ///  添加指定的值到有序列表中
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="value"></param>
        public void AddItemToList(string listId, string value)
        {
            using (IRedisClient client = redisPool.GetClient())
            {
                client.AddItemToList(listId, value);
            }
        }

        /// <summary>
        ///  获取有序列表的值的数量
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        public long GetListCount(string listId)
        {
            long count = 0;
            using (IRedisClient client = redisPool.GetReadOnlyClient())
            {
                count = client.GetListCount(listId);
            }
            return count;
        }

        /// <summary>
        ///  设置指定key的过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireIn"></param>
        public bool ExpireEntryIn(string key, TimeSpan expireIn)
        {
            bool succeed = false;
            using (IRedisClient client = redisPool.GetClient())
            {
                succeed = client.ExpireEntryIn(key, expireIn);
            }

            return succeed;
        }
        #endregion

        #region Properties        

        /**
         * @ 默认连接池的实例
         * */
        public PooledRedisClientManager ReadPool
        {
            get
            {
                return redisPool;
            }
        }
        #endregion
    }
}
