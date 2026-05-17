using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.logic.comparators;
using Elecciones.src.model.IPF;
using Elecciones.src.repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elecciones.src.service
{
    internal class CPService : IBaseService<CircunscripcionPartido, Clave>
    {
        public static CPService? instance;

        private readonly CPRepository _rep;
        private static ConexionEntityFramework? _con;

        private CPService(ConexionEntityFramework con)
        {
            _con = con;
            _rep = CPRepository.GetInstance(con);
        }

        public static CPService GetInstance(ConexionEntityFramework con)
        {
            if (instance == null || NeedsRecreation(con))
            {
                instance = new CPService(con);
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
            return _rep.GetAll();
        }

        //Datos de los partidos mas votados en cada autonomia
        public List<CircunscripcionPartido> FindMasVotadosAutonomiasOficial()
        {
            return FindAll()
                .Where(cp => cp.codCircunscripcion.EndsWith("00000"))
                .Where(cp => !cp.codCircunscripcion.StartsWith("99"))
                .GroupBy(cp => cp.codCircunscripcion)
                .Select(group => group.OrderByDescending(cp => cp, new CPComparerOficial()).First())
                .ToList();
        }

        public List<CircunscripcionPartido> FindMasVotadosAutonomiasSondeo()
        {
            return FindAll()
                .Where(cp => cp.codCircunscripcion.EndsWith("00000"))
                .Where(cp => !cp.codCircunscripcion.StartsWith("99"))
                .GroupBy(cp => cp.codCircunscripcion)
                .Select(group => group.OrderByDescending(cp => cp, new CPComparerSondeo()).First())
                .ToList();
        }

        //Datos de los partidos mas votados en cada provincia de una autonomia determinada
        public List<CircunscripcionPartido> FindMasVotadosProvinciasOficial(string codAutonomia)
        {
            return FindAll()
                .Where(cp => cp.codCircunscripcion.StartsWith(codAutonomia))
                .Where(cp => cp.codCircunscripcion.EndsWith("000") && !cp.codCircunscripcion.EndsWith("00000"))
                .GroupBy(cp => cp.codCircunscripcion)
                .Select(group => group.OrderByDescending(cp => cp, new CPComparerOficial()).First())
                .ToList();
        }

        public List<CircunscripcionPartido> FindMasVotadosProvinciasSondeo(string codAutonomia)
        {
            return FindAll()
                .Where(cp => cp.codCircunscripcion.StartsWith(codAutonomia))
                .Where(cp => cp.codCircunscripcion.EndsWith("000") && !cp.codCircunscripcion.EndsWith("00000"))
                .GroupBy(cp => cp.codCircunscripcion)
                .Select(group => group.OrderByDescending(cp => cp, new CPComparerSondeo()).First())
                .ToList();
        }

        public CircunscripcionPartido FindById(Clave id)
        {
            return _rep.GetById(id);
        }

        //Datos de todos los partidos con representacion en una circunscripcion
        private List<CircunscripcionPartido> FindByIdCircunscripcion(string cod)
        {
            return FindAll().Where(cp => cp.codCircunscripcion == cod).ToList();
        }

        public List<CircunscripcionPartido> FindByIdCircunscripcionOficial(string cod)
        {
            return FindByIdCircunscripcion(cod)
                 .Where(cp => cp.escanios > 0)
                 .OrderByDescending(cp => cp, new CPComparerOficial())
                 .ToList();
        }

        public List<CircunscripcionPartido> FindByIdCircunscripcionSondeo(string cod)
        {
            return FindByIdCircunscripcion(cod)
                 .Where(cp => cp.escaniosHastaSondeo > 0)
                 .OrderByDescending(cp => cp, new CPComparerSondeo())
                 .ToList();
        }

        public List<CircunscripcionPartido> FindByIdCircunscripcionOficialSinFiltrar(string cod)
        {
            return FindByIdCircunscripcion(cod)
                 .OrderByDescending(cp => cp, new CPComparerOficial())
                 .ToList();
        }

        public List<CircunscripcionPartido> FindByIdCircunscripcionSondeoSinFiltrar(string cod)
        {
            return FindByIdCircunscripcion(cod)
                 .OrderByDescending(cp => cp, new CPComparerSondeo())
                 .ToList();
        }

        private List<CircunscripcionPartido> FindByIdPartido(string cod)
        {
            return FindAll().Where(cp => cp.codPartido == cod).ToList();
        }

        //Datos de un partido en las distintas autonomias
        public List<CircunscripcionPartido> FindPartidoPorAutonomiasOficial(string codPartido)
        {
            return FindByIdPartido(codPartido)
                .Where(cp => cp.codCircunscripcion.EndsWith("00000"))
                .Where(cp => !cp.codCircunscripcion.StartsWith("99"))
                .OrderByDescending(cp => cp, new CPComparerOficial())
                .ToList();
        }

        public List<CircunscripcionPartido> FindPartidoPorAutonomiasSondeo(string codPartido)
        {
            return FindByIdPartido(codPartido)
                .Where(cp => cp.codCircunscripcion.EndsWith("00000"))
                .Where(cp => !cp.codCircunscripcion.StartsWith("99"))
                .OrderByDescending(cp => cp, new CPComparerSondeo())
                .ToList();
        }

        //Datos de un partido en las provincias de una autonomia dada
        public List<CircunscripcionPartido> FindPartidoPorProvinciasOficial(string codAutonomia, string codPartido)
        {
            return FindByIdPartido(codPartido)
                .Where(cp => cp.codCircunscripcion.StartsWith(codAutonomia))
                .Where(cp => cp.codCircunscripcion.EndsWith("000") && !cp.codCircunscripcion.EndsWith("00000"))
                .OrderByDescending(cp => cp, new CPComparerOficial())
                .ToList();
        }

        public List<CircunscripcionPartido> FindPartidoPorProvinciasSondeo(string codAutonomia, string codPartido)
        {
            return FindByIdPartido(codPartido)
                .Where(cp => cp.codCircunscripcion.StartsWith(codAutonomia))
                .Where(cp => cp.codCircunscripcion.EndsWith("000") && !cp.codCircunscripcion.EndsWith("00000"))
                .OrderByDescending(cp => cp, new CPComparerSondeo())
                .ToList();
        }
    }
}
