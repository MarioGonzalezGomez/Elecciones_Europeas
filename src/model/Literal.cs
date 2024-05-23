using Elecciones_Europeas.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.model.IPF
{
    public class Literal
    {
        public string codigo { get; set; }
        public string castellano { get; set; }
        public string catalan { get; set; }
        public string vasco { get; set; }
        public string gallego { get; set; }
        public string valenciano { get; set; }
        public string mallorquin { get; set; }

        ConfigManager configuration;

        public Literal()
        {
            configuration = ConfigManager.GetInstance();
        }

        public async Task ToJson(List<Literal> literales)
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\Literales.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultBufferSize = 1024
            };
            string json = JsonSerializer.Serialize(literales, options);
            await File.WriteAllTextAsync(fileName, json);
        }
        public async Task ToCsv(List<Literal> literales)
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\Partido.csv";
            string csv = $"Código;Castellano;Catalán;Euskera;Gallego;Valenciano;Mallorquín\n";
            foreach (Literal l in literales)
            {
                csv += $"{l.codigo};{l.castellano};{l.catalan};{l.vasco};{l.gallego};{l.valenciano};{l.mallorquin}\n";
            }

            await File.WriteAllTextAsync(fileName, csv);

        }
    }
}
