using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.logic.comparators
{
    public class PartidoDTOComparer : IEqualityComparer<PartidoDTO>
    {
        public bool Equals(PartidoDTO? x, PartidoDTO? y)
        {
            return x.escaniosHasta == y.escaniosHasta;
        }

        public int GetHashCode([DisallowNull] PartidoDTO obj)
        {
            return obj.codigo.GetHashCode();
        }
    }
}
