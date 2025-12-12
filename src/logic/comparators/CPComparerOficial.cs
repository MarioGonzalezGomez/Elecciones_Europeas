using Elecciones.src.model.IPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.logic.comparators
{
    internal class CPComparerOficial : IComparer<CircunscripcionPartido>
    {
        public int Compare(CircunscripcionPartido? o1, CircunscripcionPartido? o2)
        {
            // Verificar si alguno de los elementos es null
            if (o1 == null && o2 == null) return 0;
            if (o1 == null) return -1;
            if (o2 == null) return 1;

            // Si o1 tiene el código 99999, siempre va al último
            if (o1.codPartido.Equals("99999")) return -1;
            if (o2.codPartido.Equals("99999")) return 1;

            // Si o1 tiene el código 96009, siempre va antes que 99999 pero después de los otros
            if (o1.codPartido.Equals("96009"))
            {
                if (o2.codPartido.Equals("99999")) return 1;
                return -1;
            }

            if (o2.codPartido.Equals("96009"))
            {
                if (o1.codPartido.Equals("99999")) return 1;
                return -1;
            }

            // Comparar los demás elementos según los criterios especificados
            int comp = Comparer<int>.Default.Compare(o1.escaniosHasta, o2.escaniosHasta);
            if (comp == 0)
            {
                comp = Comparer<double>.Default.Compare(o1.escaniosDesde, o2.escaniosDesde);
                if (comp == 0)
                {
                    comp = Comparer<double>.Default.Compare(o1.porcentajeVoto, o2.porcentajeVoto);
                    if (comp == 0)
                    {
                        comp = Comparer<int>.Default.Compare(o1.numVotantes, o2.numVotantes);
                    }
                }
            }
            return comp;
        }

    }
}
