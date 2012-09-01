using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator.Exceptions
{
    public class CommunicationException : System.Exception
    {
        public CommunicationException()
            : base()
        { }
        public CommunicationException(string message)
            : base(message)
        { }
        public CommunicationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
