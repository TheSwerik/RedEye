using System;
using System.Collections.Generic;
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
        private readonly IEnumerator<ImageData> _enumerable;
        private bool _leftEyeFound;

        public MainWindow()
        {
            InitializeComponent();
            _enumerable = ImageData.ReadFromFile(Directory.GetCurrentDirectory() + @"\assets\LFW\").GetEnumerator();
            Pic.Source = NextImage();
            _leftEyeFound = false;
        }

        private void Image_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition((Image) sender);

            if (!_leftEyeFound)
            {
                _leftEyeFound = true;
                Console.Write("Left Eye:\t");
            }
            else
            {
                Pic.Source = NextImage();
                _leftEyeFound = false;
                Console.Write("Right Eye:\t");
            }
            
            Console.WriteLine("X: {0}, Y: {1}", (int) point.X, (int) point.Y);
        }

        private void Window_OnClosed(object? sender, EventArgs e)
        {
            _enumerable.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private BitmapImage NextImage()
        {
            if (!_enumerable.MoveNext()) throw new ArgumentException($"This was the Last element.");
            return new BitmapImage(
                new Uri(
                    (_enumerable.Current ?? throw new ArgumentException($"no pic found {_enumerable.Current}"))
                    .ImagePath
                )
            );
        }
    }
}