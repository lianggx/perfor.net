using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Danny.Lib.Web
{
    /**
     * @ 响应数据
     * */
    public class DLResponseData
    {
        public Uri Url { get; set; }
        public string Html { get; set; }
        public string SetCookieString { get; set; }
        public CookieCollection Cookies { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}
