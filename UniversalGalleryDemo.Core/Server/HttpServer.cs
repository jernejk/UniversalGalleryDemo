using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace UniversalGalleryDemo.Core.Server
{
    /// <summary>
    /// Code from: http://www.dzhang.com/blog/2012/09/18/a-simple-in-process-http-server-for-windows-8-metro-apps
    /// Other stuff: https://code.msdn.microsoft.com/windowsapps/StreamSocket-Sample-8c573931
    /// </summary>
    public class HttpServer : IDisposable
    {
        private const uint BufferSize = 8192;
        private static readonly StorageFolder LocalFolder
                     = Windows.ApplicationModel.Package.Current.InstalledLocation;

        private readonly StreamSocketListener listener;

        public HttpServer(int port)
        {
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            this.listener.BindServiceNameAsync(port.ToString());
        }

        public event Func<string, string> OnRequest;

        public void Dispose()
        {
            this.listener.Dispose();
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            // this works for text only
            StringBuilder request = new StringBuilder();
            using (IInputStream input = socket.InputStream)
            {
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            using (IOutputStream output = socket.OutputStream)
            {
                string requestMethod = request.ToString().Split('\n')[0];
                string[] requestParts = requestMethod.Split(' ');

                if (requestParts[0] == "GET")
                    await WriteResponseAsync(requestParts[1], output);
                else
                    throw new InvalidDataException("HTTP method not supported: "
                                                   + requestParts[0]);
            }
        }

        private async Task WriteResponseAsync(string path, IOutputStream os)
        {
            using (Stream resp = os.AsStreamForWrite())
            {
                bool exists = true;
                try
                {
                    // Look in the Data subdirectory of the app package
                    string filePath = "Data" + path.Replace('/', '\\');

                    string html;
                    if (OnRequest != null)
                    {
                        html = OnRequest(path);
                    }
                    else
                    {
                        html = "<html><head><title>Hello world</title><body><h1>Hello world</h1><p>You wanted to access <b>" + path + "</b>.</p></body></html>";
                    }

                    byte[] data = Encoding.UTF8.GetBytes(html);

                    string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                        "Content-Length: {0}\r\n" +
                                        "Connection: close\r\n\r\n",
                                        data.Length);
                    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                    
                    resp.Write(data, 0, data.Length);

                    //using (Stream fs = await LocalFolder.OpenStreamForReadAsync(filePath))
                    //{

                    //    await fs.CopyToAsync(resp);
                    //}
                }
                catch (FileNotFoundException)
                {
                    exists = false;
                }

                if (!exists)
                {
                    byte[] headerArray = Encoding.UTF8.GetBytes(
                                          "HTTP/1.1 404 Not Found\r\n" +
                                          "Content-Length:0\r\n" +
                                          "Connection: close\r\n\r\n");
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                }

                await resp.FlushAsync();
            }
        }
    }
}
