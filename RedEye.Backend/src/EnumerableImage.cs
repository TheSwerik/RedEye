using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using RedEye.Util;

namespace RedEye
{
    public class EnumerableImage
    {
        private readonly IEnumerator<string> _imageEnumerator;
        public EnumerableImage(string path) => _imageEnumerator = ImageUtil.GetImagePaths(path).GetEnumerator();
        public string CurrentImagePath() => _imageEnumerator.Current ??
                                            throw new ArgumentException($"no pic found {_imageEnumerator.Current}");
        public void Dispose() => _imageEnumerator.Dispose();

        public BitmapImage NextImage()
        {
            if (!_imageEnumerator.MoveNext()) throw new ArgumentException("This was the Last element.");
            return ImageUtil.GetBitmapImage(CurrentImagePath());
        }
    }
}