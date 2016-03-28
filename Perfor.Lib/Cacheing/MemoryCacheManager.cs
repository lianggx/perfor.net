using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching.Hosting;
using System.Text;

namespace Perfor.Lib.Cacheing
{
    /**
     * @ 需要 .NET 4.6支持，请勿使用
     * */
    public class MemoryCacheManager : IMemoryCacheManager, IServiceProvider
    {
        public void ReleaseCache(System.Runtime.Caching.MemoryCache cache)
        {
            if (cache != null)
                cache.Dispose();
        }

        public void UpdateCacheSize(long size, System.Runtime.Caching.MemoryCache cache)
        {
            Console.WriteLine("{0}的缓存消耗：{1}", cache.Name, size);
        }

        public object GetService(Type serviceType)
        {
            return this;
        }
    }
}
