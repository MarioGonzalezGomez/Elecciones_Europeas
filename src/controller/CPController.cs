using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.model.IPF;
using Elecciones_Europeas.src.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.controller
{
    internal class CPController
    {
        public static CPController? instance;
        private CPService _service;
        private static ConexionEntityFramework? _con;

        private CPController(ConexionEntityFramework con)
        {
            _con = con;
            this._service = CPService.GetInstance(con);
        }

        public static CPController GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new CPController(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new CPController(con);
            }
            else if (!_con._database.Equals(con._database))
            {
                instance = new CPController(con);
            }
            return instance;
        }

        public List<CircunscripcionPartido> FindAll()
        {
            return _service.FindAll();
        }
        //Datos de los partidos más votados en cada autonomía
        public List<CircunscripcionPartido> FindMasVotadosAutonomiasOficial()
        {
            return _service.FindMasVotadosAutonomiasOficial();
        }
        public List<CircunscripcionPartido> FindMasVotadosAutonomiasSondeo()
        {
            return _service.FindMasVotadosAutonomiasSondeo();
        }
        //Datos de los partidos más votados en cada provincia de una autonomía determinada
        public List<CircunscripcionPartido> FindMasVotadosProvinciasOficial(string codAutonomia)
        {
            return _service.FindMasVotadosProvinciasOficial(codAutonomia);
        }
        public List<CircunscripcionPartido> FindMasVotadosProvinciasSondeo(string codAutonomia)
        {
            return _service.FindMasVotadosProvinciasSondeo(codAutonomia);
        }

        //Datos de todos los partidos con representación en una circunscipción
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

        //Datos de un partido en las distintas autonomías
        public List<CircunscripcionPartido> FindPartidoPorAutonomiasOficial(string codPartido)
        {
            return _service.FindPartidoPorAutonomiasOficial(codPartido);
        }
        public List<CircunscripcionPartido> FindPartidoPorAutonomiasSondeo(string codPartido)
        {
            return _service.FindPartidoPorAutonomiasSondeo(codPartido);
        }

        //Datos de un partido en las provincias de una autonomía dada
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
