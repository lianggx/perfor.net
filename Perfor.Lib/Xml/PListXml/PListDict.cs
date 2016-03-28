using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

using Perfor.Lib.Extension;
using System.Xml;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Perfor.Lib.Common;

namespace Perfor.Lib.Xml.PListXml
{
    /**
     * @ Apple plist 文件读取管理
     * */
    [Serializable]
    public class PListDict : PListCollection, IDictionary<string, IPListNode>
    {
        #region Identity
        private Dictionary<string, IPListNode> properties = new Dictionary<string, IPListNode>();
        /**
         * @ 默认构造方法
         * */
        public PListDict()
        {
            this.htValue = this;
        }

        /**
     * @ 构造函数第三次重载
     * @ htValue 值
     * @ order 排序号
     * */
        public PListDict(object htValue)
        {
            this.htValue = htValue;
        }

        /**
         * @ 构造函数第四次重载
         * @ htValue 值
         * @ order 排序号
         * */
        public PListDict(object objValue, int order)
        {
            this.htValue = objValue;
            this.order = order;
        }

        #endregion

        #region Self
        /**
         * @ 重写父类的方法
         * */
        public override void WriterXml(XmlWriter writer)
        {
            foreach (var item in properties)
            {
                WriteElementKey(writer, item.Key);
                NodeValueType valueType;
                object objValue;
                GetValueType(item, out valueType, out objValue);

                string keyName = valueType.ToString().ToLower();

                if (valueType == (valueType & (NodeValueType.DICT | NodeValueType.ARRAY)))
                {
                    writer.WriteStartElement(keyName);
                    ((IPListNode)objValue).WriterXml(writer);
                    writer.WriteEndElement();
                }
                else
                    FormatXml(writer, item.Value);
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
            IEnumerable<XNode> nodes = reader.Nodes();
            IEnumerator<XNode> xnodes = nodes.GetEnumerator();
            while (xnodes.MoveNext())
            {
                XElement n = (XElement)xnodes.Current;
                xnodes.MoveNext();
                XElement nextNode = (XElement)n.NextNode;
                if (nextNode == null)
                    continue;
                IPListNode val = ParseNode(nextNode);
                val.Order = Count;
                val.Tag = n.Value;
                this.Add(n.Value, val);
                this.Value = val;
            }
        }

        /**
         * @ 重写父类的方法
         * */
        public override void WriterJson(TextWriter writer)
        {
            writer.Write(Utilities.JSON_BRACES_LEFT);
            int len = Count;
            int index = 0;
            foreach (var item in this)
            {
                NodeValueType valueType;
                object objValue;
                GetValueType(item, out valueType, out objValue);
                if (valueType == (valueType & (NodeValueType.DICT | NodeValueType.ARRAY)))
                {
                    writer.Write(string.Format("{0}{1}{0}{2}", Utilities.JSON_QUOTES, item.Key, Utilities.JSON_COLON));
                    ((IPListNode)objValue).WriterJson(writer);
                    string comma = Utilities.IsWriterComma(len, index, 1);
                    writer.Write(comma);
                }
                else
                {
                    FormatJson(writer, item.Key, item.Value);
                    string comma = Utilities.IsWriterComma(len, index, 1);
                    writer.Write(comma);
                }
                index++;
            }
            writer.Write(Utilities.JSON_BRACES_RIGHT);
        }


        /**
        * @ 获取节点的值类型
        **/
        private void GetValueType(KeyValuePair<string, IPListNode> item, out NodeValueType valueType, out object objValue)
        {
            bool hasChildren = item.Value.HasChildren;
            objValue = null;
            if (hasChildren)
            {
                valueType = GetValueType(item.Value);
                objValue = item.Value;
            }
            else
            {
                valueType = GetValueType(item.Value.Value);
                objValue = item.Value.Value;
            }
        }

        /**
         * @ 重写父类的方法
         * */
        public override void ReaderJson(JToken token)
        {
            IEnumerable<JToken> tokens = token.Values();
            foreach (var item in tokens)
            {
                IPListNode node = SwitchJToken(item);
                this.Add(node.Tag, node);
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
         * @ 重写属性
         * */
        public override bool HasChildren
        {
            get
            {
                return Count > 0;
            }
        }

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
                IPListNode plistNode = null;
                NodeValueType valueType = GetValueType(this.Value);
                if (valueType == NodeValueType.ARRAY && key.IsInt())
                {
                    IList list = this.Value as IList;
                    plistNode = list[key.ObjToInt()] as IPListNode;
                }
                else
                    plistNode = properties[key];

                return plistNode;
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
            return properties.Contains(f => f.Key == item.Key && f.Value == item.Value);
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

