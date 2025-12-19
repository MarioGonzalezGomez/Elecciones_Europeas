using System.Text.Json;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Elecciones_Europeas.src.utils;

namespace Elecciones_Europeas.src.model.IPF
{
    public class Partido
    {
        public string codigo { get; set; }
        public string codigoPadre
        {
            get => codigo;
            set
            { /* intentionally ignored - codigo is the source of truth */
            }
        }
        public string siglas { get; set; }
        public string nombre { get; set; }
        public string candidato { get; set; }
        public int independentismo { get; set; }

        ConfigManager configuration;

        public Partido()
        {
            configuration = ConfigManager.GetInstance();
        }
        public Partido(string codigo, string codigoPadre, string siglas, string nombre, string candidato, int independentismo)
        {
            configuration = ConfigManager.GetInstance();
            this.codigo = codigo;
            // codigoPadre parameter is accepted for compatibility but ignored:
            // codigoPadre will always be represented by this.codigo
            this.siglas = siglas;
            this.nombre = nombre;
            this.candidato = candidato;
            this.independentismo = independentismo;
        }
        public override string ToString()
        {
            // codigoPadre returns codigo, so this prints codigo for both fields
            return $"{codigo};{codigoPadre};{siglas};{nombre};{candidato};{independentismo}";
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
