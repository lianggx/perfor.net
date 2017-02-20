using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Perfor.Lib.Extension;

namespace Perfor.Lib.Converter.Json
{
    /// <summary>
    ///  布尔值的Json格式转换
    /// </summary>
    public class BooleanConverter : JsonConverter
    {
        /// <summary>
        ///  重写的方法，传入的类型是否可以转换
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool) || objectType == typeof(Nullable<bool>);
        }

        /// <summary>
        ///  重写的方法，读取Json字符串
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return false;

            return reader.Value.ObjToBoolean();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
            {
                int val = value.ObjToBoolean().ToInt();
                writer.WriteValue(val);
            }
        }
    }
}
