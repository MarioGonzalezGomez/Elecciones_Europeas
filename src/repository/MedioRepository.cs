using Elecciones.src.conexion;
using Elecciones.src.model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.repository
{
    public class MedioRepository
    {
        private ConexionEntityFramework conexion;
        public static MedioRepository? instance;
        private static ConexionEntityFramework? _con;

        private MedioRepository(ConexionEntityFramework con)
        {
            this.conexion = con;
            _con = con;
        }

        public static MedioRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null || NeedsRecreation(con))
            {
                instance = new MedioRepository(con);
            }
            return instance;
        }

        private static bool NeedsRecreation(ConexionEntityFramework con)
        {
            if (_con == null)
            {
                return true;
            }

            if (_con._tipoConexion != con._tipoConexion)
            {
                return true;
            }

            return !string.Equals(_con._database, con._database, StringComparison.Ordinal);
        }

        /// <summary>
        /// Obtiene todos los medios disponibles
        /// </summary>
        public List<Medio> GetAll()
        {
            try
            {
                return conexion.Set<Medio>().AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medios: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<Medio>();
            }
        }

        /// <summary>
        /// Obtiene un medio por su código
        /// </summary>
        public Medio? FindByCode(string codigo)
        {
            try
            {
                return conexion.Set<Medio>()
                    .AsNoTracking()
                    .FirstOrDefault(m => m.codigo == codigo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los medios con su descripción
        /// </summary>
        public List<Medio> GetAllWithDescription()
        {
            try
            {
                return conexion.Set<Medio>()
                    .AsNoTracking()
                    .OrderBy(m => m.codigo)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error obteniendo medios: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<Medio>();
            }
        }

        /// <summary>
        /// Inserta un nuevo medio
        /// </summary>
        public void Insert(Medio medio)
        {
            try
            {
                conexion.Set<Medio>().Add(medio);
                conexion.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error insertando medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza un medio existente
        /// </summary>
        public void Update(Medio medio)
        {
            try
            {
                conexion.Set<Medio>().Update(medio);
                conexion.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error actualizando medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Elimina un medio
        /// </summary>
        public void Delete(string codigo)
        {
            try
            {
                var medio = FindByCode(codigo);
                if (medio != null)
                {
                    conexion.Set<Medio>().Remove(medio);
                    conexion.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error eliminando medio: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
