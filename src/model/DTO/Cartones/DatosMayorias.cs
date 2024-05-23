using Elecciones_Europeas.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.model.DTO.Cartones
{
    public class DatosMayorias
    {
        //Atributos

        ConfigManager configuration;

        public DatosMayorias()
        {
            configuration = ConfigManager.GetInstance();
        }

        public async Task ToJson()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\DatosMayorias.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultBufferSize = 1024
            };
            string json = JsonSerializer.Serialize(this, options);
            await File.WriteAllTextAsync(fileName, json);
        }
        public async Task ToCsv()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\DatosMayorias.csv";
            //    string csv = $"Circunscripción;Partido;Escaños Desde;Hasta;Diferencia;Tendencia;Históricos\n";
            // csv += $"{this.circunscripcion};{this.partido};{this.escanosDesde};{this.escanosHasta};{this.diferencia};{this.tendencia};{this.escanosHistoricos}\n";
            //  await File.WriteAllTextAsync(fileName, csv);
        }
    }
}
