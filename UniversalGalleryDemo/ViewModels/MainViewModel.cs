using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UniversalGalleryDemo.Core.Providers;
using UniversalGalleryDemo.Core.Server;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace UniversalGalleryDemo.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient client = new HttpClient();
        private IImageProvider imageProvider;
        private StorageFolder cacheFolder;
        private string currentImageUrl;
        private int currentIndex;
        private HttpGalleryController httpController;

        private bool refresh;

        public MainViewModel()
        {
            Images = new List<string>();
            Query = string.Empty;
            Sort = "random";
            Additional = string.Empty;
            DelayUntilNextImageInMiliSeconds = 15000;

            if (IsInDesignMode)
            {
                // This data is for XAML designer
                CurrentImageUrl = "http://wallpapers.wallhaven.cc/wallpapers/full/wallhaven-116658.jpg";
                Query = "test query";
            }
            else
            {
                httpController = new HttpGalleryController();
                httpController.GalleryRequest += HttpController_GalleryRequest;
                httpController.StartServer(8000);

                LoadAsync();
            }
        }

        private void HttpController_GalleryRequest(object sender, HttpGalleryRequest e)
        {
            Query = e.Query ?? string.Empty;
            Sort = e.Sort ?? "random";
            Additional = e.Additional;

            if (e.DelayInMilliseconds > 0)
            {
                DelayUntilNextImageInMiliSeconds = e.DelayInMilliseconds;
            }

            refresh = true;
        }

        public string CurrentImageUrl
        {
            get { return currentImageUrl; }
            set { Set(ref currentImageUrl, value); }
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
            set { Set(ref currentIndex, value); }
        }

        public int DelayUntilNextImageInMiliSeconds { get; set; }

        public string Query { get; set; }
        public string Sort { get; set; }
        public string Additional { get; set; }
        public List<string> Images { get; set; }

        private async void LoadAsync()
        {
            CurrentIndex = 0;
            Images.Clear();

            cacheFolder = ApplicationData.Current.LocalCacheFolder;
            imageProvider = new FourWalledProvider();

            // Get first batch of images
            await LoadMoreAsync();

            // Start cycling through images
            CycleImagesAsync();
        }

        private async Task LoadMoreAsync(int startIndex = 0)
        {
            try
            {
                List<string> images = await imageProvider.GetImages(Query, Sort, startIndex, Additional);
                Images.AddRange(images);
            }
            catch (Exception exception)
            {
                // TODO: Notify user
            }
        }

        /// <summary>
        /// Cycle through images and request new pages when nearing the end.
        /// </summary>
        private async void CycleImagesAsync()
        {
            StorageFolder picturesFolders = await cacheFolder.CreateFolderAsync("pictures", CreationCollisionOption.OpenIfExists);
            picturesFolders = await picturesFolders.CreateFolderAsync(imageProvider.Id, CreationCollisionOption.OpenIfExists);

            int maxDelay = 0;
            while (true)
            {
                if (refresh)
                {
                    refresh = false;
                    LoadAsync();

                    return;
                }

                string img = Images[CurrentIndex];

                Stopwatch stopwatch = Stopwatch.StartNew();

                // Download and cache image. We should avoid downloading same images multiple times.
                try
                {
                    StorageFile file = await DownloadAndSaveImage(img, picturesFolders);
                    CurrentImageUrl = file.Path;
                }
                catch
                {
                    // TODO: Notify user?
                }

                if (CurrentIndex + 1 == Images.Count)
                {
                    // Request a new page right before last image.
                    await LoadMoreAsync(CurrentIndex + 1);
                }

                stopwatch.Stop();

                // Calculate for how long do we still need to delay next image.
                int delay = maxDelay - (int)stopwatch.ElapsedMilliseconds;
                if (delay > 0)
                {
                    await Task.Delay(delay);
                }

                maxDelay = DelayUntilNextImageInMiliSeconds;

                CurrentIndex = (++CurrentIndex) % Images.Count;
            }
        }

        private async Task<StorageFile> DownloadAndSaveImage(string url, StorageFolder folder)
        {
            // TODO: This may not work for all type of providers.
            string fileName;
            if (url.EndsWith(".jpg") || url.EndsWith(".png"))
            {
                int i = url.LastIndexOf("/");
                fileName = url.Substring(i + 1);
            }
            else
            {
                fileName = Guid.NewGuid() + ".jpg";
            }

            StorageFile pictureFile = await folder.TryGetItemAsync(fileName) as StorageFile;
            if (pictureFile == null)
            {
                // Download and cache image
                HttpResponseMessage response = await client.GetAsync(new Uri(url));
                IBuffer buffer = await response.Content.ReadAsBufferAsync();

                response.EnsureSuccessStatusCode();
                
                pictureFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBufferAsync(pictureFile, buffer);
            }

            return pictureFile;
        }
    }
}
