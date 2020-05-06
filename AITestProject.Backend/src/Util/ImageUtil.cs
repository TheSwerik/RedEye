using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace AITestProject.Util
{
    public static class ImageUtil
    {
        private static readonly BitmapImage EyeImage = GetBitmapImage(@"assets\redeye_texture.png");
        public static IEnumerable<string> GetImagePaths(string folder)
        {
            return Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                            .Where(Path.HasExtension);
        }

        public static BitmapImage GetBitmapImage(string url)
        {
            using var stream = new FileStream(url, FileMode.Open);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        public static  Image EyeTextureImage(Rectangle rect)
        {
            var eye = new System.Windows.Controls.Image() {Source = EyeImage};
            Canvas.SetLeft(eye, rect.X + (rect.Width - eye.Source.Width) / 2);
            Canvas.SetTop(eye, rect.Y + (rect.Height - eye.Source.Height) / 2 + 4); //TODO make offset in settings
            return eye;
        }
    }
}