using Elecciones.src.conexion;
using Elecciones.src.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.repository
{
    public class MedioPartidoRepository
    {
        private ConexionEntityFramework conexion;
        private static MedioPartidoRepository? instance;

        private MedioPartidoRepository(ConexionEntityFramework con)
        {
            this.conexion = con;
        }

        public static MedioPartidoRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new MedioPartidoRepository(con);
            }
            return instance;
        }

        /// <summary>
        /// Obtiene todos los datos de medio-partido
        /// </summary>
        public List<MedioPartido> GetAll()
        {
            try
            {
                return conexion.Set<MedioPartido>().ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medios-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<MedioPartido>();
            }
        }

        /// <summary>
        /// Obtiene todos los datos de un medio en una circunscripción
        /// </summary>
        public List<MedioPartido> GetByMedioAndCircunscripcion(string codMedio, string codCircunscripcion)
        {
            try
            {
                return conexion.Set<MedioPartido>()
                    .Where(mp => mp.codMedio == codMedio && mp.codCircunscripcion == codCircunscripcion)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo datos del medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<MedioPartido>();
            }
        }

        /// <summary>
        /// Obtiene todos los medios disponibles para una circunscripción
        /// </summary>
        public List<string> GetMediosByCircunscripcion(string codCircunscripcion)
        {
            try
            {
                return conexion.Set<MedioPartido>()
                    .Where(mp => mp.codCircunscripcion == codCircunscripcion)
                    .Select(mp => mp.codMedio)
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medios: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<string>();
            }
        }

        /// <summary>
        /// Obtiene un medio-partido específico por su clave compuesta
        /// </summary>
        public MedioPartido FindByKey(string codCircunscripcion, string codMedio, string codPartido)
        {
            try
            {
                return conexion.Set<MedioPartido>()
                    .FirstOrDefault(mp => mp.codCircunscripcion == codCircunscripcion && 
                                         mp.codMedio == codMedio && 
                                         mp.codPartido == codPartido);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medio-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los partidos de un medio en una circunscripción
        /// </summary>
        public List<MedioPartido> GetPartidosByMedioAndCircunscripcion(string codMedio, string codCircunscripcion)
        {
            try
            {
                return conexion.Set<MedioPartido>()
                    .Where(mp => mp.codMedio == codMedio && mp.codCircunscripcion == codCircunscripcion)
                    .OrderBy(mp => mp.codPartido)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo datos del medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<MedioPartido>();
            }
        }

        /// <summary>
        /// Inserta un nuevo registro de medio-partido
        /// </summary>
        public void Insert(MedioPartido medioPartido)
        {
            try
            {
                conexion.Set<MedioPartido>().Add(medioPartido);
                conexion.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error insertando medio-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza un registro de medio-partido existente
        /// </summary>
        public void Update(MedioPartido medioPartido)
        {
            try
            {
                conexion.Set<MedioPartido>().Update(medioPartido);
                conexion.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error actualizando medio-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Elimina un registro de medio-partido
        /// </summary>
        public void Delete(string codCircunscripcion, string codMedio, string codPartido)
        {
            try
            {
                var medioPartido = FindByKey(codCircunscripcion, codMedio, codPartido);
                if (medioPartido != null)
                {
                    conexion.Set<MedioPartido>().Remove(medioPartido);
                    conexion.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error eliminando medio-partido: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
