using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Perfor.Lib.Extension;

namespace Perfor.Lib.Converter.Json
{
    /**
     * @ 布尔值的Json格式转换
     * */
    public class BooleanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool) || objectType == typeof(Nullable<bool>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return false;

            return reader.Value.ObjToBoolean();
        }

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
