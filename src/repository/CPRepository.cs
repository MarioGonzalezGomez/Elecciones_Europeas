using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Elecciones.src.repository
{
    internal class CPRepository : IRepository<CircunscripcionPartido, Clave>
    {
        public static CPRepository? instance;

        private static ConexionEntityFramework? _con;

        private CPRepository(ConexionEntityFramework con)
        {
            _con = con;
        }

        public static CPRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null || NeedsRecreation(con))
            {
                instance = new CPRepository(con);
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
            return _con ?? throw new InvalidOperationException("CPRepository no ha sido inicializado.");
        }

        public List<CircunscripcionPartido> GetAll()
        {
            return GetConnectionOrThrow().Cps.ToList();
        }

        public CircunscripcionPartido GetById(Clave id)
        {
            return GetConnectionOrThrow().Cps.Find(id)
                ?? throw new KeyNotFoundException("No se ha encontrado la fila de CircunscripcionPartido para la clave indicada.");
        }
    }
}
