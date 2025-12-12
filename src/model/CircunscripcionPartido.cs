using System.Text.Json;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using Elecciones.src.utils;

namespace Elecciones.src.model.IPF
{
    public class CircunscripcionPartido
    {
        public string codCircunscripcion { get; set; }
        public string codPartido { get; set; }
        public int escaniosDesde { get; set; }
        public int escaniosHasta { get; set; }
        public double porcentajeVoto { get; set; }
        public int numVotantes { get; set; }
        public int escaniosDesdeHist { get; set; }
        public int escaniosHastaHist { get; set; }
        public double porcentajeVotoHist { get; set; }
        public int numVotantesHist { get; set; }
        public int escaniosDesdeSondeo { get; set; }
        public int escaniosHastaSondeo { get; set; }
        public double porcentajeVotoSondeo { get; set; }

        ConfigManager configuration;

        public CircunscripcionPartido()
        {
            configuration = ConfigManager.GetInstance();
        }

        public override string ToString()
        {
            return $"{codCircunscripcion};{codPartido};{escaniosDesde};{escaniosHasta};{porcentajeVoto};{numVotantes};" +
                $"{escaniosDesdeHist};{escaniosHastaHist};{porcentajeVotoHist};{numVotantesHist};" +
                $"{escaniosDesdeSondeo};{escaniosHastaSondeo};{porcentajeVotoSondeo}";
        }
        public async Task ToJson()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\CP.json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            await File.WriteAllTextAsync(fileName, json);
        }
        public async Task ToCsv()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\CP.csv";
            string csv = $"Circunscripcion;Partido;Escanios Desde;Esc. Hasta;Porcentaje Voto;Num. Votantes;Esc. Desde Hist.;Esc. Hasta Hist.;Porcentaje Voto Hist.;Num. Votantes Hist.;Esc. Desde Sondeo;Esc. Hasta Sondeo;Porcentaje Voto Sondeo\n{this.ToString()}";
            await File.WriteAllTextAsync(fileName, csv);

        }
    }
}
