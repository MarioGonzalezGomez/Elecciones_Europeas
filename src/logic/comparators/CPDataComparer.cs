using Elecciones.src.model.IPF;
using Elecciones.src.model.IPF.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.logic.comparators
{
    internal class CPDataComparer: IComparer<CPDataDTO>
    {
        public int Compare(CPDataDTO? o1, CPDataDTO? o2)
        {
            if (o1.codigo.Equals("99999")) return -1;

            int comp = Comparer<int>.Default.Compare(int.Parse(o1.escaniosHasta), int.Parse(o2.escaniosHasta));
            if (comp == 0)
            {
                comp = Comparer<double>.Default.Compare(int.Parse(o1.escaniosDesde), int.Parse(o2.escaniosDesde));
                if (comp == 0)
                {
                    comp = Comparer<double>.Default.Compare(double.Parse(o1.porcentajeVoto), double.Parse(o2.porcentajeVoto));
                    if (comp == 0)
                    {
                        comp = Comparer<int>.Default.Compare(int.Parse(o1.votantes), int.Parse(o2.votantes));
                    }
                }
            }
            return comp;
        }
    }
}
