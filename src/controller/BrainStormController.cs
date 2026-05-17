using Elecciones.src.conexion;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using System;
using System.Collections.Generic;

namespace Elecciones.src.controller
{
    internal class BrainStormController
    {
        public static BrainStormController? instance;

        private readonly CircunscripcionController circunscripcionController;
        private readonly CPController cpController;
        private readonly PartidoController partidoController;
        private readonly ConexionEntityFramework _connection;
        private static ConexionEntityFramework? _con;

        private BrainStormController(ConexionEntityFramework con)
        {
            _connection = con;
            _con = con;
            circunscripcionController = CircunscripcionController.GetInstance(con);
            cpController = CPController.GetInstance(con);
            partidoController = PartidoController.GetInstance(con);
            _ = partidoController.FindAll();
        }

        public static BrainStormController GetInstance(ConexionEntityFramework con)
        {
            if (instance == null || NeedsRecreation(con))
            {
                instance = new BrainStormController(con);
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

        public BrainStormDTO FindMasVotadosAutonomiasOficial(int avanceActual)
        {
            Circunscripcion esp = circunscripcionController.FindById("9900000");
            List<CircunscripcionPartido> cps = cpController.FindMasVotadosAutonomiasOficial();
            return new BrainStormDTO(esp, avanceActual, 1, cps, true, _connection);
        }

        public BrainStormDTO FindMasVotadosAutonomiasSondeo(int avanceActual)
        {
            Circunscripcion esp = circunscripcionController.FindById("9900000");
            List<CircunscripcionPartido> cps = cpController.FindMasVotadosAutonomiasSondeo();
            return new BrainStormDTO(esp, avanceActual, 1, cps, false, _connection);
        }

        public BrainStormDTO FindMasVotadosProvinciasOficial(string codAutonomia, int avanceActual, int tipoElecciones)
        {
            Circunscripcion autonomia = circunscripcionController.FindByName(codAutonomia);
            List<CircunscripcionPartido> cps = cpController.FindMasVotadosProvinciasOficial(codAutonomia);
            return new BrainStormDTO(autonomia, avanceActual, tipoElecciones, cps, true, _connection);
        }

        public BrainStormDTO FindMasVotadosProvinciasSondeo(string codAutonomia, int avanceActual, int tipoElecciones)
        {
            Circunscripcion autonomia = circunscripcionController.FindByName(codAutonomia);
            List<CircunscripcionPartido> cps = cpController.FindMasVotadosProvinciasSondeo(codAutonomia);
            return new BrainStormDTO(autonomia, avanceActual, tipoElecciones, cps, false, _connection);
        }

        public BrainStormDTO FindByIdCircunscripcionOficial(string cod, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindById(cod);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionOficial(cod);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _connection);
        }

        public BrainStormDTO FindByIdCircunscripcionOficialSinFiltrar(string cod, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindById(cod);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionOficialSinFiltrar(cod);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _connection);
        }

        public BrainStormDTO FindByIdCircunscripcionSondeo(string cod, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindById(cod);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionSondeo(cod);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _connection);
        }

        public BrainStormDTO FindByIdCircunscripcionSondeoSinFiltrar(string cod, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindById(cod);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionSondeoSinFiltrar(cod);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _connection);
        }

        public BrainStormDTO FindByNameCircunscripcionOficial(string nombreCircunscripcion, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByName(nombreCircunscripcion);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionOficial(circunscripcion.codigo);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _connection);
        }

        public BrainStormDTO FindByNameCircunscripcionSondeo(string nombreCircunscripcion, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByName(nombreCircunscripcion);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionSondeo(circunscripcion.codigo);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _connection);
        }

        public BrainStormDTO FindByNameCircunscripcionOficialSinFiltrar(string nombreCircunscripcion, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByName(nombreCircunscripcion);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionOficialSinFiltrar(circunscripcion.codigo);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _connection);
        }

        public BrainStormDTO FindByNameCircunscripcionSondeoSinFiltrar(string nombreCircunscripcion, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByName(nombreCircunscripcion);
            List<CircunscripcionPartido> cps = cpController.FindByIdCircunscripcionSondeoSinFiltrar(circunscripcion.codigo);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _connection);
        }

        public BrainStormDTO FindPartidoPorAutonomiasOficial(string codPartido, int avanceActual)
        {
            Circunscripcion esp = circunscripcionController.FindById("9900000");
            List<CircunscripcionPartido> cps = cpController.FindPartidoPorAutonomiasOficial(codPartido);
            return new BrainStormDTO(esp, avanceActual, 1, cps, true, _connection);
        }

        public BrainStormDTO FindPartidoPorAutonomiasSondeo(string codPartido, int avanceActual)
        {
            Circunscripcion esp = circunscripcionController.FindById("9900000");
            List<CircunscripcionPartido> cps = cpController.FindPartidoPorAutonomiasSondeo(codPartido);
            return new BrainStormDTO(esp, avanceActual, 1, cps, false, _connection);
        }

        public BrainStormDTO FindPartidoPorProvinciasOficial(string codAutonomia, string codPartido, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByCodAutonomia(codAutonomia);
            List<CircunscripcionPartido> cps = cpController.FindPartidoPorProvinciasOficial(codAutonomia, codPartido);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, true, _connection);
        }

        public BrainStormDTO FindPartidoPorProvinciasSondeo(string codAutonomia, string codPartido, int avanceActual, int tipoElecciones)
        {
            Circunscripcion circunscripcion = circunscripcionController.FindByCodAutonomia(codAutonomia);
            List<CircunscripcionPartido> cps = cpController.FindPartidoPorProvinciasSondeo(codAutonomia, codPartido);
            return new BrainStormDTO(circunscripcion, avanceActual, tipoElecciones, cps, false, _connection);
        }
    }
}
