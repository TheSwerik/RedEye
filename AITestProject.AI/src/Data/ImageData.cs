using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImageData
    {
        [LoadColumn(0)] public string ImagePath;
        [LoadColumn(1)] public string LeftEyeArea { get; set; }
        [LoadColumn(2)] public string RightEyeArea { get; set; }

        public static IEnumerable<ImageData> ReadFromFile(string imageFolder)
        {
            return Directory
                   .GetFiles(imageFolder, "*.*", SearchOption.AllDirectories)
                   .Where(Path.HasExtension)
                   .Select(filePath => new ImageData {ImagePath = filePath});
        }
    }
}