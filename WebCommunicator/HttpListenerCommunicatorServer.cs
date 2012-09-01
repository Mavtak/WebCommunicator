using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;

namespace WebCommunicator
{
    //helpful http://bartdesmet.net/blogs/bart/archive/2007/02/22/httplistener-for-dummies-a-simple-http-request-reflector.aspx
    public class HttpListenerCommunicatorServer<TContext> : CommunicatorServer<TContext>
        where TContext : TransmissionContext, new()
    {
        System.Net.HttpListener listener;

        public HttpListenerCommunicatorServer(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + port + "/");
        }

        public void Start()
        {
            try
            {
                listener.Start();
            }
            catch (HttpListenerException exception)
            {
                throw new Exceptions.CommunicationException("Error starting server: " + exception.Message, exception);
            }

            while (true)
            {
                HttpListenerContext context = listener.GetContext();

                HttpListenerRequest request = context.Request;

                Dictionary<string, string> inHeaders = new Dictionary<string, string>();
                foreach (string key in request.QueryString.Keys)
                    inHeaders.Add(key, request.QueryString[key]);
                MemoryStream inStream = new MemoryStream();
                StreamUtilities.CopyStream(request.InputStream, inStream, false);

                MemoryStream outStream = new MemoryStream();

                ProcessRawMessage(inHeaders, inStream, outStream);

                HttpListenerResponse response = context.Response;

                StreamUtilities.CopyStream(outStream, response.OutputStream);

                response.Close();

            }
        }
    }
}
