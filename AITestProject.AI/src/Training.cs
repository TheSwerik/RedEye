using System;
using System.IO;
using System.Linq;
using AITestProject.AI.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.VisualBasic.CompilerServices;

namespace AITestProject.AI
{
    public class Training
    {
        private readonly MLContext _mlContext;
        private readonly DirectoryInfo _assets;

        public Training()
        {
            _mlContext = new MLContext(7);
            _assets = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\assets\LFW\");
            Console.WriteLine("Found {0} Images.", ImageData.ReadFromFile(_assets.FullName).Count());
        }
    }
}