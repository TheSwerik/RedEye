using System;
using System.Collections.Generic;
using System.IO;
using Emgu.CV.Cuda;
using Microsoft.VisualBasic.FileIO;

namespace RedEye.Util
{
    public static class Config
    {
        private static readonly Dictionary<string, string> Settings;
        public static readonly bool IsCudaEnabled;

        static Config()
        {
            Settings = new Dictionary<string, string>();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\RedEye\config.csv";
            using var parser = new TextFieldParser(path) {TextFieldType = FieldType.Delimited};
            parser.SetDelimiters(": ");
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                Settings.Add(fields[0], fields[1]);
            }

            IsCudaEnabled = GetBool("Cuda") && CudaInvoke.HasCuda;

            if (Get("ScreenshotLocation").ToLowerInvariant().Contains("default"))
                Set("ScreenshotLocation", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\RedEye\Screenshots");
            Directory.CreateDirectory(Get("ScreenshotLocation"));
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