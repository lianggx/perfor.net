using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace Perfor.Lib.Web.Routes
{
    public class WebRouteHandler : IRouteHandler, IHttpHandler, IDisposable
    {
        #region Identity
        private RouteData routeData = null;
        private bool m_disposing = false;
        private HttpContext context = null;
        /// <summary>
        ///  析构函数，调用Disposable实现
        /// </summary>
        ~WebRouteHandler()
        {
            Dispose(true);
        }
        #endregion

        #region IHttpHandler
        /// <summary>
        ///  可以为HttpHandler使用
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///  处理传入的请求
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            string controller = routeData.Values["controller"].ToString();
            string action = routeData.Values["action"].ToString();
            List<KeyValuePair<string, object>> args = routeData.Values["args"] as List<KeyValuePair<string, object>>;

            string text = string.Empty;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!m_disposing && disposing)
            {
                if (context != null)
                {
                    context.Response.Flush();
                    context.Response.End();
                }
            }
            m_disposing = true;
        }
        #endregion

        #region IRouteHandler
        /// <summary>
        ///  提供处理HttpHandler类型的对象
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            routeData = requestContext.RouteData;
            return this;
        }
        #endregion
    }
}
