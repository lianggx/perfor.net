using Perfor.Lib.Common;
using Perfor.Lib.Converter.Json;
using Perfor.Lib.Enums;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Perfor.Lib.Extension
{
    /**
     * @ 强类型化对象扩展方法
     * */
    public static partial class TypeExtension
    {
        #region Identity
        // 本地时区 1970.1.1格林威治时间
        private static DateTime Greenwich_Mean_Time = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

        #endregion

        #region Validator

        public static bool IsInt(this string str)
        {
            Regex regex = new Regex(@"\d");
            return regex.IsMatch(str);
        }
        /**
         *  @ 判断传入的字符串是否为空
         * */
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /**
         *  @ 判断传入的字符串是否不为空
         * */
        public static bool IsNotNullOrEmpty(this string str)
        {
            bool isNotNull = string.IsNullOrEmpty(str) == false;
            return isNotNull;
        }

        /**
         * @ 判断数组是否空
         * */
        public static bool IsNullOrEmpty(this IEnumerable<string> array)
        {
            bool isnull = array == null || array.Count() == 0;

            return isnull;
        }

        /**
         * @ 判断数组是否不为空
         * */
        public static bool IsNotNullOrEmpty(this IEnumerable<string> array)
        {
            bool isNotNull = array != null && array.Count() > 0;

            return isNotNull;
        }

        /**
         * @ 判断数组是否不为空，并且长度等于 @len
         * @ len 数组长度
         * */
        public static bool IsNotNullAndEq<T>(this IEnumerable<T> array, int len)
        {
            bool isEq = array != null && array.Count() == len;

            return isEq;
        }

        /**
         * @ 判断数组是否不为空，并且长度大于等于 @len
         * @ len 数组长度
         * */
        public static bool IsNotNullAndGtEq<T>(this IEnumerable<T> array, int len)
        {
            bool isEq = array != null && array.Count() >= len;

            return isEq;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> value)
        {
            bool isnull = true;
            if (value == null)
                return isnull;
            if (value.Count() == 0)
                return isnull;
            return false;
        }

        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> value)
        {
            bool isNotNull = false;
            if (value == null)
                return isNotNull;
            if (value.Count() == 0)
                return isNotNull;
            return true;
        }

        /**
         * @ T 枚举类型
         * @ F 要检查的值类型
         */
        public static bool IsEnum<T, F>(this F value)
        {
            return Enum.IsDefined(typeof(T), value);
        }

        public static bool Contains<T>(this IEnumerable<T> value, Func<T, bool> func)
        {
            bool cont = false;
            if (value.IsNullOrEmpty())
                return cont;
            foreach (var item in value)
            {
                if (func(item))
                {
                    cont = true;
                    break;
                }
            }
            return cont;
        }

        public static bool IsNullableType(this Type value)
        {
            return value == typeof(Nullable);
        }
        #endregion

        #region Object.To
        public static int ObjToInt(this object value)
        {
            int result = 0;
            try
            {
                if (value != null)
                    result = Convert.ToInt32(value);
            }
            catch { }
            return result;
        }

        public static decimal ObjToDecimal(this object value)
        {
            decimal result = 0;
            try
            {
                if (value != null)
                    result = Convert.ToDecimal(value);
            }
            catch { }
            return result;
        }

        public static bool ObjToBoolean(this object value)
        {
            try
            {
                return Convert.ToBoolean(value);
            }
            catch { }
            return false;
        }

        public static DateTime ObjToDateTime(this object value)
        {
            DateTime dt = Greenwich_Mean_Time;
            try
            {
                if (value == null)
                    return dt;
                if (value.GetType() == typeof(long))
                    dt = dt.AddMilliseconds(Convert.ToInt64(value));
                else
                    dt = Convert.ToDateTime(value);
            }
            catch { }
            return dt;
        }

        public static long ObjToLong(this object value)
        {
            long val = 0;
            try
            {
                if (value == null)
                    return val;
                if (value.GetType() == typeof(DateTime))
                {
                    DateTime dt = value.ObjToDateTime();
                    val = dt.ToUnixDateTime();
                }
                else
                    val = Convert.ToInt64(value);
            }
            catch { }
            return val;
        }

        /**
         * @ 在相同对象间复制值，仅支持 Public 类型的属性，用于修改值
         * @ TSource 源对象
         * @ TTarget 目标对象
         * @ filter 不复制的属性名称，该参数为 TSource 的属性名称
         * */
        public static T CopyTo<T>(this T TSource, T TTarget, params string[] filter) where T : class
        {
            PropertyInfo[] properties = TSource.GetType().GetProperties();
            if (filter.IsNotNullOrEmpty())
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo pi = properties[i];
                    if (filter.Contains(pi.Name)) continue;
                    pi.SetValue(TTarget, pi.GetValue(TSource, null), null);
                }
            }
            else
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo pi = properties[i];
                    pi.SetValue(TTarget, pi.GetValue(TSource, null), null);
                }
            }

            return TTarget;
        }

        #endregion

        #region String.To
        public static string ToNorString(this Guid value)
        {
            return value.ToString("N");
        }

        public static byte[] ToBytes(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            return Encoding.UTF8.GetBytes(value);
        }

        public static int ToShortInt(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            Regex regex = new Regex(@"\d[0-9]");
            if (regex.IsMatch(str))
                return Convert.ToInt16(str);

            return 0;
        }

        public static int ToInt(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            Regex regex = new Regex(@"\d[0-9]");
            if (regex.IsMatch(str))
                return Convert.ToInt32(str);

            return 0;
        }

        public static long ToLong(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
            Regex regex = new Regex(@"\d[0-9]");
            if (regex.IsMatch(str))
                return Convert.ToInt64(str);

            return 0;
        }

        public static decimal ToDecimal(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
            Regex regex = new Regex(@"[0-9]\d*[\.]?\d*|-0\.\d*[0-9]\d*$");
            if (regex.IsMatch(str))
                return Convert.ToDecimal(str);

            return 0;
        }

        public static Guid ToGuid(this string str)
        {
            Guid val = Guid.Empty;
            try
            {
                val = Guid.Parse(str);
            }
            catch { }
            return val;
        }

        public static double ToDouble(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
            Regex regex = new Regex(@"[0-9]\d*[\.]?\d*|-0\.\d*[0-9]\d*$");
            if (regex.IsMatch(str))
                return Convert.ToDouble(str);

            return 0;
        }

        public static double ToFloat(this string str)
        {
            return (float)ToDecimal(str);
        }

        public static DateTime ToDateTime(this string str)
        {
            DateTime val = DateTime.Parse("1970-1-1");
            if (string.IsNullOrEmpty(str))
                return val;

            try
            {
                val = Convert.ToDateTime(str);
            }
            catch { }
            return val;
        }

        public static Nullable<bool> ToBooleanNull(this string str)
        {
            Nullable<bool> val = null;
            try
            {
                val = Convert.ToBoolean(str);
            }
            catch { }
            return val;
        }

        public static bool ToBoolean(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            Regex regex = new Regex(@"[0-9]\d*[\.]?\d*|-0\.\d*[0-9]\d*$");
            if (regex.IsMatch(str))
            {
                return Convert.ToInt64(str) > 0;
            }
            regex = new Regex(@"true|false");
            if (regex.IsMatch(str.ToLower()))
            {
                return Convert.ToBoolean(str);
            }
            return false;
        }

        public static string ToEllipsis(this string value, int length, string spl = "...")
        {
            if (value.IsNullOrEmpty())
                return "";
            int len = value.Length;
            if (len > length)
            {
                value = string.Format("{0}{1}", value.Substring(0, length), spl);
            }

            return value;
        }

        /**
         * @ 删除空格和制表位
         * */
        public static string ToTrimSpace(this string value)
        {
            Regex regex = new Regex(@"[\u3000||\u0020|\t]{2,}");

            return regex.Replace(value, " ");
        }

        /**
         * @ 将位移编码的字母和数字进行解码
         * */
        public static string DecodeText(this string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(bytes[i] + 10 - 7);
            }
            string result = Encoding.ASCII.GetString(bytes);

            return result;
        }

        /**
         * @ 将字母和数字进行位移编码，sorry 不支持中文
         * */
        public static string EncodeText(this string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(bytes[i] - 10 + 7);
            }
            string result = Encoding.ASCII.GetString(bytes);

            return result;
        }
        #endregion

        #region DateTime.To
        public static long DateDiff(this DateTime dt1, DateTime dt2, DateInterval di)
        {
            return DateAndTime.DateDiff(di, dt1, dt2);
        }

        public static long ToUnixDateTime(this DateTime dt)
        {
            long val = 0;
            try
            {
                // 除以10000，保持13位
                val = (dt.Ticks - Greenwich_Mean_Time.Ticks) / 10000;
            }
            catch { }
            return val;
        }

        public static long ToUnixDateTime(this Nullable<DateTime> dt)
        {
            long val = 0;
            try
            {
                val = ToUnixDateTime(Greenwich_Mean_Time);
            }
            catch { }
            return val;
        }

        public static string DateToWeak_ZH(this DateTime dt)
        {
            return "日一二三四五六".Substring(dt.DayOfWeek.GetHashCode(), 1);
        }
        #endregion

        #region decimal/float/double/long.To
        public static int ToInt(this long value)
        {
            return Convert.ToInt16(value);
        }
        public static int ToInt(this decimal value)
        {
            return Convert.ToInt16(value);
        }
        public static int ToInt(this float value)
        {
            return Convert.ToInt16(value);
        }
        public static int ToInt(this double value)
        {
            return Convert.ToInt16(value);
        }

        public static DateTime FromUnixDateTime(this long value)
        {
            DateTime dt = Greenwich_Mean_Time;
            try
            {
                dt = dt.AddMilliseconds(value);
            }
            catch { }
            return dt;
        }
        #endregion

        #region Other.To

        public static int ToInt(this bool value)
        {
            int val = Convert.ToInt32(value);
            return val;
        }

        public static int ToInt(this Nullable<bool> value)
        {
            if (value.HasValue)
                return Convert.ToInt32(value);
            return 0;
        }

        public static int ToInt(this Enum value)
        {
            int val = Convert.ToInt32(value);
            return val;
        }

        /**
         * @ 调用该方法前最好先调用IsEnum进行确认
         * */
        public static T ToEnum<T>(this string value)
        {
            T val = default(T);
            try
            {
                val = (T)Enum.Parse(typeof(T), value);
            }
            catch
            {
                throw new ArgumentException("由于不确定因素，此异常必须抛出，建议调用该方法前最好先调用 IsEnum 进行确认");
            }
            return val;
        }

        public static T ToEnum<T>(this int value)
        {
            return ToEnum<T>(value.ToString());
        }

        public static Enum ToEnum(this string value)
        {
            return ToEnum<Enum>(value);
        }

        /**
         * @ 将一个数组使用指定的分隔符号进行格式化后输出，还可以给数组中的每个item加上指定的前缀
         * @ separator 分隔符
         * @ prefix 指定前缀
         * */
        public static string ToJoin<T>(this IEnumerable<T> value, string separator = ",")
        {
            if (value == null || value.Count() == 0)
                return "";
            StringBuilder text = new StringBuilder();
            foreach (var item in value)
            {
                text.AppendFormat("{0}{1}", item, separator);
            }
            string val = text.ToString();
            val = val.Substring(0, val.Length - 1);

            return val;
        }
        #endregion

        #region Base64.To
        public static string Base64Decode(this string value)
        {
            if (value.IsNullOrEmpty())
                return null;
            byte[] bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Base64Encode(this string value)
        {
            if (value.IsNullOrEmpty())
                return null;
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }


        public static string ToBase64(this byte[] value)
        {
            if (value.IsNullOrEmpty())
                return string.Empty;

            string result = Convert.ToBase64String(value);

            return result;
        }

        public static byte[] FromBase64(this string value)
        {
            if (value.IsNullOrEmpty())
                return null;
            byte[] bytes = Convert.FromBase64String(value);

            return bytes;
        }


        #endregion

        #region MD5/SHA256
        public static string ToMD5(this string value)
        {
            MD5 md5 = MD5.Create();
            byte[] source = Encoding.UTF8.GetBytes(value);
            byte[] crypto = md5.ComputeHash(source, 0, source.Length);
            value = Convert.ToBase64String(crypto);
            return value;
        }

        public static string ToSHA256(this string value)
        {
            SHA256 sha256 = new SHA256Managed();
            byte[] source = Encoding.UTF8.GetBytes(value);
            byte[] crypto = sha256.ComputeHash(source, 0, source.Length);
            value = Convert.ToBase64String(crypto);
            return value;
        }
        #endregion

        #region Json

        public static T FromJson<T>(this string value, JsonSerializerSettings settings)
        {
            T obj = JsonConvert.DeserializeObject<T>(value, settings);
            return obj;
        }

        public static T FromJson<T>(this string value, JsonLowerUpper lowerUpper = JsonLowerUpper.Normal)
        {
            return FromJson<T>(value, JsonCharOption.Normal, string.Empty, lowerUpper);
        }

        public static T FromJson<T>(this string value, JsonCharOption charOption, string specifyString, JsonLowerUpper lowerUpper = JsonLowerUpper.Normal)
        {
            if (value.IsNullOrEmpty())
                return default(T);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new LimitPropertyContractResolver(charOption, specifyString, lowerUpper);
            IList<JsonConverter> converts = settings.Converters;
            converts.Add(new BooleanConverter());
            converts.Add(new DateTimeConverter());
            T obj = JsonConvert.DeserializeObject<T>(value, settings);

            return obj;
        }

        public static string ObjToJson(this object value, JsonSerializerSettings settings)
        {
            string jsonString = JsonConvert.SerializeObject(value, settings);
            return jsonString;
        }

        public static string ObjToJson(this object value, JsonLowerUpper lowerUpper = JsonLowerUpper.Lower)
        {
            return ObjToJson(value, JsonFilterOption.Normal, null, JsonCharOption.Normal, string.Empty, lowerUpper);
        }

        public static string ObjToJson(this object value, JsonFilterOption filter, string[] props, JsonLowerUpper lowerUpper = JsonLowerUpper.Lower)
        {
            return ObjToJson(value, filter, props, JsonCharOption.Normal, string.Empty, lowerUpper);
        }

        public static string ObjToJson(this object value, JsonCharOption charOption, string specifyString, JsonLowerUpper lowerUpper = JsonLowerUpper.Lower)
        {
            return ObjToJson(value, JsonFilterOption.Normal, null, charOption, specifyString, lowerUpper);
        }

        /**
         * @ 序列化对象字符串
         * */
        public static string ObjToJson(this object value, JsonFilterOption filter, string[] props, JsonCharOption charOption, string specifyString, JsonLowerUpper lowerUpper = JsonLowerUpper.Lower)
        {
            if (value == null)
                return string.Empty;
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new LimitPropertyContractResolver(filter, props, charOption, specifyString, lowerUpper);
            IList<JsonConverter> converts = settings.Converters;
            converts.Add(new BooleanConverter());
            converts.Add(new DateTimeConverter());
            string jsonString = JsonConvert.SerializeObject(value, settings);
            return jsonString;
        }
        #endregion

        #region Linq
        /// <summary>
        /// 排序规则
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sortId"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public static IQueryable<T> DataSorting<T>(this IQueryable<T> source, string sortId, string sortOrder)
        {
            if (source != null)
            {
                string sortingDir = string.Empty;
                if (sortOrder.ToUpper().Trim() == "ASC")
                    sortingDir = "OrderBy";
                else if (sortOrder.ToUpper().Trim() == "DESC")
                    sortingDir = "OrderByDescending";
                ParameterExpression param = Expression.Parameter(typeof(T), sortId);
                PropertyInfo pi = typeof(T).GetProperty(sortId);
                Type[] types = new Type[2];
                types[0] = typeof(T);
                types[1] = pi.PropertyType;
                Expression expr = Expression.Call(typeof(Queryable), sortingDir, types, source.Expression, Expression.Lambda(Expression.Property(param, sortId), param));
                IQueryable<T> query = source.AsQueryable().Provider.CreateQuery<T>(expr);
                return query;
            }
            return source;
        }

        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, int page, int size)
        {
            return new PagedList<T>(source, page, size);
        }
        #endregion
    }
}
