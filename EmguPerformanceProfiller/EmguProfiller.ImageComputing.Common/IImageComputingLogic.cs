namespace ConsoleApp.ImageComputing.Common
{
    using System.Drawing;

    public interface IImageComputingLogic
    {
        Rectangle GetImageParamsResult(string largeImagePath, string templImagePath);
    }
}
