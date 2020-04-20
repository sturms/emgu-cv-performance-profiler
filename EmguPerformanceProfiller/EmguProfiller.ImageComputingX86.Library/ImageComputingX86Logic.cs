namespace ConsoleApp.ImageComputingX86.Library
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using Emgu.CV;
    using Emgu.CV.Structure;

    public class ImageComputingX86Logic
    {
        public Rectangle GetImageParamsResult(string largeImagePath, string templImagePath)
        {
            Rectangle match = new Rectangle();
            Image<Bgr, Byte> source = new Image<Bgr, Byte>(largeImagePath);
            Image<Bgr, Byte> template = new Image<Bgr, Byte>(templImagePath);

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // Range from 0.75 to 0.95 can variate.
                if (maxValues[0] > 0.9)
                {
                    match = new Rectangle(maxLocations[0], template.Size);
                }
            }

            int w = match.Width;
            int h = match.Height;
            int x = match.X;
            int y = match.Y;

            Console.WriteLine("ImageComputingX86Logic called!");

            return new Rectangle() { Width = w, Height = h, X = x, Y = y };
        }
    }
}
