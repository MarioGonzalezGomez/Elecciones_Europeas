using Elecciones.src.conexion;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.controller
{
    internal class BrainStormController
    {
        public static BrainStormController? instance;

        private CircunscripcionController circunscripcionController;
        private CPController cpController;
        private PartidoController partidoController;
        private List<Partido> partidos;
        private static ConexionEntityFramework? _con;

        private BrainStormController(ConexionEntityFramework con)
        {
            _con = con;
            circunscripcionController = CircunscripcionController.GetInstance(con);
            cpController = CPController.GetInstance(con);
            partidoController = PartidoController.GetInstance(con);
            partidos = partidoController.FindAll();
        }

        public static BrainStormController GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new BrainStormController(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new BrainStormController(con);
            }
            else if (!_con._database.Equals(con._database))
            {
                instance = new BrainStormController(con);
            }
            return instance;
        }

        /// <summary>
        /// Obtiene los partidos más votados por autonomías (datos oficiales).
        /// NOTA: Este método es para elecciones NACIONALES únicamente (tipoElecciones != 2).
        /// Consulta el código nacional 9900000.
        /// </summary>
        public BrainStormDTO FindMasVotadosAutonomiasOficial(int avanceActual)
        {
            Circunscripcion esp = circunscripcionController.FindById("9900000");
            List<CircunscripcionPartido> cps = cpController.FindMasVotadosAutonomiasOficial();
            BrainStormDTO dto = new BrainStormDTO(esp, avanceActual, 1, cps, true, _con);
            return dto;
        }
        /// <summary>
        /// Obtiene los partidos más votados por autonomías (datos de sondeo).
        /// NOTA: Este método es para elecciones NACIONALES únicamente (tipoElecciones != 2).
        /// Consulta el código nacional 9900000.
        /// </summary>
        public BrainStormDTO FindMasVotadosAutonomiasSondeo(int avanceActual)
        {
            Circunscripcion esp = circunscripcionController.FindById("9900000");
            List<CircunscripcionPartido> cps = cpController.FindMasVotadosAutonomiasSondeo();
            BrainStormDTO dto = new BrainStormDTO(esp, avanceActual, 1, cps, false, _con);
            return dto;
        }

        public BrainStormDTO FindMasVotadosProvinciasOficial(string codAutonomia, int avanceActual, int tipoElecciones)
        {
            Circunscripcion autonomia = circunscripcionController.FindByName(codAutonomia);
            List<CircunscripcionPartido> cps = cpController.FindMasVotadosProvinciasOficial(codAutonomia);
            BrainStormDTO dto = new BrainStormDTO(autonomia, avanceActual, tipoElecciones, cps, true, _con);
            return dto;
        }
        public BrainStormDTO FindMasVotadosProvinciasSondeo(string codAutonomia, int avanceActual, int tipoElecciones)
        {
            Circunscripcion autonomia = circunscripcionController.FindByName(codAutonomia);
            List<CircunscripcionPartido> cps = cpController.FindMasVotadosProvinciasSondeo(codAutonomia);
            BrainStormDTO dto = new BrainStormDTO(autonomia, avanceActual, tipoElecciones, cps, false, _con);
            return dto;
        }

        public BrainStormDTO FindByIdCircunscripcionOficial(string cod, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindById(cod);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionOficial(cod);
            BrainStormDTO dto = new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _con);
            return dto;
        }
        public BrainStormDTO FindByIdCircunscripcionSondeo(string cod, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindById(cod);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionSondeo(cod);
            BrainStormDTO dto = new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _con);
            return dto;
        }

        public BrainStormDTO FindByNameCircunscripcionOficial(string nombreCircunscripcion, int avanceActual, int tipoElecciones)
        {

            Circunscripcion circunscripcion = circunscripcionController.FindByName(nombreCircunscripcion);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionOficial(circunscripcion.codigo);
            BrainStormDTO dto = new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _con);
            return dto;
        }
        public BrainStormDTO FindByNameCircunscripcionSondeo(string nombreCircunscripcion, int avanceActual, int tipoElecciones)
        {

            Circunscripcion circunscripcion = circunscripcionController.FindByName(nombreCircunscripcion);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionSondeo(circunscripcion.codigo);
            BrainStormDTO dto = new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _con);
            return dto;
        }
        public BrainStormDTO FindByNameCircunscripcionOficialSinFiltrar(string nombreCircunscripcion, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByName(nombreCircunscripcion);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionOficialSinFiltrar(circunscripcion.codigo);
            BrainStormDTO dto = new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _con);
            return dto;
        }
        public BrainStormDTO FindByNameCircunscripcionSondeoSinFiltrar(string nombreCircunscripcion, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByName(nombreCircunscripcion);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionSondeoSinFiltrar(circunscripcion.codigo);
            BrainStormDTO dto = new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _con);
            return dto;
        }

        /// <summary>
        /// Obtiene datos de un partido específico por autonomías (datos oficiales).
        /// NOTA: Este método es para elecciones NACIONALES únicamente (tipoElecciones != 2).
        /// Consulta el código nacional 9900000.
        /// </summary>
        public BrainStormDTO FindPartidoPorAutonomiasOficial(string codPartido, int avanceActual)
        {
            Circunscripcion esp = circunscripcionController.FindById("9900000");
            List<CircunscripcionPartido> cps = cpController.FindPartidoPorAutonomiasOficial(codPartido);
            BrainStormDTO dto = new BrainStormDTO(esp, avanceActual, 1, cps, true, _con);
            return dto;
        }
        /// <summary>
        /// Obtiene datos de un partido específico por autonomías (datos de sondeo).
        /// NOTA: Este método es para elecciones NACIONALES únicamente (tipoElecciones != 2).
        /// Consulta el código nacional 9900000.
        /// </summary>
        public BrainStormDTO FindPartidoPorAutonomiasSondeo(string codPartido, int avanceActual)
        {
            Circunscripcion esp = circunscripcionController.FindById("9900000");
            List<CircunscripcionPartido> cps = cpController.FindPartidoPorAutonomiasSondeo(codPartido);
            BrainStormDTO dto = new BrainStormDTO(esp, avanceActual, 1, cps, false, _con);
            return dto;
        }

        public BrainStormDTO FindPartidoPorProvinciasOficial(string codAutonomia, string codPartido, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByCodAutonomia(codAutonomia);
            List<CircunscripcionPartido> cps = cpController.FindPartidoPorProvinciasOficial(codAutonomia, codPartido);
            BrainStormDTO dto = new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _con);
            return dto;
        }
        public BrainStormDTO FindPartidoPorProvinciasSondeo(string codAutonomia, string codPartido, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByCodAutonomia(codAutonomia);
            List<CircunscripcionPartido> cps = cpController.FindPartidoPorProvinciasSondeo(codAutonomia, codPartido);
            BrainStormDTO dto = new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _con);
            return dto;
        }
    }
}
