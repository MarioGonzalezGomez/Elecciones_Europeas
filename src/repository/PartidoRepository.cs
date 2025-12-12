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

        private static ConexionEntityFramework _con;

        private PartidoRepository(ConexionEntityFramework con)
        {
            _con = con;
        }

        public static PartidoRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new PartidoRepository(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new PartidoRepository(con);
            }
            else if (!_con._database.Equals(con._database))
            {
                instance = new PartidoRepository(con);
            }
            return instance;
        }

        public List<Partido> GetAll()
        {
            return _con.Partidos.ToList();
        }

        public Partido GetById(string id)
        {
            return _con.Partidos.Find(id);
        }
        public Partido GetByName(string name)
        {
            return _con.Partidos.FirstOrDefault(p => p.nombre.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
