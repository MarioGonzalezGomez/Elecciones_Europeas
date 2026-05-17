using Elecciones.src.model.IPF.DTO;
using System;
using System.Collections.Generic;

namespace Elecciones.src.logic.comparators
{
    internal class CPDataComparer : IComparer<CPDataDTO>
    {
        public int Compare(CPDataDTO? o1, CPDataDTO? o2)
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

            if (string.Equals(o1.codigo, "99999", StringComparison.Ordinal))
            {
                return -1;
            }

            if (string.Equals(o2.codigo, "99999", StringComparison.Ordinal))
            {
                return 1;
            }

            int escanios1 = int.TryParse(o1.escanios, out int v1) ? v1 : 0;
            int escanios2 = int.TryParse(o2.escanios, out int v2) ? v2 : 0;
            int comp = Comparer<int>.Default.Compare(escanios1, escanios2);
            if (comp == 0)
            {
                double porcentaje1 = double.TryParse(o1.porcentajeVoto, out double p1) ? p1 : 0;
                double porcentaje2 = double.TryParse(o2.porcentajeVoto, out double p2) ? p2 : 0;
                comp = Comparer<double>.Default.Compare(porcentaje1, porcentaje2);

                if (comp == 0)
                {
                    int votantes1 = int.TryParse(o1.votantes, out int n1) ? n1 : 0;
                    int votantes2 = int.TryParse(o2.votantes, out int n2) ? n2 : 0;
                    comp = Comparer<int>.Default.Compare(votantes1, votantes2);
                }
            }

            return comp;
        }
    }
}
