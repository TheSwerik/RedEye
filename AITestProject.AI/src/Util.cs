using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AITestProject.AI
{
    public class Util
    {
        public static IEnumerable<string> GetImages(string folder)
        {
            return Directory
                .GetFiles(folder, "*.*", SearchOption.AllDirectories)
                .Where(Path.HasExtension);
        }
    }
}