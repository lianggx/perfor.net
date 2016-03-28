using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Perfor.Lib.Extension;
using Perfor.Lib.Common;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Perfor.Lib.Xml.PListXml
{
    /**
     * @ plist格式的 array 包装类
     * */
    [Serializable]
    public partial class PListArray : PListCollection, IList<IPListNode>
    {
        #region Identity
        private List<IPListNode> properties = new List<IPListNode>();
        #endregion

        #region Self
        /**
         * @ 实现 IPListNode 接口
         * */
        public override void WriterXml(XmlWriter writer)
        {
            foreach (var item in properties)
            {
                NodeValueType valueType = GetValueType(item.Value);
                string keyName = valueType.ToString().ToLower();

                if (valueType == (valueType & (NodeValueType.DICT | NodeValueType.ARRAY)))
                {
                    writer.WriteStartElement(keyName);
                    IPListNode node = (IPListNode)item;
                    node.WriterXml(writer);
                    writer.WriteEndElement();
                }
                else
                    FormatXml(writer, item);
            }
        }

        /**
         * @ 实现 IPListNode 接口
         * */
        public override void ReaderXml(XElement reader)
        {
            if (reader.IsEmpty)
                return;
            this.Tag = reader.Name.LocalName;
            IEnumerable<XNode> ns = reader.Nodes();
            foreach (var item in ns)
            {
                XElement node = (XElement)item;
                if (node == null)
                    continue;
                IPListNode val = ParseNode(node);
                val.Tag = node.Name.LocalName;
                val.Order = Count;
                properties.Add(val);
                this.htValue = val;
            }
        }

        /**
          * @ 将IPListNode对象转换为JSON字符串
          * */
        public override void WriterJson(TextWriter writer)
        {
            writer.Write(Utilities.JSON_BRACKET_LEFT);
            int len = Count;
            for (int i = 0; i < len; i++)
            {
                var item = this[i];
                NodeValueType valueType = GetValueType(item.Value);
                string keyName = valueType.ToString().ToLower();

                if (valueType == (valueType & (NodeValueType.DICT | NodeValueType.ARRAY)))
                {
                    IPListNode node = (IPListNode)item;
                    node.WriterJson(writer);
                    string comma = Utilities.IsWriterComma(len, i, 1);
                    writer.Write(comma);
                }
                else
                {
                    FormatJson(writer, "", item);
                    string comma = Utilities.IsWriterComma(len, i, 1);
                    writer.Write(comma);
                }
            }
            writer.Write(Utilities.JSON_BRACKET_RIGHT);
        }

        /**
         * @ 将JSON转换为IPListNode对象
         * */
        public override void ReaderJson(JToken token)
        {
            IEnumerable<JToken> tokens = token.Values();
            foreach (var item in tokens)
            {
                IPListNode node = SwitchJToken(item);
                this.Add(node);
            }
        }

        /**
         * @ 清理托管资源
         * */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                return;

            Clear();
            disposing = true;
        }
        #endregion

        #region Properties
        /**
         * @ 重写父类属性
         * */
        public override bool HasChildren
        {
            get { return properties.Count > 0; }
        }

        /**
         * @ 获取当前包含IPListNode对象的容器
         * */
        public List<IPListNode> List
        {
            get { return properties; }
        }
        #endregion

        #region 实现 IList 接口
        public int IndexOf(IPListNode item)
        {
            return properties.IndexOf(item);
        }

        public void Insert(int index, IPListNode item)
        {
            properties.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            properties.RemoveAt(index);
        }

        public IPListNode this[int index]
        {
            get
            {
                return properties[index];
            }
            set { this.Insert(index, value); }
        }

        public override IPListNode this[string key]
        {
            get
            {
                IPListNode node = properties.FirstOrDefault(f => f.Tag == key);
                return node;
            }
            set
            {
                IPListNode node = properties.FirstOrDefault(f => f.Tag == key);
                node.Value = value;
            }
        }

        public void Add(IPListNode item)
        {
            properties.Add(item);
        }

        public void Clear()
        {
            properties.Clear();
        }

        public bool Contains(IPListNode item)
        {
            return properties.Contains(item);
        }

        public void CopyTo(IPListNode[] array, int arrayIndex)
        {
            properties.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return properties.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IPListNode item)
        {
            return properties.Remove(item);
        }

        public IEnumerator<IPListNode> GetEnumerator()
        {
            return properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return properties.GetEnumerator();
        }
        #endregion
    }
}
