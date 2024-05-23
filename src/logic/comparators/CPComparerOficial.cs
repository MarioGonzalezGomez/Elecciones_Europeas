using Elecciones_Europeas.src.model.IPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.logic.comparators
{
    internal class CPComparerOficial : IComparer<CircunscripcionPartido>
    {
        public int Compare(CircunscripcionPartido? o1, CircunscripcionPartido? o2)
        {
            if (o1.codPartido.Equals("99999")) return -1;

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
