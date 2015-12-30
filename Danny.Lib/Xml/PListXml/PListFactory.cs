using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Danny.Lib.Extension;
using System.Xml;
using System.Xml.Linq;

namespace Danny.Lib.Xml.PListXml
{
    public class PListFactory
    {
        /**
         * @ 根据传入的值给出 LdfValueType 类型
         * @ value要判断的值
         * */
        public static LdfValueType GetValueType(object value)
        {
            LdfValueType vt = LdfValueType.STRING;
            if (value is IList)
                vt = LdfValueType.ARRAY;
            else if (value is IDictionary)
                vt = LdfValueType.DICT;
            else if (value is string)
                vt = LdfValueType.STRING;
            else if (value is DateTime)
                vt = LdfValueType.DATE;
            else if (value is int || value is long)
                vt = LdfValueType.INTEGER;
            else if (value is decimal || value is float || value is double)
                vt = LdfValueType.REAL;
            else if (value is bool)
            {
                if (value.ObjToBoolean())
                    vt = LdfValueType.TRUE;
                else
                    vt = LdfValueType.FALSE;
            }

            return vt;
        }

        /**
         * @ 在 xml 流中写入一对标记<key>key</key>
         * @ key 要写入的标记名称
         * */
        public static void WriteElementKey(XmlWriter writer, string key)
        {
            writer.WriteStartElement("key");
            writer.WriteString(key.ToString());
            writer.WriteEndElement();
        }

        /**
         * @ 格式化值的输出
         * @ ht LdfHashtable对象
         * */
        public static void FormatXml(XmlWriter writer, IPListNode ht)
        {
            string result = string.Empty;
            string keyString = GetValueType(ht.Value).ToString().ToLower();
            switch (keyString)
            {
                case "data":
                    writer.WriteStartElement("data");
                    writer.WriteString(Convert.ToBase64String(ht.Value as byte[]));
                    writer.WriteEndElement();
                    break;
                case "date":
                    writer.WriteStartElement(keyString);
                    string plistDate = ht.Value.ObjUnixToDateTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffZ");
                    writer.WriteString(plistDate);
                    writer.WriteEndElement();
                    break;
                case "key":
                case "real":
                case "string":
                case "integer":
                    writer.WriteStartElement(keyString);
                    writer.WriteString(ht.Value.ToString());
                    writer.WriteEndElement();
                    break;
                case "false":
                    writer.WriteStartElement("false");
                    writer.WriteEndElement();
                    break;
                case "true":
                    writer.WriteStartElement("true");
                    writer.WriteEndElement();
                    break;
                default:
                    writer.WriteStartElement("string");
                    writer.WriteEndElement();
                    break;
            }
        }

        /**
         * @ 把元素的值包装成接口 IPListNode 类型
         * */
        public static IPListNode ParseNode(XElement reader)
        {
            string key = reader.Name.LocalName;
            IPListNode node = new PListDict();
            string val = string.Empty;
            switch (key)
            {
                case "dict":
                    node.ReaderXml(reader);
                    break;
                case "array":
                    node = new PListArray();
                    node.ReaderXml(reader);
                    break;
                case "date":
                    node.Value = reader.Value.ToDateTime();
                    break;
                case "data":
                    node.Value = reader.Value.ToBytes();
                    break;
                case "false":
                case "true":
                    node.Value = key == "true";
                    break;
                default:
                case "real":
                case "integer":
                    node.Value = reader.Value;
                    break;
            }
            return node;
        }
    }
}
