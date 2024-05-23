using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.model.IPF;
using System.Collections.Generic;
using System.Linq;


namespace Elecciones_Europeas.src.repository
{
    internal class LiteralRepository : IRepository<Literal, string>
    {
        public static LiteralRepository? instance;

        private ConexionEntityFramework _con;

        private LiteralRepository(ConexionEntityFramework con)
        {
            this._con = con;
        }

        public static LiteralRepository GetInstance(ConexionEntityFramework con)
        {
            if (instance == null)
            {
                instance = new LiteralRepository(con);
            }
            return instance;
        }

        public List<Literal> GetAll()
        {
            return _con.Literales.ToList();
        }

        public Literal GetById(string id)
        {
            return _con.Literales.Find(id);
        }
    }

}

