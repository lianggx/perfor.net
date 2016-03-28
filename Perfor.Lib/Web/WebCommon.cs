using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Perfor.Lib.Extension;

namespace Perfor.Lib.Web
{
    /**
     * @ Web公共函数处理
     * */
    public partial class WebCommon
    {
        #region XSS.To

        //private static string CapText(Match match)
        //{
        //    string replace = match.ToString().Replace(":\"", "").Replace("\"", "");
        //   需引用 HtmlSanitizationLibrary.dll v4.0.0.0
        //    string salfeReplace = Microsoft.Security.Application.Sanitizer.GetSafeHtmlFragment(replace);
        //    string fixJson = salfeReplace.Replace("\"", "\\\"");
        //    return string.Format(":\"{0}\"", fixJson);
        //}

        //public static string FilterXss(this string value)
        //{
        //    if (value.IsNullOrEmpty())
        //        return "";

        //    Regex regex = new Regex(":\"(.*?)\"");
        //    return regex.Replace(value, new MatchEvaluator(CapText));
        //}
        #endregion

        /*
         * @ 过滤 HTML 
         * */
        public static string HtmlClear(string html)
        {
            Regex regex1 = new Regex(@"<script[\s\S]+</script *>", RegexOptions.IgnoreCase);
            Regex regex2 = new Regex(@" href *= *[\s\S]*script *:", RegexOptions.IgnoreCase);
            Regex regex3 = new Regex(@" no[\s\S]*=", RegexOptions.IgnoreCase);
            Regex regex4 = new Regex(@"<iframe[\s\S]+</iframe *>", RegexOptions.IgnoreCase);
            Regex regex5 = new Regex(@"<frameset[\s\S]+</frameset *>", RegexOptions.IgnoreCase);
            Regex regex6 = new Regex(@"\<img[^\>]+\>", RegexOptions.IgnoreCase);
            Regex regex7 = new Regex(@"</p>", RegexOptions.IgnoreCase);
            Regex regex8 = new Regex(@"<p>", RegexOptions.IgnoreCase);
            Regex regex9 = new Regex(@"<[^>]*>", RegexOptions.IgnoreCase);
            html = regex1.Replace(html, ""); //过滤<script></script>标记 
            html = regex2.Replace(html, ""); //过滤href=javascript: (<A>) 属性 
            html = regex3.Replace(html, " _disibledevent="); //过滤其它控件的on...事件 
            html = regex4.Replace(html, ""); //过滤iframe 
            html = regex5.Replace(html, ""); //过滤frameset 
            html = regex6.Replace(html, ""); //过滤frameset 
            html = regex7.Replace(html, ""); //过滤frameset 
            html = regex8.Replace(html, ""); //过滤frameset 
            html = regex9.Replace(html, "");
            html = html.Replace(" ", "");
            html = html.Replace("</strong>", "");
            html = html.Replace("<strong>", "");
            return html;
        }

        /**
        * @ 添加 cookie 到 HttpResponse 中，调用该方法默认的 cookie 过期时间为 20 分钟
        * @ name cookie的名称
        * @ value 值
        * */
        public static HttpCookie AddCookie(string name, string value)
        {
            return AddCookie(name, value, DateTime.Now.AddMinutes(20));
        }

        /**
         * @ 添加 cookie 到 HttpResponse 中
         * @ name cookie的名称
         * @ value 值
         * @ expires 过期时间
         * @ domain 作用域，默认当前域
         * */
        public static HttpCookie AddCookie(string name, string value, DateTime expires, string domain = null)
        {
            if (name.IsNullOrEmpty())
                throw new NullReferenceException("参数 name 不能为空");

            HttpResponse response = HttpContext.Current.Response;
            HttpCookie cookie = response.Cookies[name];
            if (cookie == null)
            {
                cookie = new HttpCookie(name, value);
                response.Cookies.Add(cookie);
            }

            cookie.Expires = expires;
            if (domain.IsNotNullOrEmpty())
                cookie.Domain = domain;

            response.SetCookie(cookie);

            return cookie;
        }

        /**
         * @ 获取 cookie 的值
         * @ name cookie名称
         * */
        public static string GetCookie(string name)
        {
            if (name.IsNullOrEmpty())
                throw new NullReferenceException("参数 name 不能为空");
            HttpResponse response = HttpContext.Current.Response;
            HttpCookie cookie = response.Cookies[name];
            if (cookie == null)
                return null;

            return cookie.Value;
        }
    }
}
