using Elecciones.src.conexion;
using Elecciones.src.model.DTO;
using Elecciones.src.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.controller
{
    public class MedioPartidoController
    {
        private MedioPartidoService service;

        public MedioPartidoController(ConexionEntityFramework con)
        {
            this.service = new MedioPartidoService(con);
        }

        /// <summary>
        /// Obtiene todos los datos de medios-partidos
        /// </summary>
        public List<MedioPartidoDTO> ObtenerTodosMedioPartido()
        {
            return service.GetAllMedioPartido();
        }

        /// <summary>
        /// Obtiene datos de un medio para una circunscripción específica
        /// </summary>
        public List<MedioPartidoDTO> ObtenerPorMedioYCircunscripcion(string codMedio, string codCircunscripcion)
        {
            return service.GetByMedioAndCircunscripcion(codMedio, codCircunscripcion);
        }

        /// <summary>
        /// Obtiene todos los medios disponibles para una circunscripción
        /// </summary>
        public List<string> ObtenerMediosPorCircunscripcion(string codCircunscripcion)
        {
            return service.GetMediosByCircunscripcion(codCircunscripcion);
        }

        /// <summary>
        /// Obtiene un dato específico de medio-partido
        /// </summary>
        public MedioPartidoDTO ObtenerPorClave(string codCircunscripcion, string codMedio, string codPartido)
        {
            return service.GetByKey(codCircunscripcion, codMedio, codPartido);
        }

        /// <summary>
        /// Obtiene todos los partidos de un medio en una circunscripción
        /// </summary>
        public List<MedioPartidoDTO> ObtenerPartidosPorMedioYCircunscripcion(string codMedio, string codCircunscripcion)
        {
            return service.GetPartidosByMedioAndCircunscripcion(codMedio, codCircunscripcion);
        }

        /// <summary>
        /// Crea un nuevo dato de medio-partido
        /// </summary>
        public void CrearMedioPartido(string codCircunscripcion, string codMedio, string codPartido,
                                      int escaniosDesde, int escaniosHasta, decimal votos)
        {
            service.CreateMedioPartido(codCircunscripcion, codMedio, codPartido, escaniosDesde, escaniosHasta, votos);
        }

        /// <summary>
        /// Actualiza un dato de medio-partido existente
        /// </summary>
        public void ActualizarMedioPartido(MedioPartidoDTO medioPartidoDTO)
        {
            service.UpdateMedioPartido(medioPartidoDTO);
        }

        /// <summary>
        /// Elimina un dato de medio-partido
        /// </summary>
        public void EliminarMedioPartido(string codCircunscripcion, string codMedio, string codPartido)
        {
            service.DeleteMedioPartido(codCircunscripcion, codMedio, codPartido);
        }
    }
}
