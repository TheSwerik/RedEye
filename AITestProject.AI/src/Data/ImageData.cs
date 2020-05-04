using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;

namespace AITestProject.AI.Data
{
    public class ImageData
    {
        [LoadColumn(0)] public string ImagePath;
        [LoadColumn(1)] public int LeftEyeX { get; set; }
        [LoadColumn(2)] public int LeftEyeY { get; set; }
        [LoadColumn(3)] public int RightEyeX { get; set; }
        [LoadColumn(4)] public int RightEyeY { get; set; }

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
                                          LeftEyeX = int.Parse(values[1]), LeftEyeY = int.Parse(values[2]),
                                          RightEyeX = int.Parse(values[3]), RightEyeY = int.Parse(values[4])
                                      };
                           }
                   );
        }

        public static IEnumerable<string> ReadImagesFromFile(string imageFolder)
        {
            return Directory
                   .GetFiles(imageFolder, "*.*", SearchOption.AllDirectories)
                   .Where(Path.HasExtension);
        }
    }
}