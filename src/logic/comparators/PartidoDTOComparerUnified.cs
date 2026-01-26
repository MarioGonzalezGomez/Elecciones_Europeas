using Elecciones.src.model.DTO.BrainStormDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.logic.comparators
{
    /// <summary>
    /// Comparador unificado para PartidoDTO que maneja tanto datos oficiales como de sondeo.
    /// Trata el código 99999 (OTROS) de forma especial: siempre al final si no tiene representación,
    /// o justo después de partidos con representación si tiene escaños.
    /// </summary>
    internal class PartidoDTOComparerUnified : IComparer<PartidoDTO>
    {
        private readonly bool oficiales;

        public PartidoDTOComparerUnified(bool oficiales)
        {
            this.oficiales = oficiales;
        }

        public int Compare(PartidoDTO? x, PartidoDTO? y)
        {
            // Verificar si alguno de los elementos es null
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Manejo especial para código 99999 (OTROS)
            bool xEsOtros = x.codigo.Equals("99999");
            bool yEsOtros = y.codigo.Equals("99999");

            if (xEsOtros && yEsOtros) return 0;

            // Determinar si tienen representación (escaños)
            int xEscanios = oficiales ? x.escanios : x.escaniosHastaSondeo;
            int yEscanios = oficiales ? y.escanios : y.escaniosHastaSondeo;

            bool xTieneRepresentacion = xEscanios > 0;
            bool yTieneRepresentacion = yEscanios > 0;

            // Si x es OTROS
            if (xEsOtros)
            {
                if (xTieneRepresentacion)
                {
                    // OTROS con representación: va después de todos los partidos con representación
                    return yTieneRepresentacion ? -1 : 1;
                }
                else
                {
                    // OTROS sin representación: va al final de todo
                    return -1;
                }
            }

            // Si y es OTROS
            if (yEsOtros)
            {
                if (yTieneRepresentacion)
                {
                    // OTROS con representación: va después de todos los partidos con representación
                    return xTieneRepresentacion ? 1 : -1;
                }
                else
                {
                    // OTROS sin representación: va al final de todo
                    return 1;
                }
            }

            // Comparación normal para partidos que no son OTROS
            int comp;

            if (oficiales)
            {
                // Datos oficiales: escanios > porcentajeVoto > numVotantes
                comp = Comparer<int>.Default.Compare(x.escanios, y.escanios);
                if (comp == 0)
                {
                    comp = Comparer<double>.Default.Compare(x.porcentajeVoto, y.porcentajeVoto);
                    if (comp == 0)
                    {
                        comp = Comparer<int>.Default.Compare(x.numVotantes, y.numVotantes);
                    }
                }
            }
            else
            {
                // Datos de sondeo: escaniosHastaSondeo > escaniosDesdeSondeo > escaniosHistoricos
                comp = Comparer<int>.Default.Compare(x.escaniosHastaSondeo, y.escaniosHastaSondeo);
                if (comp == 0)
                {
                    comp = Comparer<int>.Default.Compare(x.escaniosDesdeSondeo, y.escaniosDesdeSondeo);
                    if (comp == 0)
                    {
                        comp = Comparer<int>.Default.Compare(x.escaniosHistoricos, y.escaniosHistoricos);
                    }
                }
            }

            return comp;
        }
    }
}
