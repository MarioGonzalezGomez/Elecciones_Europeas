using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.mensajes.builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.mensajes
{
    internal class OrdenesPrime
    {
        public static OrdenesPrime instance;
        private PrimeMensajes builder;
        public ConexionGraficos c;

        private OrdenesPrime()
        {
            builder = PrimeMensajes.GetInstance();
            c = ConexionGraficos.GetInstancePrime();
        }

        public static OrdenesPrime GetInstance()
        {
            if (instance == null)
            {
                instance = new OrdenesPrime();
            }
            return instance;
        }

        public void ReiniciarConexion()
        {
            c.ReiniciarConexion("prime");
        }

        public void Reset()
        {
            c.EnviarMensaje(builder.Reset());
        }
    }
}
