using Perfor.Lib.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Converter.Json
{
    /**
     * @ 重写属性，使用小写
     * */
    public class LimitPropertyContractResolver : DefaultContractResolver
    {
        string[] props = null;
        JsonFilterOption filter = JsonFilterOption.Normal;
        string specifyString = string.Empty;
        JsonCharOption charOption = JsonCharOption.Normal;
        JsonLowerUpper lowerUpper = JsonLowerUpper.Normal;

        public LimitPropertyContractResolver() { }

        /**
         * @ 构造方法第二次重载
         * @ charOption 对属性名的操作
         * @ specifyString 指定字符串
         * */
        public LimitPropertyContractResolver(JsonCharOption charOption, string specifyString, JsonLowerUpper lowerUpper = JsonLowerUpper.Normal)
        {
            this.charOption = charOption;
            this.specifyString = specifyString;
            this.lowerUpper = lowerUpper;
        }

        /**
         * @ 构造方法第三次重载
         * @ propNames 属性名列表
         * @ filter 过滤操作
         * */
        public LimitPropertyContractResolver(JsonFilterOption filter, string[] props, JsonLowerUpper lowerUpper = JsonLowerUpper.Normal)
        {
            this.filter = filter;
            this.props = props;
            this.lowerUpper = lowerUpper;
        }

        /**
         * @ 构造方法第四次重载
         * @ propNames 属性名列表
         * @ filter 过滤操作
         * @ charOption 对属性名的操作
         * @ specifyString 指定字符串
         * */
        public LimitPropertyContractResolver(JsonFilterOption filter, string[] props, JsonCharOption charOption, string specifyString, JsonLowerUpper lowerUpper = JsonLowerUpper.Normal)
        {
            this.filter = filter;
            this.props = props;
            this.charOption = charOption;
            this.specifyString = specifyString;
            this.lowerUpper = lowerUpper;
        }

        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (charOption != JsonCharOption.Normal)
            {
                if (charOption == JsonCharOption.Prefix)
                    property.PropertyName = string.Format("{0}{1}", specifyString, property.PropertyName);
                else
                    property.PropertyName = property.PropertyName.Replace(specifyString, "");
            }
            if (lowerUpper != JsonLowerUpper.Normal)
            {
                if (lowerUpper == JsonLowerUpper.Lower)
                    property.PropertyName = property.PropertyName.ToLower();
                else
                    property.PropertyName = property.PropertyName.ToUpper();
            }

            return property;
        }

        /**
         * @ 进行Filter操作
         * */
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> list = base.CreateProperties(type, memberSerialization);
            if (filter != JsonFilterOption.Normal && props != null && props.Length > 0)
            {
                bool isReservation = filter == JsonFilterOption.Reservation;
                list = list.Where(f => props.Contains(f.PropertyName) == isReservation).ToList();
            }
            return list;
        }
    }
}
