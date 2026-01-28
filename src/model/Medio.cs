using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.model
{
    public class Medio
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public int comparar { get; set; }

        public Medio() { }

        public Medio(string codigo, string descripcion, int comparar = 0)
        {
            this.codigo = codigo;
            this.descripcion = descripcion;
            this.comparar = comparar;
        }

        public override string ToString()
        {
            return $"{codigo} - {descripcion}";
        }
    }
}
