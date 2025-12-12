using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System.Collections.Generic;
using System.Linq;


namespace Elecciones.src.repository
{
    internal class CPRepository : IRepository<CircunscripcionPartido, Clave>
    {
        public static CPRepository? instance;

        private static ConexionEntityFramework _con;

        private CPRepository(ConexionEntityFramework con)
        {
            _con = con;
        }

        public static CPRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new CPRepository(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new CPRepository(con);
            }
            else if (!_con._database.Equals(con._database))
            {
                instance = new CPRepository(con);
            }
            return instance;
        }

        public List<CircunscripcionPartido> GetAll()
        {
            return _con.Cps.ToList();
        }

        public CircunscripcionPartido GetById(Clave id)
        {
            return _con.Cps.Find(id);
        }
    }
}
