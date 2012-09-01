using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WebCommunicator;
using WebCommunicator.EventArgs;
using WebCommunicator.Exceptions;

namespace WebCommunicatorServerConsole
{
    class Program
    {
        private static List<string> users = new List<string>();
        private static Dictionary<string, string> accessKeysToUsers = new Dictionary<string,string>();
        private static int lastSession = 100;
        private static Dictionary<string, string> userSessions = new Dictionary<string,string>();
        private static Dictionary<string, byte[]> accessKeysToEncryptionKeys = new Dictionary<string, byte[]>();

        static void Main(string[] args)
        {
            Console.WriteLine("I am the server!");
            Console.WriteLine("loading simulated state");

            users.Add("user_1");
            users.Add("user_2");
            users.Add("user_3");

            accessKeysToUsers.Add("user_1_key", "user_1");
            accessKeysToUsers.Add("user_2_key", "user_2");

            accessKeysToEncryptionKeys.Add("user_1_key", StreamUtilities.StringToByteArray("0123456789abcdef"));

            userSessions.Add("my_user_1", "5");

            var server = new WebCommunicator.HttpListenerCommunicatorServer<MyContext>(1337);

            server.CreateNewSession += new CommunicatorServer<MyContext>.CreateNewSessionEventHandler(server_CreateNewSession);
            server.ValidateSession += new CommunicatorServer<MyContext>.ValidateSessionEventHandler(server_ValidateSession);
            server.GetCredentials += new CommunicatorServer<MyContext>.GetCredentialsEventHandler(server_GetCredentials);
            server.ProcessMessage += new CommunicatorServer<MyContext>.ProcessMessageEventHandler(server_ProcessMessage);

            try
            {
                server.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            Console.ReadKey();
        }

        static void server_GetCredentials(object sender, GetCredentialsEventArgs<MyContext> eventArgs)
        {
            if (!accessKeysToUsers.ContainsKey(eventArgs.AccessKey))
            {
                eventArgs.UserFound = false;
                eventArgs.ErrorMessage = "*Could not find user record";
                return;
            }

            if (!accessKeysToEncryptionKeys.ContainsKey(eventArgs.AccessKey))
            {
                eventArgs.UserFound = false;
                eventArgs.ErrorMessage = "*could not find encryption key";
                return;
            }

            eventArgs.Context.User = accessKeysToUsers[eventArgs.AccessKey];
            eventArgs.EncryptionKey = accessKeysToEncryptionKeys[eventArgs.AccessKey];
            eventArgs.UserFound = true;

        }
        
        static void server_CreateNewSession(object sender, CreateNewSessionEventArgs<MyContext> eventArgs)
        {
            if (!users.Contains(eventArgs.Context.User))
            {
                eventArgs.SessionCreated = false;
                return;
            }

            if (userSessions.ContainsKey(eventArgs.Context.User))
            {
                userSessions.Remove(eventArgs.Context.User);
            }

            eventArgs.NewSessionKey = (++lastSession).ToString();
            eventArgs.Context.Session = eventArgs.NewSessionKey + "_Record!"; //just to simulate the presense of metadata

            userSessions.Add(eventArgs.Context.User, eventArgs.NewSessionKey);

            eventArgs.SessionCreated = true;
        }

        static void server_ValidateSession(object sender, ValidateSessionEventArgs<MyContext> eventArgs)
        {
            if (!users.Contains(eventArgs.Context.User))
            {
                eventArgs.IsValid = false;
                return;
            }

            if (!userSessions.ContainsKey(eventArgs.Context.User))
            {
                eventArgs.IsValid = false;
                return;
            }

            eventArgs.IsValid = userSessions[eventArgs.Context.User].Equals(eventArgs.SessionKey);
            
        }


        static void server_ProcessMessage(object sender, ProcessMessageEventArgs<MyContext> eventArgs)
        {
            Message request = eventArgs.Context.Request;

            Console.WriteLine();
            Console.WriteLine("==========");
            Console.WriteLine();
            Console.WriteLine("request:");

            Console.WriteLine("Timestamp: " + eventArgs.Timestamp);
            Console.WriteLine("TransportTime: " + eventArgs.TransitTime);
            Console.WriteLine(request.ToString());
            Message response = eventArgs.Context.Response;

            if (request.Values.ContainsKey("Action"))
            {
                switch (request.Values["Action"])
                {
                    case "Time?":
                        response.Values.Add("Return", "My local time is " + DateTime.Now);
                        break;
                    case "Write":
                        if (request.Values.ContainsKey("Text"))
                            Console.WriteLine(request.Values["Text"]);
                        else
                            response.ErrorMessage = "Text not set!";
                        break;
                    default:
                        response.ErrorMessage = "Unknown action '" + request.Values["Action"];
                        break;

                }
            }
            else
                response.ErrorMessage = "I don't know what to do with this message!";

            Console.WriteLine();
            Console.WriteLine("Response:");
            Console.WriteLine(response);

            
        }
    }
}
