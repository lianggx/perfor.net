using dywebsdk.Common;
using Perfor.Lib.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dywebsdk.Cryptography
{
    /// <summary>
    ///  url签名管理类
    /// </summary>
    public class UrlValidator
    {
        /// <summary>
        ///  生成url签名（使用SHA256加密算法生成），签名+时间戳
        ///  其中，签名（sign），url签名生成的时间戳（timestamp），签名算法为：url路径（自动转小写，不包含host）+按字母顺序排序的参数（自动转小写）+签名key
        /// </summary>
        /// <param name="url">带签名的url对象</param>
        /// <param name="param">url携带的参数</param>
        /// <param name="timeStamp">是否在签名中加入参数：timestamp</param>
        /// <param name="signKey">签名key</param>
        /// <returns>包含sign和timestamp参数</returns>
        public static Dictionary<string, string> GenerateSign(Uri url, NameValueCollection param, bool timeStamp = true, string signKey = "")
        {
            // 装入数组，准备排序
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            if (param != null)
            {
                foreach (string str in param.AllKeys)
                {
                    KeyValuePair<string, string> item = new KeyValuePair<string, string>(str, param[str]);
                    list.Add(item);
                }
            }
            long? tick = null;
            string timeKey = "timestamp";
            // 如果使用时间戳，加入参数中
            if (timeStamp)
            {
                DateTime time = TimeZoneInfo.ConvertTime(Utilities.Greenwich_Mean_Time, TimeZoneInfo.Local);
                tick = (DateTime.Now.Ticks - time.Ticks) / 10000;
                list.Add(new KeyValuePair<string, string>(timeKey, tick.ToString()));
            }

            // 参数按顺序排序
            list = list.OrderBy(f => f.Key).ToList<KeyValuePair<string, string>>();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                KeyValuePair<string, string> p = list[i];
                if (!string.IsNullOrEmpty(p.Key))
                {
                    string c = ((i + 1) == list.Count) ? "" : "&";
                    builder.AppendFormat($"{p.Key}={p.Value}{c}");
                }
            }
            // 取出url路径+参数
            string input = WebUtility.UrlEncode(string.Format($"{url.AbsolutePath}/{builder.ToString()}"));
            Regex regex = new Regex("%[a-f0-9]{2}");
            input = regex.Replace(input, (MatchEvaluator)(m => m.Value.ToUpperInvariant()));
            // 加入签名key
            if (!string.IsNullOrEmpty(signKey))
            {
                input = string.Format($"{input}&{signKey}");
            }
            // 生成签名
            byte[] bytes = Encoding.UTF8.GetBytes(input.ToLower());
            SHA256 managed = SHA256.Create();
            string hashCode = WebUtility.UrlEncode(Convert.ToBase64String(managed.ComputeHash(bytes)));
            hashCode = regex.Replace(hashCode, (MatchEvaluator)(m => m.Value.ToUpperInvariant()));

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(timeKey, tick.HasValue ? tick.Value.ToString() : "");
            dict.Add("sign", hashCode);
            // 返回签名
            return dict;
        }

        /// <summary>
        ///  使用本类中的GenerateSign算法验证url签名
        /// </summary>
        /// <param name="url">请求路径，不含host</param>
        /// <param name="param">请求的参数</param>
        /// <param name="signName">请求参数中的签名参数名</param>
        /// <param name="signKey">签名key</param>
        /// <returns></returns>
        public static bool ValidatorSign(Uri url, NameValueCollection param, string signKey = "", string signName = "sign")
        {
            // 未传送签名，验证失败
            if (param == null || param.Count == 0 || string.IsNullOrEmpty(param[signName]))
            {
                return false;
            }
            string sign = string.Empty;
            NameValueCollection pm = new NameValueCollection();
            foreach (string k in param.Keys)
            {
                if (k == signName)
                {
                    sign = WebUtility.UrlDecode(param[k]);
                    continue;
                }
                pm.Add(k, param[k]);
            }

            param.Remove(signName);
            Dictionary<string, string> ger = GenerateSign(url, pm, false, signKey);
            string mysign = WebUtility.UrlDecode(ger["sign"]);
            return mysign == sign;
        }
    }
}
