using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Image = System.Windows.Controls.Image;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Drawing.Size;

namespace AITestProject
{
    public partial class MainWindow : Window
    {
        private readonly CascadeClassifier _faceCascadeClassifier =
            new CascadeClassifier(@"assets\haarcascade_frontalface_default.xml");

        private readonly CascadeClassifier _leftEyeCascadeClassifier =
            new CascadeClassifier(@"assets\haarcascade_lefteye_2splits.xml");

        private readonly CascadeClassifier _rightEyeCascadeClassifier =
            new CascadeClassifier(@"assets\haarcascade_righteye_2splits.xml");

        private readonly IEnumerator<string> _enumerable;
        private readonly FilterInfoCollection _filterInfoCollection;

        private VideoCaptureDevice _camera;


        public MainWindow()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            _enumerable = Util.GetImages(@"assets\LFW").GetEnumerator();
            NextImage();

            _filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filter in _filterInfoCollection) DeviceBox.Items.Add(filter.Name);
            DeviceBox.SelectedIndex = 0;

            _camera = new VideoCaptureDevice(_filterInfoCollection[DeviceBox.SelectedIndex].MonikerString);
            // _camera.VideoResolution = _camera.VideoCapabilities[0];
        }

        // Helper Methods:
        private void NextImage()
        {
            if (!_enumerable.MoveNext()) throw new ArgumentException("This was the Last element.");
            using var stream = new FileStream(ImagePath(), FileMode.Open);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            Pic.Source = bitmapImage;
        }

        private string ImagePath()
        {
            return _enumerable.Current ?? throw new ArgumentException($"no pic found {_enumerable.Current}");
        }

        private void Dispose()
        {
            _enumerable.Dispose();
            if (_camera == null || !_camera.IsRunning) return;
            _camera.SignalToStop();
            _camera.WaitForStop();
            _camera = null;
        }

        // UI:
        private void Window_OnClosed(object sender, EventArgs e)
        {
            Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearCanvas();
            NextImage();

            var grayImage = new Mat(ImagePath()).ToImage<Gray, byte>();
            DetectFaces(grayImage);
        }

        private void DetectFaces(IOutputArrayOfArrays grayImage)
        {
            if (grayImage == null) throw new ArgumentNullException(nameof(grayImage));
            var rectangles = _faceCascadeClassifier.DetectMultiScale(grayImage, 1.4, 0);
            ClearCanvas();
            DrawRectangle(rectangles);
        }

        private void DetectEyes(IOutputArrayOfArrays grayImage)
        {
            if (grayImage == null) throw new ArgumentNullException(nameof(grayImage));
            var leftRectangles = _leftEyeCascadeClassifier.DetectMultiScale(grayImage, 1.4, 0);
            var rightRectangles = _rightEyeCascadeClassifier.DetectMultiScale(grayImage, 1.4, 0);

            ClearCanvas();
            DrawRectangle(leftRectangles);
            DrawRectangle(rightRectangles);
        }

        private void DrawRectangle(System.Drawing.Rectangle[] rectangles)
        {
            if (rectangles.Length == 0) return;
            // Select biggest Rectangle: 
            // var rect = rectangles.OrderByDescending(r => r.Width).First();
            // Select smallest Rectangle: 
            var rect = rectangles.OrderBy(r => r.Width).First();
            var rectangle = new Rectangle();
            Canvas.SetLeft(rectangle, rect.X);
            Canvas.SetTop(rectangle, rect.Y);
            rectangle.Width = rect.Width;
            rectangle.Height = rect.Height;
            rectangle.Stroke = new SolidColorBrush() {Color = Colors.Blue, Opacity = 1f};

            canvas.Children.Add(rectangle);
            AddEyeTexture(rect);
        }

        private ImageBrush eyeBrush = new ImageBrush {ImageSource = EyeImage};
        private static readonly BitmapImage EyeImage = GetImage(@"assets\redeye_texture.png");

        private static BitmapImage GetImage(string url)
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

        private void AddEyeTexture(System.Drawing.Rectangle rect)
        {
            var eye = new Image {Source = EyeImage};
            canvas.Children.Add(eye);
            Canvas.SetLeft(eye, rect.X + (rect.Width - eye.Source.Width) / 2);
            Canvas.SetTop(eye, rect.Y + (rect.Height - eye.Source.Height) / 2 + 4);
        }

        private void RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (Pic == null) return;
            if (_camera != null && _camera.IsRunning)
            {
                _camera.SignalToStop();
                _camera.WaitForStop();
                _camera = null;
            }

            NextButton_OnClick(null, null);
            NextButton.Visibility = Visibility.Visible;
        }

        private void RadioButtonCamera_OnChecked(object sender, RoutedEventArgs e)
        {
            Pic.Source = null;
            ClearCanvas();

            NextButton.Visibility = Visibility.Hidden;
            DeviceBox_OnSelectionChanged(null, null);
        }

        private void ClearCanvas()
        {
            canvas.Children.Clear();
            canvas.Children.Add(Pic);
        }

        //Webcam stuff:
        private void DeviceBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!RadioButtonCamera.IsChecked ?? false) return;
            if (_camera != null && _camera.IsRunning)
            {
                _camera.SignalToStop();
                _camera.WaitForStop();
            }

            _detectionTimer.Start();
            _camera = new VideoCaptureDevice(_filterInfoCollection[DeviceBox.SelectedIndex].MonikerString);
            _camera.NewFrame += new NewFrameEventHandler(Camera_NewFrame);
            _camera.Start();
        }

        private readonly Stopwatch _detectionTimer = new Stopwatch();

        private void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            System.Drawing.Image img = (Bitmap) eventArgs.Frame.Clone();

            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            bi.Freeze();
            Dispatcher.BeginInvoke((Action) (() => WebcamNextFrame(bi, (Bitmap) img)));
        }

        private void WebcamNextFrame(ImageSource bi, Bitmap bitmap)
        {
            Pic.Source = bi;
            if (_detectionTimer.Elapsed.Seconds < 1) return;
            _detectionTimer.Restart();

            var test = bitmap.ToImage<Gray, byte>();
            // DetectFaces(test.Resize(444, 250, Inter.Linear));
            DetectEyes(test.Resize(444, 250, Inter.Linear));
        }
    }
}