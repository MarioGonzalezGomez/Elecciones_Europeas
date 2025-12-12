using Elecciones.src.conexion;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.controller
{
    public class CircunscripcionController
    {
        public static CircunscripcionController? instance;

        private CircunscripcionService _service;
        private static ConexionEntityFramework? _con;

        private CircunscripcionController(ConexionEntityFramework con)
        {
            _con = con;
            this._service = CircunscripcionService.GetInstance(con);
        }

        public static CircunscripcionController GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new CircunscripcionController(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new CircunscripcionController(con);
            }
            else if (!_con._database.Equals(con._database))
            {
                instance = new CircunscripcionController(con);
            }
            return instance;
        }

        public List<Circunscripcion> FindAll()
        {
            return _service.FindAll();
        }

        public List<Circunscripcion> FindAllFromBD()
        {
            return _service.FindAllFromBD();
        }

        public List<Circunscripcion> FindAllAutonomias(int db)
        {
            return _service.FindAllAutonomias(db);
        }

        public List<Circunscripcion> FindAllCircunscripcionesByNameAutonomia(string nombreAutonomia)
        {
            return _service.FindAllCircunscripcionesByNameAutonomia(nombreAutonomia);
        }

        public List<Circunscripcion> FindAllCircunscripcionesByNameAutonomiaRegional(string nombreAutonomia)
        {
            return _service.FindAllCircunscripcionesByNameAutonomiaRegional(nombreAutonomia);
        }

        public Circunscripcion FindById(string id)
        {
            return _service.FindById(id);
        }

        public Circunscripcion FindByCodAutonomia(string codAutonomia)
        {
            return _service.FindByCodAutonomia(codAutonomia);
        }

        public Circunscripcion FindByName(string name)
        {
            return _service.FindByName(name);
        }
    }
}
