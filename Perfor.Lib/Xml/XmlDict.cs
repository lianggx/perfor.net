using Perfor.Lib.Xml.PListXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Perfor.Lib.Xml
{
    /**
     * @ 常规 xml 文件转换到字典类，如果是 apple.plist.xml 文件，请使用 Perfor.Lib.Xml.PListXml.PListDict 类
     * @ 特别提示：请仔细阅读本类索引使用的注释
     * */
    [Serializable]
    public class XmlDict : PListCollection, IDictionary<string, IPListNode>
    {
        #region Identity
        private Dictionary<string, IPListNode> properties = new Dictionary<string, IPListNode>();
        public XmlDict() { }
        #endregion

        #region Self
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
                xd.Tag = key;
                xd.Order = Count;
                if (n.HasElements)
                {
                    xd.ReaderXml(n.Elements());
                }
                else
                    xd.Value = n.Value;

                if (this.ContainsKey(key))
                    key = string.Format("{0} {1}", key, Count);

                this.Add(key, xd);
            }
        }

        /**
         * @ 实现接口
         * @ reader 从 XElement 元素加载到字典中
         * */
        public override void ReaderXml(XElement reader)
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
        public override void WriterXml(XmlWriter writer)
        {
            writer.WriteStartElement(Tag);
            foreach (var key in this.Keys)
            {
                writer.WriteStartElement(key);
                object objVal = properties[key];
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
        public override void WriterJson(TextWriter writer)
        {

        }

        /**
         * @ 将JSON转换为IPListNode对象
         * */
        public override void ReaderJson(JToken token)
        {
        }

        /**
         * @ 清理托管资源
         * */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                return;

            properties.Clear();
            disposing = true;
        }
        #endregion

        #region Properties
        /**
         * @ 当前字典是否包含有子元素
         * */
        public override bool HasChildren { get { return Count > 0; } }

        /**
         * @ 获取当前包含IPListNode对象的容器
         * */
        public Dictionary<string, IPListNode> Items
        {
            get
            {
                return properties;
            }
        }

        #endregion

        #region 实现 IDictionary 接口

        public void Add(string key, IPListNode value)
        {
            properties.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return properties.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return properties.Keys; }
        }

        public bool Remove(string key)
        {
            return properties.Remove(key);
        }

        public bool TryGetValue(string key, out IPListNode value)
        {
            return properties.TryGetValue(key, out value);
        }

        public ICollection<IPListNode> Values
        {
            get { return properties.Values; }
        }

        /**
           * @ 索引，区分 Hashtable 和 IList 对象
           * @ key 如果当前对象的值是 LdfHashtable 对象，则 key 应当为 hash key
           * @ 如果当前对象值类型为 IList，则自动将 key 转换成下标
           * */
        public override IPListNode this[string key]
        {
            get
            {
                if (!this.ContainsKey(key))
                    return null;

                object objVal = properties[key];
                if (objVal == null)
                    return null;

                XmlDict xd = objVal as XmlDict;
                return xd;
            }
            set
            {
                value.Order = Count + 1;
                properties[key] = value;
            }
        }

        public void Add(KeyValuePair<string, IPListNode> item)
        {
            properties.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, IPListNode> item)
        {
            return properties.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IPListNode>[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < array.Length; i++)
            {
                KeyValuePair<string, IPListNode> item = array[i];
                properties.Add(item.Key, item.Value);
            }
        }

        public bool Remove(KeyValuePair<string, IPListNode> item)
        {
            bool remove = false;
            if (this.Contains(item))
            {
                properties.Remove(item.Key);
                remove = true;
            }

            return remove;
        }

        public IEnumerator<KeyValuePair<string, IPListNode>> GetEnumerator()
        {
            return properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return properties.GetEnumerator();
        }

        public void Clear()
        {
            properties.Clear();
        }

        public int Count
        {
            get { return properties.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion
    }
}
