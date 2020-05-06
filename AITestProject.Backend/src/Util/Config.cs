using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

namespace AITestProject.Util
{
    public class Config
    {
        private const string Path = "config.csv";
        private static readonly Dictionary<string, string> Settings;

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
        }

        public static void Set(string setting, string value) => Settings[setting] = value;
        public static string Get(string setting) => Settings[setting];
        public static int GetInt(string setting) => int.Parse(Settings[setting]);
        public static bool GetBool(string setting) => bool.Parse(Settings[setting]);
    }
}