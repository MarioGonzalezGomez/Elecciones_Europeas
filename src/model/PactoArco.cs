using Elecciones_Europeas.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Elecciones_Europeas.src.model
{
    public class PactoArco
    {
        public string codigo { set; get; }
        public double anchoDesde { set; get; }
        public double anchoHasta { set; get; }
        public double posicionDesde { set; get; }
        public double posicionHasta { set; get; }
        public bool entraIzq { set; get; }

        public PactoArco()
        {
        }

        public override string ToString()
        {
            return $"{codigo};{anchoDesde};{anchoHasta};{posicionDesde};{posicionHasta};{entraIzq}";
        }

    }
}
