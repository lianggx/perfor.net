using Perfor.Lib.Cacheing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Perfor.Forms.Test.example
{
    /// <summary>
    ///  使用示例
    /// </summary>
    public partial class ExampleCacheing
    {
        private ObjectCache memoryCache = GLCacheExpiration.Cache;
        private string dbConnectionString = "Server=.;Database=testdb;User ID=sa;Password=123456;Trusted_Connection=True";

        ObjectCache cache = MemoryCache.Default;
        public void MainTest()
        {
            // SQLTestSingle();
            // SQLTestList();
            SQLTestScalar();
        }


        #region SQLTestScalar
        /// <summary>
        ///  数据库缓存测试
        /// </summary>
        private void SQLTestScalar()
        {
            bool cusGender = false;
            string ckey = "cus";
            if (memoryCache[ckey] == null)
            {
                CacheItem item = LoadSQLDataScalar();
                cusGender = Convert.ToBoolean(item.Value);
            }
            else
            {
                Console.WriteLine("From Cache");
                cusGender = (bool)memoryCache.Get(ckey);
            }
            Console.WriteLine("gender:{0}", cusGender);
        }

        public CacheItem LoadSQLDataScalar()
        {
            Console.WriteLine("From SQL");
            DateTimeOffset dtOffset = new DateTimeOffset(DateTime.Now.AddHours(3));
            string sqlCmd = "SELECT Gender FROM dbo.Customers  WHERE ID='00000ddb92044fad8be0913b68697318'";
            CacheItem cacheItem = GLCacheExpiration.CreateSQLCache("cus", dbConnectionString, sqlCmd, dtOffset, onScalarSourceChange);

            return cacheItem;
        }

        private void onScalarSourceChange(object sender, EventArgs e)
        {
            LoadSQLDataScalar();
            Console.WriteLine("自动更新数据源");
        }
        #endregion

        #region SQLTestList
        /// <summary>
        ///  数据库缓存测试
        /// </summary>
        private void SQLTestList()
        {
            List<Customers> cus = null;
            string ckey = "cus";
            if (memoryCache[ckey] == null)
            {
                CacheItem item = LoadSQLDataList();
                cus = (List<Customers>)item.Value;
            }
            else
            {
                Console.WriteLine("From Cache");
                cus = (List<Customers>)memoryCache.Get(ckey);
            }
            Console.WriteLine("Count:{0}", cus.Count);
        }

        public CacheItem LoadSQLDataList()
        {
            Console.WriteLine("From SQL");
            DateTimeOffset dtOffset = new DateTimeOffset(DateTime.Now.AddHours(3));
            string sqlCmd = "SELECT ID,Name,Gender,Phone FROM dbo.Customer";
            CacheItem cacheItem = GLCacheExpiration.CreateSQLCache<Customers>("cus", dbConnectionString, sqlCmd, dtOffset, onListSourceChange, SqlCacheOption.List);

            return cacheItem;
        }

        private void onListSourceChange(object sender, EventArgs e)
        {
            LoadSQLDataList();
            Console.WriteLine("自动更新数据源");
        }
        #endregion

        #region SQLTestSingle
        /// <summary>
        ///  数据库缓存测试
        /// </summary>
        private void SQLTestSingle()
        {
            Customers cus = null;
            string ckey = "cus";
            if (memoryCache[ckey] == null)
            {
                CacheItem item = LoadSQLDataSingle();
                cus = (Customers)item.Value;
            }
            else
            {
                Console.WriteLine("From Cache");
                cus = (Customers)memoryCache.Get(ckey);
            }
            Console.WriteLine("Name:{0}；Phone:{1}", cus.UserName, cus.Phone);
        }

        public CacheItem LoadSQLDataSingle()
        {
            Console.WriteLine("From SQL");
            DateTimeOffset dtOffset = new DateTimeOffset(DateTime.Now.AddHours(3));
            string sqlCmd = "SELECT ID,UserName,Gender,Phone FROM dbo.Customers WHERE ID='00000ddb92044fad8be0913b68697318'";
            CacheItem cacheItem = GLCacheExpiration.CreateSQLCache<Customers>("cus", dbConnectionString, sqlCmd, dtOffset, onSingleSourceChange, SqlCacheOption.Single);

            return cacheItem;
        }

        private void onSingleSourceChange(object sender, EventArgs e)
        {
            LoadSQLDataSingle();
            Console.WriteLine("自动更新数据源");
        }
        #endregion



        /// <summary>
        ///  缓存文件测试
        /// </summary>
        private void FileTest(string path)
        {
            DateTimeOffset dtOffset = new DateTimeOffset(DateTime.Now.AddHours(3));
            string filePath = path;
            string key = "file";
            GLCacheExpiration.CreateFileCache(key, filePath, dtOffset);
        }
    }


}
