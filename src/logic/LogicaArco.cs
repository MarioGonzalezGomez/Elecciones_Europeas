using Elecciones.src.model;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using Elecciones.src.model.IPF.DTO;
using Elecciones.src.service;
using Elecciones.src.utils;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Elecciones.src.logic
{
    public class LogicaArco
    {
        private readonly double gradosTotales = 180;
        public double escaniosTotales;

        ConfigManager configuration;

        public LogicaArco(int escaniosTotales)
        {
            this.escaniosTotales = escaniosTotales;
            configuration = ConfigManager.GetInstance();
        }

        public List<PactoArco> GetPactos(List<CPDataDTO> partidosIzq, List<CPDataDTO> partidosDer)
        {
            List<PactoArco> pacto = new List<PactoArco>();
            double posicionIzqDesde = 90;
            double posicionIzqHasta = 90;
            double posicionDerDesde = -90;
            double posicionDerHasta = -90;
            if (partidosIzq != null)
            {
                for (int i = 0; i < partidosIzq.Count; i++)
                {
                    PactoArco item = new PactoArco();
                    item.codigo = partidosIzq[i].codigo;
                    item.anchoDesde = GetAncho(partidosIzq[i], false);
                    item.anchoHasta = GetAncho(partidosIzq[i], true);
                    item.posicionDesde = posicionIzqDesde;
                    item.posicionHasta = posicionIzqHasta;
                    item.entraIzq = true;
                    posicionIzqDesde -= item.anchoDesde;
                    posicionIzqHasta -= item.anchoHasta;
                    pacto.Add(item);
                }
            }
            if (partidosDer != null)
            {
                for (int i = 0; i < partidosDer.Count; i++)
                {
                    PactoArco item = new PactoArco();
                    item.codigo = partidosDer[i].codigo;
                    item.anchoDesde = GetAncho(partidosDer[i], false);
                    item.anchoHasta = GetAncho(partidosDer[i], true);
                    item.posicionDesde = posicionDerDesde;
                    item.posicionHasta = posicionDerHasta;
                    item.entraIzq = false;
                    posicionDerDesde += item.anchoDesde;
                    posicionDerHasta += item.anchoHasta;
                    pacto.Add(item);
                }
            }

            return pacto;
        }

        //TRUE en caso de Hasta, FALSE en caso de Desde
        private double GetAncho(CPDataDTO data, bool hasta)
        {
            return hasta ? double.Parse(data.escaniosHasta) * gradosTotales / escaniosTotales : double.Parse(data.escaniosDesde) * gradosTotales / escaniosTotales;
        }

        public async Task ToJson(List<PactoArco> pactos)
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\Arco.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultBufferSize = 1024
            };
            string json = JsonSerializer.Serialize(pactos, options);
            await File.WriteAllTextAsync(fileName, json);
        }
        public async Task ToCsv(List<PactoArco> pactos)
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\Arco.csv";
            string csv = $"Codigo;AnchoDesde;AnchoHasta;PosicionDesde;PosicionHasta;EntraIzquierda?\n";
            pactos.ForEach(pacto => csv += $"{pacto.ToString()}\n");
            await File.WriteAllTextAsync(fileName, csv);

        }
    }
}
