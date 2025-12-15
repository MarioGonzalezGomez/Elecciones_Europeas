using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Elecciones.src.service;

namespace Elecciones.src.utils
{
    public class ConfigManager
    {
        private static ConfigManager instance;
        private static readonly object _lock = new();
        private Dictionary<string, string> config;
        private string path;
        private readonly FileLoggerService _logger = FileLoggerService.GetInstance();

        private ConfigManager(string ruta)
        {
            // If ruta is null/empty, use executable folder
            if (string.IsNullOrWhiteSpace(ruta))
            {
                ruta = Path.Combine(AppContext.BaseDirectory, "config.ini");
            }

            path = Path.GetFullPath(ruta);
            config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            EnsureConfigExists();
        }

        public static ConfigManager GetInstance(string ruta = null)
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = new ConfigManager(ruta);
                }
            }
            return instance;
        }

        public string GetValue(string key)
        {
            if (config.ContainsKey(key))
                return config[key] ?? string.Empty;
            return string.Empty;
        }

        public void SetValue(string key, string value)
        {
            config[key] = value ?? string.Empty;
        }

        public Dictionary<string, string> ReadConfig()
        {
            config.Clear();

            try
            {
                if (!File.Exists(path))
                {
                    _logger.LogInfo($"Config file not found at '{path}', using empty config.");
                    return config;
                }

                var lines = File.ReadAllLines(path);
                foreach (var rawLine in lines)
                {
                    // Skip empty or whitespace-only lines
                    if (string.IsNullOrWhiteSpace(rawLine))
                        continue;

                    // Skip comment lines starting with #
                    var line = rawLine.Trim();
                    if (line.StartsWith('#'))
                        continue;

                    // Split at first '=' only
                    int idx = line.IndexOf('=');
                    if (idx <= 0)
                        continue;

                    var key = line.Substring(0, idx).Trim();
                    var value = line.Substring(idx + 1).Trim();

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        config[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed reading config file '{path}'", ex);
            }

            return config;
        }

        public void SaveConfig()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var kvp in config)
                {
                    sb.AppendLine($"{kvp.Key}={kvp.Value}");
                }
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? AppContext.BaseDirectory);
                File.WriteAllText(path, sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed saving config file '{path}'", ex);
            }
        }

        private void EnsureConfigExists()
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(path))
                {
                    // Create a minimal default config file with a comment header
                    var defaultContent = new StringBuilder();
                    defaultContent.AppendLine("# Elecciones - config.ini");
                    defaultContent.AppendLine("# If you edit this file, keep KEY=VALUE per line");
                    File.WriteAllText(path, defaultContent.ToString());
                }
                // Try to load existing content
                ReadConfig();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to ensure config file '{path}' exists", ex);
            }
        }
    }
}
