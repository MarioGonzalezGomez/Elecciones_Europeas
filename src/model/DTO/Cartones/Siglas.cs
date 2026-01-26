using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.utils;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Elecciones.src.model.DTO.Cartones
{
    public class Siglas
    {
        string circunscripcion;
        string partido;
        int escanosDesde;
        int escanosHasta;
        int diferencia;
        string tendencia;
        int escanosHistoricos;

        ConfigManager configuration;

        public Siglas(string circunscripcion, PartidoDTO dto)
        {
            configuration = ConfigManager.GetInstance();
            this.circunscripcion = circunscripcion;
            partido = dto.codigo;
            escanosDesde = dto.escaniosDesdeSondeo;
            escanosHasta = dto.escanios;
            diferencia = dto.diferenciaEscanios;
            tendencia = dto.tendencia;
            escanosHistoricos = dto.escaniosHistoricos;
        }

        public async Task ToJson()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\{partido}.json";
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
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\{partido}.csv";
            string csv = $"Circunscripci�n;Partido;Esca�os Desde;Hasta;Diferencia;Tendencia;Hist�ricos\n";
            csv += $"{this.circunscripcion};{this.partido};{this.escanosDesde};{this.escanosHasta};{this.diferencia};{this.tendencia};{this.escanosHistoricos}\n";
            await File.WriteAllTextAsync(fileName, csv);
        }
    }
}
