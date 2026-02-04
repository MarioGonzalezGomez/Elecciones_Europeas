using Elecciones.src.conexion;
using Elecciones.src.model.IPF;
using Elecciones.src.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Elecciones.src.model.DTO.BrainStormDTO
{
    public class BrainStormDTO
    {
        public CircunscripcionDTO circunscripcionDTO
        {
            get; set;
        }
        public int numPartidos
        {
            get; set;
        }
        public bool oficiales
        {
            get; set;
        }
        public List<PartidoDTO> partidos
        {
            get; set;
        }
        ConfigManager configuration;

        public BrainStormDTO(Circunscripcion c, int avanceActual, int tipoElecciones, List<CircunscripcionPartido> cps, bool oficiales, ConexionEntityFramework con)
        {
            configuration = ConfigManager.GetInstance();
            circunscripcionDTO = CircunscripcionDTO.FromCircunscripcion(c, avanceActual, tipoElecciones, con);
            partidos = new List<PartidoDTO>();
            this.oficiales=oficiales;
            foreach (var partido in cps)
            {
                PartidoDTO dto = PartidoDTO.FromCP(partido, oficiales, con);
                // dto.codigo = dto.codigo.StartsWith("09") ? $"00{dto.codigo.Substring(2)}" : dto.codigo;
                if (dto != null) { partidos.Add(dto); }

            }
            this.numPartidos = partidos.Where(par => (oficiales ? par.escanios : par.escaniosHastaSondeo) > 0).Count();
        }
        public BrainStormDTO(BrainStormDTO dto)
        {
            this.circunscripcionDTO = dto.circunscripcionDTO;
            this.numPartidos = dto.partidos.Where(par => (this.oficiales ? par.escanios : par.escaniosHastaSondeo) > 0).Count();
            this.partidos = dto.partidos;
            this.configuration = dto.configuration;
        }

        public async Task ToJson(string ruta = "BrainStorm")
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\{ruta}.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultBufferSize = 1024
            };
            string json = JsonSerializer.Serialize(this, options);
            await File.WriteAllTextAsync(fileName, json);
        }
        public async Task ToCsv(string ruta = "BrainStorm", string nombreSondeo = "")
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\{ruta}.csv";
            if (configuration.GetValue("subcarpetas").Equals("1"))
            {
                fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\{ruta}.csv";
            }
            else
            {
                fileName = $"{configuration.GetValue("rutaArchivos")}\\{ruta}.csv";
            }
            PartidoDTO ultimo;
            PartidoDTO siguiente;
            if (circunscripcionDTO.codigo.EndsWith("00000")) { 
                ultimo = new PartidoDTO();
                siguiente = new PartidoDTO();
            } else {
                ultimo = this.partidos.FirstOrDefault(p => p.esUltimoEscano != 0) ?? new PartidoDTO();
                siguiente = this.partidos.FirstOrDefault(p => p.luchaUltimoEscano != 0) ?? new PartidoDTO();
            }
            string resultado = "Código;Nombre;Escrutado;Esca�os;Mayoría;Avance;Participacion;Participacion Historica;Media de Participacion;Votantes;Últimas Elecciones;Numero de partidos;Ultimo;Siguiente;Resto;NombreSondeo\n";
            resultado += $"{circunscripcionDTO.codigo};{circunscripcionDTO.nombre};{circunscripcionDTO.escrutado.ToString("F2")};{circunscripcionDTO.escaniosTotales};{circunscripcionDTO.mayoria};{circunscripcionDTO.numAvance};{circunscripcionDTO.participacion.ToString("F2")};{circunscripcionDTO.participacionHistorica.ToString("F2")};{circunscripcionDTO.participacionMedia.ToString("F2")};{circunscripcionDTO.numVotantesTotales};{circunscripcionDTO.anioUltimasElecciones};{numPartidos};{ultimo.siglas};{siguiente.siglas};{siguiente.restoVotos};{nombreSondeo}\n";
            resultado += $"Código;Padre;Siglas;Candidato;Escaños;Escaños Desde Sondeo;Hasta Sondeo;Históricos;% Voto;Votantes;Diferencia de escaños;Tendencia;Diferencia de votos;Tendendia;Votantes Historico;Nombre\n";
            foreach (var p in partidos)
            {
                string codigo = p.padre;
                string codigoPadre = codigo;
                int difVotos = p.numVotantes - p.numVotantesHistoricos;
                string tendenciaVotos = p.numVotantesHistoricos == 0 ? "*" :
                  difVotos > 0 ? "+" :
                  difVotos == 0 ? "=" :
                  "-";
                difVotos = difVotos < 0 ? difVotos * (-1) : difVotos;
                double porcentajeVotoTruncado = Math.Truncate(p.porcentajeVoto * 10) / 10;
                resultado += $"{p.codigo};{codigoPadre};{p.siglas};{p.candidato};{p.escanios};{p.escaniosDesdeSondeo};{p.escaniosHastaSondeo};{p.escaniosHistoricos};{porcentajeVotoTruncado.ToString()};{p.numVotantes};{p.diferenciaEscanios};{p.tendencia};{difVotos};{tendenciaVotos};{p.numVotantesHistoricos};{p.nombre}\n";
            }

            await File.WriteAllTextAsync(fileName, resultado);
        }
    }
}
