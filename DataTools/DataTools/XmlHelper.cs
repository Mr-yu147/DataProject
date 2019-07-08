using System;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Schema;

namespace DataTools
{
    // 学习【.net中读写config文件各种方法】博客
    // http://www.cnblogs.com/fish-li/archive/2011/12/18/2292037.html
    /// <summary>
    /// V1.0
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// XML序列化
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="o">被序列化数据</param>
        /// <param name="encoding">格式</param>
        private static void XmlSerializeInternal(Stream stream, object o, Encoding encoding)
        {
            if (o == null)
                throw new ArgumentNullException("o");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            //System.Runtime.Serialization.DataContractSerializer dataContract = new System.Runtime.Serialization.DataContractSerializer(o.GetType());
            XmlSerializer serializer = new XmlSerializer(o.GetType());

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\r\n";
            settings.Encoding = encoding;
            settings.IndentChars = "    ";
            //settings.NewLineHandling = NewLineHandling.Replace;
            //settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
            settings.NewLineOnAttributes = true;
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, o);
                writer.Close();
            }
        }

        /// <summary>
        /// 将一个对象序列化为XML字符串
        /// </summary>
        /// <param name="o">要序列化的对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>序列化产生的XML字符串</returns>
        public static string XmlSerialize(object o, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializeInternal(stream, o, encoding);

                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 将一个对象按XML序列化的方式写入到一个文件
        /// </summary>
        /// <param name="o">要序列化的对象</param>
        /// <param name="path">保存文件路径</param>
        /// <param name="encoding">编码方式</param>
        public static void XmlSerializeToFile(object o, string path, Encoding encoding)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializeInternal(file, o, encoding);
            }
        }

        /// <summary>
        /// 从XML字符串中反序列化对象
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="s">包含对象的XML字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserialize<T>(string s, Encoding encoding)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(s)))
            {
                using (StreamReader sr = new StreamReader(ms, encoding))
                {
                    return (T)mySerializer.Deserialize(sr);
                }
            }
        }

        /// <summary>
        /// 读入一个文件，并按XML的方式反序列化对象。
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserializeFromFile<T>(string path, Encoding encoding)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            string xml = File.ReadAllText(path, encoding);
            return XmlDeserialize<T>(xml, encoding);
        }
    }
}

/// <summary>
/// SerialDictionary(支持 XML 序列化)V1.0
/// </summary>
/// <typeparam name="TKey">键类型</typeparam>
/// <typeparam name="TValue">值类型</typeparam>
[XmlRoot("SerialDictionary")]
[Serializable]
public class SerialDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
    #region 构造函数
    public SerialDictionary()
    { }

    public SerialDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
    { }

    public SerialDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
    { }

    public SerialDictionary(int capacity) : base(capacity)
    { }

    public SerialDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
    { }

    protected SerialDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
    { }
    #endregion 构造函数

    #region IXML序列化成员
    public XmlSchema GetSchema() { return null; }

    /// <summary>
    /// 从对象的 XML 表示形式生成该对象(反序列化)
    /// </summary>
    /// <param name="xr"></param>
    public void ReadXml(XmlReader xr)
    {
        if (xr.IsEmptyElement)
            return;
        var ks = new XmlSerializer(typeof(TKey));
        var vs = new XmlSerializer(typeof(TValue));
        xr.Read();
        while (xr.NodeType != XmlNodeType.EndElement)
        {
            xr.ReadStartElement("Item");
            xr.ReadStartElement("Key");
            var key = (TKey)ks.Deserialize(xr);
            xr.ReadEndElement();
            xr.ReadStartElement("Value");
            var value = (TValue)vs.Deserialize(xr);
            xr.ReadEndElement();
            Add(key, value);
            xr.ReadEndElement();
            xr.MoveToContent();
        }
        xr.ReadEndElement();
    }

    /// <summary>
    ///  将对象转换为其 XML 表示形式(序列化)
    /// </summary>
    /// <param name="xw"></param>
    public void WriteXml(XmlWriter xw)
    {
        var ks = new XmlSerializer(typeof(TKey));
        var vs = new XmlSerializer(typeof(TValue));
        foreach (var key in Keys)
        {
            xw.WriteStartElement("Item");
            xw.WriteStartElement("Key");
            ks.Serialize(xw, key);
            xw.WriteEndElement();
            xw.WriteStartElement("Value");
            vs.Serialize(xw, this[key]);
            xw.WriteEndElement();
            xw.WriteEndElement();
        }
    }
    #endregion IXmlSerializable Members
}
