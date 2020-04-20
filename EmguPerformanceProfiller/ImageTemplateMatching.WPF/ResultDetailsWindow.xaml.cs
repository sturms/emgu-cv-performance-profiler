namespace ImageTemplateMatching.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using ConsoleApplication4;

    public partial class ResultDetailsWindow : Window
    {
        private readonly Dictionary<string, string> _largeImageFilePaths;
        private readonly ImageLogic _imageLogic;

        public ResultDetailsWindow(ResultDetails details)
            : this()
        {
            _largeImageFilePaths = new Dictionary<string, string>();
            _largeImageFilePaths.Add(Path.GetFileName(details.RectLargeImagePath), details.RectLargeImagePath);
            if (details.ImageResizedTimes > 0)
            {
                _largeImageFilePaths.Add(Path.GetFileName(details.ResizedRectLargeImagePath), details.ResizedRectLargeImagePath);
            }

            this.cmbxResultImages.ItemsSource = _largeImageFilePaths.Select(x => x.Key).ToList();
            this.SetWindowFieldsData(details);
        }

        public ResultDetailsWindow()
        {
            InitializeComponent();
            _imageLogic = new ImageLogic();
        }

        private void cmbxResultImages_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedImagePath = _largeImageFilePaths.First(x => x.Key == this.cmbxResultImages.SelectedValue.ToString()).Value;
            if (File.Exists(selectedImagePath))
            {
                this.imgResult.Source = this.BitmapFromUri(new Uri(selectedImagePath));
            }
        }

        private void SetWindowFieldsData(ResultDetails details)
        {
            // Basic info
            this.txtbTimeEllapsed.Text = details.EllapsedTime.ToString();
            this.txtbSearchesPerformed.Text = details.ImageSearchTimes.ToString();
            this.txtbRanAsync.Text = details.IsSynchronousOperationPerformed ? "No" : "Yes";
            this.txtbImageScaledDownTimes.Text = details.ImageResizedTimes.ToString();

            Rectangle smallImageRect = details.IsSynchronousOperationPerformed
                ? details.SyncImageSearchResults.First()
                : details.AsyncImageSearchResults.Select(x => x.Value).First();

            var originalSmallImageRect = new Rectangle();

            // Resized image info
            if (details.ImageResizedTimes > 0)
            {
                int scaleRatio = (details.ImageResizedTimes + 1);
                this.grdResizedImages.Visibility = Visibility.Visible;
                this.txtbResizedLargeImageSize.Text = $"W: {(int)(details.OriginalLargeImageWidth / scaleRatio)}, H: {(int)(details.OriginalLargeImageHeight / scaleRatio)}";
                this.txtbResizedSmallImageSize.Text = $"W: {smallImageRect.Width}, H: {smallImageRect.Height}";
                this.txtbResizedSmallImagePos.Text = $"X: {smallImageRect.X}, Y: {smallImageRect.Y}";

                originalSmallImageRect = _imageLogic.CalculateImageOriginalSizeAndPosition(smallImageRect, details.ImageResizedTimes);
            }

            // Original image info
            this.txtbOriginalLargeImageSize.Text = $"W: {details.OriginalLargeImageWidth}, H: {details.OriginalLargeImageHeight}";
            this.txtbOriginalSmallImageSize.Text = $"W: {originalSmallImageRect.Width}, H: {originalSmallImageRect.Height}";
            this.txtbOriginalSmallImagePos.Text = $"X: {originalSmallImageRect.X}, Y: {originalSmallImageRect.Y}";
        }

        private ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }
    }
}
