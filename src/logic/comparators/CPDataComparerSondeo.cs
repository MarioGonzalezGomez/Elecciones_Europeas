using Elecciones.src.model.IPF.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.logic.comparators
{
    /// <summary>
    /// Comparador para CPDataDTO en modo sondeo.
    /// Ordena por: escaniosHastaSondeo > escaniosDesdeSondeo > escaniosHistoricos
    /// </summary>
    internal class CPDataComparerSondeo : IComparer<CPDataDTO>
    {
        public int Compare(CPDataDTO? o1, CPDataDTO? o2)
        {
            if (o1 == null && o2 == null) return 0;
            if (o1 == null) return -1;
            if (o2 == null) return 1;

            // Partido "99999" (OTROS) siempre al final
            if (o1.codigo.Equals("99999")) return -1;
            if (o2.codigo.Equals("99999")) return 1;

            // Comparar por escaniosHastaSondeo (primero)
            int comp = Comparer<int>.Default.Compare(
                int.TryParse(o1.escaniosHastaSondeo, out int esc1Hasta) ? esc1Hasta : 0,
                int.TryParse(o2.escaniosHastaSondeo, out int esc2Hasta) ? esc2Hasta : 0);

            if (comp == 0)
            {
                // En caso de empate, comparar por escaniosDesdeSondeo
                comp = Comparer<int>.Default.Compare(
                    int.TryParse(o1.escaniosDesdeSondeo, out int esc1Desde) ? esc1Desde : 0,
                    int.TryParse(o2.escaniosDesdeSondeo, out int esc2Desde) ? esc2Desde : 0);

                if (comp == 0)
                {
                    // En caso de empate, comparar por escaniosHistoricos
                    comp = Comparer<int>.Default.Compare(
                        int.TryParse(o1.escaniosHistoricos, out int escHist1) ? escHist1 : 0,
                        int.TryParse(o2.escaniosHistoricos, out int escHist2) ? escHist2 : 0);
                }
            }

            return comp;
        }
    }
}
