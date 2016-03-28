using Perfor.Lib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.Security;

namespace Perfor.Lib.Web.Routes
{
    public class WebRoute : RouteBase
    {
        #region identity
        private Type routeType = null;
        private string typeName = string.Empty;

        /// <summary>
        ///  路由类型
        /// </summary>
        /// <param name="type"></param>
        public WebRoute(Type type)
        {
            routeType = type;
        }

        /// <summary>
        ///  路由类型名称
        /// </summary>
        /// <param name="typeName"></param>
        public WebRoute(string typeName)
        {
            if (typeName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("typeName");
            }
            this.typeName = typeName;
        }
        #endregion

        /// <summary>
        ///  获取路由数据
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            RouteData data = new RouteData(this, new WebRouteHandler());
            string url = httpContext.Request.RawUrl;
            // 获取 controller 和 action
            string[] paths = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (paths == null || paths.Length < 2)
            {
                // 转到默认页
                return null;
            }
            List<KeyValuePair<string, object>> args = null;
            // 获取调用参数
            if (httpContext.Request.HttpMethod.ToUpper() == "POST" || url.IndexOf('?') > 0)
            {
                WebParams wp = new WebParams();
                args = new List<KeyValuePair<string, object>>(wp.Count);
                foreach (string k in wp.Keys)
                {
                    KeyValuePair<string, object> kv = new KeyValuePair<string, object>(k, wp[k]);
                    args.Add(kv);
                }
            }
            else if (paths.Length > 2)
            {
                // 如果参数是以  / 线进行分隔进行传递
                int len = paths.Length;
                args = new List<KeyValuePair<string, object>>(len);
                int index = 1;
                for (int i = 2; i < len; i++)
                {
                    KeyValuePair<string, object> kv = new KeyValuePair<string, object>(index.ToString(), paths[i]);
                    args.Add(kv);
                    index++;
                }
            }

            // 创建调用路径
            data.Values.Add("controller", paths[0]);
            data.Values.Add("action", paths[1]);
            data.Values.Add("args", args);

            return data;
        }


        /// <summary>
        ///  返回显示的虚拟路径
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            // 返回 null ，用户输入什么就是什么
            return null;
        }
    }
}
