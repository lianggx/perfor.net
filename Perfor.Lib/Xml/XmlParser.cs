using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Perfor.Lib.Xml
{
    /**
     * @ xml 序列化类
     * */
    public sealed class XmlParser
    {
        #region Identity
        /**
         * @ 私有构造函数，成员都是 static ，无需初始化
         * */
        private XmlParser()
        {
        }
        #endregion

        /**
         * @ 检查 xml 对象的声明是否符合规范
         * */
        internal static void CheckXmlDeclaration(XmlDocument xmlDocument)
        {
            if (xmlDocument == null)
            {
                throw new InvalidOperationException(string.Format("无法使用空对象创建XML声明,请检查", new object[0]));
            }
            XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration(_version, _encoding, _standalone);
            if (string.IsNullOrEmpty(xmlDocument.FirstChild.Value))
            {
                xmlDocument.InsertBefore(newChild, xmlDocument.DocumentElement);
            }
            else
            {
                xmlDocument.ReplaceChild(newChild, xmlDocument.FirstChild);
            }
        }

        /**
         * @ 将文本转换成 XmlDocument 对象
         * */
        public static XmlDocument ConverStringToXML(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new Exception(string.Format("无法将空字符串序列化为XML文档，请检查", new object[0]));
            }
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(text);
            CheckXmlDeclaration(xmlDocument);
            return xmlDocument;
        }

        /**
         * @ 将 xml 文档转换成指定 T 类型对象
         * */
        public static T ConverXMLToObject<T>(XmlDocument xmlDocument) where T : class
        {
            if (xmlDocument == null)
            {
                throw new InvalidOperationException(string.Format("无法将空的XML文档反序列化为：{0}类型,造成异常的原因可能是因为:XML文档未正确生成，请检查", typeof(T)));
            }
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReader xmlReader = XmlReader.Create(new StringReader(xmlDocument.OuterXml));
            return (T)serializer.Deserialize(xmlReader);
        }

        /**
         * @ 将 xml 文档转换成文本形式
         * */
        public static string ConverXMLToString(XmlDocument xmlDocument)
        {
            if (xmlDocument == null)
            {
                return "";
            }
            StringWriter writer = new StringWriter();
            xmlDocument.Save(writer);
            return writer.ToString();
        }

        /**
         * @ 将指定 T 类型对象转换成 xml 文档对象
         * */
        public static XmlDocument CreateDocument<T>(T obj) where T : class
        {
            if (obj == null)
            {
                throw new InvalidOperationException(string.Format("序列化对象：{0}类型 的过程中，发生了异常,造成异常的原因可能是因为类型：{1} 的对象为空，请检查", typeof(T), typeof(T)));
            }
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream inStream = new MemoryStream();

            serializer.Serialize((Stream)inStream, obj);
            inStream.Position = 0;
            xmlDocument.Load(inStream);
            CheckXmlDeclaration(xmlDocument);
            return xmlDocument;
        }

        #region Properties
        private static string _encoding = "utf-8";
        /**
         * @ 获取或者设置 xml 文档的编码
         * */
        public static string Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
            }
        }

        private static string _standalone = "yes";
        /**
         * @ 获取或者设置 xml 标准
         * */
        public static string Standalone
        {
            get
            {
                return _standalone;
            }
            set
            {
                if ((value.ToLower() != "yes") && (value.ToLower() != "no"))
                {
                    throw new InvalidOperationException("值只能为yes或no");
                }
                _standalone = value;
            }
        }

        private static string _version = "1.0";
        /**
         * @ 获取或者设置 xml 版本号
         * */
        public static string Version
        {
            get
            {
                return _version;
            }
            set
            {
                if (Convert.ToDouble(value) < 0.0)
                {
                    throw new InvalidOperationException("值必须设置为数字");
                }
                _version = value;
            }
        }
        #endregion
    }
}
