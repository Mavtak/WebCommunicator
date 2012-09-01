    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using System.IO;

namespace WebCommunicator
{
    public class AspNetCommunicatorServer<TContext> : CommunicatorServer<TContext>
        where TContext : TransmissionContext, new()
    {
        public AspNetCommunicatorServer()
        { }

        public void ProcessRequest(HttpContext httpContext)
        {
            HttpRequest request = httpContext.Request;
            HttpResponse response = httpContext.Response;


            var inHeaders = new Dictionary<string, string>();
            foreach (string key in request.QueryString.Keys)
                inHeaders.Add(key, request.QueryString[key]);

            var inStream = new MemoryStream();
            StreamUtilities.CopyStream(request.InputStream, inStream, false);

            var outStream = new MemoryStream();

            ProcessRawMessage(inHeaders, inStream, outStream);

            StreamUtilities.CopyStream(outStream, response.OutputStream);

            response.Flush();
        }

        public void RespondWithScreamingError(HttpContext httpContext, string error)
        {
            var response = httpContext.Response;

            var outStream = new MemoryStream();
            RespondWithScreamingError(outStream, error);
            
            StreamUtilities.CopyStream(outStream, response.OutputStream);

            response.Flush();
        }
    }
}
