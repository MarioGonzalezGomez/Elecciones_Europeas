using Elecciones.src.conexion;
using Elecciones.src.model.DTO;
using Elecciones.src.model;
using Elecciones.src.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.service
{
    public class MedioService
    {
        private MedioRepository repository;

        public MedioService(ConexionEntityFramework con)
        {
            this.repository = MedioRepository.GetInstance(con);
        }

        /// <summary>
        /// Obtiene todos los medios
        /// </summary>
        public List<MedioDTO> GetAllMedios()
        {
            try
            {
                List<Medio> medios = repository.GetAll();
                return medios.Select(m => MedioDTO.FromMedio(m)).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error en servicio de medios: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<MedioDTO>();
            }
        }

        /// <summary>
        /// Obtiene un medio por su código
        /// </summary>
        public MedioDTO GetMedioByCode(string codigo)
        {
            try
            {
                Medio medio = repository.FindByCode(codigo);
                return medio != null ? MedioDTO.FromMedio(medio) : null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los medios con sus descripciones ordenados
        /// </summary>
        public List<MedioDTO> GetAllMediosWithDescription()
        {
            try
            {
                List<Medio> medios = repository.GetAllWithDescription();
                return medios.Select(m => MedioDTO.FromMedio(m)).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medios: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<MedioDTO>();
            }
        }

        /// <summary>
        /// Crea un nuevo medio
        /// </summary>
        public void CreateMedio(string codigo, string descripcion, int comparar = 0)
        {
            try
            {
                Medio medio = new Medio(codigo, descripcion, comparar);
                repository.Insert(medio);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creando medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza un medio existente
        /// </summary>
        public void UpdateMedio(MedioDTO medioDTO)
        {
            try
            {
                Medio medio = new Medio(medioDTO.codigo, medioDTO.descripcion, medioDTO.comparar);
                repository.Update(medio);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error actualizando medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Elimina un medio
        /// </summary>
        public void DeleteMedio(string codigo)
        {
            try
            {
                repository.Delete(codigo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error eliminando medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
