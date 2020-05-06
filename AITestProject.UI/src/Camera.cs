using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using AForge.Video;
using AForge.Video.DirectShow;
using AITestProject.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace AITestProject
{
    public class Camera
    {
        private readonly MainWindow _mainWindow;
        private readonly Stopwatch _detectionTimer;

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly FilterInfoCollection _filterInfoCollection;
        private VideoCaptureDevice _camera;

        public Camera(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _detectionTimer = new Stopwatch();
            _filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }

        public void Start(int deviceBoxSelectedIndex)
        {
            _camera = new VideoCaptureDevice(_filterInfoCollection[deviceBoxSelectedIndex].MonikerString);
            _camera.NewFrame += Camera_NewFrame;
            _camera.Start();
            _detectionTimer.Start();
        }

        private void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var bitmap = (Bitmap) eventArgs.Frame.Clone();
            var bitmapImage = ImageUtil.GetBitmapImage(bitmap);
            _mainWindow.Dispatcher.BeginInvoke((Action) (() => WebcamNextFrame(bitmap, bitmapImage)));
        }

        private void WebcamNextFrame(Bitmap bitmap, ImageSource bitmapImage)
        {
            _mainWindow.Pic.Source = bitmapImage;
            if (_detectionTimer.Elapsed.Seconds < 1) return;
            _detectionTimer.Restart();
            _mainWindow.DrawDetection(bitmap.ToImage<Gray, byte>().Resize(444, 250, Inter.Linear));
        }

        public IEnumerable<string> GetDevices() => from FilterInfo f in _filterInfoCollection select f.Name;


        public void Dispose()
        {
            if (_camera == null || !_camera.IsRunning) return;
            _camera.SignalToStop();
            _camera.WaitForStop();
            _camera = null;
            _detectionTimer.Stop();
        }
    }
}