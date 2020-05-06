using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RedEye.Util;

namespace RedEye
{
    public class Camera
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly FilterInfoCollection _filterInfoCollection;
        private readonly Stopwatch _detectionTimer;
        private readonly MainWindow _mainWindow;
        private readonly double _updateTime;
        private VideoCaptureDevice _camera;

        public Camera(MainWindow mainWindow)
        {
            _filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _detectionTimer = new Stopwatch();
            _mainWindow = mainWindow;
            _updateTime = 1000.0 / Config.GetInt("DetectionFrequency"); // 1 second / updates per second
        }

        public IEnumerable<string> GetDevices() => from FilterInfo f in _filterInfoCollection select f.Name;

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
            var time = _detectionTimer.Elapsed.Seconds * 1000 + _detectionTimer.Elapsed.Milliseconds;
            if (time < _updateTime) return;
            _detectionTimer.Restart();
            _mainWindow.DrawDetection(bitmap.ToImage<Gray, byte>().Resize(444, 250, Inter.Linear));
        }

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