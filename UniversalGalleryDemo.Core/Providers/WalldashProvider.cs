using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UniversalGalleryDemo.Core.Models;
using Windows.Foundation;
using Windows.Web.Http;

namespace UniversalGalleryDemo.Core.Providers
{
    public class WallDashProvider : IImageProvider
    {
        public async Task<List<string>> GetImages(string query, string sort, int startFrom, string additional)
        {
            if (string.IsNullOrWhiteSpace(additional))
            {
                // HACK: Because WwwFormUrlDecoder does not want empty strings...
                additional = "dummy=dummy";
            }
            
            WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(additional);

            sort = GetCorrectOrderBy(sort);

            // Additional configuration.
            int imagesPerPage = GetInteger(decoder, "imagesPerPage", 16);
            string resolution = GetValue(decoder, "resolution", "all");
            string alsoFind = GetValue(decoder, "also_find", "l");
            string color = GetValue(decoder, "color", "+");
            string showSfw = GetValue(decoder, "show_sfw", "true");
            string showSketchy = GetValue(decoder, "show_sketchy", "false");
            string showNsfw = GetValue(decoder, "show_nsfw", "false");
            string username = GetValue(decoder, "username", string.Empty);
            string password = GetValue(decoder, "password", string.Empty);
            string sessionId = GetValue(decoder, "session_id", string.Empty);
            string _ = GetValue(decoder, "_", "1432239046438");

            int startIndex = imagesPerPage * startFrom;
            
            query = WebUtility.UrlEncode(query);
            string url = "http://walldash.net/backend/main_page/get_images.php?";
            url += $"resolution={resolution}&sort_by={sort}&also_find={alsoFind}&";
            url += $"color={color}&show_nsfw={showNsfw}&show_sketchy={showSketchy}&";
            url += $"show_sfw={showSfw}&tags={query}&username={username}&";
            url += $"password={password}&session_id={sessionId}&imgs_to_show={imagesPerPage}&";
            url += $"imgs_shown={startIndex}&_={_}";

            HttpClient client = new HttpClient();
            string json = await client.GetStringAsync(new Uri(url));

            // Parse JSON
            PictureResponse images = ParseImages(json);

            // Get just links
            return images.data.Select(img => $"http://walldash.net/imgs/{img.id}.jpg").ToList();
        }

        public PictureResponse ParseImages(string content)
        {
            // Remove warnings at start of JSON due invalid session ID. (server is unable to log current request)
            if (content.IndexOf('{') != 0)
            {
                int i = content.IndexOf('{');

                if (i > 0)
                {
                    content = content.Substring(i);
                }
            }
            
            return JsonConvert.DeserializeObject<PictureResponse>(content);
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

        private string GetCorrectOrderBy(string originalSort)
        {
            string orderBy = originalSort.ToLowerInvariant();

            switch (orderBy)
            {
                case "views":
                case "numberOfViews":
                case "number_of_views":
                    // By number of views
                    orderBy = "v";
                    break;

                case "random":
                case "ran":
                    // By random
                    orderBy = "r";
                    break;

                case "latest":
                case "chrono":
                case "":
                    // By latest
                    orderBy = "c";
                    break;

                case "favorites":
                case "favs":
                case "fav":
                    // By number of favorites
                    orderBy = "f";
                    break;
            }

            return orderBy;
        }
    }
}
