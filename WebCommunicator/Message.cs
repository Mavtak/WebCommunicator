using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.IO;
using System.Xml.Linq;

namespace WebCommunicator
{
    public class Message
    {
        internal const string AccessKeyHeaderName = "AccessKey";

        internal const string TimestampControlValueName = "Timestamp";
        internal const string NewSessionKeyControlValueName = "NewSessionKey";
        internal const string SessionKeyControlValueName = "SessionKey";
        internal const string CommunicationErrorMessageControlValueName = "CommunicationErrorMessage";
        internal const string ErrorMessageControlValueName = "ErrorMessage";
        internal const string TimeSpanUntilRetryControlValueName = "TimeSpanUntilRetry";
        internal const string LibraryVersionControlValueName = "LibraryVersion";
        internal const string ControlActionControlValueName = "Action";
        internal const string PingActionControlValueName = "Ping";


        private const string messageNodeName = "Message";
        private const string valuesNodeName = "Values";
        private const string controlValuesNodeName = "ControlValues";
        private const string innerXmlNodeName = "Payload";
        

        internal Dictionary<string, string> controlValues;
        public Dictionary<string, string> Values { get; private set; }

        public MessagePayload Payload { get; private set; }

        public Message()
        {
            controlValues = new Dictionary<string, string>();
            Values = new Dictionary<string, string>();
            Payload = new MessagePayload();
        }

        public Message(Dictionary<string, string> values, MessagePayload payload)
        {
            this.controlValues = new Dictionary<string, string>();
            this.Values = values ?? (new Dictionary<string, string>());
            this.Payload = payload ?? (new MessagePayload());
        }

        public Message(Stream stream)
            : this()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(stream);
            readFromXml(xmlDoc.ChildNodes[xmlDoc.ChildNodes.Count - 1]);
        }


        public static Message CreatePing()
        {
            var result = new Message();
            result.ReplaceControlValue(ControlActionControlValueName, PingActionControlValueName);

            return result;
        }

        internal void ReplaceControlValue(string key, string value)
        {
            if (controlValues.ContainsKey(key))
            {
                if(value != null)
                    controlValues[key] = value;
                else
                    controlValues.Remove(key);
            }
            else
            {
                if(value != null)
                    controlValues.Add(key, value);
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (controlValues.ContainsKey(Message.ErrorMessageControlValueName))
                    return controlValues[Message.ErrorMessageControlValueName];
                return null;
            }
            set
            {
                this.ReplaceControlValue(Message.ErrorMessageControlValueName, value);
            }
        }
        internal string CommunicationErrorMessage
        {
            get
            {
                if (controlValues.ContainsKey(Message.CommunicationErrorMessageControlValueName))
                    return controlValues[Message.CommunicationErrorMessageControlValueName];
                return null;
            }
            set
            {
                this.ReplaceControlValue(Message.CommunicationErrorMessageControlValueName, value);
            }
        }

        #region write/restore code
        private void readFromXml(XmlNode messageNode)
        {
            //TODO: read into XElement structure?

            XmlNode valuesNode = messageNode.SelectSingleNode(Message.valuesNodeName);
            if (valuesNode != null)
                foreach (XmlAttribute valueAttribute in valuesNode.Attributes)
                    Values.Add(valueAttribute.Name, valueAttribute.Value);

            XmlNode controlValuesNode = messageNode.SelectSingleNode(Message.controlValuesNodeName);
            if (controlValuesNode != null)
                foreach (XmlAttribute controlValueAttribute in controlValuesNode.Attributes)
                    controlValues.Add(controlValueAttribute.Name, controlValueAttribute.Value);

            XmlNode xmlPayloadNode = messageNode.SelectSingleNode(Message.innerXmlNodeName);
            if (xmlPayloadNode != null)
            {
                foreach (XmlNode node in xmlPayloadNode.ChildNodes)
                {
                    Payload.Add(node);
                }
            }
        }

        private static XmlWriterSettings writerSettings
        {
            get
            {
                return new XmlWriterSettings
                    {
                        Encoding = Encoding.UTF8,
                        OmitXmlDeclaration = true,
                        Indent = false
                    };
            }
        }

        private void writeToXml(XmlWriter writer)
        {
            writer.WriteStartElement(messageNodeName);

            if (controlValues.Keys.Count > 0)
            {
                writer.WriteStartElement(controlValuesNodeName);
                foreach (string key in controlValues.Keys)
                    writer.WriteAttributeString(key, controlValues[key]);
                writer.WriteEndElement();
            }

            if (Values.Keys.Count > 0)
            {
                writer.WriteStartElement(valuesNodeName);
                foreach (string key in Values.Keys)
                    writer.WriteAttributeString(key, Values[key]);
                writer.WriteEndElement();
            }

            if (Payload.Count > 0)
            {
                writer.WriteStartElement(innerXmlNodeName);
                foreach (var node in Payload)
                    node.WriteTo(writer);
                writer.WriteEndElement();
            }


            writer.WriteEndElement();
        }
        public void Serialize(MemoryStream stream)
        {

            XmlWriter writer = XmlWriter.Create(stream, writerSettings);

            writeToXml(writer);

            writer.Close();
        }
        public MemoryStream Serialize()
        {
            MemoryStream result = new MemoryStream();

            Serialize(result);

            return result;

        }
        
        #endregion

        public void ToFile(string filename)
        {
            var writer = XmlWriter.Create(filename, writerSettings);
            writeToXml(writer);
            writer.Close();
        }

        public override string ToString()
        {
            XmlWriterSettings settings = writerSettings;
            settings.Indent = true;
            settings.NewLineChars = Environment.NewLine;

            var builder = new StringBuilder();
            var writer = XmlWriter.Create(builder, settings);
            writeToXml(writer);
            writer.Close();
            return builder.ToString();
        }

    }
}
