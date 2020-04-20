namespace ConsoleApp.ImageComputing.Common
{
    using System;
    using System.Drawing;
    using ConsoleApp.ImageComputing.Common;
    using ImageComputingX64.Library;
    using ImageComputingX86.Library;

    public class ImageComputingLogic : IImageComputingLogic
    {
        private Lazy<ImageComputingX64Logic> _logicX64 = new Lazy<ImageComputingX64Logic>();
        private Lazy<ImageComputingX86Logic> _logicX86 = new Lazy<ImageComputingX86Logic>();

        public Rectangle GetImageParamsResult(string largeImagePath, string templImagePath)
        {
            Rectangle rect;

            if (Environment.Is64BitOperatingSystem)
            {
                rect = _logicX64.Value.GetImageParamsResult(largeImagePath, templImagePath);
            }
            else
            {
                rect = _logicX86.Value.GetImageParamsResult(largeImagePath, templImagePath);
            }

            return rect;
        }
    }
}
