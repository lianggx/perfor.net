using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;

using Perfor.Lib.Extension;
using System.Runtime.Serialization;

namespace Perfor.Lib.Web
{
    /**
     * @ 创建 Web参数包装对象
     * */
    [Serializable]
    public class WebParams : Dictionary<string, object>, IDisposable
    {
        #region Identity
        /**
         * @ 析构函数，清理托管资源
         * */
        ~WebParams()
        {
            Dispose(false);
        }

        /**
         * @ 序列化参数构造函数
         * */
        protected WebParams(SerializationInfo info, StreamingContext contex)
            : base(info, contex)
        {
            InitParams();
        }

        /**
         * @ 默认构造函数
         * */
        public WebParams()
        {
            InitParams();
        }
        #endregion

        /**
         * @ 初始化，将参数加入集合中
         * */
        private void InitParams()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
                return;

            // URL参数
            AddParams(context.Request.QueryString);
            // 表单提交参数
            AddParams(context.Request.Form);
        }

        /**
         * @ 添加参数
         * */
        private void AddParams(NameValueCollection collections)
        {
            if (collections == null || collections.Count == 0 || collections.AllKeys.IsNullOrEmpty())
                return;

            foreach (var key in collections.AllKeys)
            {
                if (key == null) continue;
                string k = key.ToLower().Trim();
                if (this.ContainsKey(k))
                    continue;

                string valueStr = HttpContext.Current.Server.UrlDecode(collections[k]);
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
         * @ isHtmlEncode 是否进行 HtmlEncode
         * */
        public string GetValue(string key, bool isHtmlEncode = true)
        {
            if (!this.ContainsKey(key))
                return null;

            object value = string.Empty;
            this.TryGetValue(key, out value);
            string result = HttpUtility.UrlDecode(value.ToString());
            if (isHtmlEncode)
                result = HttpUtility.HtmlEncode(result);

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
