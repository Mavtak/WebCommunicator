using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator.EventArgs
{
    public class ValidateSessionEventArgs<TContext> : System.EventArgs
        where TContext : TransmissionContext, new()
    {
        public TContext Context { get; private set; }
        public string SessionKey { get; private set; }
        private bool? isValid;

        public ValidateSessionEventArgs(TContext context, string sessionKey)
        {
            this.Context = context;
            this.SessionKey = sessionKey;
            isValid = null;
        }

        public bool? IsValid
        {
            get
            {
                return isValid;
            }
            set
            {
                if (isValid != null)
                    throw new Exceptions.ProgrammingException("Is Valid already set.");
                isValid = value;
            }
        }
    }
}
