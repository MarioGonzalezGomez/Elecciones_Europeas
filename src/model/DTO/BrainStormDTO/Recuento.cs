using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.controller;
using Elecciones_Europeas.src.model.IPF;
using Elecciones_Europeas.src.utils;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.model.DTO.BrainStormDTO
{
    public class Recuento
    {
        public string BLOQUE { get; set; }
        public string PARTIDO { get; set; }
        public string SIGLA_OFICIAL { get; set; }
        public string CODIGO { get; set; }
        public int ORDEN { get; set; }
        public int ESCANOS_DESDE { get; set; }
        public int ESCANOS_HASTA { get; set; }
        public int DIF_ESCANOS { get; set; }
        public int FLECHA_ESCANOS { get; set; }
        public double VOTOS { get; set; }
        public int VOTANTES { get; set; }
        public int DIF_VOTANTES { get; set; }
        public int FLECHA_VOTANTES { get; set; }

        ConfigManager configuration;

        public Recuento()
        {
            configuration = ConfigManager.GetInstance();
        }

        public Recuento(string bloque, string nombre, string siglas, string codigo, int orden, int escanosDesde, int escanosHasta, int difEscanos, double porcetajeVoto, int votantes, int difVotantes, char tendenciaVotos)
        {
            configuration = ConfigManager.GetInstance();
            this.BLOQUE = bloque;
            this.PARTIDO = nombre;
            this.SIGLA_OFICIAL = siglas;
            this.CODIGO = codigo;
            this.ORDEN = orden;
            this.ESCANOS_DESDE = escanosDesde;
            this.ESCANOS_HASTA = escanosHasta;
            this.DIF_ESCANOS = difEscanos;
            this.VOTOS = porcetajeVoto;
            this.VOTANTES = votantes;
            this.DIF_VOTANTES = difVotantes;
            this.FLECHA_VOTANTES = tendenciaVotos;
        }

        public List<Recuento> FromBrainstormDTO(BrainStormDTO dto, ConexionEntityFramework con)
        {
            List<Recuento> recuentos = new List<Recuento>();
            List<PartidoDTO> ordenCod = dto.partidos.OrderBy(par => par.codigo).ToList();

            foreach (PartidoDTO partido in ordenCod)
            {
                Recuento recuento = new Recuento();
                recuento.BLOQUE = $"Partido_{ordenCod.IndexOf(partido) + 1}";
                string cod = $"{configuration.GetValue("codigoRegional")}{partido.codigo.Substring(2)}";
                recuento.PARTIDO = PartidoController.GetInstance(con).FindById(cod).nombre;
                recuento.SIGLA_OFICIAL = partido.siglas;
                recuento.CODIGO = partido.codigo;
                recuento.ORDEN = dto.partidos.IndexOf(partido) + 1;
                recuento.ESCANOS_DESDE = partido.escaniosDesde;
                recuento.ESCANOS_HASTA = partido.escaniosHasta;
                recuento.DIF_ESCANOS = partido.diferenciaEscanios;
                recuento.FLECHA_ESCANOS = GetTendencia(partido.tendencia);
                recuento.VOTOS = partido.porcentajeVoto;
                recuento.VOTANTES = partido.numVotantes;
                recuento.DIF_VOTANTES = Math.Abs(partido.numVotantes - partido.numVotantesHistoricos);
                string tendencia = partido.numVotantesHistoricos == 0 ? "*" :
                      partido.numVotantes - partido.numVotantesHistoricos > 0 ? "+" :
                     partido.numVotantes - partido.numVotantesHistoricos == 0 ? "=" :
                      "-";
                recuento.FLECHA_VOTANTES = GetTendencia(tendencia);
                recuentos.Add(recuento);
            }
            return recuentos;
        }

        private int GetTendencia(string tendencia)
        {
            return tendencia == "+" || tendencia == "*" ? 1 : tendencia == "-" ? -1 : 0;
        }

    }
}
