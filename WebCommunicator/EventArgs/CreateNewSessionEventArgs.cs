using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator.EventArgs
{
    public class CreateNewSessionEventArgs<TContext> : System.EventArgs
        where TContext : TransmissionContext, new()
    {
        public TContext Context { get; private set; }
        private string newSessionKey;
        private bool? sessionCreated;

        private string errorMessage = null;

        public CreateNewSessionEventArgs(TContext context)
        {
            this.Context = context;
            this.newSessionKey = null;
            this.sessionCreated = null;
        }

        public string NewSessionKey
        {
            get
            {
                return newSessionKey;
            }
            set
            {
                if (newSessionKey != null)
                    throw new Exceptions.ProgrammingException("New Session Key already set.");
                newSessionKey = value;
            }
        }

        public bool? SessionCreated
        {
            get
            {
                return sessionCreated;
            }
            set
            {
                if (sessionCreated != null)
                    throw new Exceptions.ProgrammingException("Session Created already set.");
                sessionCreated = value;
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
