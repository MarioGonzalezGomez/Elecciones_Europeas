using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.model.IPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.model.DTO.BrainStormDTO
{
    public class CircunscripcionDTO
    {
        public string codigo { get; set; }
        public string nombre { get; set; }
        public double escrutado { get; set; }
        public int escaniosTotales { get; set; }
        public int mayoria { get; set; }
        public int numAvance { get; set; }
        public double participacion { get; set; }
        public double participacionHistorica { get; set; }
        public double participacionMedia { get; set; }
        public int numVotantesTotales { get; set; }
        public string anioUltimasElecciones { get; set; }

        private CircunscripcionDTO(string codigo, string nombre, double escrutado, int escaniosTotales, int numVotantesTotales)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.escrutado = escrutado;
            this.escaniosTotales = escaniosTotales;
            this.numVotantesTotales = numVotantesTotales;
        }

        public static CircunscripcionDTO FromCircunscripcion(Circunscripcion c, int avanceActual, int tipoElecciones, ConexionEntityFramework con)
        {
            CircunscripcionDTO dto = new CircunscripcionDTO(c.codigo, c.nombre, c.escrutado, c.escanios, c.votantes);
            int mayor = dto.escaniosTotales / 2;
            if (int.IsEvenInteger(mayor))
            {
                mayor += 1;
            }
            dto.mayoria = mayor;
            dto.numAvance = avanceActual;
            Circunscripcion padre = tipoElecciones == 1
            ? CircunscripcionController.GetInstance(con).FindById("9900000")
            : CircunscripcionController.GetInstance(con).FindById(c.comunidad + "00000");

            double participacion = 0.0;
            double participacionHistorica = 0.0;
            double participacionMedia = 0.0;

            switch (avanceActual)
            {
                case 1:
                    participacion = c.avance1;
                    participacionHistorica = c.avance1Hist;
                    participacionMedia = padre.avance1;
                    break;
                case 2:
                    participacion = c.avance2;
                    participacionHistorica = c.avance2Hist;
                    participacionMedia = padre.avance2;
                    break;
                case 3:
                    participacion = c.avance3;
                    participacionHistorica = c.avance3Hist;
                    participacionMedia = padre.avance3;
                    break;
                case 4:
                    participacion = c.participacionFinal;
                    participacionHistorica = c.participacionHist;
                    participacionMedia = padre.participacionFinal;
                    break;
            }

            dto.participacion = participacion;
            dto.participacionHistorica = participacionHistorica;
            dto.participacionMedia = participacionMedia;
            dto.anioUltimasElecciones = "Año Últimas";

            return dto;
        }
    }
}
