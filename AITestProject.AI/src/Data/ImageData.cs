using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImageData
    {
        [LoadColumn(0)] public string ImagePath;
        [LoadColumn(1)] public int[] LeftEyeCoordinate { get; set; }
        [LoadColumn(2)] public int[] RightEyeCoordinate { get; set; }

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
                                          LeftEyeCoordinate = new int[] {int.Parse(values[1]), int.Parse(values[2])},
                                          RightEyeCoordinate = new int[] {int.Parse(values[3]), int.Parse(values[4])}
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