using System.Globalization;
using Elecciones.src.utils;

namespace Elecciones.src.mensajes.builders
{
    /// <summary>
    /// Clase base abstracta para los constructores de mensajes IPF.
    /// Contiene los métodos comunes de construcción de señales.
    /// </summary>
    public abstract class IPFMensajesBase
    {
        protected string _bd;
        protected ConfigManager configuration;

        protected IPFMensajesBase()
        {
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            _bd = configuration.GetValue("bdIPF");
        }

        #region Helpers

        /// <summary>
        /// Devuelve true si la fuente de vídeo del slot indicado está en modo DIRECTO (live).
        /// </summary>
        public bool IsVideoLive(int index)
        {
            if (index < 1 || index > 6) return false;
            var val = configuration.GetValue($"video{index}_isLive");
            if (string.IsNullOrEmpty(val)) return false;
            return val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Devuelve la ruta persistida para el slot de vídeo indicado (vacío si no existe).
        /// </summary>
        public string GetVideoPath(int index)
        {
            if (index < 1 || index > 6) return string.Empty;
            return configuration.GetValue($"video{index}_path") ?? string.Empty;
        }

        /// <summary>
        /// Helper para leer propiedades double de un DTO via reflection de forma segura.
        /// </summary>
        protected static double SafeGetDouble(object dto, string propName, double fallback = 0.0)
        {
            if (dto == null) return fallback;
            var prop = dto.GetType().GetProperty(propName);
            if (prop == null) return fallback;
            var val = prop.GetValue(dto);
            if (val == null) return fallback;
            if (val is double d) return d;
            if (double.TryParse(val.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            return fallback;
        }

        #endregion

        #region Constructores de Señales

        /// <summary>
        /// General constructor for itemset/itemgo/itemget. If using itemgo, pass tipoItem = 2 and provide animTime &amp; delay.
        /// values may be null for itemget or event-run itemset.
        /// </summary>
        protected string EventBuild(string objeto, string propiedad, string? values, int tipoItem, double animTime = 0.0, double delay = 0.0)
        {
            string animStr = animTime.ToString(CultureInfo.InvariantCulture);
            string delayStr = delay.ToString(CultureInfo.InvariantCulture);

            static string FormatValue(string raw)
            {
                if (raw == null) return "0";
                var trimmed = raw.Trim();

                if (trimmed.Length >= 2 &&
                    ((trimmed[0] == '\'' && trimmed[^1] == '\'') || (trimmed[0] == '\"' && trimmed[^1] == '\"')))
                {
                    return trimmed;
                }

                if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return trimmed;
                }

                if (double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _))
                {
                    return trimmed;
                }

                if (bool.TryParse(trimmed, out var b))
                {
                    return b ? "true" : "false";
                }

                if (trimmed.IndexOfAny(new[] { '(', ')', '[', ']', '{', '}', '/', '.', ',' }) >= 0)
                {
                    return trimmed;
                }

                return $"'{trimmed}'";
            }

            if (tipoItem == 1) // itemset
            {
                if (!string.IsNullOrEmpty(values))
                {
                    var formatted = FormatValue(values);
                    return $"itemset('<{_bd}>{objeto}','{propiedad}',{formatted});";
                }
                else
                {
                    return $"itemset('<{_bd}>{objeto}','{propiedad}');";
                }
            }

            if (tipoItem == 2) // itemgo (animation)
            {
                var val = string.IsNullOrEmpty(values) ? "0" : values;
                var formatted = FormatValue(val);
                return $"itemgo('<{_bd}>{objeto}','{propiedad}',{formatted},{animStr},{delayStr});";
            }

            if (tipoItem == 3) // itemget
            {
                return $"itemget('<{_bd}>{objeto}','{propiedad}');";
            }

            return $"itemset('<{_bd}>{objeto}','{propiedad}');";
        }

        protected string EventBuild(string objeto, string propiedad, string values, int tipoItem)
        {
            return EventBuild(objeto, propiedad, (string?)values, tipoItem, 0.0, 0.0);
        }

        protected string EventBuild(string objeto, string propiedad, int tipoItem)
        {
            return EventBuild(objeto, propiedad, null, tipoItem, 0.0, 0.0);
        }

        protected string EventBuild(string objeto, string propiedad)
        {
            return $"itemset('<{_bd}>{objeto}','{propiedad}');";
        }

        protected string CambioPipe(string objeto, string propiedad, string value)
        {
            return $"itemset('{objeto}','{propiedad}', '{value}');";
        }

        /// <summary>
        /// Build an EVENT_RUN signal. If animTime or delay are non-zero the signal will be an itemgo with placeholders,
        /// otherwise it will be a simple itemset acting as a "play" button.
        /// </summary>
        protected string EventRunBuild(string objeto, double animTime = 0.0, double delay = 0.0)
        {
            if (animTime != 0.0 || delay != 0.0)
            {
                return EventBuild(objeto, "EVENT_RUN", "0", 2, animTime, delay);
            }
            else
            {
                return EventBuild(objeto, "EVENT_RUN", null, 1);
            }
        }

        #endregion

        #region Mensajes Comunes

        public string Reset()
        {
            return EventRunBuild("RESET");
        }

        public string Prepara(string objeto)
        {
            return EventRunBuild($"{objeto}/PREPARA");
        }

        public string Entra(string objeto)
        {
            return EventRunBuild($"{objeto}/ENTRA");
        }

        public string Encadena(string objeto)
        {
            return EventRunBuild($"{objeto}/ENCADENA");
        }

        public string Oculta_Desoculta(bool ocultar, string objeto)
        {
            return ocultar ? EventBuild(objeto, "OBJ_CULL", "1", 1) : EventBuild(objeto, "OBJ_CULL", "0", 1);
        }

        public string CambiaTexto(string objeto, string texto)
        {
            return EventBuild(objeto, "TEXT_STRING", $"{texto}", 1);
        }

        public string Sale(string objeto)
        {
            return EventRunBuild($"{objeto}/SALE");
        }

        #endregion
    }
}
