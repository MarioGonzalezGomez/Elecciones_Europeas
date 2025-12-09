using System.Collections.Generic;
using System.IO;

namespace Elecciones_Europeas.src.utils
{
    public class ConfigManager
    {
        public static ConfigManager instance;
        private Dictionary<string, string> config;
        private string path;

        private ConfigManager(string ruta)
        {
            path = ruta;
            config = new Dictionary<string, string>();
        }

        public static ConfigManager GetInstance(string ruta = "config.ini")
        {
            if (instance == null)
            {
                instance = new ConfigManager(ruta);
            }
            return instance;
        }

        public string GetValue(string key)
        {
            if (config.ContainsKey(key))
                return config[key];
            return "";
        }

        public void SetValue(string key, string value)
        {
            config[key] = value;
        }

        public Dictionary<string, string> ReadConfig()
        {
            config.Clear();
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                // Skip empty or whitespace-only lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Skip comment lines
                if (line.TrimStart().StartsWith('#'))
                    continue;

                // Split only once and store result
                var parts = line.Split('=');
                
                // Validate that we have exactly 2 parts (key and value)
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    
                    // Only add if key is not empty
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        config[key] = value;
                    }
                }
            }
            return config;
        }

        public void SaveConfig()
        {
            string lines = "";
            foreach (var key in config.Keys)
            {
                var value = config[key];
                var line = $"{key}={value}\n";
                lines += line;
            }
          File.WriteAllText($"{path}", lines);
        }
    }
}
