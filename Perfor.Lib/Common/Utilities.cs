using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Perfor.Lib.Common
{
    /**
     * @ 公共功能处理类
     * */
    public partial class Utilities
    {
        #region Identity
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
         * @ 图片压缩
         * @ source 图片流
         * @ width 目标宽度
         * @ height 目标高度
         * */
        public static Bitmap CompressPic(Stream source, int width, int height)
        {
            Image img = null;
            Bitmap bmp = null;
            Graphics grap = null;
            try
            {
                img = Image.FromStream(source);
                bmp = new Bitmap(width, height);
                grap = Graphics.FromImage(bmp);
                grap.DrawImage(img, new Rectangle(0, 0, width, height));
            }
            finally
            {
                if (null != img) img.Dispose();
            }
            return bmp;
        }

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

        /**
         * @ 创建验证码
         * @ sessionKey 保存验证码的会话 key 
         * @ session HttpSessionStateBase 对象
         * */
        public static byte[] CreateCode(string sessionKey, HttpSessionStateBase session)
        {
            CheckCode cc = new CheckCode();
            byte[] bytes = cc.WriterCode(4, 100, 50, 20, "宋体", 14, FontStyleType.NORMAL, 1, 1, sessionKey, CodeStyleType.NUMBER);
            session[sessionKey] = cc.SessionCode;
            return bytes;
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
    }
}
