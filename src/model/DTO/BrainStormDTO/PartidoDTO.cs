using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.model.IPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.model.DTO.BrainStormDTO
{
    public class PartidoDTO
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
        public string candidato
        {
            get; set;
        }
        public int escaniosDesde
        {
            get; set;
        }
        public int escaniosHasta
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
        public double porcentajeVotoHistorico
        {
            get; set;
        }
        public int numVotantes
        {
            get; set;
        }
        public int numVotantesHistoricos
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
        public string independentismo
        {
            get; set;
        }
        public string nombre
        {
            get; set;
        }
        public int esUltimoEscano
        {
            get; set;
        }
        public int luchaUltimoEscano
        {
            get; set;
        }
        public int restoVotos
        {
            get; set;
        }

        private PartidoDTO(string codigo, int escaniosHistoricos, int numVotantes)
        {
            this.codigo = codigo;
            this.escaniosHistoricos = escaniosHistoricos;
            this.numVotantes = numVotantes;
        }

        public PartidoDTO()
        {

        }

        public static PartidoDTO FromCP(CircunscripcionPartido cp, bool oficiales, ConexionEntityFramework con)
        {
            PartidoDTO dto = new PartidoDTO(cp.codPartido, cp.escaniosHastaHist, cp.numVotantes);
            Partido partido = PartidoController.GetInstance(con).FindById(cp.codPartido);
            if (partido != null)
            {
                dto.padre = partido.codigoPadre;
                dto.siglas = partido.siglas;
                dto.candidato = partido.candidato;
                dto.escaniosDesde = oficiales ? cp.escaniosDesde : cp.escaniosDesdeSondeo;
                dto.escaniosHasta = oficiales ? cp.escaniosHasta : cp.escaniosHastaSondeo;
                dto.porcentajeVoto = oficiales ? cp.porcentajeVoto : cp.porcentajeVotoSondeo;
                dto.porcentajeVotoHistorico = cp.porcentajeVotoHist;
                dto.numVotantes = cp.numVotantes;
                dto.numVotantesHistoricos = cp.numVotantesHist;
                dto.independentismo = partido.independentismo.ToString();
                dto.nombre = partido.nombre;
                dto.esUltimoEscano = cp.esUltimoEscano;
                dto.luchaUltimoEscano = cp.luchaUltimoEscano;
                dto.restoVotos = cp.restoVotos;

                int dif = cp.escaniosHastaHist == 0 ? dto.escaniosHasta : dto.escaniosHasta - cp.escaniosHastaHist;
                dto.diferenciaEscanios = int.Abs(dif);
                string tendencia = cp.escaniosHastaHist == 0 ? "*" :
                       dif > 0 ? "+" :
                       dif == 0 ? "=" :
                       "-";
                dto.tendencia = tendencia;
                return dto;
            }
            return null;
        }
    }
}
