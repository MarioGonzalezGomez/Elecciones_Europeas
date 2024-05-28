using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.controller;
using Elecciones_Europeas.src.model.IPF;
using Elecciones_Europeas.src.repository;
using System.Collections.Generic;
using System.Linq;

namespace Elecciones_Europeas.src.service
{
    internal class CircunscripcionService : IBaseService<Circunscripcion, string>
    {
        public static CircunscripcionService? instance;

        private CircunscripcionRepository _rep;
        private static ConexionEntityFramework? _con;

        private CircunscripcionService(ConexionEntityFramework con)
        {
            _con = con;
            this._rep = CircunscripcionRepository.GetInstance(con);
        }

        public static CircunscripcionService GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new CircunscripcionService(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new CircunscripcionService(con);
            }
            return instance;
        }

        public List<Circunscripcion> FindAll()
        {
            return _rep.GetAll();
        }

        public List<Circunscripcion> FindAllFromBD()
        {
            return _rep.GetAllFromBD();
        }

        public List<Circunscripcion> FindAllAutonomias(int db)
        {
            List<Circunscripcion> circunscripciones = db == 1 ? _rep.GetAll()
               .Where(cir => cir.codigo.EndsWith("00000"))
               .Where(cir => cir.codigo.StartsWith("99"))
               .OrderBy(cir => cir.codigo)
               .ToList() :
               _rep.GetAll()
               .Where(cir => cir.codigo.EndsWith("00000"))
               .OrderBy(cir => cir.codigo)
               .ToList();
            Circunscripcion spain = circunscripciones[circunscripciones.Count - 1];
            circunscripciones.RemoveAt(circunscripciones.Count - 1);
            circunscripciones.Insert(0, spain);
            return circunscripciones;
        }

        public List<Circunscripcion> FindAllCircunscripcionesByNameAutonomia(string nombreAutonomia)
        {
            Circunscripcion autonomia = _rep.GetByName(nombreAutonomia);
            return _rep.GetAll()
               .Where(cir => cir.codigo.StartsWith(autonomia.comunidad))
               .Where(cir => !cir.codigo.EndsWith("00000"))
               .Where(cir => cir.codigo.EndsWith("000"))
               .OrderBy(cir => cir.codigo)
               .ToList();
        }

        public List<Circunscripcion> FindAllCircunscripcionesByNameAutonomiaRegional(string nombreAutonomia)
        {
            Circunscripcion autonomia = _rep.GetByName(nombreAutonomia);
            return _rep.GetAll()
               .Where(cir => cir.codigo.StartsWith(autonomia.comunidad))
               .Where(cir => !cir.codigo.EndsWith("00000"))
               .OrderBy(cir => cir.codigo)
               .ToList();
        }

        public Circunscripcion FindById(string id)
        {
            return _rep.GetById(id);
        }

        public Circunscripcion FindByCodAutonomia(string codAutonomia)
        {
            return _rep.GetById($"{codAutonomia}00000");
        }

        public Circunscripcion FindByName(string name)
        {
            return _rep.GetByName(name);
        }
    }
}
