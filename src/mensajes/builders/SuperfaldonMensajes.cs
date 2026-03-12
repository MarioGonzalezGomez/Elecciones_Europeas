using System.Runtime.InteropServices;
using System.Text;

using System.Text;
using Elecciones.src.model.DTO.BrainStormDTO;

namespace Elecciones.src.mensajes.builders
{
    /// <summary>
    /// Clase especializada para señales de Superfaldón (pantalla completa con overlay).
    /// Incluye: Sedes SF, Fichas SF, Pactómetro SF, Mayorías SF, Bipartidismo SF, Ganador SF.
    /// </summary>
    internal class SuperfaldonMensajes : IPFMensajesBase
    {
        private static SuperfaldonMensajes? instance;

        private SuperfaldonMensajes() : base() { }

        public static SuperfaldonMensajes GetInstance()
        {
            instance ??= new SuperfaldonMensajes();
            return instance;
        }

        #region Carrusel (incluye sedes)

        public string superfaldonEntra(bool oficiales) => oficiales ? EventRunBuild("Superfaldon/Oficial/Entra") : EventRunBuild("Superfaldon/Sondeo/Entra");
        public string superfaldonSale(bool oficiales) => oficiales ? EventRunBuild("Superfaldon/Oficial/Sale") : EventRunBuild("Superfaldon/Sondeo/Sale");

        #endregion

        #region Sedes

        public string desplegarSede(string codPartido)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventBuild("codigoSede", "MAP_STRING_PAR", $"'{codPartido}'", 1));
            sb.Append(EventRunBuild("Superfaldon/Sedes/DespliegaSede"));
            return sb.ToString();
        }
        public string encadenarSede(string codPartido)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventBuild("nextCodigoSede", "MAP_STRING_PAR", $"'{codPartido}'", 1));
            sb.Append(EventRunBuild("Superfaldon/Sedes/PreparaEncadenaSede"));
            sb.Append(EventRunBuild("Superfaldon/Sedes/EncadenaSede"));
            return sb.ToString();
        }
        public string replegarSede(string codPartido)
        {
            return EventRunBuild("Superfaldon/Sedes/RepliegaSede");
        }

        #endregion

        #region Actualiza

        public string sfFichasSale() => EventRunBuild("ReloadPartidos");

        #endregion

        #region Ultimo

        public string ultimoEntra() => EventRunBuild("ULTIMO/Entra");
        public string ultimoSale() => EventRunBuild("ULTIMO/Sale");

        #endregion

        #region Pactómetro Superfaldón

        // TODO: Construir señal para entrada del gráfico PACTÓMETRO en SUPERFALDÓN
        private readonly double pxTotalesPactometroSF = 1748;
        private double acumuladoSFIzqDesde = 0;
        private double acumuladoSFIzqHasta = 0;
        private double acumuladoSFDerDesde = 0;
        private double acumuladoSFDerHasta = 0;
        private int acumuladoSFEscanosIzqDesde = 0;
        private int acumuladoSFEscanosIzqHasta = 0;
        private int acumuladoSFEscanosDerDesde = 0;
        private int acumuladoSFEscanosDerHasta = 0;
        private bool ultimaEntradaSFIzqFuePrimera = false;
        private bool ultimaEntradaSFDerFuePrimera = false;

        private readonly List<string> partidosEnPactoSFDerecha = new List<string>();
        private readonly List<string> partidosEnPactoSFIzquierda = new List<string>();

        public string sfPactometroEntra()
        {
            ResetPactometroSFState();
            return EventRunBuild("Pactometro/Entra");
        }

        // TODO: Construir señal para encadenar entre gráficos PACTÓMETRO en SUPERFALDÓN
        public string sfPactometroEncadena()
        {
            return EventRunBuild("Pactometro/Encadena");
        }

        // TODO: Construir señal para salida del gráfico PACTÓMETRO en SUPERFALDÓN
        public string sfPactometroSale()
        {
            ResetPactometroSFState();
            StringBuilder sb = new StringBuilder();
            sb.Append(EventRunBuild("Pactometro/Sale"));
            sb.Append(EventRunBuild("Pactometro/reinicioPactometroIzq"));
            sb.Append(EventRunBuild("Pactometro/reinicioPactometroDer"));
            return sb.ToString();
        }

        public string sfPactometroReinicio()
        {
            ResetPactometroSFState();
            StringBuilder sb = new StringBuilder();
            sb.Append(EventRunBuild("Pactometro/reinicioPactometroIzq"));
            sb.Append(EventRunBuild("Pactometro/reinicioPactometroDer"));
            return sb.ToString();
        }

        public string pactometroPartidoEntra(BrainStormDTO dto, PartidoDTO pSeleccionado, bool izquierda)
        {
            if (dto == null || pSeleccionado == null || string.IsNullOrWhiteSpace(pSeleccionado.codigo))
            {
                return "";
            }

            bool oficiales = dto.oficiales;
            int totalEscanos = SafeEscanosTotalesSF(dto);
            var (escanosDesde, escanosHasta) = GetEscanosPartidoSF(pSeleccionado, oficiales);
            bool esPrimeroEnLado;

            if (izquierda)
            {
                if (partidosEnPactoSFIzquierda.Contains(pSeleccionado.codigo))
                {
                    return "";
                }

                partidosEnPactoSFIzquierda.Add(pSeleccionado.codigo);
                esPrimeroEnLado = partidosEnPactoSFIzquierda.Count == 1;

                acumuladoSFEscanosIzqDesde += escanosDesde;
                acumuladoSFEscanosIzqHasta += escanosHasta;
                acumuladoSFIzqDesde += CalcAnchoPactoSF(escanosDesde, totalEscanos, pxTotalesPactometroSF);
                acumuladoSFIzqHasta += CalcAnchoPactoSF(escanosHasta, totalEscanos, pxTotalesPactometroSF);
            }
            else
            {
                if (partidosEnPactoSFDerecha.Contains(pSeleccionado.codigo))
                {
                    return "";
                }

                partidosEnPactoSFDerecha.Add(pSeleccionado.codigo);
                esPrimeroEnLado = partidosEnPactoSFDerecha.Count == 1;

                acumuladoSFEscanosDerDesde += escanosDesde;
                acumuladoSFEscanosDerHasta += escanosHasta;
                acumuladoSFDerDesde += CalcAnchoPactoSF(escanosDesde, totalEscanos, pxTotalesPactometroSF);
                acumuladoSFDerHasta += CalcAnchoPactoSF(escanosHasta, totalEscanos, pxTotalesPactometroSF);
            }

            if (izquierda)
            {
                ultimaEntradaSFIzqFuePrimera = esPrimeroEnLado;
            }
            else
            {
                ultimaEntradaSFDerFuePrimera = esPrimeroEnLado;
            }

            return "";
        }

        public int pactometroNumPartidosLado(bool izquierda)
        {
            return izquierda ? partidosEnPactoSFIzquierda.Count : partidosEnPactoSFDerecha.Count;
        }

        public bool pactometroUltimaEntradaFuePrimera(bool izquierda)
        {
            return izquierda ? ultimaEntradaSFIzqFuePrimera : ultimaEntradaSFDerFuePrimera;
        }

        public (int escanosDesde, int escanosHasta, double barraDesde, double barraHasta) pactometroAcumuladosLado(bool izquierda)
        {
            return izquierda
                ? (acumuladoSFEscanosIzqDesde, acumuladoSFEscanosIzqHasta, acumuladoSFIzqDesde, acumuladoSFIzqHasta)
                : (acumuladoSFEscanosDerDesde, acumuladoSFEscanosDerHasta, acumuladoSFDerDesde, acumuladoSFDerHasta);
        }

        private static (int escanosDesde, int escanosHasta) GetEscanosPartidoSF(PartidoDTO partido, bool oficiales)
        {
            if (partido == null)
            {
                return (0, 0);
            }

            if (oficiales)
            {
                int escanos = Math.Max(0, partido.escanios);
                return (escanos, escanos);
            }

            return (Math.Max(0, partido.escaniosDesdeSondeo), Math.Max(0, partido.escaniosHastaSondeo));
        }

        private static int SafeEscanosTotalesSF(BrainStormDTO dto)
        {
            return dto?.circunscripcionDTO?.escaniosTotales > 0 ? dto.circunscripcionDTO.escaniosTotales : 1;
        }

        private static double CalcAnchoPactoSF(int escanos, int totalEscanos, double pxTotales)
        {
            if (totalEscanos <= 0)
            {
                return 0;
            }

            return (escanos * pxTotales) / totalEscanos;
        }

        private void ResetPactometroSFState()
        {
            acumuladoSFIzqDesde = 0;
            acumuladoSFIzqHasta = 0;
            acumuladoSFDerDesde = 0;
            acumuladoSFDerHasta = 0;
            acumuladoSFEscanosIzqDesde = 0;
            acumuladoSFEscanosIzqHasta = 0;
            acumuladoSFEscanosDerDesde = 0;
            acumuladoSFEscanosDerHasta = 0;
            ultimaEntradaSFIzqFuePrimera = false;
            ultimaEntradaSFDerFuePrimera = false;
            partidosEnPactoSFDerecha.Clear();
            partidosEnPactoSFIzquierda.Clear();
        }

        #endregion

        #region Mayorías Superfaldón

        // TODO: Construir señal para entrada del gráfico MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasEntra() => "";

        // TODO: Construir señal para encadenar entre gráficos MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasEncadena() => "";

        // TODO: Construir señal para salida del gráfico MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasSale() => "";

        #endregion

        #region CCAA

        public string CCAAEntra() => EventRunBuild("CCAA/Entra");
        public string CCAAESale() => EventRunBuild("CCAA/Entra");

        #endregion

        #region Escrutado

        public string EscrutadoEntra() => EventRunBuild("Escrutado/Entra");
        public string EscrutadoSale() => EventRunBuild("Escrutado/Sale");

        #endregion
    }
}
