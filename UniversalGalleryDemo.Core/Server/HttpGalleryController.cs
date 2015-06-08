using System;
using System.Net;

namespace UniversalGalleryDemo.Core.Server
{
    public class HttpGalleryController
    {
        private HttpServer server;

        public event EventHandler<HttpGalleryRequest> GalleryRequest;

        public void StartServer(int port)
        {
            server = new HttpServer(port);
            server.OnRequest += Server_OnRequest;
        }

        public void StopServer()
        {
            server.OnRequest -= Server_OnRequest;
            server.Dispose();
        }

        private string Server_OnRequest(string path)
        {
            string html = "<h1>Slide show control panel</h1><br />";
            html += "<form action = \"index.html\">";
            html += "    Query: <input type=\"text\" name=\"query\" /><br />";
            html += "    Sort: <input type=\"text\" name=\"sort\" /><br />";
            html += "    Additional: <input type=\"text\" name=\"additional\" /><br />";
            html += "    Delay (in ms): <input type=\"number\" name=\"delay\" value=\"15000\" /><br />";
            html += "    <input type=\"submit\" value=\"Submit\"><br /><br />";
            html += "</form>";
            
            if (path.Contains("?"))
            {
                HttpGalleryRequest request = new HttpGalleryRequest();

                string changes = string.Empty;

                string query = path.Substring(path.IndexOf("?") + 1);
                string[] args = query.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var argument in args)
                {
                    string[] data = argument.Split('=');
                    if (data.Length != 2)
                    {
                        continue;
                    }

                    string key = data[0];
                    string value = WebUtility.UrlDecode(data[1]);

                    switch (key.ToLowerInvariant())
                    {
                        case "order":
                        case "sort":
                            changes += $" - Order by {value}<br />";
                            request.Sort = value;
                            break;

                        case "query":
                        case "q":
                            changes += $" - Query by {value}<br />";
                            request.Query = value;
                            break;

                        case "delay":
                            int delay;
                            if (int.TryParse(value, out delay))
                            {
                                request.DelayInMilliseconds = delay;
                                changes += $" - Delayed for {value}<br />";
                            }
                            break;

                        case "additional":
                            request.Additional = value;
                            changes += $" - Additional data for {value}<br />";
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(changes))
                {
                    html += $"<p>Changes:<br />{changes}</p>";

                    GalleryRequest?.Invoke(this, request);
                }
            }

            return $"<html><body>{html}</body></html>";
        }
    }
}
