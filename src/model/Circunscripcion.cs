using System.Text.Json;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using Elecciones.src.utils;

namespace Elecciones.src.model.IPF
{
    public class Circunscripcion
    {
        public string codigo { get; set; }
        public string comunidad { get; set; }
        public string provincia { get; set; }
        public string municipio { get; set; }
        public string nombre { get; set; }
        public double escrutado { get; set; }
        public int escanios { get; set; }
        public double avance1 { get; set; }
        public double avance2 { get; set; }
        public double avance3 { get; set; }
        public double participacionFinal { get; set; }
        public int votantes { get; set; }
        public int escaniosHistoricos { get; set; }
        public double avance1Hist { get; set; }
        public double avance2Hist { get; set; }
        public double avance3Hist { get; set; }
        public double participacionHist { get; set; }

        ConfigManager configuration;

        public Circunscripcion()
        {
            configuration = ConfigManager.GetInstance();
        }

        public override string? ToString()
        {
            return $"{codigo};{comunidad};{provincia};{municipio};{nombre};{escrutado};" +
                $"{escanios};{avance1};{avance2};{avance3};{participacionFinal};{votantes};{escaniosHistoricos};" +
                $"{avance1Hist};{avance2Hist};{avance3Hist};{participacionHist}";
        }
        public async Task ToJson()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\Circunscripcion.json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            await File.WriteAllTextAsync(fileName, json);
        }
        public async Task ToCsv()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\Circunscripcion.csv";
            string csv = $"Codigo;CCAA;Provincia;Municipio;Descripcion;Escrutado;Escanios;Avance 1;Avance2;Avance3;Participacion;Votantes;Escanios Historicos;Avance 1 Historico;Avance 2 Historico;Avance 3 Historico;Participacion Historica\n{this.ToString()}";
            await  File.WriteAllTextAsync(fileName, csv);

        }
    }
}
