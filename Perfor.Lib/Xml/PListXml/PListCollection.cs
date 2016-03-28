using Perfor.Lib.Common;
using Perfor.Lib.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Perfor.Lib.Xml.PListXml
{
    public abstract class PListCollection : IPListNode
    {
        #region Identity
        ~PListCollection()
        {
            Dispose(false);
        }
        #endregion

        #region IPListNode
        public abstract IPListNode this[string key] { get; set; }

        /**
         * @ 实现 IPListNode 接口
         * */
        public abstract void WriterXml(XmlWriter writer);

        /**
         * @ 实现 IPListNode 接口
         * */
        public abstract void ReaderXml(XElement reader);

        /**
          * @ 将IPListNode对象转换为JSON字符串
          * */
        public abstract void WriterJson(TextWriter writer);

        /**
         * @ 将JSON转换为IPListNode对象
         * */
        public abstract void ReaderJson(JToken token);

        /**
         * @ 把plist对象反序列化为plist格式的文件
         * */
        public virtual void ToXmlFile(string file)
        {
            ToXmlString();
            using (FileStream stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                WriterToStream(stream);
                stream.Flush();
            }
        }

        /**
         * @ 从plist文件反序列化为plist对象
         * */
        public virtual void FromXmlFile(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException(string.Format("未找到文件：{0}", file));
            }

            FromStream(new FileStream(file, FileMode.Open, FileAccess.Read));
        }

        /**
         * @ 转换成JSON字符串
         * */
        public virtual string ToJson()
        {
            string result = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                TextWriter writer = new StreamWriter(ms);
                WriterJson(writer);
                writer.Flush();
                result = Encoding.UTF8.GetString(ms.ToArray());
            }

            return result;
        }

        /**
         * @ 将json字符串反序列化为plist对象
         * */
        public virtual IPListNode FromJson(string json)
        {
            IPListNode node = null;
            JToken token = JsonConvert.DeserializeObject<JToken>(json);
            if (token.Type == JTokenType.Array)
                node = new PListArray();
            else if (token.Type == JTokenType.Object)
                node = new PListDict();

            node.ReaderJson(token);

            return node;
        }

        /**
         * @ 序列化为 plist 格式的字符串
         * */
        public virtual string ToXmlString()
        {
            string result = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                WriterToStream(ms);
                result = Encoding.UTF8.GetString(ms.ToArray());
                ms.Flush();
            }

            return result;
        }

        /**
         * @ 将plist格式字符串反序列化为plist对象
         * */
        public virtual void FromXmlString(string plist)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plist);
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                FromStream(ms);
            }
        }

        /**
         * @ 接口实现，清理资源
         * */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /**
         * @ 清理托管资源
         * */
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                return;

            disposing = true;
        }

        #region Properties
        protected string tag = string.Empty;
        /**
         * @ 标签名称
         * */
        public virtual string Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        protected int order = 0;
        /**
         * @ 排序号
         * */
        public virtual int Order
        {
            get { return order; }
            set { order = value; }
        }

        protected object htValue = string.Empty;
        /**
         * @ 包装的值
         * */
        public virtual Object Value
        {
            get { return htValue; }
            set { htValue = value; }
        }

        /**
         * @ 实现 IPListNode 接口
         * */
        public abstract bool HasChildren { get; }
        #endregion
        #endregion

        #region Self
        /**
        * @ 从序列化的字节流加载 PList 对象
        * */
        protected virtual void FromStream(Stream stream)
        {
            XDocument doc = XDocument.Load(stream, LoadOptions.None);
            XElement dict = doc.Root.Element("dict");
            ReaderXml(dict);
        }

        /**
         * @ 将当前对象以 xml 形式写入 stream
         * @ stream 流对象
         * */
        protected virtual void WriterToStream(Stream stream)
        {
            XmlTextWriter xmlwriter = new XmlTextWriter(stream, Encoding.UTF8);
            xmlwriter.Formatting = System.Xml.Formatting.Indented;
            xmlwriter.WriteStartDocument();
            xmlwriter.WriteDocType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
            xmlwriter.WriteStartElement("plist");
            xmlwriter.WriteAttributeString("version", "1.0");
            xmlwriter.WriteStartElement("dict");
            WriterXml(xmlwriter);
            xmlwriter.WriteEndElement();
            xmlwriter.WriteEndElement();
            xmlwriter.WriteEndDocument();
            xmlwriter.Flush();
        }

        /**
       * @ 根据传入的值给出 LdfValueType 类型
       * @ value要判断的值
       * */
        protected virtual NodeValueType GetValueType(object value)
        {
            NodeValueType vt = NodeValueType.STRING;
            if (value is PListArray)
                vt = NodeValueType.ARRAY;
            else if (value is PListDict)
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
        protected virtual void WriteElementKey(XmlWriter writer, string key)
        {
            writer.WriteStartElement("key");
            writer.WriteString(key.ToString());
            writer.WriteEndElement();
        }

        /**
         * @ 格式化值的输出
         * @ ht LdfHashtable对象
         * */
        protected virtual void FormatXml(XmlWriter writer, IPListNode ht)
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
                    string plistDate = ht.Value.ObjToDateTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffZ");
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
        protected virtual void FormatJson(TextWriter writer, string key, IPListNode ht)
        {
            string result = string.Empty;
            NodeValueType valueType = GetValueType(ht.Value);
            if (key.IsNotNullOrEmpty())
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

        protected virtual void FormatJson(TextWriter writer, object text)
        {
            writer.Write(Utilities.JSON_QUOTES);
            writer.Write(text);
            writer.Write(Utilities.JSON_QUOTES);
        }

        /**
         * @ 把元素的值包装成接口 IPListNode 类型
         * */
        protected virtual IPListNode ParseNode(XElement reader)
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

        /**
 * @ 转换JToken对象到IPListNode
 * */
        protected IPListNode SwitchJToken(JToken token)
        {
            IPListNode node = null;
            switch (token.Type)
            {
                case JTokenType.Array:
                    node = new PListArray();
                    SetNodeTag(node, token);
                    node.ReaderJson(token);
                    break;
                case JTokenType.Object:
                    node = new PListDict();
                    SetNodeTag(node, token);
                    node.ReaderJson(token);
                    break;
                default:
                    node = new PListDict();
                    if (token.Type == JTokenType.Property)
                    {
                        JProperty jproperty = (JProperty)token;
                        IPListNode childrenNode = null;
                        if (jproperty.Value.Type == JTokenType.Array) { childrenNode = new PListArray(); }
                        else { childrenNode = new PListDict(); }

                        childrenNode.Tag = jproperty.Name;
                        ((PListDict)node).Add(childrenNode.Tag, childrenNode);
                        childrenNode.ReaderJson(jproperty.Value);
                    }
                    else
                    {
                        // 如果上级是数组不用设置 tag 的
                        SetNodeTag(node, token);
                        JValue jValue = (JValue)token;
                        node.Value = jValue.Value;
                    }
                    break;
            }
            return node;
        }

        /**
         * @ 设置节点的 Tag 属性
         * @ node 要设置的节点
         * @ token 当前要处理的json 对象
         * */
        private void SetNodeTag(IPListNode node, JToken token)
        {
            // 如果上级是数组不用设置 tag 的
            if (token.Parent.Type == JTokenType.Array)
                return;

            JProperty jproperty = (JProperty)token.Parent;
            node.Tag = jproperty.Name;
        }
        #endregion
    }
}
