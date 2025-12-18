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
                    // Create a reasonable default config file with common keys and the new horaAvance keys.
                    var defaultContent = new StringBuilder();
                    defaultContent.AppendLine("# Elecciones - config.ini");
                    defaultContent.AppendLine("# If you edit this file, keep KEY=VALUE per line");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("ipPrime=127.0.0.1");
                    defaultContent.AppendLine("puertoPrime=8080");
                    defaultContent.AppendLine("activoPrime=0");
                    defaultContent.AppendLine("ipIPF=172.28.51.26");
                    defaultContent.AppendLine("puertoIPF=5123");
                    defaultContent.AppendLine("bdIPF=Cartones");
                    defaultContent.AppendLine("activoIPF=1");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# Conexión IPF secundaria (señales puntuales a equipo con Faldones)");
                    defaultContent.AppendLine("ipIPF2=127.0.0.1");
                    defaultContent.AppendLine("puertoIPF2=5124");
                    defaultContent.AppendLine("bdIPF2=FALDONES2");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("dataServer=172.28.51.21");
                    defaultContent.AppendLine("dataServerBackup=172.28.51.22");
                    defaultContent.AppendLine("dataPort=3306");
                    defaultContent.AppendLine("dataDB1=elecciones_extremadura_2025");
                    defaultContent.AppendLine("conexionDefault1=1");
                    defaultContent.AppendLine("dataDB2=");
                    defaultContent.AppendLine("conexionDefault2=1");
                    defaultContent.AppendLine("dataDB3=");
                    defaultContent.AppendLine("conexionDefault3=1");
                    defaultContent.AppendLine("user=root");
                    defaultContent.AppendLine("password=auto1041");
                    defaultContent.AppendLine("rutaArchivos=C:\\Elecciones\\Datos");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# 0 Para poner rutas sin crear subcarpetar JSON,CSV -> 1 Para crearlas");
                    defaultContent.AppendLine("subcarpetas=1");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# Nivel Nacional  2-Autonomica ");
                    defaultContent.AppendLine("tipoElecciones=2");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# Modifica los pares de botones de Oficial/Sondeo, para variar entre elecciones");
                    defaultContent.AppendLine("numEleccionesSimultaneas=1");
                    defaultContent.AppendLine("tablasGraficosPrincipal=2");
                    defaultContent.AppendLine("headerTabla1=FALDÓN");
                    defaultContent.AppendLine("headerTabla2=CARTÓN");
                    defaultContent.AppendLine("headerTabla3=SUPERFALDÓN");
                    defaultContent.AppendLine("headerTabla4=PANTALLA");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# Theme por defecto: 1 Light, 2 Dark, 3 Blue");
                    defaultContent.AppendLine("theme=1");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# Hace que la búsqueda de circunscripciones disponibles se haga filtrada (0) o completa (1)");
                    defaultContent.AppendLine("regional=0");
                    defaultContent.AppendLine("codigoRegionalBD1=10");
                    defaultContent.AppendLine("codigoRegionalBD2=00");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# 0 inactiva   1 activa");
                    defaultContent.AppendLine("botoneraExtra=1");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# Horas configurables para avances de participacion (editable)");
                    defaultContent.AppendLine("# Formato libre (por ejemplo 20:15), dejar vacío si no se quiere mostrar");
                    defaultContent.AppendLine("horaAvance1=");
                    defaultContent.AppendLine("horaAvance2=");
                    defaultContent.AppendLine("horaAvance3=");
                    defaultContent.AppendLine("horaParticipacion=");
                    defaultContent.AppendLine();
                    defaultContent.AppendLine("# Horas históricas (pueden diferir de las horas actuales)");
                    defaultContent.AppendLine("horaAvance1Historico=");
                    defaultContent.AppendLine("horaAvance2Historico=");
                    defaultContent.AppendLine("horaAvance3Historico=");
                    defaultContent.AppendLine("horaParticipacionHistorico=");
                    defaultContent.AppendLine();

                    File.WriteAllText(path, defaultContent.ToString());
                }
                // Try to load existing content
                ReadConfig();

                // Ensure newly-introduced keys exist in the in-memory config (without overwriting existing values)
                var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"horaAvance1", "" },
                    {"horaAvance2", "" },
                    {"horaAvance3", "" },
                    {"horaParticipacion", "" },
                    {"horaAvance1Historico", "" },
                    {"horaAvance2Historico", "" },
                    {"horaAvance3Historico", "" },
                    { "horaParticipacionHistorico", "" }
                };

                foreach (var kv in defaults)
                {
                    if (!config.ContainsKey(kv.Key))
                    {
                        config[kv.Key] = kv.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to ensure config file '{path}' exists", ex);
            }
        }
    }
}
