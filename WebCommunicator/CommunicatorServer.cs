using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WebCommunicator.EventArgs;
using System.Security.Cryptography;
using System.IO;

namespace WebCommunicator
{
    public class CommunicatorServer<TContext>
        where TContext : TransmissionContext, new()
    {
        private const int bufferSize = 512;

        public delegate void GetCredentialsEventHandler(object sender, EventArgs.GetCredentialsEventArgs<TContext> eventArgs);
        public delegate void ValidateSessionEventHandler(object sender, EventArgs.ValidateSessionEventArgs<TContext> eventArgs);
        public delegate void CreateNewSessionEventHandler(object sender, EventArgs.CreateNewSessionEventArgs<TContext> eventArgs);
        public delegate void ProcessMessageEventHandler(object sender, EventArgs.ProcessMessageEventArgs<TContext> eventArgs);

        public event GetCredentialsEventHandler GetCredentials;
        public event ValidateSessionEventHandler ValidateSession;
        public event CreateNewSessionEventHandler CreateNewSession;
        public event ProcessMessageEventHandler ProcessMessage;

        //TODO: Internal communication error
        

        public CommunicatorServer()
        {
            StreamUtilities.RunCryptoSanityCheck();
        }
        

        public void ProcessRawMessage(Dictionary<string, string> inHeaders, MemoryStream inStream, MemoryStream outStream)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            DateTime? timestamp = null;
            TimeSpan? transitTime = null;

            Aes aes = null;
            TContext context = new TContext ();
            string sessionKey = null;

            Message responseMessage = new Message();

            //first, process the access key to get user and encryption informaiton

            try
            {
                if (inHeaders.ContainsKey(Message.AccessKeyHeaderName))
                {
                    var getCredentialsArgs = new EventArgs.GetCredentialsEventArgs<TContext>(inHeaders[Message.AccessKeyHeaderName], context);
                    GetCredentials(this, getCredentialsArgs);

                    //the code using this library proceses the access key and sets the appropreate values
                    if (getCredentialsArgs.UserFound != true)
                    {
                        if (!String.IsNullOrEmpty(getCredentialsArgs.ErrorMessage))
                            throw new Exceptions.CommunicationException(getCredentialsArgs.ErrorMessage);
                        else
                            throw new Exceptions.CommunicationException("Could not find user");
                    }

                    context = getCredentialsArgs.Context;
                    byte[] encryptionKey = getCredentialsArgs.EncryptionKey;
                    aes = StreamUtilities.GetAesCryptoTransform(encryptionKey, null);
                }

                //Now context *might* be set.  Its value depends on the using code, and only makes sense to that code.

                //at any rate, we now have enough information to parse the message
                Message inMessage = inStream.ToMessage(aes);

                //respond to a simple ping
                if (inMessage.controlValues.ContainsKey(Message.ControlActionControlValueName)
                    && inMessage.controlValues[Message.ControlActionControlValueName].Equals(Message.PingActionControlValueName))
                {
                    responseMessage = new Message();
                    Common.WriteMessageStream(outStream, responseMessage, null, null, false);
                    return;
                }


                //now let's validate the session
                ValidateSessionEventArgs<TContext> validateSessionArgs = null;
                if (inMessage.controlValues.ContainsKey(Message.SessionKeyControlValueName))
                {
                    //remote client has asserted a session key

                    validateSessionArgs = new ValidateSessionEventArgs<TContext>(context, inMessage.controlValues[Message.SessionKeyControlValueName]);
                    ValidateSession(this, validateSessionArgs);

                    //the code using this library will verify the session and set values acordingly
                    if (validateSessionArgs.IsValid == true)
                    {
                        sessionKey = validateSessionArgs.SessionKey;
                    }
                }

                if (validateSessionArgs == null || validateSessionArgs.IsValid != true)
                {
                    //create new session if none exists or current is otherwise invalid

                    CreateNewSessionEventArgs<TContext> createNewSessionArgs = new CreateNewSessionEventArgs<TContext>(context);
                    CreateNewSession(this, createNewSessionArgs);

                    if (createNewSessionArgs.SessionCreated != true)
                    {
                        if (!String.IsNullOrEmpty(createNewSessionArgs.ErrorMessage))
                            throw new Exceptions.CommunicationException(createNewSessionArgs.ErrorMessage);
                        else
                            throw new Exceptions.CommunicationException("Could not create a session.");
                    }

                    //the coding using this library has created a new session

                    sessionKey = createNewSessionArgs.NewSessionKey;

                    //send the new key to the client
                    responseMessage.controlValues.Add(Message.NewSessionKeyControlValueName, sessionKey);
                }

                //Now sessionKey and sessionRecord *might* be set.  Their values depend on the using code, and only make sense to that code.

                //Now we can pull the timestamp out of the request message
                if (inMessage.controlValues.ContainsKey(Message.TimestampControlValueName))
                {
                    try
                    {
                        timestamp = Convert.ToDateTime(inMessage.controlValues[Message.TimestampControlValueName]);
                    }
                    catch
                    {
                        throw new Exceptions.CommunicationException("Could not parse timestamp.");
                    }

                    //also, we calculate the messages age to allow the using code to reject old messages, which may be malitious
                    transitTime = now.Subtract(timestamp.Value);
                }

                //The request message is now completely parsed.
                //Now we send all the data off to the using code for processing

                var processMessageArgs = new ProcessMessageEventArgs<TContext>(inMessage, timestamp, transitTime, context, responseMessage);
                ProcessMessage(this, processMessageArgs);
                
            }
            catch (Exceptions.CommunicationException communicationException)
            {
                aes = null; //disable encrypton on the response
                responseMessage.CommunicationErrorMessage = communicationException.Message;
            }

            //now we can prepare the outgoing message to be sent.
            Common.WriteMessageStream(outStream, responseMessage, sessionKey, aes, aes != null);
        }

        internal void RespondWithScreamingError(MemoryStream outStream, string error)
        {
            Message responseMessage = new Message();
            responseMessage.CommunicationErrorMessage = error;

            Common.WriteMessageStream(outStream, responseMessage, null, null, false);
        }
    }
}
