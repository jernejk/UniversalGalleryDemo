using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniversalGalleryDemo.Core.Providers;
using Windows.ApplicationModel;
using Windows.Storage;

namespace UniversalGalleryDemo.Tests
{
    [TestClass]
    public class FourWalledProviderTest
    {
        [TestMethod]
        public void TestEmptyRequest()
        {
            FourWalledProvider parser = new FourWalledProvider();

            List<string> list = parser.ParseImages(string.Empty);

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count == 0);
        }

        [TestMethod]
        public async Task TestIndexRequest()
        {
            List<string> list = await GetResponseFor("nature.html");

            Assert.IsNotNull(list);
            Assert.AreEqual(30, list.Count);
            Assert.IsTrue(list.All(p => p.StartsWith("http")), "Not all URLs start with http!");
        }

        private async Task<List<string>> GetResponseFor(string fileName)
        {
            FourWalledProvider provider = new FourWalledProvider();

            StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("TestData");
            folder = await folder.GetFolderAsync("fourwalled");

            StorageFile file = await folder.GetFileAsync(fileName);
            string html = await FileIO.ReadTextAsync(file);

            return provider.ParseImages(html);
        }
    }
}
