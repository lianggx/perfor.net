using System;
using System.Text.RegularExpressions;

namespace Perfor.Lib.Common
{
    /**
     * @ 公共功能处理类
     * */
    public partial class Utilities
    {
        #region Identity
        // 本地时区 1970.1.1格林威治时间
        public static DateTime Greenwich_Mean_Time = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
        /**冒号**/
        public const String JSON_COLON = ":";
        /**双引号**/
        public const String JSON_QUOTES = "\"";
        /**大括号-左**/
        public const String JSON_BRACES_LEFT = "{";
        /**大括号-右**/
        public const String JSON_BRACES_RIGHT = "}";
        /**中括号-左**/
        public const String JSON_BRACKET_LEFT = "[";
        /**中括号-右**/
        public const String JSON_BRACKET_RIGHT = "]";
        #endregion

        /**
         * @ 将传人的时间转换为yyyy-MM-dd 00:00:00且dd为当月最后一天的格式
         * */
        public static string GetLastMonth(DateTime date)
        {
            DateTime dt1 = new DateTime(date.Year, date.Month, 1);
            return string.Format("{0}-{1} 00:00:00", date.ToString("yyyy-MM"), dt1.AddMonths(1).AddDays(-1).Day);
        }

        /**
         * @ 获取一个新的 GUID ，并且不包含 "-" 符号的字符串表现形式
         * */
        public static string GetGuidNorString()
        {
            return Guid.NewGuid().ToString("N");
        }

        /*
         * @ 过滤 SQL 注入的字符
         * */
        public static string DetectSQLInjection(string sqlCmdText)
        {
            Regex regex = new Regex("'");
            sqlCmdText = regex.Replace(sqlCmdText, "''");
            regex = new Regex("-");
            sqlCmdText = regex.Replace(sqlCmdText, "");
            return sqlCmdText;
        }

        /**
         * @ 根据长度判断是否需要写入逗号
         * @ len 循环长度
         * @ index 当前下标
         * @ incremental 增量
         * @ chars 输入的分隔符号
         * */
        public static string IsWriterComma(int len, int index, int incremental, string chars = ",")
        {
            string result = (index + incremental == len) ? "" : chars;

            return result;
        }

        #region Function IsValidEmail/Phone/IdCard
        public static bool IsValidEmail(string value)
        {
            return Regex.IsMatch(value, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        public static bool IsValidIdCard(string value)
        {
            return Regex.IsMatch(value, @"^\d{17}([0-9]|X)$");
        }

        public static bool IsValidPhone(string value)
        {
            return Regex.IsMatch(value, "1[3|5|7|8|][0-9]{9}");
        }
        #endregion

        public static string UrlSubParser(string url)
        {
            Uri uri = new Uri(url);
            object[] args = new object[] { uri.Scheme, uri.Host, (uri.Port == 80) ? "" : (":" + uri.Port.ToString()), uri.AbsolutePath };
            return string.Format("{0}://{1}{2}{3}", args);
        }
    }
}
