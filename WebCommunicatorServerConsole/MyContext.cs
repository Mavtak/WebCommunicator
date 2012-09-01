using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebCommunicator;

namespace WebCommunicatorServerConsole
{
    public class MyContext : TransmissionContext
    {
        public string User { get; set; }
        public string Session { get; set; }
    }
}
