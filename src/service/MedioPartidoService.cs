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
    public class MedioPartidoService
    {
        private MedioPartidoRepository repository;

        public MedioPartidoService(ConexionEntityFramework con)
        {
            this.repository = MedioPartidoRepository.GetInstance(con);
        }

        /// <summary>
        /// Obtiene todos los datos de medio-partido
        /// </summary>
        public List<MedioPartidoDTO> GetAllMedioPartido()
        {
            try
            {
                List<MedioPartido> medioPartidos = repository.GetAll();
                return medioPartidos.Select(mp => MedioPartidoDTO.FromMedioPartido(mp)).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo datos de medios-partidos: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<MedioPartidoDTO>();
            }
        }

        /// <summary>
        /// Obtiene datos de un medio para una circunscripción
        /// </summary>
        public List<MedioPartidoDTO> GetByMedioAndCircunscripcion(string codMedio, string codCircunscripcion)
        {
            try
            {
                List<MedioPartido> medioPartidos = repository.GetByMedioAndCircunscripcion(codMedio, codCircunscripcion);
                return medioPartidos.Select(mp => MedioPartidoDTO.FromMedioPartido(mp)).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo datos del medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<MedioPartidoDTO>();
            }
        }

        /// <summary>
        /// Obtiene todos los medios disponibles para una circunscripción
        /// </summary>
        public List<string> GetMediosByCircunscripcion(string codCircunscripcion)
        {
            try
            {
                return repository.GetMediosByCircunscripcion(codCircunscripcion);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medios: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<string>();
            }
        }

        /// <summary>
        /// Obtiene un dato específico de medio-partido
        /// </summary>
        public MedioPartidoDTO GetByKey(string codCircunscripcion, string codMedio, string codPartido)
        {
            try
            {
                MedioPartido medioPartido = repository.FindByKey(codCircunscripcion, codMedio, codPartido);
                return medioPartido != null ? MedioPartidoDTO.FromMedioPartido(medioPartido) : null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo dato de medio-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los partidos de un medio en una circunscripción
        /// </summary>
        public List<MedioPartidoDTO> GetPartidosByMedioAndCircunscripcion(string codMedio, string codCircunscripcion)
        {
            try
            {
                List<MedioPartido> medioPartidos = repository.GetPartidosByMedioAndCircunscripcion(codMedio, codCircunscripcion);
                return medioPartidos.Select(mp => MedioPartidoDTO.FromMedioPartido(mp)).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo partidos del medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<MedioPartidoDTO>();
            }
        }

        /// <summary>
        /// Crea un nuevo dato de medio-partido
        /// </summary>
        public void CreateMedioPartido(string codCircunscripcion, string codMedio, string codPartido, 
                                      int escaniosDesde, int escaniosHasta, decimal votos)
        {
            try
            {
                MedioPartido medioPartido = new MedioPartido(codCircunscripcion, codMedio, codPartido, 
                                                             escaniosDesde, escaniosHasta, votos);
                repository.Insert(medioPartido);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creando dato de medio-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza un dato de medio-partido existente
        /// </summary>
        public void UpdateMedioPartido(MedioPartidoDTO medioPartidoDTO)
        {
            try
            {
                MedioPartido medioPartido = new MedioPartido(medioPartidoDTO.codCircunscripcion, 
                                                             medioPartidoDTO.codMedio, 
                                                             medioPartidoDTO.codPartido, 
                                                             medioPartidoDTO.escaniosDesde, 
                                                             medioPartidoDTO.escaniosHasta, 
                                                             medioPartidoDTO.votos);
                repository.Update(medioPartido);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error actualizando dato de medio-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Elimina un dato de medio-partido
        /// </summary>
        public void DeleteMedioPartido(string codCircunscripcion, string codMedio, string codPartido)
        {
            try
            {
                repository.Delete(codCircunscripcion, codMedio, codPartido);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error eliminando dato de medio-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
