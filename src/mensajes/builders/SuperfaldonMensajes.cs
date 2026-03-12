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

        #region Superfaldón

        public string superfaldonEntra(bool oficiales) => oficiales?EventRunBuild("Superfaldon/Oficial/Entra") : EventRunBuild("Superfaldon/Sondeo/Entra");
        public string superfaldonSale(bool oficiales) => oficiales ? EventRunBuild("Superfaldon/Oficial/Sale") : EventRunBuild("Superfaldon/Sondeo/Sale");

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

        #region Sedes

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
