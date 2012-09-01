using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;

namespace WebCommunicator
{
    internal static class Utilities
    {
        /// <summary>
        /// Pulled from http://blogs.msdn.com/b/ericwhite/archive/2008/12/22/convert-xelement-to-xmlnode-and-convert-xmlnode-to-xelement.aspx
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal static XElement GetXElement(this XmlNode node)
        {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
        }

        /// <summary>
        /// Pulled from http://blogs.msdn.com/b/ericwhite/archive/2008/12/22/convert-xelement-to-xmlnode-and-convert-xmlnode-to-xelement.aspx
        /// Modified:  changed `return xmlDoc;` to `return xmlDoc.LastChild;`.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static XmlNode GetXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc.LastChild;
            }
        }
    }
}
