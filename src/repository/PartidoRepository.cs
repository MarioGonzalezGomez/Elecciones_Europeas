using Elecciones.src.conexion;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elecciones.src.repository
{
    internal class PartidoRepository : IRepository<Partido, string>
    {
        public static PartidoRepository? instance;

        private static ConexionEntityFramework? _con;

        private PartidoRepository(ConexionEntityFramework con)
        {
            _con = con;
        }

        public static PartidoRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null || NeedsRecreation(con))
            {
                instance = new PartidoRepository(con);
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

        private static ConexionEntityFramework GetConnectionOrThrow()
        {
            return _con ?? throw new InvalidOperationException("PartidoRepository no ha sido inicializado.");
        }

        public List<Partido> GetAll()
        {
            return GetConnectionOrThrow().Partidos.ToList();
        }

        public Partido GetById(string id)
        {
            return GetConnectionOrThrow().Partidos.Find(id)
                ?? throw new KeyNotFoundException($"No se ha encontrado el partido con id '{id}'.");
        }

        public Partido GetByName(string name)
        {
            return GetConnectionOrThrow().Partidos
                .FirstOrDefault(p => p.nombre.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? throw new KeyNotFoundException($"No se ha encontrado el partido '{name}'.");
        }
    }
}
