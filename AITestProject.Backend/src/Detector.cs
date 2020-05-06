using System;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Emgu.CV;

namespace AITestProject
{
    public static class Detector
    {
        private const string Path = @"assets\haarcascades\haarcascade_";
        private static readonly CascadeClassifier FaceCascadeClassifier = new CascadeClassifier(Path + "frontalface_default.xml");
        private static readonly CascadeClassifier LeftEyeCascadeClassifier = new CascadeClassifier(Path + "lefteye_2splits.xml");
        private static readonly CascadeClassifier RightEyeCascadeClassifier = new CascadeClassifier(Path + "righteye_2splits.xml");

        public enum DetectionObject
        {
            Face,
            LeftEye,
            RightEye
        }

        public static Rectangle Detect(IOutputArrayOfArrays grayImage, DetectionObject detectionObject)
        {
            if (grayImage == null) throw new ArgumentNullException(nameof(grayImage));
            var rectangles = detectionObject switch
            {
                DetectionObject.Face => FaceCascadeClassifier.DetectMultiScale(grayImage, 1.4, 0),
                DetectionObject.LeftEye => LeftEyeCascadeClassifier.DetectMultiScale(grayImage, 1.4, 0),
                DetectionObject.RightEye => RightEyeCascadeClassifier.DetectMultiScale(grayImage, 1.4, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(detectionObject), detectionObject, null)
            };
            return rectangles.Length == 0 ? new Rectangle() : rectangles.OrderBy(r => r.Width).First();
        }

        public static System.Windows.Shapes.Rectangle ConvertRectangle(Rectangle rect)
        {
            var rectangle = new System.Windows.Shapes.Rectangle();
            Canvas.SetLeft(rectangle, rect.X);
            Canvas.SetTop(rectangle, rect.Y);
            rectangle.Width = rect.Width;
            rectangle.Height = rect.Height;
            rectangle.Stroke = new SolidColorBrush {Color = Colors.Blue, Opacity = 1f};
            return rectangle;
        }
    }
}