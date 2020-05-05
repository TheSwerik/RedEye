using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace AITestProject
{
    public partial class MainWindow : Window
    {
        private readonly CascadeClassifier _cascadeClassifier =
            new CascadeClassifier(@"assets\haarcascade_frontalface_default.xml");

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
            canvas.Children.Clear();
            NextImage();
            canvas.Children.Add(Pic);

            var grayImage = new Mat(ImagePath()).ToImage<Gray, byte>();
            DetectFaces(grayImage);
        }

        private void DetectFaces(IOutputArrayOfArrays grayImage)
        {
            if (grayImage == null) throw new ArgumentNullException(nameof(grayImage));
            var rectangles = _cascadeClassifier.DetectMultiScale(grayImage, 1.4, 0);

            foreach (var rect in rectangles)
            {
                var rectangle = new Rectangle();
                Canvas.SetLeft(rectangle, rect.X);
                Canvas.SetTop(rectangle, rect.Y);
                rectangle.Width = rect.Width;
                rectangle.Height = rect.Height;
                rectangle.Stroke = new SolidColorBrush() {Color = Colors.Red, Opacity = 1f};

                canvas.Children.Add(rectangle);
            }
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
            DeviceBox.Visibility = StartButton.Visibility = Visibility.Hidden;
        }

        private void RadioButtonCamera_OnChecked(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            Pic.Source = null;
            canvas.Children.Add(Pic);

            NextButton.Visibility = Visibility.Hidden;
            DeviceBox.Visibility = StartButton.Visibility = Visibility.Visible;
            DeviceBox_OnSelectionChanged(null, null);
        }

        //Webcam stuff:
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DeviceBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!RadioButtonCamera.IsChecked ?? false) return;
            if (_camera != null && _camera.IsRunning)
            {
                _camera.SignalToStop();
                _camera.WaitForStop();
            }

            _camera = new VideoCaptureDevice(_filterInfoCollection[DeviceBox.SelectedIndex].MonikerString);
            _camera.NewFrame += new NewFrameEventHandler(Camera_NewFrame);
            _camera.Start();
        }

        private void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
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
                Dispatcher.BeginInvoke((Action) (() => Test(bi)));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Test(ImageSource bi)
        {
            Pic.Source = bi;
        }
    }
}