using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.model
{
    public class MedioPartido
    {
        public string codCircunscripcion { get; set; }
        public string codMedio { get; set; }
        public string codPartido { get; set; }
        public int escaniosDesde { get; set; }
        public int escaniosHasta { get; set; }
        public decimal votos { get; set; }

        public MedioPartido() { }

        public MedioPartido(string codCircunscripcion, string codMedio, string codPartido, 
            int escaniosDesde, int escaniosHasta, decimal votos)
        {
            this.codCircunscripcion = codCircunscripcion;
            this.codMedio = codMedio;
            this.codPartido = codPartido;
            this.escaniosDesde = escaniosDesde;
            this.escaniosHasta = escaniosHasta;
            this.votos = votos;
        }

        public override string ToString()
        {
            return $"{codCircunscripcion}|{codMedio}|{codPartido} - Desde: {escaniosDesde}, Hasta: {escaniosHasta}, Votos: {votos}%";
        }
    }
}
