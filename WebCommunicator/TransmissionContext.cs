using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator
{
    public class TransmissionContext
    {
        //TODO: ensure that Request and Response are not reassigned
        public Message Request { get; set; }
        public Message Response { get; set; }
    }
}
