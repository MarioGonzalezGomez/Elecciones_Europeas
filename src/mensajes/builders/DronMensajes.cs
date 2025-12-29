namespace Elecciones.src.mensajes.builders
{
    /// <summary>
    /// Clase especializada para señales de Dron (futuro).
    /// Preparada para futuras implementaciones de gráficos de Dron.
    /// </summary>
    internal class DronMensajes : IPFMensajesBase
    {
        private static DronMensajes? instance;

        private DronMensajes() : base() { }

        public static DronMensajes GetInstance()
        {
            instance ??= new DronMensajes();
            return instance;
        }

        // Este espacio está reservado para futuras implementaciones
        // de señales específicas para gráficos de Dron.
    }
}
