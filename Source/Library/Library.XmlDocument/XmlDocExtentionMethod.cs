﻿using System;
using System.Xml;

namespace BlackPearl.Library.Extentions
{
    public static class XmlDocExtentionMethod
    {
        public static XmlDocument CreateChildNode(this XmlDocument doc, string name, XmlNode parent, out XmlElement newElement)
        {
            var c = doc.CreateNode(XmlNodeType.Element, name, "") as XmlElement;
            parent.AppendChild(c);
            newElement = c;
            return doc;
        }

        public static XmlDocument CreateChildNode(this XmlDocument doc, string name, string value, XmlNode parent, out XmlElement xmlElement)
        {
            doc.CreateChildNode(name, parent, out XmlElement c);
            c.InnerText = value;
            xmlElement = c;
            return doc;
        }

        public static XmlDocument CreateChildNodeWithAttribute(this XmlDocument doc, string name, XmlNode parent, out XmlElement newElement, params ValueTuple<string, string>[] attributes)
        {
            doc.CreateChildNode(name, parent, out XmlElement c);
            foreach (ValueTuple<string, string> att in attributes)
            {
                c.SetAttribute(att.Item1, att.Item2);
            }
            newElement = c;
            return doc;
        }
    }
}
