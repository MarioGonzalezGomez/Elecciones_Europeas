using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.model.DTO
{
    public class MedioDTO
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public int comparar { get; set; }

        public MedioDTO() { }

        public MedioDTO(string codigo, string descripcion, int comparar = 0)
        {
            this.codigo = codigo;
            this.descripcion = descripcion;
            this.comparar = comparar;
        }

        public static MedioDTO FromMedio(Medio medio)
        {
            if (medio == null) return null;
            return new MedioDTO(medio.codigo, medio.descripcion, medio.comparar);
        }

        public override string ToString()
        {
            return $"{codigo} - {descripcion}";
        }
    }
}
