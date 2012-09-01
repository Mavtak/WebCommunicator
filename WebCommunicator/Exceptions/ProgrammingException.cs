using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator.Exceptions
{
    internal class ProgrammingException : CommunicationException
    {
        public ProgrammingException(string message)
            : base("Programming Exception: " + message)
        { }
    }
}
