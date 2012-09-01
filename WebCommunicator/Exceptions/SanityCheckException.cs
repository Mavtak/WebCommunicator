using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator.Exceptions
{
    public class SanityCheckException : Exception
    {
        public SanityCheckException()
            : base()
        { }

        public SanityCheckException(string message)
            : base(message)
        { }

        public SanityCheckException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
