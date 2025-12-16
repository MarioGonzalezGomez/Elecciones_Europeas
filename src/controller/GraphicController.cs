using Elecciones.src.logic;
using Elecciones.src.mensajes;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.conexion;
using Elecciones.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.controller
{
    public class GraphicController
    {
        public static GraphicController? instance;
        public ObservableInt primeActivo
        {
            get; set;
        }
        public ObservableInt ipfActivo
        {
            get; set;
        }

        private OrdenesIPF? ipf;
        private OrdenesPrime? prime;

        private ConfigManager config;

        private GraphicController()
        {
            config = ConfigManager.GetInstance();
            config.ReadConfig();
            primeActivo = new ObservableInt();
            primeActivo.CambioDeElecciones += PrimeActivoChange;
            primeActivo.Valor = int.Parse(config.GetValue("activoPrime"));
            ipfActivo = new ObservableInt();
            ipfActivo.CambioDeElecciones += IpfActivoChange;
            ipfActivo.Valor = int.Parse(config.GetValue("activoIPF"));
            InitializeConexiones();
        }

        public static GraphicController GetInstance()
        {
            if (instance == null)
            {
                instance = new GraphicController();
            }
            return instance;
        }

        private void InitializeConexiones()
        {
            if (primeActivo.Valor == 1)
            {
                prime = OrdenesPrime.GetInstance();
            }
            if (ipfActivo.Valor == 1)
            {
                ipf = OrdenesIPF.GetInstance();
            }
        }

        // New: video configuration helpers
        /// <summary>
        /// Setea el modo de la fuente de vídeo (true = DIRECTO, false = PREGRABADO) y lo persiste.
        /// </summary>
        public void SetVideoMode(int slotIndex, bool isLive)
        {
            if (slotIndex < 1 || slotIndex > 6) return;
            config.SetValue($"video{slotIndex}_isLive", isLive ? "1" : "0");
            config.SaveConfig();

            // Aquí se podría notificar a IPF/Prime si existiera un mensaje definido.
            // Ejemplo (si se implementa en builder): ipf?.SendVideoMode(slotIndex, isLive);
        }

        /// <summary>
        /// Setea la ruta del fichero a usar para la fuente de vídeo y lo persiste.
        /// </summary>
        public void SetVideoPath(int slotIndex, string path)
        {
            if (slotIndex < 1 || slotIndex > 6) return;
            config.SetValue($"video{slotIndex}_path", path ?? string.Empty);
            config.SaveConfig();

            // Aquí se podría notificar a IPF/Prime si existiera un mensaje definido.
            // Ejemplo (si se implementa en builder): ipf?.SendVideoPath(slotIndex, path);
        }

        //SEÑALES ESPECIALES
        public void ReiniciarConexionPrime()
        {
            if (prime != null) { prime.ReiniciarConexion(); }
        }
        public void ReiniciarConexionIpf()
        {
            if (ipf != null) { ipf.ReiniciarConexion(); }
        }

        public void Reset()
        {
            if (primeActivo.Valor == 1) { prime.Reset(); }
            if (ipfActivo.Valor == 1) { ipf.Reset(); }
        }
        //PRIME - ROTULOS TD
        public void SubirRotulosPrime()
        {
            if (primeActivo.Valor == 1) { prime.SubirRotulos(); }
        }
        public void BajarRotulosPrime()
        {
            if (primeActivo.Valor == 1) { prime.BajarRotulos(); }
        }

        private void PrimeActivoChange(object? sender, EventArgs e)
        {
            if (primeActivo.Valor == 0)
            {
                prime.c.CerrarConexion();
                prime = null;
            }
            else if (primeActivo.Valor == 1)
            {
                prime = OrdenesPrime.GetInstance();
            }
        }
        private void IpfActivoChange(object? sender, EventArgs e)
        {
            if (ipfActivo.Valor == 0)
            {
                ipf.c.CerrarConexion();
                ipf = null;
            }
            else if (ipfActivo.Valor == 1)
            {
                ipf = OrdenesIPF.GetInstance();
            }
        }

        //ESPECIFICAS
        //CAMBIO ENTRE OFI Y SONDEO
        public void SondeoUOficial(bool oficiales)
        {
            if (ipfActivo.Valor == 1) { ipf.SondeoUOficial(oficiales); }
        }

        //ANIMACIONES
        public void PrimerosResultados(bool activa)
        {
            if (ipfActivo.Valor == 1) { ipf.PrimerosResultados(activa); }
        }
        public void AnimacionSondeo(bool activa)
        {
            if (ipfActivo.Valor == 1) { ipf.AnimacionSondeo(activa); }
        }

        public string RecibirPrimerosResultados()
        {
            if (ipfActivo.Valor == 1) { return ipf.RecibirPrimerosResultados(); } else { return null; }
        }
        public string RecibirAnimacionSondeo()
        {
            if (ipfActivo.Valor == 1) { return ipf.RecibirPrimerosResultados(); } else { return null; }
        }

        //PROYECCION
        public void Proyeccion(bool activa)
        {
            if (ipfActivo.Valor == 1) { ipf.Proyeccion(activa); }
        }

        //GIROS
        public void DeSondeoAOficiales()
        {
            if (ipfActivo.Valor == 1) { ipf.DeSondeoAOficiales(); }
        }

        //CAMBIO DE ELECCIONES
        public void CambioElecciones(bool europa)
        {
            if (ipfActivo.Valor == 1) { ipf.CambioElecciones(europa); }
        }

        //RELOJ
        public void EntraReloj(int segundos)
        {
            if (ipfActivo.Valor == 1) { ipf.RelojEntra(segundos); }
        }
        public void SaleReloj()
        {
            if (ipfActivo.Valor == 1) { ipf.RelojSale(); }
        }

        //TICKER
        public void TickerEntra(bool oficial, BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerEntra(oficial, dto); }
        }
        public void TickerEncadena(bool oficial, BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerEncadena(oficial, dto); }
        }
        public void TickerActualiza(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerActualiza(dto); }
        }
        public void TickerSale(bool oficial)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerSale(oficial); }
        }

        public void TickerActualizaEscrutado()
        {
            if (ipfActivo.Valor == 1) { ipf.TickerActualizaEscrutado(); }
        }
        public void TickerActualizaDatos()
        {
            if (ipfActivo.Valor == 1) { ipf.TickerActualizaDatos(); }
        }
        public void TickerActualizaDatosIndividualizado(List<PartidoDTO> partidos)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerActualizaDatosIndividualizado(partidos); }
        }

        public void TickerYaNoEstaIndividualizado(List<PartidoDTO> partidos)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerYaNoEstaIndividualizado(partidos); }
        }
        public void TickerActualizaPosiciones()
        {
            if (ipfActivo.Valor == 1) { ipf.TickerActualizaPosiciones(); }
        }
        public void TickerActualizaNumPartidos()
        {
            if (ipfActivo.Valor == 1) { ipf.TickerActualizaNumPartidos(); }
        }

        public void TickerVotosEntra(bool oficial)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerVotosEntra(oficial); }
        }
        public void TickerVotosSale(bool oficial)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerVotosSale(oficial); }
        }
        public void TickerHistoricosEntra(bool oficial)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerHistoricosEntra(oficial); }
        }
        public void TickerHistoricosSale(bool oficial)
        {
            if (ipfActivo.Valor == 1) { ipf.TickerHistoricosSale(oficial); }
        }
        public void TickerMillonesEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.TickerMillonesEntra(); }
        }
        public void TickerMillonesSale()
        {
            if (ipfActivo.Valor == 1) { ipf.TickerMillonesSale(); }
        }

        public void TickerFotosEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.TickerFotosEntra(); }
        }
        public void TickerFotosSale()
        {
            if (ipfActivo.Valor == 1) { ipf.TickerFotosSale(); }
        }

        //PP_PSOE
        public void PP_PSOEEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.PP_PSOEEntra(); }
        }
        public void PP_PSOESale()
        {
            if (ipfActivo.Valor == 1) { ipf.PP_PSOESale(); }
        }

        //DESPLIEGAS
        public void Despliega4()
        {
            if (ipfActivo.Valor == 1) { ipf.Despliega4(); }
        }
        public void Despliega5()
        {
            if (ipfActivo.Valor == 1) { ipf.Despliega5(); }
        }
        public void RecuperaTodos()
        {
            if (ipfActivo.Valor == 1) { ipf.RecuperaTodos(); }
        }

        //PACTOS
        public void pactosEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.pactosEntra(); }
        }
        public void pactosReinicio()
        {
            if (ipfActivo.Valor == 1) { ipf.pactosReinicio(); }
        }
        public void pactosSale()
        {
            if (ipfActivo.Valor == 1) { ipf.pactosSale(); }
        }

        public void pactosEntraDerecha(int posicionPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.pactosEntraDerecha(posicionPartido); }
        }
        public void pactosEntraIzquierda(int posicionPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.pactosEntraIzquierda(posicionPartido); }
        }

        public void pactosSaleDerecha(int posicionPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.pactosSaleDerecha(posicionPartido); }
        }
        public void pactosSaleIzquierda(int posicionPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.pactosSaleIzquierda(posicionPartido); }
        }

        //INDEPENDENTISMO
        public void independentismoEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.independentismoEntra(); }
        }
        public void independentismoReinicio()
        {
            if (ipfActivo.Valor == 1) { ipf.independentismoReinicio(); }
        }
        public void independentismoSale()
        {
            if (ipfActivo.Valor == 1) { ipf.independentismoSale(); }
        }

        public void independentismoEntraDerecha(int posicionPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.independentismoEntraDerecha(posicionPartido); }
        }
        public void independentismoEntraIzquierda(int posicionPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.independentismoEntraIzquierda(posicionPartido); }
        }

        public void independentismoSaleDerecha(int posicionPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.independentismoSaleDerecha(posicionPartido); }
        }
        public void independentismoSaleIzquierda(int posicionPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.independentismoSaleIzquierda(posicionPartido); }
        }

        //SEDES
        public void SedesEntra(bool tickerIn, string codPartido)
        {
            if (ipfActivo.Valor == 1) { ipf.SedesEntra(tickerIn, codPartido); }
        }
        public void SedesEncadena(bool tickerIn, string codPartidoSiguiente, string codPartidoAnterior = "")
        {
            if (ipfActivo.Valor == 1) { ipf.SedesEncadena(tickerIn, codPartidoSiguiente, codPartidoAnterior); }
        }
        public void SedesSale(bool tickerIn, string codPartido = "")
        {
            if (ipfActivo.Valor == 1) { ipf.SedesSale(tickerIn, codPartido); }
        }


        //CARTONES
        //PARTICIPACION
        public void participacionEntra(BrainStormDTO dto, int avance)
        {
            if (ipfActivo.Valor == 1) { ipf.participacionEntra(dto, avance); }
        }
        public void participacionEncadena(BrainStormDTO dto, int avance)
        {
            if (ipfActivo.Valor == 1) { ipf.participacionEncadena(dto, avance); }
        }
        public void participacionSale()
        {
            if (ipfActivo.Valor == 1) { ipf.participacionSale(); }
        }

        //CCAA
        public void ccaaEntra(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1) { ipf.ccaaEntra(dto); }
        }
        public void ccaaEncadena()
        {
            if (ipfActivo.Valor == 1) { ipf.ccaaEncadena(); }
        }
        public void ccaaSale()
        {
            if (ipfActivo.Valor == 1) { ipf.ccaaSale(); }
        }

        //FICHAS DE PARTIDO
        public void fichaEntra(bool oficiales, BrainStormDTO dto, PartidoDTO partido)
        {
            if (ipfActivo.Valor == 1) { ipf.fichaEntra(oficiales, dto, partido); }
        }
        public void fichaEncadena(bool oficiales, BrainStormDTO dto, PartidoDTO partido)
        {
            if (ipfActivo.Valor == 1) { ipf.fichaEncadena(oficiales, dto, partido); }
        }
        public void fichaSale(bool oficiales)
        {
            if (ipfActivo.Valor == 1) { ipf.fichaSale(oficiales); }
        }

        //PACTOMETRO
        public void pactometroEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.pactometroEntra(); }
        }
        public void pactometroEncadena()
        {
            if (ipfActivo.Valor == 1) { ipf.pactometroEncadena(); }
        }
        public void pactometroSale()
        {
            if (ipfActivo.Valor == 1) { ipf.pactometroSale(); }
        }

        public void pactometroVictoria()
        {
            if (ipfActivo.Valor == 1) { ipf.pactometroVictoria(); }
        }

        //MAYORIAS
        public void mayoriasEntra(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1) { ipf.mayoriasEntra(dto); }
        }
        public void mayoriasEncadena(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1) { ipf.mayoriasEncadena(dto); }
        }
        public void mayoriasSale()
        {
            if (ipfActivo.Valor == 1) { ipf.mayoriasSale(); }
        }

        //SUPERFALDON
        public void superfaldonEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.superfaldonEntra(); }
        }
        public void superfaldonSale()
        {
            if (ipfActivo.Valor == 1) { ipf.superfaldonSale(); }
        }

        //SEDES
        public void superfaldonSedesEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.superfaldonSedesEntra(); }
        }
        public void superfaldonSedesEncadena()
        {
            if (ipfActivo.Valor == 1) { ipf.superfaldonSedesEncadena(); }
        }
        public void superfaldonSedesSale()
        {
            if (ipfActivo.Valor == 1) { ipf.superfaldonSedesSale(); }
        }

        //SUPERFALDON - FICHAS
        public void sfFichasEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.sfFichasEntra(); }
            // Enviar TickerFotosEntra a IPF2 (equipo secundario con Faldones)
            ConexionGraficos.EnviarTickerFotosIPF2();
        }
        public void sfFichasEncadena()
        {
            if (ipfActivo.Valor == 1) { ipf.sfFichasEncadena(); }
            // Enviar TickerFotosEntra a IPF2 (equipo secundario con Faldones)
            ConexionGraficos.EnviarTickerFotosIPF2();
        }
        public void sfFichasSale()
        {
            if (ipfActivo.Valor == 1) { ipf.sfFichasSale(); }
        }

        //SUPERFALDON - PACTOMETRO
        public void sfPactometroEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.sfPactometroEntra(); }
        }
        public void sfPactometroEncadena()
        {
            if (ipfActivo.Valor == 1) { ipf.sfPactometroEncadena(); }
        }
        public void sfPactometroSale()
        {
            if (ipfActivo.Valor == 1) { ipf.sfPactometroSale(); }
        }

        //SUPERFALDON - MAYORIAS
        public void sfMayoriasEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.sfMayoriasEntra(); }
        }
        public void sfMayoriasEncadena()
        {
            if (ipfActivo.Valor == 1) { ipf.sfMayoriasEncadena(); }
        }
        public void sfMayoriasSale()
        {
            if (ipfActivo.Valor == 1) { ipf.sfMayoriasSale(); }
        }

        //SUPERFALDON - BIPARTIDISMO
        public void sfBipartidismoEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.sfBipartidismoEntra(); }
        }
        public void sfBipartidismoEncadena()
        {
            if (ipfActivo.Valor == 1) { ipf.sfBipartidismoEncadena(); }
        }
        public void sfBipartidismoSale()
        {
            if (ipfActivo.Valor == 1) { ipf.sfBipartidismoSale(); }
        }

        //SUPERFALDON - GANADOR
        public void sfGanadorEntra()
        {
            if (ipfActivo.Valor == 1) { ipf.sfGanadorEntra(); }
        }
        public void sfGanadorEncadena()
        {
            if (ipfActivo.Valor == 1) { ipf.sfGanadorEncadena(); }
        }
        public void sfGanadorSale()
        {
            if (ipfActivo.Valor == 1) { ipf.sfGanadorSale(); }
        }
    }
}