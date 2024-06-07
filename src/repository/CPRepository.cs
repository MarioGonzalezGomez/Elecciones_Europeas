using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.controller;
using Elecciones_Europeas.src.model.IPF;
using Elecciones_Europeas.src.service;
using System.Collections.Generic;
using System.Linq;


namespace Elecciones_Europeas.src.repository
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
