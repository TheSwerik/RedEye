using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImageData
    {
        [LoadColumn(0)] public string ImagePath;

        [LoadColumn(1)] public string Label;

        public static IEnumerable<ImageData> ReadFromFile(string imageFolder)
        {
            return Directory
                   .GetFiles(imageFolder)
                   .Where(filePath => Path.GetExtension(filePath) != ".md")
                   .Select(filePath => new ImageData {ImagePath = filePath, Label = Path.GetFileName(filePath)});
        }
    }
}