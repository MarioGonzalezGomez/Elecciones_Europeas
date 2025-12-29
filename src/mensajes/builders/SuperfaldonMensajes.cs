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

        #region Superfaldón Base

        public string superfaldonEntra() => Entra("SUPERFALDON");
        public string superfaldonSale() => Sale("SUPERFALDON");

        #endregion

        #region Último Superfaldón

        public string ultimoSuperEntra()
        {
            string signal = "";
            signal += EventBuild("Oficial_Codigo", "MAP_LLSTRING_LOAD") + "\n";
            signal += EventBuild("UltimoEscanoCSV", "MAP_LLSTRING_LOAD") + "\n";
            signal += Entra("ULTIMOESCANO") + "\n";
            return signal;
        }

        public string ultimoSuperSale() => Sale("ULTIMOESCANO");

        #endregion

        #region Sedes Superfaldón

        public string superfaldonSedesEntra() => Entra("SEDES");
        public string superfaldonSedesEncadena() => Entra("SEDES/ENCADENA");
        public string superfaldonSedesSale() => Sale("SEDES");

        #endregion

        #region Fichas Superfaldón

        // TODO: Construir señal para entrada del gráfico FICHAS en SUPERFALDÓN
        public string sfFichasEntra() => "";

        // TODO: Construir señal para encadenar entre gráficos FICHAS en SUPERFALDÓN
        public string sfFichasEncadena() => "";

        // TODO: Construir señal para salida del gráfico FICHAS en SUPERFALDÓN
        public string sfFichasSale() => "";

        #endregion

        #region Pactómetro Superfaldón

        // TODO: Construir señal para entrada del gráfico PACTÓMETRO en SUPERFALDÓN
        public string sfPactometroEntra() => "";

        // TODO: Construir señal para encadenar entre gráficos PACTÓMETRO en SUPERFALDÓN
        public string sfPactometroEncadena() => "";

        // TODO: Construir señal para salida del gráfico PACTÓMETRO en SUPERFALDÓN
        public string sfPactometroSale() => "";

        #endregion

        #region Mayorías Superfaldón

        // TODO: Construir señal para entrada del gráfico MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasEntra() => "";

        // TODO: Construir señal para encadenar entre gráficos MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasEncadena() => "";

        // TODO: Construir señal para salida del gráfico MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasSale() => "";

        #endregion

        #region Bipartidismo Superfaldón

        // TODO: Construir señal para entrada del gráfico BIPARTIDISMO en SUPERFALDÓN
        public string sfBipartidismoEntra() => "";

        // TODO: Construir señal para encadenar entre gráficos BIPARTIDISMO en SUPERFALDÓN
        public string sfBipartidismoEncadena() => "";

        // TODO: Construir señal para salida del gráfico BIPARTIDISMO en SUPERFALDÓN
        public string sfBipartidismoSale() => "";

        #endregion

        #region Ganador Superfaldón

        // TODO: Construir señal para entrada del gráfico GANADOR en SUPERFALDÓN
        public string sfGanadorEntra() => "";

        // TODO: Construir señal para encadenar entre gráficos GANADOR en SUPERFALDÓN
        public string sfGanadorEncadena() => "";

        // TODO: Construir señal para salida del gráfico GANADOR en SUPERFALDÓN
        public string sfGanadorSale() => "";

        #endregion
    }
}
