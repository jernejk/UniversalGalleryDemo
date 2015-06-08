using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Linq;
using System.Threading.Tasks;
using UniversalGalleryDemo.Core.Models;
using UniversalGalleryDemo.Core.Providers;
using Windows.ApplicationModel;
using Windows.Storage;

namespace UniversalGalleryDemo.Tests
{
    /// <summary>
    /// Test if provider works for used scenarios.
    /// </summary>
    [TestClass]
    public class WallDashProviderTest
    {
        [TestMethod]
        public void TestEmptyRequest()
        {
            WallDashProvider parser = new WallDashProvider();

            var response = parser.ParseImages(string.Empty);

            Assert.IsNull(response);
        }

        [TestMethod]
        public async Task TestIndexRequest()
        {
            PictureResponse response = await GetResponseFor("get_images_index.json");

            CheckForValidResponse(response);
        }

        [TestMethod]
        public async Task TestIndexRequestWithWarnings()
        {
            // Request: http://walldash.net/backend/main_page/get_images.php?resolution=all&sort_by=r&also_find=l&color=+&show_nsfw=false&show_sketchy=false&show_sfw=true&tags=&username=&password=&session_id=&imgs_to_show=16&imgs_shown=0&_=1432239046438
            PictureResponse response = await GetResponseFor("get_images_index_with_warnings.json");

            CheckForValidResponse(response);
        }

        [TestMethod]
        public async Task IntegrationTest()
        {
            IImageProvider provider = new WallDashProvider();
            var images = await provider.GetImages(string.Empty, "random", 0, string.Empty);

            Assert.IsNotNull(images);
            Assert.AreEqual(images.Count, 16);
            Assert.IsTrue(images.All(img => img.StartsWith("http://") && img.EndsWith(".jpg")));
        }

        [TestMethod]
        public async Task IntegrationTest2()
        {
            IImageProvider provider = new WallDashProvider();
            var images = await provider.GetImages(string.Empty, "views", 16, "imagesPerPage=24");

            Assert.IsNotNull(images);
            Assert.AreEqual(images.Count, 24);
            Assert.IsTrue(images.All(img => img.StartsWith("http://") && img.EndsWith(".jpg")));
        }

        private static void CheckForValidResponse(PictureResponse response)
        {
            Assert.IsNotNull(response);
            Assert.AreEqual(response.data.Length, 16);
            Assert.IsTrue(response.data.All(p => !p.favorited));
            Assert.IsTrue(response.data.All(p => { int t; return int.TryParse(p.id, out t); }));
        }

        private async Task<PictureResponse> GetResponseFor(string fileName)
        {
            WallDashProvider provider = new WallDashProvider();

            StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("TestData");
            folder = await folder.GetFolderAsync("walldash");

            StorageFile file = await folder.GetFileAsync(fileName);
            string json = await FileIO.ReadTextAsync(file);

            // Request: http://walldash.net/backend/main_page/get_images.php?resolution=all&sort_by=r&also_find=l&color=+&show_nsfw=false&show_sketchy=false&show_sfw=true&tags=&username=&password=&session_id=7991034147-213.157.236.119&imgs_to_show=16&imgs_shown=0&_=1432239046438
            return provider.ParseImages(json);
        }
    }
}
