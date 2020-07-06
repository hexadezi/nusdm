using System.IO;
using System.Text.Json;

namespace nusdm
{
    public class Settings
    {
        public string CommonKey { get; set; } = "";
        public string TitleFile{ get; set; } = "json";
        public string NintendoBaseUrl{ get; set; } = "http://ccs.cdn.c.shop.nintendowifi.net/ccs/download/";
    }


    public static class SettingsProvider
    {
        public static Settings Settings { get; set; } = null;
        public static string Savefile { get; set; } = "app.config";

        static SettingsProvider()
        {
            if (!File.Exists(Savefile))
            {
                Settings = new Settings();
                Save();
            }
            if (Settings == null)
            {
                Load();
                Save();
            }
        }

        public static void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(Settings, options);
            File.WriteAllText(Savefile, jsonString);
        }

        public static void Load()
        {
            string jsonString = File.ReadAllText(Savefile);
            Settings = JsonSerializer.Deserialize<Settings>(jsonString);
        }
    }

}
