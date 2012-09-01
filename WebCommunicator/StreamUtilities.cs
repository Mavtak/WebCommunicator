using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Security.Cryptography;

namespace WebCommunicator
{
    public static class StreamUtilities
    {
        private const int bufferSize = 99999;

        public static void CopyStream(Stream source, Stream dest, bool rewindSource)
        {
            long restorePosition1 = -1;

            if (source.CanSeek)
            {

                restorePosition1 = source.Position;

                if (rewindSource)
                    source.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                if (rewindSource)
                    throw new Exception("Can't rewind source");
            }

            byte[] buffer = new byte[bufferSize];
            int readAmount;
            int readOffset = 0;
            while ((readAmount = source.Read(buffer, readOffset, bufferSize)) > 0)
            {
                dest.Write(buffer, 0, readAmount);
            }

            if (source.CanSeek)
                source.Seek(restorePosition1, SeekOrigin.Begin);
        }
        public static void CopyStream(Stream source, Stream dest)
        {
            CopyStream(source, dest, true);
        }

        public static int PeekByte(MemoryStream stream)
        {
            long position = stream.Position;
            byte[] result = new byte[1];
            if (stream.Read(result, 0, 1) != 1)
                throw new Exception("Could not peek.");

            stream.Seek(position, SeekOrigin.Begin);
            return result[0];
        }

        public static string ReadUntilChar(MemoryStream stream, char character, int max, bool rewind)
        {
            int numRead = 0;

            long savedPosition = -1;
            if (rewind)
                savedPosition = stream.Position;

            StringBuilder result = new StringBuilder();
            char current;
            while ((current = (char)(stream.ReadByte())) != character)
            {
                result.Append(current);
                numRead++;
                if (numRead == max)
                {
                    if (rewind)
                        stream.Seek(savedPosition, SeekOrigin.Begin);
                    return null;
                }

            }

            if (rewind)
                stream.Seek(savedPosition, SeekOrigin.Begin);

            return result.ToString();
        }

        public static Aes GetAesCryptoTransform(byte[] key, byte[] initalVector)
        {
            Aes result = AesManaged.Create();
            if (key != null)
                result.Key = key;
            else
                result.GenerateKey();
            //result.GenerateIV();
            return result;
        }

        public static void RunCryptoTransform(Stream input, ICryptoTransform cryptoTransform, MemoryStream result, bool rewindSource)
        {
            //input.Seek(0, SeekOrigin.Begin);

            CryptoStream cryptoStream = new CryptoStream(result, cryptoTransform, CryptoStreamMode.Write);

            CopyStream(input, cryptoStream, rewindSource);

            cryptoStream.FlushFinalBlock();
        }

        public static MemoryStream RunCryptoTransform(Stream input, ICryptoTransform cryptoTransform, bool rewindSource)
        {
            MemoryStream result = new MemoryStream((int)input.Length);//TODO: is this initial length right?
            RunCryptoTransform(input, cryptoTransform, result, rewindSource);
            return result;
        }
        public static MemoryStream RunCryptoTransform(Stream input, ICryptoTransform cryptoTransform)
        {
            return RunCryptoTransform(input, cryptoTransform, true);
        }

        public static byte[] StringToByteArray(string input)
        {
            return System.Text.Encoding.UTF8.GetBytes(input);
        }
        public static MemoryStream StringToMemoryStream(string input)
        {
            MemoryStream result = new MemoryStream();

            WriteStringToMemoryStream(result, input);

            return result;
        }

        public static void WriteStringToMemoryStream(MemoryStream stream, string input)
        {
            byte[] inputArray = StringToByteArray(input);
            stream.Write(inputArray, 0, inputArray.Length);
        }

        public static string MemoryStreamToBase64String(MemoryStream input)
        {
            return Convert.ToBase64String(input.ToArray());
        }
        public static string MemoryStreamToString(MemoryStream input)
        {
            return Encoding.UTF8.GetString(input.ToArray());
        }

        public static MemoryStream ToMemoryStream(byte[] input)
        {
            return new MemoryStream(input);
        }

        public static bool StreamsEqual(Stream stream1, Stream stream2)
        {
            if (stream1.Length != stream2.Length)
                return false;

            long restorePosition1 = stream1.Position;
            long restorePosition2 = stream2.Position;

            stream1.Seek(0, SeekOrigin.Begin);
            stream2.Seek(0, SeekOrigin.Begin);

            int byte1;

            while ((byte1 = stream1.ReadByte()) != -1)
            {
                if (byte1 != stream2.ReadByte())
                {
                    stream1.Seek(restorePosition1, SeekOrigin.Begin);
                    stream2.Seek(restorePosition2, SeekOrigin.Begin);
                    return false;
                }
            }

            stream1.Seek(restorePosition1, SeekOrigin.Begin);
            stream2.Seek(restorePosition2, SeekOrigin.Begin);

            return true;

        }

        //TODO: include this in an automated test
        public static void RunCryptoSanityCheck()
        {
            Stream inputData;
            Stream encryptedData;
            Stream decryptedData;

            try
            {
                inputData = StringToMemoryStream("Hello, world!  This is a test.");

                Aes transform = GetAesCryptoTransform(null, null);

                encryptedData = RunCryptoTransform(inputData, transform.CreateEncryptor());

                decryptedData = RunCryptoTransform(encryptedData, transform.CreateDecryptor());
            }
            catch (Exception exception)
            {
                throw new Exceptions.SanityCheckException("Unexpected error running sanity check.", exception);
            }

            if (!StreamsEqual(inputData, decryptedData))
                throw new Exceptions.SanityCheckException("Sanity check failed.");

        }
    }
}
