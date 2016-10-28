using System;
using System.IO;
using System.Net;
using System.Text;
using dywebsdk.Common;
using System.Collections.Specialized;
using Perfor.Lib.Models;
using System.Net.Http;
using Perfor.Lib.Common;
using Microsoft.AspNetCore.Http;

namespace Perfor.Lib.Web
{
    public class WebHelper
    {
        /// <summary>
        ///  创建 HttpWebRequest 对象
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpWebRequest Create(string url)
        {
            return (HttpWebRequest)WebRequest.Create(url);
        }

        /// <summary>
        ///  使用 Get 方式调用远程服务
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFrom(string url)
        {
            HttpWebRequest request = Create(url);
            using (WebResponse response = (HttpWebResponse)request.GetResponseAsync().Result)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        ///  使用 Post 方式调用远程服务
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string PostTo(string url, string data)
        {
            Uri uri = new Uri(url);
            if (!string.IsNullOrEmpty(uri.Query))
            {
                data = string.Format("{0}&{1}", uri.Query.Substring(1), data);
            }
            url = Utilities.UrlSubParser(url);
            HttpWebRequest request = Create(url);
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            request.Method = "POST";
            using (Stream stream = request.GetRequestStreamAsync().Result)
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
            using (WebResponse response = request.GetResponseAsync().Result)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        ///  调用远程服务
        /// </summary>
        /// <param name="url">待调用的url</param>
        /// <param name="method">调用方式</param>
        /// <param name="postData">提交的参数</param>
        /// <returns></returns>
        public static CallResult InvokeHttp(string url, HttpMethod method, params object[] postData)
        {
            if (postData != null && postData.Length > 0 && postData.Length % 2 != 0)
                throw new ArgumentException("postData参数必须成对出现");
            NameValueCollection param = new NameValueCollection();
            for (int i = 0; i < postData.Length; i++)
            {
                param.Add(postData[i].ToString(), postData[i + 1].ToString());
                i++;
            }
            return InvokeHttp(url, method, param);
        }

        /// <summary>
        ///  调用远程服务
        /// </summary>
        /// <param name="url">待调用的url</param>
        /// <param name="method">调用方式</param>
        /// <param name="postData">提交的参数</param>
        /// <returns></returns>
        public static CallResult InvokeHttp(string url, HttpMethod method, NameValueCollection postData)
        {
            string dataStr = string.Empty;
            if (postData != null && postData.Count > 0)
            {
                foreach (string k in postData.Keys)
                {
                    dataStr += string.Format($"{k}={WebUtility.UrlEncode(postData[k])}&");
                }
                dataStr = dataStr.Substring(0, dataStr.Length - 1);
            }
            CallResult result = new CallResult();

            result.Source = CreateUrlData(url, dataStr);

            if (method == HttpMethod.Get)
            {
                url = result.Source.ToString();
                result.Message = WebHelper.GetFrom(url);
            }
            else if (method == HttpMethod.Post)
            {
                result.Message = WebHelper.PostTo(url, dataStr);
            }

            return result;
        }

        /// <summary>
        ///  拼接url和参数
        /// </summary>
        /// <param name="url">要拼接的url</param>
        /// <param name="postData">url参数</param>
        /// <returns></returns>
        private static string CreateUrlData(string url, string postData)
        {
            if (!string.IsNullOrEmpty(postData))
            {
                string c = url.IndexOf('?') > 0 ? "&" : "?";
                url = string.Format($"{url}{c}{postData}");
            }

            return url;
        }

        /// <summary>
        ///  获取客户端请求的真实IP地址
        /// </summary>
        /// <param name="req">客户端请求对象</param>
        /// <returns></returns>
        public static string GetRealIP(HttpRequest req)
        {
            string realip = string.Empty;
            if (req == null)
                return realip;
            realip = req.Headers["x-real-ip"];
            if (string.IsNullOrEmpty(realip))
            {
                realip = req.Host.Host;
            }

            return realip;
        }
    }
}

