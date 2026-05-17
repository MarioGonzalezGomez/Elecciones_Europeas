using Elecciones.src.conexion;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System;
using System.Collections.Generic;

namespace Elecciones.src.controller
{
    internal class CPController
    {
        public static CPController? instance;
        private readonly CPService _service;
        private static ConexionEntityFramework? _con;

        private CPController(ConexionEntityFramework con)
        {
            _con = con;
            _service = CPService.GetInstance(con);
        }

        public static CPController GetInstance(ConexionEntityFramework con)
        {
            if (instance == null || NeedsRecreation(con))
            {
                instance = new CPController(con);
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

        public List<CircunscripcionPartido> FindAll()
        {
            return _service.FindAll();
        }

        // Datos de los partidos mas votados en cada autonomia
        public List<CircunscripcionPartido> FindMasVotadosAutonomiasOficial()
        {
            return _service.FindMasVotadosAutonomiasOficial();
        }

        public List<CircunscripcionPartido> FindMasVotadosAutonomiasSondeo()
        {
            return _service.FindMasVotadosAutonomiasSondeo();
        }

        // Datos de los partidos mas votados en cada provincia de una autonomia determinada
        public List<CircunscripcionPartido> FindMasVotadosProvinciasOficial(string codAutonomia)
        {
            return _service.FindMasVotadosProvinciasOficial(codAutonomia);
        }

        public List<CircunscripcionPartido> FindMasVotadosProvinciasSondeo(string codAutonomia)
        {
            return _service.FindMasVotadosProvinciasSondeo(codAutonomia);
        }

        // Datos de todos los partidos con representacion en una circunscripcion
        public List<CircunscripcionPartido> FindByIdCircunscripcionOficial(string cod)
        {
            return _service.FindByIdCircunscripcionOficial(cod);
        }

        public List<CircunscripcionPartido> FindByIdCircunscripcionSondeo(string cod)
        {
            return _service.FindByIdCircunscripcionSondeo(cod);
        }

        public List<CircunscripcionPartido> FindByIdCircunscripcionOficialSinFiltrar(string cod)
        {
            return _service.FindByIdCircunscripcionOficialSinFiltrar(cod);
        }

        public List<CircunscripcionPartido> FindByIdCircunscripcionSondeoSinFiltrar(string cod)
        {
            return _service.FindByIdCircunscripcionSondeoSinFiltrar(cod);
        }

        // Datos de un partido en las distintas autonomias
        public List<CircunscripcionPartido> FindPartidoPorAutonomiasOficial(string codPartido)
        {
            return _service.FindPartidoPorAutonomiasOficial(codPartido);
        }

        public List<CircunscripcionPartido> FindPartidoPorAutonomiasSondeo(string codPartido)
        {
            return _service.FindPartidoPorAutonomiasSondeo(codPartido);
        }

        // Datos de un partido en las provincias de una autonomia dada
        public List<CircunscripcionPartido> FindPartidoPorProvinciasOficial(string codAutonomia, string codPartido)
        {
            return _service.FindPartidoPorProvinciasOficial(codAutonomia, codPartido);
        }

        public List<CircunscripcionPartido> FindPartidoPorProvinciasSondeo(string codAutonomia, string codPartido)
        {
            return _service.FindPartidoPorProvinciasSondeo(codAutonomia, codPartido);
        }

        public CircunscripcionPartido FindById(Clave id)
        {
            return _service.FindById(id);
        }
    }
}
