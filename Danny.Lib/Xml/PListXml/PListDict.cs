using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

using Danny.Lib.Extension;
using System.Xml;
using System.Runtime.Serialization;

namespace Danny.Lib.Xml.PListXml
{
    /**
     * @ Apple plist 文件读取管理
     * */
    [Serializable]
    public class PListDict : Dictionary<string, IPListNode>, IPListNode
    {
        #region Identity
        ~PListDict()
        {
            Dispose(false);
        }

        /**
         * @ 默认构造方法
         * */
        public PListDict()
        {
            this.objValue = this;
        }

        /**
     * @ 构造函数第三次重载
     * @ htValue 值
     * @ order 排序号
     * */
        public PListDict(object htValue)
            : this(htValue, 0)
        {
            this.objValue = htValue;
        }

        /**
         * @ 构造函数第四次重载
         * @ htValue 值
         * @ order 排序号
         * */
        public PListDict(object objValue, int order)
        {
            this.objValue = objValue;
            this.order = order;
        }

        /**
         * @ 序列化参数构造函数
         * */
        protected PListDict(SerializationInfo info, StreamingContext contex)
            : base(info, contex) { }
        #endregion

        /*
         * @ 实现父类的方法
         * */
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /**
         * @ 索引，区分 Hashtable 和 IList 对象
         * @ key 如果当前对象的值是 LdfHashtable 对象，则 key 应当为 hash key
         * @ 如果当前对象值类型为 IList，则自动将 key 转换成下标
         * */
        public new IPListNode this[string key]
        {
            get
            {
                IPListNode ht = null;
                LdfValueType lvt = PListFactory.GetValueType(this.objValue);
                if (lvt == LdfValueType.ARRAY && key.IsInt())
                {
                    IList list = this.objValue as IList;
                    ht = list[key.ObjToInt()] as IPListNode;
                }
                else
                    ht = base[key];

                return ht;
            }
            set
            {
                value.Order = this.Count + 1;
                base[key] = value;
            }
        }

        /**
         * @ 从序列化的 xml 文件加载 PList 对象
         * */
        public void FromFile(string file)
        {
            if (File.Exists(file) == false)
                return;

            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                FromStream(stream);
            }
        }

        /**
        * @ 从序列化的字符串加载 PList 对象
        * */
        public void FromString(string xml)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                FromStream(ms);
            }
        }

        /**
        * @ 从序列化的字节流加载 PList 对象
        * */
        public void FromStream(Stream stream)
        {
            XDocument doc = XDocument.Load(stream);
            XElement dict = doc.Root.Element("dict");
            ReaderXml(dict);
        }

        /**
         * @ 将 LdfHashtable 转换成字符串
         * */
        public string ToPListString()
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
         * @ 将当前对象以 xml 形式写入 stream
         * @ stream 流对象
         * */
        private void WriterToStream(Stream stream)
        {
            XmlTextWriter xmlwriter = new XmlTextWriter(stream, Encoding.UTF8);
            xmlwriter.Formatting = Formatting.Indented;
            xmlwriter.WriteStartDocument();
            xmlwriter.WriteDocType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
            xmlwriter.WriteStartElement("plist");
            xmlwriter.WriteAttributeString("version", "1.0");
            xmlwriter.WriteStartElement("dict");
            this.WriterXml(xmlwriter);
            xmlwriter.WriteEndElement();
            xmlwriter.WriteEndElement();
            xmlwriter.WriteEndDocument();
            xmlwriter.Flush();
        }

        /**
         * @ 将序列化的 LdfHashtable 对象以 xml 的形式写入文件
         * @ fileName 文件名称
         * */
        public void ToXmlFile(string file)
        {
            ToPListString();
            using (FileStream stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                WriterToStream(stream);
                stream.Flush();
            }
        }

        /**
         * @ 实现 IPListNode 接口
         * */
        public void WriterXml(XmlWriter writer)
        {
            foreach (var item in this)
            {
                PListFactory.WriteElementKey(writer, item.Key);
                LdfValueType lvt;
                bool hasChildren = item.Value.HasChildren;
                object objValue = null;
                if (hasChildren)
                {
                    lvt = PListFactory.GetValueType(item.Value);
                    objValue = item.Value;
                }
                else
                {
                    lvt = PListFactory.GetValueType(item.Value.Value);
                    objValue = item.Value.Value;
                }

                string keyName = lvt.ToString().ToLower();

                if (lvt == (lvt & (LdfValueType.DICT | LdfValueType.ARRAY)))
                {
                    writer.WriteStartElement(keyName);
                    ((IPListNode)objValue).WriterXml(writer);
                    writer.WriteEndElement();
                }
                else
                    PListFactory.FormatXml(writer, item.Value);
            }
        }

        /**
         * @ 实现 IPListNode 接口
         * */
        public void ReaderXml(XElement reader)
        {
            if (reader.IsEmpty)
                return;
            this.tag = reader.Name.LocalName;
            IEnumerable<XNode> nodes = reader.Nodes();
            IEnumerator<XNode> xnodes = nodes.GetEnumerator();
            while (xnodes.MoveNext())
            {
                XElement n = (XElement)xnodes.Current;
                xnodes.MoveNext();
                XElement nextNode = (XElement)n.NextNode;
                if (nextNode == null)
                    continue;
                IPListNode val = PListFactory.ParseNode(nextNode);
                val.Order = this.Count;
                val.Tag = n.Value;
                this.Add(n.Value, val);
                this.Value = val;
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
        private void Dispose(bool disposing)
        {
            if (disposing)
                return;

            this.Clear();
            disposing = true;
        }

        #region Properties
        private int order = 0;
        /**
         * @ 排序号
         * */
        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        private object objValue = string.Empty;
        /**
         * @ 包装的值
         * */
        public Object Value
        {
            get { return objValue; }
            set { objValue = value; }
        }

        private string tag = string.Empty;
        /**
         * @ 标签名称
         * */
        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        /**
         * @ 实现接口
         * */
        public bool HasChildren
        {
            get
            {
                return this.Count > 0;
            }
        }
        #endregion
    }
}

