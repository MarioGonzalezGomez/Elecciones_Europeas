using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Elecciones.src.logic.comparators
{
    public class PartidoDTOComparer : IEqualityComparer<PartidoDTO>
    {
        public bool Equals(PartidoDTO? x, PartidoDTO? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.escanios == y.escanios;
        }

        public int GetHashCode([DisallowNull] PartidoDTO obj)
        {
            return obj.escanios.GetHashCode();
        }
    }
}
