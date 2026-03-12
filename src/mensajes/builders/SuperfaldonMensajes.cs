using System.Runtime.InteropServices;
using System.Text;

namespace Elecciones.src.mensajes.builders
{
    /// <summary>
    /// Clase especializada para señales de Superfaldón (pantalla completa con overlay).
    /// Incluye: Sedes SF, Fichas SF, Pactómetro SF, Mayorías SF, Bipartidismo SF, Ganador SF.
    /// </summary>
    internal class SuperfaldonMensajes : IPFMensajesBase
    {
        private static SuperfaldonMensajes? instance;

        private SuperfaldonMensajes() : base() { }

        public static SuperfaldonMensajes GetInstance()
        {
            instance ??= new SuperfaldonMensajes();
            return instance;
        }

        #region Carrusel (incluye sedes)

        public string superfaldonEntra(bool oficiales) => oficiales ? EventRunBuild("Superfaldon/Oficial/Entra") : EventRunBuild("Superfaldon/Sondeo/Entra");
        public string superfaldonSale(bool oficiales) => oficiales ? EventRunBuild("Superfaldon/Oficial/Sale") : EventRunBuild("Superfaldon/Sondeo/Sale");

        #endregion

        #region Sedes

        public string desplegarSede(string codPartido)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventBuild("codigoSede", "MAP_STRING_PAR", $"'{codPartido}'", 1));
            sb.Append(EventRunBuild("Superfaldon/Sedes/DespliegaSede"));
            return sb.ToString();
        }
        public string encadenarSede(string codPartido)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventBuild("nextCodigoSede", "MAP_STRING_PAR", $"'{codPartido}'", 1));
            sb.Append(EventRunBuild("Superfaldon/Sedes/PreparaEncadenaSede"));
            sb.Append(EventRunBuild("Superfaldon/Sedes/EncadenaSede"));
            return sb.ToString();
        }
        public string replegarSede(string codPartido)
        {
            return EventRunBuild("Superfaldon/Sedes/RepliegaSede");
        }

        #endregion

        #region Actualiza

        public string sfFichasSale() => EventRunBuild("ReloadPartidos");

        #endregion

        #region Ultimo

        public string ultimoEntra() => EventRunBuild("ULTIMO/Entra");
        public string ultimoSale() => EventRunBuild("ULTIMO/Sale");

        #endregion

        #region Pactómetro Superfaldón

        public string pactometroEntra() => EventRunBuild("PACTOMETRO/Entra");
        public string pactometroReinicio() => EventRunBuild("PACTOMETRO/Prepara");
        public string pactometroSale() => EventRunBuild("PACTOMETRO/Sale");

        public string pactometroPartidoEntra(bool oficiales, string codPartido, bool izq)
        {
            StringBuilder sb = new StringBuilder();
            string sondeo = oficiales ? "" : "Sondeo";
            string lado = izq ? "Izq" : "Der";
            bool primerPartido = false;

            if (primerPartido)
            {
                sb.Append(EventBuild($"PACTOMETRO/PrimerPartido{lado}{sondeo}", "MAP_STRING_PAR", $"'{codPartido}'", 1));
            }
            else
            {
                sb.Append(EventBuild($"PACTOMETRO/SiguientePartido{lado}{sondeo}", "MAP_STRING_PAR", $"'{codPartido}'", 1));
            }

            sb.Append(EventBuild($"PACTOMETRO/CurrentPartidos{lado}{sondeo}", "MAP_INT_PAR", numPartidosEnEseLado, 1));
            sb.Append(EventBuild($"PACTOMETRO/Escanos{lado}", "MAP_INT_PAR", acumuladoLado, 1));
            if (!oficiales)
            {
                sb.Append(EventBuild($"PACTOMETRO/Escanos{lado}Hasta", "MAP_INT_PAR", acumuladoLadoHasta, 1));
            }

            return sb.ToString();
        }


        #endregion

        #region CCAA

        public string CCAAEntra() => EventRunBuild("CCAA/Entra");
        public string CCAAESale() => EventRunBuild("CCAA/Entra");

        #endregion

        #region Escrutado

        public string EscrutadoEntra() => EventRunBuild("Escrutado/Entra");
        public string EscrutadoSale() => EventRunBuild("Escrutado/Sale");

        #endregion
    }
}
