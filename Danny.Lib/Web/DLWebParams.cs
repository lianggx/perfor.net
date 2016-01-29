using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;

using Danny.Lib.Extension;
using System.Runtime.Serialization;

namespace Danny.Lib.Web
{
    /**
     * @ 创建 Web参数包装对象
     * */
    [Serializable]
    public class DLWebParams : Dictionary<string, object>, IDisposable
    {
        #region Identity
        /***
         * @ 析构函数，清理托管资源
         * */
        ~DLWebParams()
        {
            Dispose(false);
        }

        /**
         * @ 序列化参数构造函数
         * */
        protected DLWebParams(SerializationInfo info, StreamingContext contex)
            : base(info, contex) { }

        /**
         * @ 默认构造函数
         * */
        public DLWebParams()
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
        private void AddParams(NameValueCollection values)
        {
            if (values == null || values.Count == 0)
                return;

            foreach (var key in values.AllKeys)
            {
                string k = key.ToLower().Trim();
                if (this.ContainsKey(k))
                    continue;

                this.Add(k, values[k]);
            }
        }

        /**
         * @ 覆盖父类的索引
         * */
        public new string this[string key]
        {
            get
            {
                if (this.ContainsKey(key))
                    return base[key].ToString();
                return string.Empty;
            }
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
