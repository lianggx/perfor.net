using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Danny.Lib.Extension;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Danny.Lib.Common;

namespace Danny.Lib.Xml.PListXml
{
    public class PListFactory
    {
        /**
         * @ 根据传入的值给出 LdfValueType 类型
         * @ value要判断的值
         * */
        public static NodeValueType GetValueType(object value)
        {
            NodeValueType vt = NodeValueType.STRING;
            if (value is IList)
                vt = NodeValueType.ARRAY;
            else if (value is IDictionary)
                vt = NodeValueType.DICT;
            else if (value is string)
                vt = NodeValueType.STRING;
            else if (value is DateTime)
                vt = NodeValueType.DATE;
            else if (value is int || value is long)
                vt = NodeValueType.INTEGER;
            else if (value is decimal || value is float || value is double)
                vt = NodeValueType.REAL;
            else if (value is bool)
            {
                if (value.ObjToBoolean())
                    vt = NodeValueType.TRUE;
                else
                    vt = NodeValueType.FALSE;
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
            NodeValueType valueType = GetValueType(ht.Value);
            string keyString = valueType.ToString().ToLower();
            writer.WriteStartElement(keyString);
            switch (valueType)
            {
                case NodeValueType.DATA:
                    writer.WriteString(Convert.ToBase64String(ht.Value as byte[]));
                    break;
                case NodeValueType.DATE:
                    string plistDate = ht.Value.ObjUnixToDateTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffZ");
                    writer.WriteString(plistDate);
                    break;
                //case "key":
                case NodeValueType.REAL:
                case NodeValueType.STRING:
                case NodeValueType.INTEGER:
                    writer.WriteString(ht.Value.ToString());
                    break;
            }
            writer.WriteEndElement();
        }

        /**
        * @ 格式化值的输出
        * @ ht LdfHashtable对象
        * */
        public static void FormatJson(TextWriter writer, string key, IPListNode ht)
        {
            string result = string.Empty;
            NodeValueType valueType = GetValueType(ht.Value);
            if (key.IsNotNull())
            {
                writer.Write(string.Format("{0}{1}{0}{2}", Utilities.JSON_QUOTES, key, Utilities.JSON_COLON));
            }
            string keyString = valueType.ToString().ToLower();
            switch (valueType)
            {
                case NodeValueType.DATA:
                    string data = Convert.ToBase64String(ht.Value as byte[]);
                    FormatJson(writer, data);
                    break;
                case NodeValueType.DATE:
                    long unixDate = ht.Value.ObjToLong();
                    FormatJson(writer, unixDate);
                    break;
                //case "key":                
                case NodeValueType.STRING:
                    FormatJson(writer, ht.Value);
                    break;
                case NodeValueType.REAL:
                case NodeValueType.INTEGER:
                    writer.Write(ht.Value);
                    break;
                case NodeValueType.TRUE:
                case NodeValueType.FALSE:
                    writer.Write(ht.Value.ObjToInt());
                    break;
            }
        }

        private static void FormatJson(TextWriter writer, object text)
        {
            writer.Write(Utilities.JSON_QUOTES);
            writer.Write(text);
            writer.Write(Utilities.JSON_QUOTES);
        }

        /**
         * @ 把元素的值包装成接口 IPListNode 类型
         * */
        public static IPListNode ParseNode(XElement reader)
        {
            string key = reader.Name.LocalName;
            NodeValueType valueType = key.ToUpper().ToEnum<NodeValueType>();
            IPListNode node = new PListDict();
            string val = string.Empty;
            switch (valueType)
            {
                case NodeValueType.DICT:
                    node.ReaderXml(reader);
                    break;
                case NodeValueType.ARRAY:
                    node = new PListArray();
                    node.ReaderXml(reader);
                    break;
                case NodeValueType.DATE:
                    node.Value = reader.Value.ToDateTime();
                    break;
                case NodeValueType.DATA:
                    node.Value = reader.Value.ToBytes();
                    break;
                case NodeValueType.FALSE:
                case NodeValueType.TRUE:
                    node.Value = valueType == NodeValueType.TRUE;
                    break;                
                case NodeValueType.REAL:
                    node.Value = reader.Value.ToDecimal();
                    break;
                case NodeValueType.INTEGER:
                    node.Value = reader.Value.ToInt();
                    break;
                default:
                    node.Value = reader.Value;
                    break;
            }
            return node;
        }
    }
}
