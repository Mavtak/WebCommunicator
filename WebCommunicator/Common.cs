using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Security.Cryptography;

namespace WebCommunicator
{
    public static class Common
    {
        public static Version LibraryVersion
        {
            get
            {
                return InternalLibraryVersion.GetLibraryVersion();
            }
        }

        public static Message ToMessage(this MemoryStream inStream, Aes aes = null)
        {
            if (inStream.Length == 0)
                throw new Exceptions.CommunicationException("read stream blank!");

            MemoryStream plainStream;

            inStream.Seek(0, SeekOrigin.Begin);

            //TODO: clean this up
            string beforeGreaterThan = StreamUtilities.ReadUntilChar(inStream, '<', 5, true);
            string beforeExclaimation = StreamUtilities.ReadUntilChar(inStream, '!', 5, true);

            bool streamEncrypted;
            if (beforeGreaterThan != null && beforeExclaimation == null)
                streamEncrypted = false;
            else if (beforeGreaterThan == null && beforeExclaimation != null)
                streamEncrypted = true;
            else if (beforeGreaterThan.Length < beforeExclaimation.Length)
                streamEncrypted = false;
            else
                streamEncrypted = true;

            if (streamEncrypted)
            {

                if (aes == null)
                    throw new Exceptions.CommunicationException("oops!  Got an encrypted response, but don't know the key.");

                

                string numBytesForIvString = StreamUtilities.ReadUntilChar(inStream, '!', 10, false);

                //int bleh = StreamUtilities.PeekByte(inStream);
                //string test2 = StreamUtilities.MemoryStreamToString(inStream);

                int numBytesForIv;
                try
                {
                    numBytesForIv = Convert.ToInt32(numBytesForIvString);
                }
                catch
                {
                    throw new Exceptions.CommunicationException("Error parsing the number of bytes for the Initialization Vector");
                }

                byte[] iv = new byte[numBytesForIv];
                int numRead = inStream.Read(iv, 0, numBytesForIv);

                if (numRead != numBytesForIv)
                    throw new Exceptions.CommunicationException("Error reading the initialization vector.");

                aes.IV = iv;

                try
                {
                    plainStream = StreamUtilities.RunCryptoTransform(inStream, aes.CreateDecryptor(), false);

                    plainStream.Seek(0, SeekOrigin.Begin);
                    //string beforeGreaterThan2 = StreamUtilities.ReadUntilChar(inStream, 'x', 50, true);
                    //string test3 = StreamUtilities.MemoryStreamToString(plainStream);
                    //inStream.Seek(0, SeekOrigin.Begin);
                }
                catch (CryptographicException exception)
                {
                    throw new Exceptions.CommunicationException("Error decrypting response.", exception);
                }
            }
            else
            {
                plainStream = inStream;
            }

            Message result = new Message(plainStream);

            if (result.controlValues.Count == 0 && result.Values.Count == 0 && result.Payload.Count == 0)
                throw new Exceptions.CommunicationException("Blank response.");

            if (!String.IsNullOrEmpty(result.CommunicationErrorMessage))
                throw new Exceptions.CommunicationException(result.CommunicationErrorMessage);

            return result;
        }

        public static void WriteMessageStream(MemoryStream outStream, Message outMessage, string sessionKey, Aes aes, bool encrypt)
        {
            outMessage.ReplaceControlValue(Message.TimestampControlValueName, DateTime.Now.ToUniversalTime().ToString());
            outMessage.ReplaceControlValue(Message.SessionKeyControlValueName, sessionKey);
            if (encrypt)
            {
                aes.GenerateIV();

                StreamUtilities.WriteStringToMemoryStream(outStream, aes.IV.Length + "!");
                outStream.Write(aes.IV, 0, aes.IV.Length);
                StreamUtilities.RunCryptoTransform(outMessage.Serialize(), aes.CreateEncryptor(), outStream, true);
            }
            else
            {
                outMessage.Serialize(outStream);
            }
        }
    }
}
