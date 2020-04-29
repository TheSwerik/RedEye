using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AITestProject.AI.Data;

namespace AITestProject.Trainer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IEnumerator<string> _enumerable;
        private bool _leftEyeFound;
        private readonly StreamWriter _writer;
        private string _dataFilePath;

        public MainWindow()
        {
            InitializeComponent();

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // File Stuff:
            _dataFilePath = Directory.GetCurrentDirectory() + @"\assets\ImageData.tsv";

            var lines = 0;
            if (!File.Exists(_dataFilePath)) File.Create(_dataFilePath).Close();
            else lines = File.ReadLines(_dataFilePath).Count();

            _writer = new StreamWriter(new FileStream(_dataFilePath, FileMode.Append));

            // init:
            _enumerable = ImageData.ReadImagesFromFile(Directory.GetCurrentDirectory() + @"\assets\LFW\")
                                   .Skip(lines)
                                   .GetEnumerator();
            Pic.Source = NextImage();
            _leftEyeFound = false;
        }

        private void Image_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition((Image) sender);

            if (!_leftEyeFound)
            {
                AppendToFile(point);
                _leftEyeFound = true;
                Console.Write("Left Eye:\t");
            }
            else
            {
                AppendToFile(point);
                _leftEyeFound = false;
                Console.Write("Right Eye:\t");
                Pic.Source = NextImage();
            }

            Console.WriteLine("X: {0}, Y: {1}", (int) point.X, (int) point.Y);
        }

        private void Window_OnClosed(object sender, EventArgs e)
        {
            _enumerable.Dispose();
            _writer.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private BitmapImage NextImage()
        {
            if (!_enumerable.MoveNext()) throw new ArgumentException("This was the Last element.");
            return new BitmapImage(new Uri(ImagePath()));
        }

        private string ImagePath()
        {
            return _enumerable.Current ?? throw new ArgumentException($"no pic found {_enumerable.Current}");
        }

        private void AppendToFile(Point p)
        {
            if (!_leftEyeFound) _writer.Write(ImagePath() + ", " + (int) p.X + ", " + (int) p.Y);
            else
            {
                _writer.WriteLine(", " + (int) p.X + ", " + (int) p.Y);
                _writer.Flush();
            }
        }
    }
}