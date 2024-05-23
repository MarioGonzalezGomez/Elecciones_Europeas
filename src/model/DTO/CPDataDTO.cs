using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elecciones_Europeas.src.model.DTO.BrainStormDTO;
using Elecciones_Europeas.src.utils;

namespace Elecciones_Europeas.src.model.IPF.DTO
{
    public class CPDataDTO
    {
        public string codigo { get; set; }
        public string siglas { get; set; }
        public string escaniosDesde { get; set; }
        public string escaniosHasta { get; set; }
        public string escaniosHistoricos { get; set; }
        public string diferenciaEscanios { get; set; }
        public string votantes { get; set; }
        public string votantesHistoricos { get; set; }
        public string diferenciaVotantes { get; set; }
        public string porcentajeVoto { get; set; }
        public string porcentajeVotoHist { get; set; }


        public CPDataDTO() { }

        public CPDataDTO(string codigo, string siglas, string escaniosDesde, string escaniosHasta, string escaniosHistoricos, string diferenciaEscanios, string votantes, string votantesHistoricos, string diferenciaVotantes, string porcentajeVoto, string porcentajeVotoHist)
        {
            this.codigo = codigo;
            this.siglas = siglas;
            this.escaniosDesde = escaniosDesde;
            this.escaniosHasta = escaniosHasta;
            this.escaniosHistoricos = escaniosHistoricos;
            this.diferenciaEscanios = diferenciaEscanios;
            this.votantes = votantes;
            this.votantesHistoricos = votantesHistoricos;
            this.diferenciaVotantes = diferenciaVotantes;
            this.porcentajeVoto = porcentajeVoto;
            this.porcentajeVotoHist = porcentajeVotoHist;
        }

        public static List<CPDataDTO> FromBSDto(BrainStormDTO dto)
        {
            List<CPDataDTO> lista = new List<CPDataDTO>();
            foreach (PartidoDTO p in dto.partidos)
            {
                int difesc = p.escaniosHasta - p.escaniosHistoricos;
                int difVotos = p.numVotantes - p.numVotantesHistoricos;
                lista.Add(new CPDataDTO(p.codigo, p.siglas, p.escaniosDesde.ToString(), p.escaniosHasta.ToString(), p.escaniosHistoricos.ToString(), difesc.ToString(), p.numVotantes.ToString(), p.numVotantesHistoricos.ToString(), difVotos.ToString(), p.porcentajeVoto.ToString(), p.porcentajeVotoHistorico.ToString()));
            }
            return lista;
        }

        public override bool Equals(object? obj)
        {
            ConfigManager cm = ConfigManager.GetInstance();
            return obj is CPDataDTO dto && (codigo == dto.codigo || codigo == cm.GetValue("codigoRegional") + dto.codigo.Substring(2));
        }
    }
}
