using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elecciones.src.conexion;
using Elecciones.src.logic;
using Elecciones.src.mensajes;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using Elecciones.src.model.IPF.DTO;
using Elecciones.src.utils;

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
            if (primeActivo.Valor == 1 && prime != null) { prime.Reset(); }
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.Reset(); }
        }
        //PRIME - ROTULOS TD
        public void SubirRotulosPrimeTD()
        {
            if (primeActivo.Valor == 1 && prime != null) { prime.SubirRotulosTD(); }
        }
        public void BajarRotulosPrimeTD()
        {
            if (primeActivo.Valor == 1 && prime != null) { prime.BajarRotulosTD(); }
        }
        public async void SubirRotulosPrimeEsp(int delay = 0)
        {
            if (delay > 0) await Task.Delay(delay);
            if (primeActivo.Valor == 1 && prime != null) { prime.SubirRotulosEsp(); }
        }
        public async void BajarRotulosPrimeEsp(int delay = 0)
        {
            if (delay > 0) await Task.Delay(delay);
            if (primeActivo.Valor == 1 && prime != null) { prime.BajarRotulosEsp(); }
        }

        private void PrimeActivoChange(object? sender, EventArgs e)
        {
            if (primeActivo.Valor == 0)
            {
                if (prime != null)
                {
                    prime.c.CerrarConexion();
                }
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
                if (ipf != null)
                {
                    ipf.c.CerrarConexion();
                }
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
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.SondeoUOficial(oficiales); }
        }

        //ANIMACIONES
        public void PrimerosResultados(bool activa)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.PrimerosResultados(activa); }
        }
        public void AnimacionSondeo(bool activa)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.AnimacionSondeo(activa); }
        }

        public string RecibirPrimerosResultados()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { return ipf.RecibirPrimerosResultados(); } else { return string.Empty; }
        }
        public string RecibirAnimacionSondeo()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { return ipf.RecibirPrimerosResultados(); } else { return string.Empty; }
        }

        //GIROS
        public void DeSondeoAOficiales()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.DeSondeoAOficiales(); }
        }

        //CAMBIO DE ELECCIONES
        public void CambioElecciones(bool europa)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.CambioElecciones(europa); }
        }

        //RELOJ
        public void EntraReloj(int segundos)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.RelojEntra(segundos); }
        }
        public void SaleReloj()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.RelojSale(); }
        }

        //TICKER
        public void TickerEntra(bool oficial, BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerEntra(oficial, dto); }
        }
        public void TickerEncadena(bool oficial, BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerEncadena(oficial, dto); }
        }
        public void TickerActualiza(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerActualiza(dto); }
        }
        public void TickerSale(bool oficial, BrainStormDTO? dto = null)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerSale(oficial, dto); }
        }

        public void TickerEscanosEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerEscanosEntra(); }
        }
        public void TickerEscanosSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerEscanosSale(); }
        }
        public void TickerVotosEntra(bool oficiales)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerVotosEntra(oficiales); }
            //if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerVotosEntra(); }
        }
        public void TickerVotosSale(bool oficiales)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerVotosSale(oficiales); }
        }
        public void TickerHistoricosEntra(bool oficiales)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerHistoricosEntra(oficiales); }
        }
        public void TickerHistoricosSale(bool oficiales)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerHistoricosSale(oficiales); }
        }
        public void TickerHistoricosEntraInd()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerHistoricosEntraInd(); }
        }

        public void TickerHistoricosSaleInd()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerHistoricosSaleInd(); }
        }
        public void TickerHistoricosEntraCom()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerHistoricosEntraCom(); }
        }
        public void TickerHistoricosSaleCom()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerHistoricosSaleCom(); }
        }
        public void TickerMillonesEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerMillonesEntra(); }
        }
        public void TickerMillonesSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerMillonesSale(); }
        }

        public void TickerFotosEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerFotosEntra(); }
        }
        public void TickerFotosSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerFotosSale(); }
        }

        //VIDEOS EN TICKER
        public void VideoIn(BrainStormDTO dto, PartidoDTO partidoSeleccionado)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.VideoIn(dto, partidoSeleccionado); }
        }
        public void VideoOut(BrainStormDTO dto, PartidoDTO partidoSeleccionado)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.VideoOut(dto, partidoSeleccionado); }
        }
        public void VideoOutTodos(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.VideoOutTodos(dto); }
        }
        public void VideoInTodos(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.VideoInTodos(dto); }
        }


        //TICKER TD
        public void TickerTDEntra(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerTDEntra(dto); }
        }
        public void TickerTDActualiza(BrainStormDTO dtoAnterior, BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerTDActualiza(dtoAnterior, dto); }
        }
        public void TickerTDSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.TickerTDSale(); }
        }

        //PACTOS
        public void pactosEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactosEntra(); }
        }
        public void pactosReinicio(string tipoGrafico)
        {
            switch (tipoGrafico)
            {
                case "FICHAS":
                    if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactosReinicio(); }
                    break;

                case "ÚLTIMO ESCAÑO":
                    break;
                default:
                    // Si no es ningún tipo específico de pacto gráfico, no hacer nada
                    break;
            }
        }
        public void pactosSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactosSale(); }
        }

        public void pactosEntraDerecha(BrainStormDTO dto, PartidoDTO pSeleccionado)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactosEntraDerecha(dto, pSeleccionado); }
        }
        public void pactosEntraIzquierda(BrainStormDTO dto, PartidoDTO pSeleccionado)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactosEntraIzquierda(dto, pSeleccionado); }
        }

        public void pactosSaleDerecha()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactosSaleDerecha(); }
        }
        public void pactosSaleIzquierda()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactosSaleIzquierda(); }
        }

        //SEDES
        public void SedesEntra(PartidoDTO seleccionado)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.SedesEntra(seleccionado); }
        }
        public void SedesEncadena(PartidoDTO seleccionado)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.SedesEncadena(seleccionado); }
        }
        public void SedesSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.SedesSale(); }
        }


        //CARTONES
        //PARTICIPACION

        internal void CartonesActualiza()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.CartonesActualiza(); }
        }

        public void participacionEntra(BrainStormDTO dto, int avance)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.participacionEntra(dto, avance); }
        }
        public void participacionEncadena(BrainStormDTO dto, int avance)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.participacionEncadena(dto, avance); }
        }
        public void participacionSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.participacionSale(); }
        }

        //CCAA
        public void ccaaEntra(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ccaaEntra(dto); }
        }
        public void ccaaEncadena()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ccaaEncadena(); }
        }
        public void ccaaSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ccaaSale(); }
        }

        //FICHAS DE PARTIDO
        public void fichaEntra(bool oficiales, BrainStormDTO dto, PartidoDTO partido)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.fichaEntra(oficiales, dto, partido); }
        }
        public void fichaEncadena(bool oficiales, BrainStormDTO dto, PartidoDTO partido)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.fichaEncadena(oficiales, dto, partido); }
        }
        public void fichaActualiza(bool oficiales, BrainStormDTO dtoAnterior, BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.fichaActualiza(oficiales, dtoAnterior, dto); }
        }
        public void fichaSale(bool oficiales)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.fichaSale(oficiales); }
        }

        //PACTOMETRO
        public void pactometroEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactometroEntra(); }
        }
        public void pactometroEncadena()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactometroEncadena(); }
        }
        public void pactometroSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactometroSale(); }
        }

        public void pactometroVictoria()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.pactometroVictoria(); }
        }

        //MAYORIAS
        public void mayoriasEntra(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.mayoriasEntra(dto); }
        }
        public void mayoriasEncadena(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.mayoriasEncadena(dto); }
        }
        public void mayoriasSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.mayoriasSale(); }
        }

        //CARTON PARTIDOS
        public void cartonPartidosEntra(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.cartonPartidosEntra(dto); }
        }
        public void cartonPartidosActualiza(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.cartonPartidosActualiza(dto); }
        }
        public void cartonPartidosSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.cartonPartidosSale(); }
        }

        //ULTIMO
        public void ultimoEntra(BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ultimoEntra(dto); }
        }
        public void ultimoEncadena(BrainStormDTO dtoAnterior, BrainStormDTO dto)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ultimoEncadena(dtoAnterior, dto); }
        }
        public bool ultimoActualiza(BrainStormDTO dtoNuevo)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { return ipf.ultimoActualiza(dtoNuevo); }
            return false;
        }
        public void ultimoLimpiaPartidos()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ultimoLimpiaPartidos(); }
        }
        public void ultimoEntraPartido(BrainStormDTO dto, CPDataDTO partido, bool esIzquierda)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ultimoEntraPartido(dto, partido, esIzquierda); }
        }
        public void ultimoSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ultimoSale(); }
        }

        //ULTIMO SUPERFADON

        public void ultimoSuperEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ultimoSuperEntra(); }
        }
        public void ultimoSuperSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.ultimoSuperSale(); }
        }
        public bool ultimoSuperCambia(BrainStormDTO dtoNuevo)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { return ipf.ultimoSuperCambia(dtoNuevo); }
            return false;
        }

        //SUPERFALDON
        public void superfaldonEntra(bool oficiales)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.superfaldonEntra(oficiales); }
        }
        public void superfaldonEntra()
        {
            superfaldonEntra(true);
        }
        public void superfaldonSale(bool oficiales)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.superfaldonSale(oficiales); }
        }
        public void superfaldonSale()
        {
            superfaldonSale(true);
        }

        //ACTUALIZA
        public void sfActualiza()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfActualiza(); }
        }

        //ESCRUTADO/CCAA/ULTIMO
        public void sfEscrutadoEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfEscrutadoEntra(); }
        }
        public void sfEscrutadoSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfEscrutadoSale(); }
        }
        public void sfCCAAEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfCCAAEntra(); }
        }
        public void sfCCAASale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfCCAASale(); }
        }

        //SEDES (solo carrusel superfaldón)
        public void sfDesplegarSede(string codPartido)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfDesplegarSede(codPartido); }
        }
        public void sfEncadenarSede(string codPartido)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfEncadenarSede(codPartido); }
        }
        public void sfReplegarSede()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfReplegarSede(); }
        }

        // Compatibilidad con llamadas legacy de superfaldón
        public void sfFichasEntra() => sfEscrutadoEntra();
        public void sfFichasEncadena() => sfEscrutadoEntra();
        public void sfFichasSale() => sfEscrutadoSale();
        public void superfaldonSedesEntra() { }
        public void superfaldonSedesEncadena() { }
        public void superfaldonSedesSale() => sfReplegarSede();
        public void sfMayoriasEntra() => sfCCAAEntra();
        public void sfMayoriasEncadena() => sfCCAAEntra();
        public void sfMayoriasSale() => sfCCAASale();
        public void sfBipartidismoEntra() => ultimoSuperEntra();
        public void sfBipartidismoEncadena() => ultimoSuperEntra();
        public void sfBipartidismoSale() => ultimoSuperSale();
        public void sfGanadorSale() => ultimoSuperSale();
        public void sfGanadorEntra() => ultimoSuperEntra();

        //SUPERFALDON - PACTOMETRO
        public void sfPactometroEntra()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfPactometroEntra(); }
        }
        public void sfPactometroEncadena()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfPactometroEncadena(); }
        }
        public void sfPactometroReinicio()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfPactometroReinicio(); }
        }
        public void sfPactometroSale()
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfPactometroSale(); }
        }
        public void sfPactometroPartidoEntra(BrainStormDTO dto, PartidoDTO partido, bool izquierda)
        {
            if (ipfActivo.Valor == 1 && ipf != null) { ipf.sfPactometroPartidoEntra(dto, partido, izquierda); }
        }

        

        public void pactometroActualiza(BrainStormDTO dtoActualizado, string tipoGrafico)
        {
            switch (tipoGrafico)
            {
                case "FICHAS":
                    if (ipfActivo.Valor == 1 && ipf != null) { ipf.ActualizaPactometroFichas(dtoActualizado); }
                    break;

                case "ÚLTIMO ESCAÑO":
                    if (ipfActivo.Valor == 1 && ipf != null) { ipf.ActualizaPactometroUltimoEscano(dtoActualizado); }
                    break;
                default:
                    // Si no es ningún tipo específico de pacto gráfico, no hacer nada
                    break;
            }
        }

        
    }
}


