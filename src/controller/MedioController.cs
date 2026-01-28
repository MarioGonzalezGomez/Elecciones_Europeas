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
    public class MedioController
    {
        private MedioService service;

        public MedioController(ConexionEntityFramework con)
        {
            this.service = new MedioService(con);
        }

        /// <summary>
        /// Obtiene todos los medios disponibles
        /// </summary>
        public List<MedioDTO> ObtenerTodosMedios()
        {
            return service.GetAllMedios();
        }

        /// <summary>
        /// Obtiene un medio específico por su código
        /// </summary>
        public MedioDTO ObtenerMedioPorCodigo(string codigo)
        {
            return service.GetMedioByCode(codigo);
        }

        /// <summary>
        /// Obtiene todos los medios con sus descripciones
        /// </summary>
        public List<MedioDTO> ObtenerMediosConDescripcion()
        {
            return service.GetAllMediosWithDescription();
        }

        /// <summary>
        /// Crea un nuevo medio
        /// </summary>
        public void CrearMedio(string codigo, string descripcion, int comparar = 0)
        {
            service.CreateMedio(codigo, descripcion, comparar);
        }

        /// <summary>
        /// Actualiza un medio existente
        /// </summary>
        public void ActualizarMedio(MedioDTO medioDTO)
        {
            service.UpdateMedio(medioDTO);
        }

        /// <summary>
        /// Elimina un medio
        /// </summary>
        public void EliminarMedio(string codigo)
        {
            service.DeleteMedio(codigo);
        }
    }
}
