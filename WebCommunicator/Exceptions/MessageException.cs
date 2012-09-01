using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator.Exceptions
{
    public class MessageException : System.Exception
    {
        public MessageException()
            : base()
        { }
        public MessageException(string message)
        { }
        public MessageException(string message, Exception innerException)
        { }
    }
}
