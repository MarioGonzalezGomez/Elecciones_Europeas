using System.Text.Json;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Elecciones.src.utils;

namespace Elecciones.src.model.IPF
{
    public class Partido
    {
        public string codigo { get; set; }
        public string codigoPadre { get; set; }
        public string siglas { get; set; }
        public string nombre { get; set; }
        public string candidato { get; set; }
        public int independentismo { get; set; }
        public int esUltimoEscano{ get; set; }
        public int luchaUltimoEscano{ get; set; }
        public int restoVotos{ get; set; }

        ConfigManager configuration;

        public Partido()
        {
            configuration = ConfigManager.GetInstance();
        }
        public Partido(string codigo, string codigoPadre, string siglas, string nombre, string candidato, int independentismo, int esUltimoEscano, int luchaUltimoEscano, int restoVotos, ConfigManager configuration)
        {
            configuration = ConfigManager.GetInstance();
            this.codigo = codigo;
            this.codigoPadre = codigoPadre;
            this.siglas = siglas;
            this.nombre = nombre;
            this.candidato = candidato;
            this.independentismo = independentismo;
            this.esUltimoEscano = esUltimoEscano;
            this.luchaUltimoEscano = luchaUltimoEscano;
            this.restoVotos = restoVotos;
        }
        public override string ToString()
        {
            return $"{codigo};{codigoPadre};{siglas};{nombre};{candidato};{independentismo};";
        }
        public async Task ToJson()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\Partido.json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            await File.WriteAllTextAsync(fileName, json);
        }
        public async Task ToCsv()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\Partido.csv";
            string csv = $"Codigo;Siglas;Codigo Padre;Nombre;Candidato;Tendencia\n{this.ToString()}";
            await File.WriteAllTextAsync(fileName, csv);

        }
    }
}
