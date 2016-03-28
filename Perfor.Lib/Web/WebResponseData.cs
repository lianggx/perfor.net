using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Perfor.Lib.Web
{
    /**
     * @ 响应数据
     * */
    public class WebResponseData
    {
        /// <summary>
        ///  连接状态码
        /// </summary>
        public int Status { get; set; }
        /**
         * @ Response url
         * */
        public Uri Url { get; set; }
        /**
         * @ Response Content
         * */
        public string Html { get; set; }
        /**
         * @ Response headers Set-Cookie
         * */
        public string SetCookieString { get; set; }
        /**
         * @ Response Cookies
         * */
        public CookieCollection Cookies { get; set; }
        /**
         * @ Response headers
         * */
        public Dictionary<string, string> Headers { get; set; }
    }
}
