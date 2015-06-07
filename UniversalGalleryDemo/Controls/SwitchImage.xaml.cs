using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace UniversalGalleryDemo.Controls
{
    /// <summary>
    /// This control is used to eliminate flickering between changing images.
    /// On weaker devices like Raspberry Pi 2 loading a new image can take more than 0.5 seconds!
    /// </summary>
    public sealed partial class SwitchImage : UserControl
    {
        public static readonly DependencyProperty CurrentImageProperty =
            DependencyProperty.Register("CurrentImage", typeof(string), typeof(SwitchImage), new PropertyMetadata(null, OnCurrentImageChanged));

        public SwitchImage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets current image URL.
        /// Old image will be replaced with current one when current image is loaded.
        /// </summary>
        public string CurrentImage
        {
            get { return (string)GetValue(CurrentImageProperty); }
            set { SetValue(CurrentImageProperty, value); }
        }

        /// <summary>
        /// Keep old image until new image is loaded.
        /// </summary>
        public async void ChangeImage()
        {
            if (CurrentImage == null)
            {
                return;
            }
            
            BitmapImage bi = new BitmapImage();

            // Check if URL is local or from other source (http://, \\, ftp://, etc.)
            if (CurrentImage.IndexOf(":\\") == 1 || CurrentImage.StartsWith("file://"))
            {
                var file = await StorageFile.GetFileFromPathAsync(CurrentImage);
                await bi.SetSourceAsync(await file.OpenReadAsync());
            }
            else
            {
                bi.UriSource = new Uri(CurrentImage);
            }

            img.Source = bi;
        }

        /// <summary>
        /// Listens for current image URL.
        /// </summary>
        /// <param name="d">Switch image control</param>
        /// <param name="e">Which dependency object has changed</param>
        private static void OnCurrentImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Trigger change image
            ((SwitchImage)d).ChangeImage();
        }
    }
}
