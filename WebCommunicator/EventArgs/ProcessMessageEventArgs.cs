using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator.EventArgs
{
    public class ProcessMessageEventArgs<TContext> : System.EventArgs
        where TContext : TransmissionContext, new()
    {
        public DateTime? Timestamp { get; private set; }
        public TimeSpan? TransitTime { get; private set; }
        public TContext Context { get; private set; }

        //TODO: more message stats for consumption by the using code

        public ProcessMessageEventArgs(Message request, DateTime? timestamp, TimeSpan? transitTime, TContext context, Message response)
        {
            this.Context = context;

            this.Context.Request = request;
            this.Context.Response = response;

            this.Timestamp = timestamp;
            this.TransitTime = transitTime;
        }
    }
}
