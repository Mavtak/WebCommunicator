using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Cryptography;
using System.IO;

namespace WebCommunicator
{
    public class CommunicatorClient
    {
        private string url;
        private string accessKey;
        private byte[] encryptionKey;
        private string sessionKey;

        private int defaultTries = 5;

        private DateTime? nextCalibrate;
        private TimeSpan calibrateFrequency = new TimeSpan(0, 15, 0);

        //TODO: fill this out!  It's so seasy!
        private TimeSpan adjustTimestamp = new TimeSpan();

        #region constructors
        public CommunicatorClient(string url, string accessKey, byte[] encryptionKey)
        {
            this.url = url;
            this.accessKey = accessKey;
            this.encryptionKey = encryptionKey;
            this.sessionKey = null;

            StreamUtilities.RunCryptoSanityCheck();
        }
        public CommunicatorClient(string url, string accessKey, string encryptionKey)
            : this(url, accessKey, Encoding.UTF8.GetBytes(encryptionKey))
        { }
        public CommunicatorClient(string url, string accessKey)
            : this(url, accessKey, (byte[])null)
        { }
        #endregion

        //private string timestampPrefix()
        //{
        //    return DateTime.Now.ToString("yyyyMMddHHmmssFFFFFFF");
        //}

        private Message sendMessage(Message request, bool encryptMessage)
        {
            lock (request)
            {
                //TODO: learn how to use compile flags
                //request.ToFile(timestampPrefix() + "_request.xml");

                //calibrate
                if (nextCalibrate == null || nextCalibrate < DateTime.Now)
                {
                    request.ReplaceControlValue(Message.LibraryVersionControlValueName, InternalLibraryVersion.GetLibraryVersion().ToString());

                    nextCalibrate = DateTime.Now.Add(calibrateFrequency);
                }

                System.Net.WebClient webClient = new System.Net.WebClient();

                if (accessKey != null)
                    webClient.QueryString[Message.AccessKeyHeaderName] = accessKey;

                //set encryption is using...
                Aes aes = null;
                if (encryptionKey != null)
                {
                    try
                    {
                        aes = StreamUtilities.GetAesCryptoTransform(encryptionKey, null);
                    }
                    catch (CryptographicException exception)
                    {
                        throw new Exceptions.CommunicationException("Encryption exception.  Is the encryption key the right length?", exception);
                    }
                }

                //message is all ready to go.
                MemoryStream outgoingStream = new MemoryStream();
                Common.WriteMessageStream(outgoingStream, request, sessionKey, aes, encryptMessage);


                //TODO: fix these last-minute converstions to/from arrays
                byte[] responseBytes;
                try
                {
                    responseBytes = webClient.UploadData(url, outgoingStream.ToArray());
                }
                catch (System.Net.WebException exception)
                {
                    throw new Exceptions.CommunicationException(exception.Message);
                }
                MemoryStream responseStream = StreamUtilities.ToMemoryStream(responseBytes);

                Message response = responseStream.ToMessage(aes);

                //update sessionKey
                if (response.controlValues.ContainsKey(Message.NewSessionKeyControlValueName))
                    sessionKey = response.controlValues[Message.NewSessionKeyControlValueName];

                //response.ToFile(timestampPrefix() + "_response.xml");
                return response;
            }
            
        }

        public Message SendMessage(Message outMessage, bool encryptMessage, int tries)
        {
            Message response = null;

            while (tries != 0)
            {
                try
                {
                    response = sendMessage(outMessage, encryptMessage);
                    if (!String.IsNullOrEmpty(response.CommunicationErrorMessage))
                        throw new Exceptions.CommunicationException(response.CommunicationErrorMessage);
                    return response;
                }
                catch (Exceptions.CommunicationException exception)
                {
                    tries--;
                    if (tries == 0)
                        throw exception;
                    response = null;
                }
            }
            
            return response;
        }

        public Message SendMessage(Message request, bool encryptMessage)
        {
            return SendMessage(request, encryptMessage, defaultTries);
        }

        public Message PingServer()
        {
            var pingMessage = Message.CreatePing();
            return sendMessage(pingMessage, true);
        }
    }
}
