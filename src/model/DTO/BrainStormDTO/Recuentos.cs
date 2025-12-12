using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.model.IPF;
using Elecciones.src.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Elecciones.src.model.DTO.BrainStormDTO
{
    public class Recuentos
    {
        public string CIRCUNSCRIPCION { get; set; }
        public string CODIGO_CIRCUNSCRIPCION { get; set; }
        public int ESCANOS { get; set; }
        public double ESCRUTADO { get; set; }
        public double PARTICIPACION { get; set; }
        public double PARTICIPACION_HISTORICO { get; set; }
        public int PARTIDOS_TOTALES { get; set; }
        public int PARTIDOS_CON_ESCANO { get; set; }
        public List<Recuento> RECUENTOS { get; set; }

        ConfigManager configuration;

        public Recuentos()
        {
            configuration = ConfigManager.GetInstance();
        }

        public Recuentos(string cIRCUNSCRIPCION, string cODIGO, int eSCANOS, double eSCRUTADO, double pARTICIPACION, double pARTICIPACION_HISTORICO, int pARTIDOS_TOTALES, int pARTIDOS_CON_ESCANO, List<Recuento> recuentos)
        {
            CIRCUNSCRIPCION = cIRCUNSCRIPCION;
            CODIGO_CIRCUNSCRIPCION = cODIGO;
            ESCANOS = eSCANOS;
            ESCRUTADO = eSCRUTADO;
            PARTICIPACION = pARTICIPACION;
            PARTICIPACION_HISTORICO = pARTICIPACION_HISTORICO;
            PARTIDOS_TOTALES = pARTIDOS_TOTALES;
            PARTIDOS_CON_ESCANO = pARTIDOS_CON_ESCANO;
            RECUENTOS = recuentos;
            configuration = ConfigManager.GetInstance();
        }


        public List<Recuentos> GetRecuentos(bool oficiales, int avanceActual, int eleccion, ConexionEntityFramework con)
        {
            List<Recuentos> recuentos = new List<Recuentos>();
            Circunscripcion circunscripcionCentral = CircunscripcionController.GetInstance(con).FindById($"{configuration.GetValue("codigoRegional")}00000");
            Recuentos central = GetRecuento(oficiales, avanceActual, eleccion, circunscripcionCentral.nombre, con);
            recuentos.Add(central);
            List<Circunscripcion> provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(circunscripcionCentral.nombre);
            foreach (Circunscripcion c in provincias)
            {
                Recuentos provincia = GetRecuento(oficiales, avanceActual, eleccion, c.nombre, con);
                recuentos.Add(provincia);
            }
            return recuentos;
        }
        private Recuentos GetRecuento(bool oficiales, int avanceActual, int eleccion, string circunscripcion, ConexionEntityFramework con)
        {
            Recuentos recuentos = new Recuentos();
            BrainStormDTO bsDto = oficiales ? BrainStormController.GetInstance(con).FindByNameCircunscripcionOficialSinFiltrar(circunscripcion, avanceActual, eleccion) : BrainStormController.GetInstance(con).FindByNameCircunscripcionSondeoSinFiltrar(circunscripcion, avanceActual, eleccion);
            Recuento r = new Recuento();
            List<Recuento> recuentolist = r.FromBrainstormDTO(bsDto, con);
            recuentos.CIRCUNSCRIPCION = bsDto.circunscripcionDTO.nombre;
            recuentos.CODIGO_CIRCUNSCRIPCION = bsDto.circunscripcionDTO.codigo;
            recuentos.ESCANOS = bsDto.circunscripcionDTO.escaniosTotales;
            recuentos.ESCRUTADO = bsDto.circunscripcionDTO.escrutado;
            recuentos.PARTICIPACION = bsDto.circunscripcionDTO.participacion;
            recuentos.PARTICIPACION_HISTORICO = bsDto.circunscripcionDTO.participacionHistorica;
            recuentos.PARTIDOS_TOTALES = bsDto.partidos.Count;
            recuentos.PARTIDOS_CON_ESCANO = bsDto.partidos.Where(par => par.escaniosHasta > 0).ToList().Count();
            recuentos.RECUENTOS = recuentolist;
            return recuentos;
        }

        public async Task ToJson(List<Recuentos> recuentos, string ruta = "datos_escrutado")
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\{ruta}.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultBufferSize = 1024
            };
            string json = JsonSerializer.Serialize(recuentos, options);
            await File.WriteAllTextAsync(fileName, json);
        }
    }
}
