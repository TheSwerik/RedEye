using System.Collections.Generic;
using Emgu.CV.Cuda;
using Microsoft.VisualBasic.FileIO;

namespace RedEye.Util
{
    public class Config
    {
        private const string Path = "config.csv";
        private static readonly Dictionary<string, string> Settings;
        public static readonly bool IsCudaEnabled;

        static Config()
        {
            Settings = new Dictionary<string, string>();
            using var parser = new TextFieldParser(Path) {TextFieldType = FieldType.Delimited};
            parser.SetDelimiters(": ");
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                Settings.Add(fields[0], fields[1]);
            }

            IsCudaEnabled = GetBool("Cuda") && CudaInvoke.HasCuda;
        }

        public static void Set(string setting, string value)
        {
            Settings[setting] = value;
        }

        public static string Get(string setting)
        {
            return Settings[setting];
        }

        public static int GetInt(string setting)
        {
            return int.Parse(Settings[setting]);
        }

        public static bool GetBool(string setting)
        {
            return bool.Parse(Settings[setting]);
        }
    }
}