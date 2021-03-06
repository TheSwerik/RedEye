﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.Cuda;
using RedEye.Util;

namespace RedEye
{
    public static class Detector
    {
        public enum DetectionObject
        {
            Face,
            LeftEye,
            RightEye
        }

        private const string Path = @"assets\haarcascades\haarcascade_";

        private static readonly Dictionary<DetectionObject, CascadeClassifier> Classifiers;
        private static readonly Dictionary<DetectionObject, CudaCascadeClassifier> CudaClassifiers;

        static Detector()
        {
            Classifiers = new Dictionary<DetectionObject, CascadeClassifier>
                          {
                              {DetectionObject.Face, new CascadeClassifier(Path + "frontalface_default.xml")},
                              {DetectionObject.LeftEye, new CascadeClassifier(Path + "lefteye_2splits.xml")},
                              {DetectionObject.RightEye, new CascadeClassifier(Path + "righteye_2splits.xml")}
                          };

            CudaClassifiers = new Dictionary<DetectionObject, CudaCascadeClassifier>();
            if (!Config.IsCudaEnabled) return;
            CudaClassifiers.Add(DetectionObject.Face, new CudaCascadeClassifier(Path + "frontalface_default.xml"));
            CudaClassifiers.Add(DetectionObject.LeftEye, new CudaCascadeClassifier(Path + "lefteye_2splits.xml"));
            CudaClassifiers.Add(DetectionObject.RightEye, new CudaCascadeClassifier(Path + "righteye_2splits.xml"));
        }

        public static Rectangle Detect(IOutputArrayOfArrays grayImage, DetectionObject detectionObject)
        {
            if (grayImage == null) throw new ArgumentNullException(nameof(grayImage));
            var rectangles = Classifiers[detectionObject].DetectMultiScale(grayImage, 1.4, Config.GetInt("Neighbors"));
            if (rectangles.Length == 0) return Rectangle.Empty;
            if (!Config.GetBool("PickAverage")) return rectangles.OrderBy(r => r.Width).First();
            return new Rectangle(
                (int) rectangles.Average(r => r.X),
                (int) rectangles.Average(r => r.Y),
                (int) rectangles.Average(r => r.Width),
                (int) rectangles.Average(r => r.Height)
            );
        }

        public static IEnumerable<Rectangle> DetectAll(IOutputArrayOfArrays grayImage, DetectionObject detectionObject)
        {
            if (grayImage == null) throw new ArgumentNullException(nameof(grayImage));
            return Classifiers[detectionObject].DetectMultiScale(grayImage, 1.4, Config.GetInt("Neighbors"));
        }

        public static Rectangle DetectCuda(IOutputArrayOfArrays grayImage, DetectionObject detectionObject)
        {
            if (grayImage == null) throw new ArgumentNullException(nameof(grayImage));

            using GpuMat mat = new GpuMat();
            CudaClassifiers[detectionObject].DetectMultiScale(grayImage, mat);
            var rectangles = CudaClassifiers[detectionObject].Convert(mat);

            if (rectangles.Length == 0) return Rectangle.Empty;
            if (!Config.GetBool("PickAverage")) return rectangles.OrderBy(r => r.Width).First();
            return new Rectangle(
                (int) rectangles.Average(r => r.X),
                (int) rectangles.Average(r => r.Y),
                (int) rectangles.Average(r => r.Width),
                (int) rectangles.Average(r => r.Height)
            );
        }

        public static void Dispose()
        {
            foreach (var classifier in Classifiers.Values) classifier.Dispose();
            foreach (var classifier in CudaClassifiers.Values) classifier.Dispose();
        }

        // Helper:
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