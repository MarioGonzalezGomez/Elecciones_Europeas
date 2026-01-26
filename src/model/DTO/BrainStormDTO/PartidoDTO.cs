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
        public int escanios
        {
            get; set;
        }
        public int escaniosDesdeSondeo
        {
            get; set;
        }
        public int escaniosHastaSondeo
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
            PartidoDTO dto = new PartidoDTO(cp.codPartido, cp.escaniosHist, cp.numVotantes);
            Partido partido = PartidoController.GetInstance(con).FindById(cp.codPartido);
            if (partido != null)
            {
                dto.padre = partido.codigoPadre;
                dto.siglas = partido.siglas;
                dto.candidato = partido.candidato;
                dto.escanios = oficiales ? cp.escanios : 0;
                dto.escaniosDesdeSondeo = cp.escaniosDesdeSondeo;
                dto.escaniosHastaSondeo = cp.escaniosHastaSondeo;
                dto.porcentajeVoto = oficiales ? cp.porcentajeVoto : cp.porcentajeVotoSondeo;
                dto.porcentajeVotoHistorico = cp.porcentajeVotoHist;
                dto.numVotantes = cp.numVotantes;
                dto.numVotantesHistoricos = cp.numVotantesHist;
                dto.nombre = partido.nombre;
                dto.esUltimoEscano = cp.esUltimoEscano;
                dto.luchaUltimoEscano = cp.luchaUltimoEscano;
                dto.restoVotos = cp.restoVotos;

                int dif = cp.escaniosHist == 0 ? (oficiales ? dto.escanios : dto.escaniosHastaSondeo) : (oficiales ? dto.escanios : dto.escaniosHastaSondeo) - cp.escaniosHist;
                dto.diferenciaEscanios = int.Abs(dif);
                string tendencia = cp.escaniosHist == 0 ? "*" :
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
