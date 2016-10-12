using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

using Perfor.Lib.Extension;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Perfor.Lib.Web
{
    /**
     * @ 创建 Web参数包装对象
     * */
    public class WebParams : Dictionary<string, object>, IDisposable
    {
        #region Identity
        private HttpRequest request = null;
        /**
         * @ 析构函数，清理托管资源
         * */
        ~WebParams()
        {
            Dispose(false);
        }


        /**
         * @ 默认构造函数
         * */
        public WebParams(HttpRequest request)
        {
            this.request = request;
            InitParams();
        }
        #endregion

        /**
         * @ 初始化，将参数加入集合中
         * */
        private void InitParams()
        {
            if (request == null)
                return;

            // URL参数
            AddParams(request.Query);
            // 表单提交参数
            AddParams(request.Form);
        }

        /**
         * @ 添加参数
         * */
        private void AddParams(IQueryCollection collections)
        {
            if (collections == null || collections.Count == 0 || collections.Keys.IsNullOrEmpty())
                return;

            foreach (var key in collections.Keys)
            {
                if (key == null) continue;
                string k = key.ToLower().Trim();
                if (this.ContainsKey(k))
                    continue;

                string valueStr = WebUtility.UrlDecode(collections[k]);
                this.Add(k, valueStr);
            }
        }

        /**
         * @ 添加参数
         * */
        private void AddParams(IFormCollection collections)
        {
            if (collections == null || collections.Count == 0 || collections.Keys.IsNullOrEmpty())
                return;

            foreach (var key in collections.Keys)
            {
                if (key == null) continue;
                string k = key.ToLower().Trim();
                if (this.ContainsKey(k))
                    continue;

                string valueStr = WebUtility.UrlDecode(collections[k]);
                this.Add(k, valueStr);
            }
        }

        /**
         * @ 覆盖父类的索引
         * */
        public new string this[string key]
        {
            get { return GetValue(key); }
        }

        /**
         * @ 获取Action的参数
         * @ key 参数名称
         * */
        public string GetValue(string key)
        {
            if (!this.ContainsKey(key))
                return null;

            object value = string.Empty;
            this.TryGetValue(key, out value);
            string result = WebUtility.UrlDecode(value.ToString());

            return result;
        }

        /**
         * @ 接口实现，清理资源
         * */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /**
         * @ 清理托管资源
         * */
        private void Dispose(bool disposing)
        {
            if (disposing)
                return;

            this.Clear();
            disposing = true;
        }
    }
}
