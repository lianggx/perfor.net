using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Perfor.Lib.Extension;
using Newtonsoft.Json;

namespace Perfor.Lib.Converter.Json
{
    /**
 * @ 日期的Json格式转换
 * */
    public class DateTimeConverter : DateTimeConverterBase
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(Nullable<DateTime>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return false;

            return reader.Value.ObjToDateTime();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
            {
                writer.WriteValue(value.ObjToLong());
            }
        }
    }
}
