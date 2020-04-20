namespace ConsoleApplication4
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    public class ImageLogic
    {
        public Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        public void ResizeImageAndSave(string srcImagePath, string resizedImagePath, int scaleDownTimes, long quality = 100L)
        {
            var srcImage = Image.FromFile(srcImagePath);
            if (srcImage == null)
            {
                throw new FileNotFoundException(srcImagePath);
            }

            int minimizedWidth = scaleDownTimes > 0 ? (srcImage.Width / (scaleDownTimes + 1)) : srcImage.Width;
            int minimizedHeight = scaleDownTimes > 0 ? (srcImage.Height / (scaleDownTimes + 1)) : srcImage.Height;

            using (Bitmap resizedImage = this.ResizeImage(srcImage, minimizedWidth, minimizedHeight))
            {
                // Changes the image quality
                ImageCodecInfo pngEncoder = GetEncoder(ImageFormat.Png);
                Encoder myEncoder = Encoder.Quality;
                var myEncoderParameters = new EncoderParameters(1);
                var myEncoderParameter = new EncoderParameter(myEncoder, quality);
                myEncoderParameters.Param[0] = myEncoderParameter;

                // Save image with specified quality
                resizedImage.Save(resizedImagePath, pngEncoder, myEncoderParameters);
            }

            srcImage.Dispose();
        }

        public void DrawRectangleOntoImageAndSave(string sourceImagePath, string newImagePath, Rectangle rect)
        {
            Image image = Image.FromFile(sourceImagePath);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.DrawRectangle(new Pen(Brushes.Red, 1), rect);
            }
            image.Save(newImagePath, ImageFormat.Png);
        }

        public void DeletePreviousRectangleImages(string dir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            var files = dirInfo.GetFiles().Where(x => x.Name.StartsWith("rect_"));
            foreach (var f in files)
            {
                f.Delete();
            }
        }

        public Rectangle CalculateImageOriginalSizeAndPosition(Rectangle scaledDownImageParams, int scaleDownTimes)
        {
            // If no scaling performed, returns given rectangle params
            if (scaleDownTimes == 0)
            {
                return scaledDownImageParams;
            }

            // Multiplying by 1 will not make any changes, therefore increment by 1
            int scaleRatio = (scaleDownTimes + 1);
            return new Rectangle
            {
                X = scaledDownImageParams.X * scaleRatio,
                Y = scaledDownImageParams.Y * scaleRatio,
                Width = scaledDownImageParams.Width * scaleRatio,
                Height = scaledDownImageParams.Height * scaleRatio
            };
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
