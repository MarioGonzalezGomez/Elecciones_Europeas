using Elecciones.src.conexion;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elecciones.src.repository
{
    internal class CircunscripcionRepository : IRepository<Circunscripcion, string>
    {
        public static CircunscripcionRepository? instance;

        private static ConexionEntityFramework _con;

        private CircunscripcionRepository(ConexionEntityFramework con)
        {
            _con = con;
        }

        public static CircunscripcionRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new CircunscripcionRepository(con);
            }
            else if (_con._tipoConexion != con._tipoConexion)
            {
                instance = new CircunscripcionRepository(con);
            }
            else if (!_con._database.Equals(con._database))
            {
                instance = new CircunscripcionRepository(con);
            }
            return instance;
        }

        public List<Circunscripcion> GetAll()
        {
            return _con.Circunscripciones.ToList();
        }

        public List<Circunscripcion> GetAllFromBD()
        {
            using (var newContext = new ConexionEntityFramework(_con._tipoConexion, _con.db))
            {
                var values = newContext.Circunscripciones.ToList();
                return newContext.Circunscripciones.ToList();
            }
        }

        public Circunscripcion GetById(string id)
        {
            return _con.Circunscripciones.Find(id);
        }
        public Circunscripcion GetByName(string name)
        {
            return _con.Circunscripciones.FirstOrDefault(p => p.nombre.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
