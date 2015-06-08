using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;

namespace UniversalGalleryDemo.Core.Providers
{
    public class FourWalledProvider : IImageProvider
    {
        public string Id { get; } = "fourwalled";

        public async Task<List<string>> GetImages(string query, string sort, int startFrom, string additional)
        {
            if (string.IsNullOrWhiteSpace(additional))
            {
                // HACK: Because WwwFormUrlDecoder does not want empty strings...
                additional = "dummy=dummy";
            }

            WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(additional);

            if (sort == "r" || sort == "random" || sort == "rand")
            {
                sort = "random";
            }
            else
            {
                sort = "search";
            }

            // Additional configuration.
            int imagesPerPage = GetInteger(decoder, "imagesPerPage", 30);
            string board = GetValue(decoder, "board", string.Empty);
            string widthAspect = GetValue(decoder, "width_aspect", string.Empty);
            string sfw = GetValue(decoder, "sfw", "0");

            int startIndex = imagesPerPage * startFrom;

            query = WebUtility.UrlEncode(query);
            // &board=&width_aspect=&tags=nature&searchstyle=larger&sfw=0&offset=0
            string url = "http://4walled.cc/results.php?";
            url += $"board={board}&width_aspect={widthAspect}&tags={query}&";
            url += $"searchstyle=larger&sfw={sfw}&offset={startFrom}&search={sort}";

            HttpClient client = new HttpClient();
            string html = await client.GetStringAsync(new Uri(url));

            return ParseImages(html);
        }

        public List<string> ParseImages(string html)
        {
            List<string> images = new List<string>();

            if (string.IsNullOrWhiteSpace(html) || html.Length <= 10)
            {
                return images;
            }

            string startPart = "http://4walled.cc/thumb/";
            int startLenght = startPart.Length;

            string endPart = "'";
            int endLenght = endPart.Length;
            
            int i = 0;
            while (true)
            {
                i = html.IndexOf(startPart, i + 1);
                if (i < 0)
                {
                    break;
                }

                int j = html.IndexOf(endPart, i + startLenght);
                string thumbnailUrl = html.Substring(i, j - i);

                // This will not work if original is not .jpg
                images.Add(thumbnailUrl.Replace("/thumb/", "/src/"));

                i = j + 1;
            }

            return images;
        }

        private int GetInteger(WwwFormUrlDecoder decoder, string key, int defaultValue)
        {
            string value = GetValue(decoder, key);
            int temp;

            return int.TryParse(value, out temp) ? temp : defaultValue;
        }

        private string GetValue(WwwFormUrlDecoder decoder, string key, string defaultValue = null)
        {
            try
            {
                return decoder.GetFirstValueByName(key);
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }
    }
}
