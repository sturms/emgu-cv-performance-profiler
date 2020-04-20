namespace ImageTemplateMatching.WPF
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;

    public class ResultDetails
    {
        public int ImageSearchTimes { get; set; }
        public bool IsSynchronousOperationPerformed { get; set; }
        public long EllapsedTime { get; set; }
        public int ImageResizedTimes { get; set; }
        public int OriginalLargeImageWidth { get; set; }
        public int OriginalLargeImageHeight { get; set; }
        public string RectLargeImagePath { get; set; }
        public string ResizedRectLargeImagePath { get; set; }
        public List<Rectangle> SyncImageSearchResults { get; set; } = new List<Rectangle>();
        public ConcurrentDictionary<int, Rectangle> AsyncImageSearchResults { get; set; } = new ConcurrentDictionary<int, Rectangle>();
    }
}
