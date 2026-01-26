using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.utils;
using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Elecciones.src.model.IPF.DTO
{
    public class SedesDTO
    {
        public string codigo
        {
            get; set;
        }
        public string padre
        {
            get; set;
        }
        public string siglas
        {
            get; set;
        }
        public string literalPartido
        {
            get; set;
        }
        public string candidato
        {
            get; set;
        }
        public int escanios
        {
            get; set;
        }
        public int escaniosHistoricos
        {
            get; set;
        }
        public double porcentajeVoto
        {
            get; set;
        }
        public double porcentajeVotoHist
        {
            get; set;
        }
        public int numVotantes
        {
            get; set;
        }
        public int numVotantesHist
        {
            get; set;
        }
        public int diferenciaEscanios
        {
            get; set;
        }
        public string tendencia
        {
            get; set;
        }

        ConfigManager configuration;

        public SedesDTO(CircunscripcionPartido cp, bool oficiales, ConexionEntityFramework con)
        {
            configuration = ConfigManager.GetInstance();
            SedesDTO dto = FromCP(cp, oficiales, con);
            this.codigo = dto.codigo;
            this.padre = dto.padre;
            this.siglas = dto.siglas;
            this.literalPartido = dto.literalPartido;
            this.candidato = dto.candidato;
            this.escanios = dto.escanios;
            this.escaniosHistoricos = dto.escaniosHistoricos;
            this.porcentajeVoto = dto.porcentajeVoto;
            this.porcentajeVotoHist = dto.porcentajeVotoHist;
            this.numVotantes = dto.numVotantes;
            this.numVotantesHist = dto.numVotantesHist;
            this.diferenciaEscanios = dto.diferenciaEscanios;
            this.tendencia = dto.tendencia;
        }

        private SedesDTO()
        {
            configuration = ConfigManager.GetInstance();
        }

        public static SedesDTO FromCP(CircunscripcionPartido cp, bool oficiales, ConexionEntityFramework con)
        {
            SedesDTO dto = new SedesDTO();
            Partido partido = PartidoController.GetInstance(con).FindById(cp.codPartido);
            dto.codigo = cp.codPartido;
            dto.padre = partido.codigoPadre;
            dto.siglas = partido.siglas;
            dto.literalPartido = partido.nombre;
            dto.candidato = partido.candidato;
            dto.escanios = cp.escanios;
            dto.escaniosHistoricos = cp.escaniosHist;
            dto.porcentajeVoto = oficiales ? cp.porcentajeVoto : cp.porcentajeVotoSondeo;
            dto.porcentajeVotoHist = cp.porcentajeVotoHist;
            dto.numVotantes = cp.numVotantes;
            dto.numVotantesHist = cp.numVotantesHist;

            int dif = cp.escaniosHist == 0 ? 0 : dto.escanios - cp.escaniosHist;
            dto.diferenciaEscanios = dif;
            string tendencia = (cp.escaniosHist == 0) ? "*" :
                   (dif > 0) ? "+" :
                   (dif == 0) ? "=" :
                   "-";
            if (dif < 0)
            {
                dif = dif * (-1);
            }
            dto.tendencia = tendencia;
            return dto;
        }
        public static SedesDTO FromPartidoDTO(PartidoDTO pdto, ConexionEntityFramework con)
        {
            SedesDTO dto = new SedesDTO();
            ConfigManager cm = ConfigManager.GetInstance();
            string codigoRegional = pdto.codigo;
            Partido partido = PartidoController.GetInstance(con).FindById(codigoRegional);
            dto.codigo = pdto.codigo;
            if (partido != null)
            {
                dto.padre = partido.codigoPadre;
                dto.siglas = partido.siglas;
                dto.literalPartido = partido.nombre;
                dto.candidato = partido.candidato;
            }
            else
            {
                dto.padre = pdto.codigo;
            }
            dto.escanios = pdto.escanios;
            dto.escaniosHistoricos = pdto.escaniosHistoricos;
            dto.porcentajeVoto = pdto.porcentajeVoto;
            dto.porcentajeVotoHist = pdto.porcentajeVotoHistorico;
            dto.numVotantes = pdto.numVotantes;
            dto.numVotantesHist = pdto.numVotantesHistoricos;

            int dif = dto.escaniosHistoricos == 0 ? 0 : dto.escanios - dto.escaniosHistoricos;
            dto.diferenciaEscanios = dif;
            string tendencia = (dto.escaniosHistoricos == 0) ? "*" :
                   (dif > 0) ? "+" :
                   (dif == 0) ? "=" :
                   "-";
            if (dif < 0)
            {
                dif = dif * (-1);
            }
            dto.tendencia = tendencia;
            return dto;
        }

        public async Task ToJson()
        {
            string fileName = $"{configuration.GetValue("rutaArchivos")}\\JSON\\Sedes.json";
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

            string fileName = $"{configuration.GetValue("rutaArchivos")}\\CSV\\Sedes.csv";

            int difVotos = numVotantes - numVotantesHist;
            string tendenciaVotos = numVotantesHist == 0 ? "*" :
              difVotos > 0 ? "+" :
              difVotos == 0 ? "=" :
              "-";
            difVotos = difVotos < 0 ? difVotos * (-1) : difVotos;
            double porcentajeVotoTruncado = Math.Truncate(porcentajeVoto * 10) / 10;

            string csv = $"C�digo;Padre;Siglas;Candidato;Esca�os;Hist�ricos;% Voto;Votantes;Diferencia de esca�os;Tendencia;Diferencia de votos;Tendendia\n";
            csv += $"{codigo};{padre};{this.siglas};{this.candidato};{this.escanios};{this.escaniosHistoricos};{porcentajeVotoTruncado.ToString()};{numVotantes.ToString("#,##0").Replace(",", ".")};{diferenciaEscanios};{tendencia};{difVotos.ToString("#,##0").Replace(",", ".")};{tendenciaVotos}\n";
            await File.WriteAllTextAsync(fileName, csv);
        }
    }
}
