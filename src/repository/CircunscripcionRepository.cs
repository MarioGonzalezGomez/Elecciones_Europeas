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

        private static ConexionEntityFramework? _con;

        private CircunscripcionRepository(ConexionEntityFramework con)
        {
            _con = con;
        }

        public static CircunscripcionRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null || NeedsRecreation(con))
            {
                instance = new CircunscripcionRepository(con);
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
            return _con ?? throw new InvalidOperationException("CircunscripcionRepository no ha sido inicializado.");
        }

        public List<Circunscripcion> GetAll()
        {
            return GetConnectionOrThrow().Circunscripciones.ToList();
        }

        public List<Circunscripcion> GetAllFromBD()
        {
            ConexionEntityFramework con = GetConnectionOrThrow();
            using (var newContext = new ConexionEntityFramework(con._tipoConexion, con.db))
            {
                return newContext.Circunscripciones.ToList();
            }
        }

        public Circunscripcion GetById(string id)
        {
            return GetConnectionOrThrow().Circunscripciones.Find(id)
                ?? throw new KeyNotFoundException($"No se ha encontrado la circunscripción con id '{id}'.");
        }

        public Circunscripcion GetByName(string name)
        {
            return GetConnectionOrThrow().Circunscripciones
                .FirstOrDefault(p => p.nombre.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? throw new KeyNotFoundException($"No se ha encontrado la circunscripción '{name}'.");
        }
    }
}
