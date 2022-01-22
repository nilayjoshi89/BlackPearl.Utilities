using System;
using System.Xml;

namespace BlackPearl.Library.Extentions
{
    public static class XmlDocExtentionMethod
    {
        public static XmlElement AppendChild(this XmlDocument doc, string elementName)
        {
            var c = doc.CreateNode(XmlNodeType.Element, elementName, "") as XmlElement;
            doc.AppendChild(c);
            return c;
        }
        public static XmlElement CreateChildNode(this XmlNode parent, string name)
        {
            XmlDocument doc = parent.OwnerDocument;
            var c = doc.CreateNode(XmlNodeType.Element, name, "") as XmlElement;
            parent.AppendChild(c);
            return c;
        }
        public static XmlElement CreateChildNode(this XmlNode parent, string name, string value)
        {
            XmlElement c = parent.CreateChildNode(name);
            c.InnerText = value;
            return c;
        }
        public static XmlNode CreateChildNode(this XmlNode parent, string name, params ValueTuple<string, string>[] attributes)
        {
            XmlElement c = parent.CreateChildNode(name);
            foreach (ValueTuple<string, string> att in attributes)
            {
                c.SetAttribute(att.Item1, att.Item2);
            }
            return c;
        }
    }
}
