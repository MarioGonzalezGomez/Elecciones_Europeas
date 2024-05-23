using Elecciones_Europeas.src.model.IPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.logic.comparators
{
    internal class CPComparerSondeo : IComparer<CircunscripcionPartido>
    {
        public int Compare(CircunscripcionPartido? o1, CircunscripcionPartido? o2)
        {
            if (o1.codPartido.Equals("99999")) return -1;

            int comp = Comparer<int>.Default.Compare(o1.escaniosHastaSondeo, o2.escaniosHastaSondeo);
            if (comp == 0)
            {
                comp = Comparer<double>.Default.Compare(o1.escaniosDesdeSondeo, o2.escaniosDesdeSondeo);
                if (comp == 0)
                {
                    comp = Comparer<double>.Default.Compare(o1.porcentajeVotoSondeo, o2.porcentajeVotoSondeo);
                }
            }
            return comp;
        }

    }
}
