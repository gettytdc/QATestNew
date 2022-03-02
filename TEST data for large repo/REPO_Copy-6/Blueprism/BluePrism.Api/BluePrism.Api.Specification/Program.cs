namespace BluePrism.Api.Specification
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;

    public class Program
    {
        private static void Main()
        {
            var listener = new HttpListener();

            var address = StartListenerOnRandomPort(listener);
            Process.Start("cmd", $"/c start {address}");

            while (true)
            {
                HandleNextRequest(listener);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void HandleNextRequest(HttpListener listener)
        {
            try
            {
                var context = listener.GetContext();

                void Send404Response()
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                }

                if (context.Request.HttpMethod != "GET")
                {
                    Send404Response();
                    return;
                }

                var prefix = listener.Prefixes.FirstOrDefault(context.Request.Url.AbsoluteUri.StartsWith);

                if (prefix == null)
                {
                    Send404Response();
                    return;
                }

                var relativeUri = context.Request.Url.AbsoluteUri.Substring(prefix.Length).Trim('/');

                byte[] responseBuffer;

                switch (relativeUri)
                {
                    case "":
                    case "index.html":
                        responseBuffer = GetEmbeddedFile("index.html");
                        break;

                    case "api.yaml":
                        responseBuffer = GetEmbeddedFile("api.yaml");
                        break;

                    default:
                        Send404Response();
                        return;
                }

                context.Response.ContentLength64 = responseBuffer.LongLength;
                context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
                context.Response.StatusCode = 200;

                context.Response.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static byte[] GetEmbeddedFile(string fileName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"BluePrism.Api.Specification.{fileName}") ?? throw new Exception($"Cannot find expected embedded file: {fileName}"))
            using (var reader = new StreamReader(stream))
            {
                return Encoding.UTF8.GetBytes(reader.ReadToEnd());
            }
        }

        private static string StartListenerOnRandomPort(HttpListener listener)
        {
            string address;
            var retryCount = 0;
            var random = new Random();

            do
            {
                var port = random.Next(49152, 65536);
                address = $"http://localhost:{port}/";
                listener.Prefixes.Clear();
                listener.Prefixes.Add(address);

                retryCount++;
                if (retryCount > 5)
                    throw new Exception("Unable to start web server");

            } while (!TryStartListener(listener));

            return address;
        }

        private static bool TryStartListener(HttpListener listener)
        {
            try
            {
                listener.Start();
                return true;
            }
            catch (HttpListenerException)
            {
                return false;
            }
        }
    }
}
