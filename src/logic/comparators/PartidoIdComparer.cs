using Elecciones.src.model.DTO.BrainStormDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.logic.comparators
{
    /// <summary>
    /// Comparador simple por ID de partido (código).
    /// Útil para ordenar partidos por su identificador en orden ascendente.
    /// </summary>
    internal class PartidoIdComparer : IComparer<PartidoDTO>
    {
        public int Compare(PartidoDTO? x, PartidoDTO? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.Compare(x.codigo, y.codigo, StringComparison.Ordinal);
        }
    }
}
