using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RedEye.Util;
using Point = System.Windows.Point;

namespace RedEye
{
    public partial class MainWindow
    {
        private static readonly string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) +
                                              @"\RedEye\Sources";

        private readonly Camera _camera;
        private readonly EnumerableImage _images;

        public MainWindow()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            _images = new EnumerableImage(Path);
            _camera = new Camera(this);

            // Init Combobox:
            foreach (var filter in _camera.GetDevices()) DeviceBox.Items.Add(filter);
            DeviceBox.SelectedIndex = 0;

            RadioButtonImage.IsChecked = !(RadioButtonCam.IsChecked = Config.GetBool("StartWithCamera"));

            Console.WriteLine("CUDA " + (Config.IsCudaEnabled ? "On" : "Off"));
        }

        private void Window_OnClosed(object sender, EventArgs e)
        {
            _images.Dispose();
            _camera.Dispose();
            Detector.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void NextButton_OnClick(object? sender, RoutedEventArgs? e)
        {
            Pic.Source = _images.NextImage();
            Dispatcher.BeginInvoke((Action) DetectAsync, DispatcherPriority.ContextIdle);
        }

        private void RadioButtonImage_OnChecked(object sender, RoutedEventArgs e)
        {
            _camera.Dispose();
            Dispatcher.BeginInvoke((Action) SwitchToImage, DispatcherPriority.ContextIdle);
        }

        private void RadioButtonCamera_OnChecked(object sender, RoutedEventArgs e)
        {
            DeviceBox_OnSelectionChanged(null, null);
            NextButton.Visibility = Visibility.Hidden;
            DeviceBox.Visibility = Visibility.Visible;
            NextButtonRow.Height = new GridLength(0);
            DeviceComboRow.Height = GridLength.Auto;
        }

        private void DeviceBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            if (!RadioButtonCam.IsChecked ?? false) return;
            _camera.Dispose();
            _camera.Start(DeviceBox.SelectedIndex);
        }

        private void Window_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = (Window) sender;
            window.FontSize = window.ActualHeight / 35;
            MainCanvas.Height = window.ActualHeight / 2;
            RadioButtonGrid.Margin = new Thickness(
                window.ActualHeight / 20,
                0,
                window.ActualHeight / 20,
                window.ActualHeight / 100
            );
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action) SavePNG, DispatcherPriority.ContextIdle);
        }

        // Helper Methods:
        private void ClearCanvas()
        {
            MainCanvas.Children.Clear();
            MainCanvas.Children.Add(Pic);
        }

        public void DrawDetection(IOutputArrayOfArrays grayImage)
        {
            grayImage = ((Image<Gray, byte>) grayImage)
                .Resize(Math.Max((int) MainCanvas.ActualWidth, 1), (int) MainCanvas.ActualHeight, Inter.Linear);
            ClearCanvas();

            if (Config.GetBool("DrawRectangles"))
            {
                IEnumerable<Rectangle> rectangles;
                if (Config.GetBool("Face")) rectangles = Detector.DetectAll(grayImage, Detector.DetectionObject.Face);
                else
                    rectangles = Detector.DetectAll(grayImage, Detector.DetectionObject.LeftEye)
                                         .Concat(Detector.DetectAll(grayImage, Detector.DetectionObject.RightEye));
                foreach (var rectangle in rectangles) MainCanvas.Children.Add(Detector.ConvertRectangle(rectangle));
            }

            if (Config.GetBool("Face"))
            {
                MainCanvas.Children.Add(
                    Detector.ConvertRectangle(Detector.Detect(grayImage, Detector.DetectionObject.Face))
                );
            }
            else if (Config.IsCudaEnabled)
            {
                var mat = new GpuMat<byte>(grayImage);
                var image = new CudaImage<Gray, byte>();
                mat.ConvertTo(image, DepthType.Cv8U);
                MainCanvas.Children.Add(
                    ImageUtil.EyeTextureImage(
                        Detector.DetectCuda(image, Detector.DetectionObject.LeftEye)));
                MainCanvas.Children.Add(
                    ImageUtil.EyeTextureImage(
                        Detector.DetectCuda(image, Detector.DetectionObject.RightEye)));
            }
            else
            {
                MainCanvas.Children.Add(
                    ImageUtil.EyeTextureImage(
                        Detector.Detect(grayImage, Detector.DetectionObject.LeftEye),
                        MainCanvas.ActualWidth / 250));
                MainCanvas.Children.Add(
                    ImageUtil.EyeTextureImage(
                        Detector.Detect(grayImage, Detector.DetectionObject.RightEye),
                        MainCanvas.ActualWidth / 250));
            }
        }

        private void SwitchToImage()
        {
            Pic.Source = null;

            NextButton_OnClick(null, null);
            NextButton.Visibility = Visibility.Visible;
            DeviceBox.Visibility = Visibility.Hidden;
            DeviceComboRow.Height = new GridLength(0);
            NextButtonRow.Height = GridLength.Auto;
        }

        private void DetectAsync() { DrawDetection(new Mat(_images.CurrentImagePath()).ToImage<Gray, byte>()); }

        private void SavePNG()
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(Pic);
            var rtb = new RenderTargetBitmap((int) bounds.Width, (int) bounds.Height, 96d, 96d, PixelFormats.Default);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(MainCanvas);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            rtb.Render(dv);
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            try
            {
                var ms = new MemoryStream();
                pngEncoder.Save(ms);
                ms.Close();
                File.WriteAllBytes(
                    Config.Get("ScreenshotLocation") + $@"\RedEye {DateTime.Now:yyyy-MM-dd hh-mm-ss}.png",
                    ms.ToArray());
            }
            catch (Exception err)
            {
                const string message = "Failed to Save Image:\n";
                MessageBox.Show(message + err, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}