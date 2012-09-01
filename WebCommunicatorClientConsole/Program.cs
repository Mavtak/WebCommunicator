using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Security.Cryptography;

using WebCommunicator;

namespace WebCommunicatorConsole
{
    class Program
    {
        private static WebCommunicator.CommunicatorClient client;

        static void Main(string[] args)
        {
            byte[] lul = Encoding.UTF8.GetBytes("<");

            Console.WriteLine("I am the client!");

            WebCommunicator.StreamUtilities.RunCryptoSanityCheck();
            Console.WriteLine("\tpassed crypto sanity check.");

            byte[] key = StreamUtilities.StringToByteArray("0123456789abcdef");
            client = new CommunicatorClient("http://localhost:1337", "user_1_key", key);

            Message message1 = new Message();
            message1.Values.Add("Action", "Time?");
            send(message1);

            Message message2 = new Message();
            message2.Values.Add("Action", "Write");
            message2.Values.Add("Text", "Hello from over the network!");
            send(message2);


            Console.ReadKey();
        }

        private static void send(Message message)
        {
            Message response = null;
            try
            {
                Console.WriteLine();
                Console.WriteLine("==========");
                Console.WriteLine();
                Console.WriteLine("request: ");
                Console.WriteLine(message);
                response = client.SendMessage(message, true);
            }
            catch (WebCommunicator.Exceptions.CommunicationException exception)
            {
                Console.WriteLine("Communication Error: " + exception.Message);
            }

            if (response == null)
                return;

            Console.WriteLine();
            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());

            if (response.Values.ContainsKey("Return"))
            {
                Console.WriteLine("Server responded with the following message:");
                Console.WriteLine(response.Values["Return"]);
            }
        }

        static void writeStream(Stream stream)
        {
            lock (stream)
            {
                byte[] streamBuffer = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(streamBuffer, 0, (int)stream.Length);

                string streamString = Encoding.UTF8.GetString(streamBuffer);

                Console.WriteLine(streamString);
            }
        }
    }
}
