using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;

namespace WebCommunicator
{
    public class MessagePayload : List<XElement>
    {
        public MessagePayload()
        { }

        
        public void Add(XmlNode node)
        {
            Add(node.GetXElement());
        }

        public List<XmlNode> GetXmlNodes()
        {
            var result = new List<XmlNode>(this.Count);

            foreach(var element in this)
                result.Add(element.GetXmlNode());

            return result;
        }
    }
}
