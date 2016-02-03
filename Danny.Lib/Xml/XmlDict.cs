using Danny.Lib.Xml.PListXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization;

namespace Danny.Lib.Xml
{
    /**
     * @ 常规 xml 文件转换到字典类，如果是 apple.plist.xml 文件，请使用 Danny.Lib.Xml.PListXml.PListDict 类
     * @ 特别提示：请仔细阅读本类索引使用的注释
     * */
    [Serializable]
    public class XmlDict : Dictionary<string, object>, IPListNode
    {
        #region Identity
        ~XmlDict()
        {
            Dispose(false);
        }
        public XmlDict() { }
        protected XmlDict(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion

        /**
         * @ 加载 xml 文件
         * @ fileName 文件全路径
         * */
        public void Load(string fileName)
        {
            XDocument doc = XDocument.Load(fileName);
            if (doc == null && doc.Root == null || doc.Root.HasElements == false)
                return;
            IEnumerable<XElement> nodes = doc.Root.Elements();
            ReaderXml(nodes);
        }

        /**
         * @ 索引读取对象
         * @ 自动兼容常规 xml 重复元素的问题
         * @ 如果一个节点下存在多个相同名称的子元素，集合中的命名规则为：key index，如name 1,name 2,name 3
         * @ 使用索引获取子元素时，仅需要传入对应的下标即可如 XmlDict["name 1"]， XmlDict["name 2"]，
         * */
        public new XmlDict this[string key]
        {
            get
            {
                if (this.ContainsKey(key) == false)
                    throw new KeyNotFoundException(string.Format("元素 \"{0}\" 不包含Key：\"{1}\"", this.tag, key));

                object objVal = base[key];
                if (objVal == null)
                    return null;

                XmlDict xd = objVal as XmlDict;
                return xd;
            }
        }


        /**
         * @ 读取 xml 节点到字典
         * @ nodes xml节点集合
         * */
        private void ReaderXml(IEnumerable<XElement> nodes)
        {
            if (nodes.Count() == 0)
                return;
            foreach (var n in nodes)
            {
                string key = n.Name.LocalName.ToLower();
                XmlDict xd = new XmlDict();
                xd.tag = key;
                xd.order = this.Count;
                if (n.HasElements)
                {
                    xd.ReaderXml(n.Elements());
                }
                else
                    xd.Value = n.Value;

                if (this.ContainsKey(key))
                    key = string.Format("{0} {1}", key, this.Count);

                this.Add(key, xd);
            }
        }

        /**
         * @ 实现接口
         * @ reader 从 XElement 元素加载到字典中
         * */
        public void ReaderXml(XElement reader)
        {
            if (reader == null && reader.HasElements == false)
                return;
            IEnumerable<XElement> nodes = reader.Elements();
            ReaderXml(nodes);
        }

        /**
         * @ 使用提供的 XmlWriter 对象，将当前字典写入 XmlWriter 流中
         * @ writer 已初始化的 XmlWriter 对象
         * */
        public void WriterXml(XmlWriter writer)
        {
            writer.WriteStartElement(tag);
            foreach (var key in this.Keys)
            {
                writer.WriteStartElement(key);
                object objVal = base[key];
                XmlDict xd = objVal as XmlDict;
                if (xd.HasChildren)
                {
                    xd.WriterXml(writer);
                }
                else
                    writer.WriteString(xd.Value.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /**
          * @ 将IPListNode对象转换为JSON字符串
          * */
        public void WriterJson(TextWriter writer)
        {
        }

        /**
         * @ 将JSON转换为IPListNode对象
         * */
        public void ReaderJson(TextReader reader)
        {
        }

        /**
         * @ 将当前对象转换为 xml 的字符串表现形式
         * */
        public string ToXml()
        {
            string xml = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
                WriterXml(writer);
                writer.Flush();
                xml = Encoding.UTF8.GetString(ms.ToArray());
            }
            return xml;
        }

        /**
         * @ 实现父类的接口
         * */
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
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
        private string tag = string.Empty;
        /**
         * @ 标签名称
         * */
        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        private object xmlValue = string.Empty;
        /**
         * @ 标签包含的值
         * @ 如果存在子元素，则返回所有子元素的 xml 表现形式
         * */
        public object Value
        {
            get
            {
                if (this.HasChildren)
                {
                    return ToXml();
                }
                return xmlValue;
            }
            set { xmlValue = value; }
        }

        /**
         * @ 当前字典是否包含有子元素
         * */
        public bool HasChildren { get { return this.Count > 0; } }

        private int order = 0;
        /**
         * @ 当前字典在父容器中的排序号
         * */
        public int Order
        {
            get { return order; }
            set { order = value; }
        }
        #endregion
    }
}
