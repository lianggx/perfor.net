using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Danny.Lib.Xml.PListXml
{
    public interface IPListNode : IDisposable
    {
        /**
         * @ 将对象写入 xmlWriter 流中
         * @ writer 对象
         * */
        void WriterXml(XmlWriter writer);

        /***
         * @ 从 XElement 读取元素
         * @ reader XElement 对象
         * */
        void ReaderXml(XElement reader);

        #region Properties
        /***
         * @ 是否存在子元素
         * */
        bool HasChildren { get; }

        /**
         * @ 排序号
         * */
        int Order { get; set; }
        /**
         * @ 包装的值
         * */
        Object Value { get; set; }

        /*
         * @ 标签名称
         * */
        string Tag { get; set; }

        #endregion
    }
}
