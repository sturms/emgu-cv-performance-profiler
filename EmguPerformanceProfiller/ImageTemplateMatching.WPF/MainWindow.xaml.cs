namespace ImageTemplateMatching.WPF
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using ConsoleApp.ImageComputing.Common;
    using ConsoleApplication4;

    public partial class MainWindow : Window
    {
        private const string ResizedLargeImageNameWithoutExtension = "resized_main_image";
        private const string ResizedSmallImageNameWithoutExtension = "resized_small_image";
        private const string FileDialogExtensions = "PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
        private readonly ImageLogic _imageLogic;
        private readonly IImageComputingLogic _computingLogic;
        private CancellationTokenSource _tokenSource = null;
        private readonly List<Rectangle> _syncImageSearchResults;
        private readonly ConcurrentDictionary<int, Rectangle> _asyncImageSearchResults;
        private ResultDetails _resultDetails;

        public MainWindow()
        {
            InitializeComponent();
            _imageLogic = new ImageLogic();
            _computingLogic = new ImageComputingLogic();
            _syncImageSearchResults = new List<Rectangle>();
            _asyncImageSearchResults = new ConcurrentDictionary<int, Rectangle>();
        }

        private void btnChoseLargeImgDir_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.DefaultExt = ".png";
            ofd.Filter = FileDialogExtensions;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtLargeImagePath.Text = ofd.FileName;
            }
        }

        private void btnChoseSmallImgDir_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.DefaultExt = ".png";
            ofd.Filter = FileDialogExtensions;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtSmallImagePath.Text = ofd.FileName;
            }
        }

        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private async void btnSearchMatchingImages_Click(object sender, RoutedEventArgs e)
        {
            // No need to perform any other operations if image file paths are not given
            if (string.IsNullOrEmpty(this.txtLargeImagePath.Text)
                || string.IsNullOrEmpty(this.txtSmallImagePath.Text))
            {
                System.Windows.Forms.MessageBox.Show("All fields must be filled!");
                return;
            }

            int scaleDownTimes = (int)this.slValue.Value;
            string resizedLargeImagePath = this.txtLargeImagePath.Text.Replace(Path.GetFileNameWithoutExtension(this.txtLargeImagePath.Text), ResizedLargeImageNameWithoutExtension);
            string resizedSmallImagePath = this.txtSmallImagePath.Text.Replace(Path.GetFileNameWithoutExtension(this.txtSmallImagePath.Text), ResizedSmallImageNameWithoutExtension);

            // Clear previous data and files
            _syncImageSearchResults.Clear();
            _asyncImageSearchResults.Clear();
            _resultDetails = null;

            File.Delete(resizedLargeImagePath);
            File.Delete(resizedSmallImagePath);
            _imageLogic.DeletePreviousRectangleImages(Path.GetDirectoryName(this.txtLargeImagePath.Text));

            if (scaleDownTimes > 0)
            {
                _imageLogic.ResizeImageAndSave(this.txtLargeImagePath.Text, resizedLargeImagePath, scaleDownTimes, 90L);
                _imageLogic.ResizeImageAndSave(this.txtSmallImagePath.Text, resizedSmallImagePath, scaleDownTimes, 90L);
            }

            // Gets corresponding file path depending whether images has been resized or not
            string largeImgPath = scaleDownTimes > 0 ? resizedLargeImagePath : this.txtLargeImagePath.Text;
            string smallImgPath = scaleDownTimes > 0 ? resizedSmallImagePath : this.txtSmallImagePath.Text;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            _tokenSource = new CancellationTokenSource();
            var pOptions = new ParallelOptions();
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            pOptions.CancellationToken = _tokenSource.Token;
            int imageSearchTimes = (int)this.slImageSearchTimes.Value;
            bool isSynchronousOperation = this.rbtnRunSync.IsChecked == true;

            // Update the UI before search has been started
            this.txtStatus.Text = "In progress";
            this.btnCancelSearch.IsEnabled = true;
            this.btnSearchMatchingImages.IsEnabled = false;
            this.btnViewDetails.IsEnabled = false;

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    if (isSynchronousOperation)
                    {
                        for (int i = 0; i < imageSearchTimes; i++)
                        {
                            Rectangle rect = _computingLogic.GetImageParamsResult(largeImgPath, smallImgPath);
                            _syncImageSearchResults.Add(rect);
                        }
                    }
                    else
                    {
                        Parallel.For(0, imageSearchTimes, i =>
                        {
                            Rectangle rect = _computingLogic.GetImageParamsResult(largeImgPath, smallImgPath);
                            _asyncImageSearchResults.TryAdd(i, rect);

                            // The way to exit the parallel for-each loop
                            pOptions.CancellationToken.ThrowIfCancellationRequested();
                        });
                    }
                }, _tokenSource.Token)
               .ContinueWith((t) =>
               {
                   _tokenSource.Dispose();
                   _tokenSource = null;
                   watch.Stop();
                   long elapsedMs = watch.ElapsedMilliseconds;

                   // Save result images where red rectangle is drawn
                   string rectLargeImagePath = this.txtLargeImagePath.Text.Replace(Path.GetFileNameWithoutExtension(this.txtLargeImagePath.Text), "rect_" + Path.GetFileNameWithoutExtension(this.txtLargeImagePath.Text));
                   string resizedRectLargeImagePath = string.Empty;
                   if (scaleDownTimes > 0)
                   {
                       resizedRectLargeImagePath = resizedLargeImagePath.Replace(Path.GetFileNameWithoutExtension(resizedLargeImagePath), "rect_" + ResizedLargeImageNameWithoutExtension);
                       Rectangle resizedSmallImageRect = this.GetSingleImageSearchResult(isSynchronousOperation);

                       // We want to get the original function position and size on the screen.
                       // Functions are scaled down only to increase performance!
                       Rectangle originalImageRect = _imageLogic.CalculateImageOriginalSizeAndPosition(resizedSmallImageRect, scaleDownTimes);
                       _imageLogic.DrawRectangleOntoImageAndSave(this.txtLargeImagePath.Text, rectLargeImagePath, originalImageRect);

                       _imageLogic.DrawRectangleOntoImageAndSave(resizedLargeImagePath, resizedRectLargeImagePath, this.GetSingleImageSearchResult(isSynchronousOperation));
                   }
                   else
                   {
                       _imageLogic.DrawRectangleOntoImageAndSave(this.txtLargeImagePath.Text, rectLargeImagePath, this.GetSingleImageSearchResult(isSynchronousOperation));
                   }

                   var originalLargeImage = Image.FromFile(this.txtLargeImagePath.Text);

                   // Map result details information
                   _resultDetails = new ResultDetails()
                   {
                       OriginalLargeImageWidth = originalLargeImage.Width,
                       OriginalLargeImageHeight = originalLargeImage.Height,
                       EllapsedTime = elapsedMs,
                       ImageResizedTimes = scaleDownTimes,
                       AsyncImageSearchResults = _asyncImageSearchResults,
                       SyncImageSearchResults = _syncImageSearchResults,
                       ImageSearchTimes = imageSearchTimes,
                       RectLargeImagePath = rectLargeImagePath,
                       ResizedRectLargeImagePath = resizedRectLargeImagePath,
                       IsSynchronousOperationPerformed = isSynchronousOperation,
                   };

                   originalLargeImage.Dispose();

                   // Update UI after search has been completed
                   this.lblElapsedTime.Content = "Ellapsed time(ms): " + elapsedMs;
                   this.txtStatus.Text = "Ready";
                   this.btnCancelSearch.IsEnabled = false;
                   this.btnSearchMatchingImages.IsEnabled = true;
                   this.btnViewDetails.IsEnabled = true;

               }, _tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, ui);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ex)
            {
                ex.Handle((x) =>
                {
                    return true;
                });

                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            catch (Exception)
            {
            }
        }

        private Rectangle GetSingleImageSearchResult(bool isSynchronousOperation)
        {
            if (isSynchronousOperation)
            {
                return _syncImageSearchResults.First();
            }
            else
            {
                return _asyncImageSearchResults.First().Value;
            }
        }

        private void btnCancelSearch_Click(object sender, RoutedEventArgs e)
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }

            this.txtStatus.Text = "Cancelled";
            this.btnCancelSearch.IsEnabled = false;
            this.btnSearchMatchingImages.IsEnabled = true;

            // Re-initialize token source so that after token disposal no exception would be thrown
            _tokenSource = new CancellationTokenSource();
        }

        private void btnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var detailsWindow = new ResultDetailsWindow(_resultDetails);
            detailsWindow.Show();
        }

        private void slImageSearchTimes_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
