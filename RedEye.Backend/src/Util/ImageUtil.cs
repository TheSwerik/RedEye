using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace RedEye.Util
{
    public static class ImageUtil
    {
        private static readonly BitmapImage EyeImage = GetBitmapImage(@"assets\redeye_texture.png");
        private static readonly int VOffset = Config.GetInt("EyeImageVerticalOffset");

        public static IEnumerable<string> GetImagePaths(string folder)
        {
            return Directory
                   .GetFiles(folder, "*.*", SearchOption.AllDirectories)
                   .Where(Path.HasExtension);
        }

        public static Image EyeTextureImage(Rectangle rect, double scale = 1)
        {
            if (rect.Equals(Rectangle.Empty)) return new Image();
            var width = scale * 300;
            var eye = new Image {Source = EyeImage, Width = width};
            Canvas.SetLeft(eye, rect.X + (rect.Width - width) / 2);
            Canvas.SetTop(eye, rect.Y + (rect.Height - width) / 2 + VOffset * scale);
            return eye;
        }

        public static BitmapImage GetBitmapImage(string url)
        {
            using var stream = new FileStream(url, FileMode.Open);
            return GetBitmapImage(stream);
        }

        public static BitmapImage GetBitmapImage(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);
            stream.Seek(0, SeekOrigin.Begin);
            return GetBitmapImage(stream);
        }

        private static BitmapImage GetBitmapImage(Stream stream)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}