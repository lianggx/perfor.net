using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Perfor.Lib.Extension
{
    public static class ObjectExtension
    {
        #region Identity
        // 本地时区 1970.1.1格林威治时间
        private static DateTime Greenwich_Mean_Time = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);

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
            IEnumerable<PropertyInfo> properties = TSource.GetType().GetRuntimeProperties();
            if (filter.IsNotNullOrEmpty())
            {
                foreach (PropertyInfo pi in properties)
                {
                    if (filter.Contains(pi.Name)) continue;
                    pi.SetValue(TTarget, pi.GetValue(TSource, null), null);
                }
            }
            else
            {
                foreach (PropertyInfo pi in properties)
                {
                    pi.SetValue(TTarget, pi.GetValue(TSource, null), null);
                }
            }

            return TTarget;
        }

        #endregion
    }
}
