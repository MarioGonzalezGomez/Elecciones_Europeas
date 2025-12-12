using Elecciones.src.conexion;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System.Collections.Generic;


namespace Elecciones.src.controller
{
    internal class PartidoController
    {
        public static PartidoController? instance;

        private PartidoService _service;
        private static ConexionEntityFramework? _con;

        private PartidoController(ConexionEntityFramework con)
        {
            _con = con;
            this._service = PartidoService.GetInstance(con);
        }

        public static PartidoController GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new PartidoController(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new PartidoController(con);
            }
            else if (!_con._database.Equals(con._database))
            {
                instance = new PartidoController(con);
            }
            return instance;
        }

        public List<Partido> FindAll()
        {
            return _service.FindAll();
        }

        public Partido FindById(string id)
        {
            return _service.FindById(id);
        }

        public Partido FindByName(string name)
        {
            return _service.FindByName(name);
        }
    }
}
