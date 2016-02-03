using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Danny.Lib.Extension;
using Danny.Lib.Common;

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
                NodeValueType lvt = PListFactory.GetValueType(item.Value);
                string keyName = lvt.ToString().ToLower();

                if (lvt == (lvt & (NodeValueType.DICT | NodeValueType.ARRAY)))
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
          * @ 将IPListNode对象转换为JSON字符串
          * */
        public void WriterJson(TextWriter writer)
        {
            writer.Write(Utilities.JSON_BRACKET_LEFT);
            int len = this.Count();
            for (int i = 0; i < len; i++)
            {
                var item = this[i];
                NodeValueType lvt = PListFactory.GetValueType(item.Value);
                string keyName = lvt.ToString().ToLower();

                if (lvt == (lvt & (NodeValueType.DICT | NodeValueType.ARRAY)))
                {
                    IPListNode node = (IPListNode)item;
                    node.WriterJson(writer);
                    string comma = Utilities.IsWriterComma(len, i, 1);
                    writer.Write(comma);
                }
                else
                {
                    PListFactory.FormatJson(writer, "", item);
                    string comma = Utilities.IsWriterComma(len, i, 1);
                    writer.Write(comma);
                }
            }
            writer.Write(Utilities.JSON_BRACKET_RIGHT);
        }

        /**
         * @ 将 LdfHashtable 转换成JSON字符串
         * */
        public string ToJson()
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
         * @ 将JSON转换为IPListNode对象
         * */
        public void ReaderJson(TextReader reader)
        {
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
