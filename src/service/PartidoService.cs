using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.controller;
using Elecciones_Europeas.src.model.IPF;
using Elecciones_Europeas.src.repository;
using System.Collections.Generic;
using System.Drawing;


namespace Elecciones_Europeas.src.service
{
    internal class PartidoService : IBaseService<Partido, string>
    {
        public static PartidoService? instance;

        private PartidoRepository _rep;
        private static ConexionEntityFramework? _con;
        private PartidoService(ConexionEntityFramework con)
        {
            _con = con;
            this._rep = PartidoRepository.GetInstance(con);
        }

        public static PartidoService GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new PartidoService(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new PartidoService(con);
            }
            else if (!_con._database.Equals(con._database))
            {
                instance = new PartidoService(con);
            }
            return instance;
        }

        public List<Partido> FindAll()
        {
            return _rep.GetAll();
        }

        public Partido FindById(string id)
        {
            return _rep.GetById(id);
        }

        public Partido FindByName(string name)
        {
            return _rep.GetByName(name);
        }
    }
}
