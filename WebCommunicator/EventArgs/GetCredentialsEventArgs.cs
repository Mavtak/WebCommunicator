using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator.EventArgs
{
    public class GetCredentialsEventArgs<TContext> : System.EventArgs
        where TContext : TransmissionContext, new()
    {
        public string AccessKey { get; private set; }
        public TContext Context { get; private set; }

        byte[] encryptionKey;

        bool? userFound;
        string errorMessage;

        public GetCredentialsEventArgs(string accessKey, TContext context)
        {
            this.AccessKey = accessKey;
            this.Context = context;
        }

        public byte[] EncryptionKey
        {
            get
            {
                return encryptionKey;
            }
            set
            {
                if (encryptionKey != null)
                    throw new Exceptions.ProgrammingException("Encryption Key already set.");
                encryptionKey = value;
            }
        }

        public bool? UserFound
        {
            get
            {
                return userFound;
            }
            set
            {
                if (userFound != null)
                    throw new Exceptions.ProgrammingException("User Found already set.");
                userFound = value;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                if (errorMessage != null)
                    throw new Exceptions.ProgrammingException("Error Message already set.");
                errorMessage = value;
            }
        }


    }
}
