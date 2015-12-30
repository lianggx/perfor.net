using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Danny.Lib.Xml.PListXml
{
    public partial class PListArray : List<IPListNode>, IPListNode
    {
        #region Identity
        ~PListArray()
        {
            Dispose(false);
        }

        public PListArray() { }
        #endregion

        /**
         * @ 实现 IPListNode 接口
         * */
        public void WriterXml(XmlWriter writer)
        {
            foreach (var item in this)
            {
                LdfValueType lvt = PListFactory.GetValueType(item.Value);
                string keyName = lvt.ToString().ToLower();

                if (lvt == (lvt & (LdfValueType.DICT | LdfValueType.ARRAY)))
                {
                    writer.WriteStartElement(keyName);
                    IPListNode node = (IPListNode)item;
                    node.WriterXml(writer);
                    writer.WriteEndElement();
                }
                else
                    PListFactory.FormatXml(writer, item);
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
            IEnumerable<XNode> ns = reader.Nodes();
            foreach (var item in ns)
            {
                XElement node = (XElement)item;
                if (node == null)
                    continue;
                IPListNode val = PListFactory.ParseNode(node);
                val.Tag = node.Name.LocalName;
                val.Order = this.Count;
                base.Add(val);
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
        private string tag = string.Empty;
        /**
         * @ 标签名称
         * */
        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        private int order = 0;
        /**
         * @ 排序号
         * */
        public int Order
        {
            get { return order; }
            set { order = value; }
        }


        private object htValue = string.Empty;
        /**
         * @ 包装的值
         * */
        public Object Value
        {
            get { return this; }
            set { htValue = value; }
        }

        /**
         * @ 实现 IPListNode 接口
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
