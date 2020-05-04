using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImageData
    {
        // [LoadColumn(4)]
        // [ColumnName("Label")]
        // public float CurrentPrice { get; set; }

        [LoadColumn(0)] public string ImagePath;
        [LoadColumn(1, 2)] [VectorType(2)] public int[] LeftEyeArea { get; set; }
        [LoadColumn(3, 4)] [VectorType(2)] public int[] RightEyeArea { get; set; }

        public static IEnumerable<ImageData> ReadDataFromFile(string imageFolder)
        {
            var datafilePath = Directory
                               .GetFiles(imageFolder, "*.tsv", SearchOption.AllDirectories)
                               .First();

            return File
                   .ReadLines(datafilePath)
                   .Select(line =>
                           {
                               var values = line.Split(",");
                               return new ImageData
                                      {
                                          ImagePath = values[0],
                                          LeftEyeArea = new[] {int.Parse(values[1]), int.Parse(values[2])},
                                          RightEyeArea = new[] {int.Parse(values[3]), int.Parse(values[4])}
                                      };
                           });
        }

        public static IEnumerable<string> ReadImagesFromFile(string imageFolder)
        {
            return Directory
                   .GetFiles(imageFolder, "*.*", SearchOption.AllDirectories)
                   .Where(Path.HasExtension);
        }
    }
}