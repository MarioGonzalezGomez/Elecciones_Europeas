using Elecciones.src.conexion;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System;
using System.Collections.Generic;


namespace Elecciones.src.controller
{
    internal class PartidoController
    {
        public static PartidoController? instance;

        private readonly PartidoService _service;
        private static ConexionEntityFramework? _con;

        private PartidoController(ConexionEntityFramework con)
        {
            _con = con;
            this._service = PartidoService.GetInstance(con);
        }

        public static PartidoController GetInstance(ConexionEntityFramework con)
        {
            if (instance == null || NeedsRecreation(con))
            {
                instance = new PartidoController(con);
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
