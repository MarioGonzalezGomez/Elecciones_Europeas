using Elecciones.src.model.IPF;
using System;
using System.Collections.Generic;

namespace Elecciones.src.logic.comparators
{
    internal class CPComparerSondeo : IComparer<CircunscripcionPartido>
    {
        public int Compare(CircunscripcionPartido? o1, CircunscripcionPartido? o2)
        {
            if (ReferenceEquals(o1, o2))
            {
                return 0;
            }

            if (o1 is null)
            {
                return 1;
            }

            if (o2 is null)
            {
                return -1;
            }

            if (string.Equals(o1.codPartido, "99999", StringComparison.Ordinal))
            {
                return -1;
            }

            if (string.Equals(o2.codPartido, "99999", StringComparison.Ordinal))
            {
                return 1;
            }

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
