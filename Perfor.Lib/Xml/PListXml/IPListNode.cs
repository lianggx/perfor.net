using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Perfor.Lib.Xml.PListXml
{
    public interface IPListNode : IDisposable
    {
        /**
         * @ 实现 
         * */
        void WriterXml(XmlWriter writer);

        /**
         * @ 实现 
         * */
        void ReaderXml(XElement reader);

        /**
         * @ 将IPListNode对象转换为JSON字符串
         * */
        void WriterJson(TextWriter writer);

        /**
         * @ 将JSON转换为IPListNode对象
         * */
        void ReaderJson(JToken token);

        /**
         * @ 将对象写入 xmlWriter 流中
         * @ writer 对象
         * */
        void ToXmlFile(string file);

        /**
         * @ 从 XElement 读取元素
         * @ reader XElement 对象
         * */
        void FromXmlFile(string file);

        /**
         * @ 将IPListNode对象转换为JSON字符串
         * */
        string ToJson();

        /**
         * @ 将JSON转换为IPListNode对象
         * */
        IPListNode FromJson(string json);

        /**
         * @ 序列化为plist字符串
         * */
        string ToXmlString();

        /**
         * @ 自定义实现索引
         * */
        IPListNode this[string key] { get; set; }

        /**
         * @ 从plist字符串反序列化
         * */
        void FromXmlString(string plist);

        #region Properties
        /**
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
