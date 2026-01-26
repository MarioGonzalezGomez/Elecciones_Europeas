using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elecciones.src.conexion;
using Elecciones.src.mensajes.builders;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF.DTO;

namespace Elecciones.src.mensajes
{
    /// <summary>
    /// Clase que orquesta el envío de mensajes IPF usando los builders especializados.
    /// Cada tipo de rótulo usa su builder correspondiente.
    /// </summary>
    internal class OrdenesIPF
    {
        public static OrdenesIPF? instance;
        
        // Builders especializados por tipo de rótulo
        private readonly FaldonMensajes faldonBuilder;
        private readonly CartonMensajes cartonBuilder;
        private readonly SuperfaldonMensajes superfaldonBuilder;
        private readonly DronMensajes dronBuilder;
        
        public ConexionGraficos c;

        private OrdenesIPF()
        {
            faldonBuilder = FaldonMensajes.GetInstance();
            cartonBuilder = CartonMensajes.GetInstance();
            superfaldonBuilder = SuperfaldonMensajes.GetInstance();
            dronBuilder = DronMensajes.GetInstance();
            c = ConexionGraficos.GetInstanceIPF();
        }

        public static OrdenesIPF GetInstance()
        {
            instance ??= new OrdenesIPF();
            return instance;
        }

        #region Especiales

        public void ReiniciarConexion()
        {
            c.ReiniciarConexion("ipf");
        }

        public void Reset()
        {
            c.EnviarMensaje(faldonBuilder.Reset());
        }

        #endregion

        #region Faldón - Cambio Sondeo/Oficial

        public void SondeoUOficial(bool oficiales)
        {
            c.EnviarMensaje(faldonBuilder.SondeoUOficial(oficiales));
        }

        #endregion

        #region Faldón - Animaciones

        public void PrimerosResultados(bool activo)
        {
            c.EnviarMensaje(faldonBuilder.PrimerosResultados(activo));
        }

        public void AnimacionSondeo(bool activo)
        {
            c.EnviarMensaje(faldonBuilder.AnimacionSondeo(activo));
        }

        public string RecibirPrimerosResultados()
        {
            return c.RecibirMensaje(faldonBuilder.RecibirPrimerosResultados());
        }

        public string RecibirAnimacionSondeo()
        {
            return c.RecibirMensaje(faldonBuilder.RecibirAnimacionSondeo());
        }

        #endregion

        #region Faldón - Proyección

        public void Proyeccion(bool activo)
        {
            c.EnviarMensaje(faldonBuilder.Proyeccion(activo));
        }

        #endregion

        #region Faldón - Giros

        public void DeSondeoAOficiales()
        {
            c.EnviarMensaje(faldonBuilder.DeSondeoAOficiales());
        }

        #endregion

        #region Faldón - Cambio Elecciones

        public void CambioElecciones(bool europa)
        {
            c.EnviarMensaje(faldonBuilder.CambioElecciones(europa));
        }

        #endregion

        #region Faldón - Reloj

        public void RelojEntra(int segundos)
        {
            c.EnviarMensaje(faldonBuilder.EntraReloj());
        }

        public void RelojSale()
        {
            c.EnviarMensaje(faldonBuilder.SaleReloj());
        }

        #endregion

        #region Faldón - Ticker

        public void TickerEntra(bool oficial, BrainStormDTO dto)
        {
            c.EnviarMensaje(faldonBuilder.TickerEntra(oficial, dto));
        }

        public void TickerEncadena(bool oficial, BrainStormDTO dto)
        {
            c.EnviarMensaje(faldonBuilder.TickerEncadena(oficial));
        }

        public void TickerActualiza(BrainStormDTO dto)
        {
            c.EnviarMensaje(faldonBuilder.TickerActualiza());
        }

        public void TickerSale(bool oficial)
        {
            c.EnviarMensaje(faldonBuilder.TickerSale(oficial));
        }

        public void TickerActualizaEscrutado()
        {
            c.EnviarMensaje(faldonBuilder.TickerActualizaEscrutado());
        }

        public void TickerActualizaDatos()
        {
            c.EnviarMensaje(faldonBuilder.TickerActualizaDatos());
        }

        public void TickerActualizaDatosIndividualizado(List<PartidoDTO> partidos)
        {
            c.EnviarMensaje(faldonBuilder.TickerActualizaDatosIndividualizado(partidos));
        }

        public void TickerYaNoEstaIndividualizado(List<PartidoDTO> partidos)
        {
            c.EnviarMensaje(faldonBuilder.TickerYaNoEstaIndividualizado(partidos));
        }

        public void TickerActualizaPosiciones()
        {
            c.EnviarMensaje(faldonBuilder.TickerActualizaPosiciones());
        }

        public void TickerActualizaNumPartidos()
        {
            c.EnviarMensaje(faldonBuilder.TickerActualizaNumPartidos());
        }

        public void TickerEscanosEntra() { }
        public void TickerEscanosSale() { }

        public void TickerVotosEntra(bool oficiales)
        {
            c.EnviarMensaje(faldonBuilder.TickerVotosEntra(oficiales));
        }

        public void TickerVotosSale(bool oficiales)
        {
            c.EnviarMensaje(faldonBuilder.TickerVotosSale(oficiales));
        }

        public void TickerHistoricosEntra(bool oficiales)
        {
            c.EnviarMensaje(faldonBuilder.TickerHistoricosEntra(oficiales));
        }

        public void TickerHistoricosSale(bool oficiales)
        {
            c.EnviarMensaje(faldonBuilder.TickerHistoricosSale(oficiales));
        }

        public void TickerHistoricosEntraInd() { }
        public void TickerHistoricosSaleInd() { }
        public void TickerHistoricosEntraCom() { }
        public void TickerHistoricosSaleCom() { }

        public void TickerMillonesEntra()
        {
            c.EnviarMensaje(faldonBuilder.TickerMillonesEntra());
        }

        public void TickerMillonesSale()
        {
            c.EnviarMensaje(faldonBuilder.TickerMillonesSale());
        }

        public void TickerFotosEntra()
        {
            c.EnviarMensaje(faldonBuilder.TickerFotosEntra());
        }

        public void TickerFotosSale()
        {
            c.EnviarMensaje(faldonBuilder.TickerFotosSale());
        }

        #endregion

        #region Faldón - Video

        public void VideoIn(BrainStormDTO dto, PartidoDTO partidoSeleccionado)
        {
            c.EnviarMensaje(faldonBuilder.VideoIn(dto, partidoSeleccionado));
        }

        public void VideoOut(BrainStormDTO dto, PartidoDTO partidoSeleccionado)
        {
            c.EnviarMensaje(faldonBuilder.VideoOut(dto, partidoSeleccionado));
        }

        public void VideoOutTodos(BrainStormDTO dto)
        {
            c.EnviarMensaje(faldonBuilder.VideoOutTodos(dto));
        }

        public void VideoInTodos(BrainStormDTO dto)
        {
            c.EnviarMensaje(faldonBuilder.VideoInTodos(dto));
        }

        #endregion

        #region Faldón - Ticker TD

        public void TickerTDEntra(BrainStormDTO dto)
        {
            c.EnviarMensaje(faldonBuilder.TickerTDEntra(dto));
        }

        public void TickerTDActualiza(BrainStormDTO dtoAnterior, BrainStormDTO dto)
        {
            c.EnviarMensaje(faldonBuilder.TickerTDActualiza(dtoAnterior, dto));
        }

        public void TickerTDSale()
        {
            c.EnviarMensaje(faldonBuilder.TickerTDSale());
        }

        #endregion

        #region Faldón - PP_PSOE

        public void PP_PSOEEntra()
        {
            c.EnviarMensaje(faldonBuilder.PP_PSOEEntra());
        }

        public void PP_PSOESale()
        {
            c.EnviarMensaje(faldonBuilder.PP_PSOESale());
        }

        #endregion

        #region Faldón - Despliegas

        public void Despliega4()
        {
            c.EnviarMensaje(faldonBuilder.Despliega4());
        }

        public void Despliega5()
        {
            c.EnviarMensaje(faldonBuilder.Despliega5());
        }

        public void RecuperaTodos()
        {
            c.EnviarMensaje(faldonBuilder.RecuperaTodos());
        }

        #endregion

        #region Faldón - Pactos

        public void pactosEntra()
        {
            c.EnviarMensaje(faldonBuilder.pactosEntra());
        }

        public void pactosReinicio()
        {
            c.EnviarMensaje(faldonBuilder.pactosReinicio());
        }

        public void pactosSale()
        {
            c.EnviarMensaje(faldonBuilder.pactosSale());
        }

        public void pactosEntraDerecha(int posicionPartido)
        {
            c.EnviarMensaje(faldonBuilder.pactosEntraDerecha(posicionPartido));
        }

        public void pactosEntraIzquierda(int posicionPartido)
        {
            c.EnviarMensaje(faldonBuilder.pactosEntraIzquierda(posicionPartido));
        }

        public void pactosSaleDerecha(int posicionPartido)
        {
            c.EnviarMensaje(faldonBuilder.pactosSaleDerecha(posicionPartido));
        }

        public void pactosSaleIzquierda(int posicionPartido)
        {
            c.EnviarMensaje(faldonBuilder.pactosSaleIzquierda(posicionPartido));
        }

        #endregion

        #region Faldón - Sedes

        public void SedesEntra(bool tickerIn, BrainStormDTO dto, PartidoDTO seleccionado)
        {
            c.EnviarMensaje(faldonBuilder.SedesEntra(tickerIn, seleccionado.codigo));
        }

        public void SedesEncadena(bool tickerIn, string codPartidoSiguiente, string codPartidoAnterior)
        {
            c.EnviarMensaje(faldonBuilder.SedesEncadena(tickerIn, codPartidoSiguiente, codPartidoAnterior));
        }

        public void SedesSale(bool tickerIn)
        {
            c.EnviarMensaje(faldonBuilder.SedesSale(tickerIn));
        }

        #endregion

        #region Cartón - Actualiza

        public void CartonesActualiza()
        {
            c.EnviarMensaje(cartonBuilder.CartonesActualiza());
        }

        #endregion

        #region Cartón - Participación

        public void participacionEntra(BrainStormDTO dto, int avance)
        {
            c.EnviarMensaje(cartonBuilder.participacionEntra(dto, avance));
        }

        public void participacionEncadena(BrainStormDTO dto, int avance)
        {
            c.EnviarMensaje(cartonBuilder.participacionEncadena(dto, avance));
        }

        public void participacionSale()
        {
            c.EnviarMensaje(cartonBuilder.participacionSale());
        }

        #endregion

        #region Cartón - CCAA

        public void ccaaEntra(BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.ccaaEntra(dto));
        }

        public void ccaaEncadena()
        {
            c.EnviarMensaje(cartonBuilder.ccaaEncadena());
        }

        public void ccaaSale()
        {
            c.EnviarMensaje(cartonBuilder.ccaaSale());
        }

        #endregion

        #region Cartón - Fichas

        public void fichaEntra(bool oficiales, BrainStormDTO dto, PartidoDTO partido)
        {
            c.EnviarMensaje(cartonBuilder.fichaEntra(oficiales, dto, partido));
        }

        public void fichaEncadena(bool oficiales, BrainStormDTO dto, PartidoDTO partido)
        {
            c.EnviarMensaje(cartonBuilder.fichaEncadena(oficiales, dto, partido));
        }

        public void fichaActualiza(bool oficiales, BrainStormDTO dtoAnterior, BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.fichaActualiza(oficiales, dtoAnterior, dto));
        }

        public void fichaSale(bool oficiales)
        {
            c.EnviarMensaje(cartonBuilder.fichaSale(oficiales));
        }

        #endregion

        #region Cartón - Pactómetro

        public void pactometroEntra()
        {
            c.EnviarMensaje(cartonBuilder.pactometroEntra());
        }

        public void pactometroEncadena()
        {
            c.EnviarMensaje(cartonBuilder.pactometroEncadena());
        }

        public void pactometroSale()
        {
            c.EnviarMensaje(cartonBuilder.pactometroSale());
        }

        public void pactometroVictoria()
        {
            c.EnviarMensaje(cartonBuilder.pactometroVictoria());
        }

        #endregion

        #region Cartón - Mayorías

        public void mayoriasEntra(BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.mayoriasEntra(dto));
        }

        public void mayoriasEncadena(BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.mayoriasEncadena(dto));
        }

        public void mayoriasSale()
        {
            c.EnviarMensaje(cartonBuilder.mayoriasSale());
        }

        #endregion

        #region Cartón - Partidos

        public void cartonPartidosEntra(BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.cartonPartidosEntra(dto));
        }

        public void cartonPartidosActualiza(BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.cartonPartidosActualiza(dto));
        }

        public void cartonPartidosSale()
        {
            c.EnviarMensaje(cartonBuilder.cartonPartidosSale());
        }

        #endregion

        #region Cartón - Último Escaño

        public void ultimoEntra(BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.ultimoEntra(dto));
        }

        public void ultimoEncadena(BrainStormDTO dtoAnterior, BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.ultimoEncadena(dtoAnterior, dto));
        }

        public void ultimoActualiza(BrainStormDTO dtoAnterior, BrainStormDTO dto)
        {
            c.EnviarMensaje(cartonBuilder.ultimoActualiza(dtoAnterior, dto));
        }

        public void ultimoEntraPartido(BrainStormDTO dto, CPDataDTO partido, bool esIzquierda)
        {
            c.EnviarMensaje(cartonBuilder.ultimoEntraPartido(dto, partido, esIzquierda));
        }

        public void ultimoLimpiaPartidos()
        {
            c.EnviarMensaje(cartonBuilder.ultimoLimpiaPartidos());
        }

        public void ultimoSale()
        {
            c.EnviarMensaje(cartonBuilder.ultimoSale());
        }

        #endregion

        #region Superfaldón - Último

        public void ultimoSuperEntra()
        {
            c.EnviarMensaje(superfaldonBuilder.ultimoSuperEntra());
        }

        public void ultimoSuperSale()
        {
            c.EnviarMensaje(superfaldonBuilder.ultimoSuperSale());
        }

        #endregion

        #region Superfaldón - Base

        public void superfaldonEntra()
        {
            c.EnviarMensaje(superfaldonBuilder.superfaldonEntra());
        }

        public void superfaldonSale()
        {
            c.EnviarMensaje(superfaldonBuilder.superfaldonSale());
        }

        #endregion

        #region Superfaldón - Sedes

        public void superfaldonSedesEntra()
        {
            c.EnviarMensaje(superfaldonBuilder.superfaldonSedesEntra());
        }

        public void superfaldonSedesEncadena()
        {
            c.EnviarMensaje(superfaldonBuilder.superfaldonSedesEncadena());
        }

        public void superfaldonSedesSale()
        {
            c.EnviarMensaje(superfaldonBuilder.superfaldonSedesSale());
        }

        #endregion

        #region Superfaldón - Fichas

        public void sfFichasEntra()
        {
            c.EnviarMensaje(superfaldonBuilder.sfFichasEntra());
        }

        public void sfFichasEncadena()
        {
            c.EnviarMensaje(superfaldonBuilder.sfFichasEncadena());
        }

        public void sfFichasSale()
        {
            c.EnviarMensaje(superfaldonBuilder.sfFichasSale());
        }

        #endregion

        #region Superfaldón - Pactómetro

        public void sfPactometroEntra()
        {
            c.EnviarMensaje(superfaldonBuilder.sfPactometroEntra());
        }

        public void sfPactometroEncadena()
        {
            c.EnviarMensaje(superfaldonBuilder.sfPactometroEncadena());
        }

        public void sfPactometroSale()
        {
            c.EnviarMensaje(superfaldonBuilder.sfPactometroSale());
        }

        #endregion

        #region Superfaldón - Mayorías

        public void sfMayoriasEntra()
        {
            c.EnviarMensaje(superfaldonBuilder.sfMayoriasEntra());
        }

        public void sfMayoriasEncadena()
        {
            c.EnviarMensaje(superfaldonBuilder.sfMayoriasEncadena());
        }

        public void sfMayoriasSale()
        {
            c.EnviarMensaje(superfaldonBuilder.sfMayoriasSale());
        }

        #endregion

        #region Superfaldón - Bipartidismo

        public void sfBipartidismoEntra()
        {
            c.EnviarMensaje(superfaldonBuilder.sfBipartidismoEntra());
        }

        public void sfBipartidismoEncadena()
        {
            c.EnviarMensaje(superfaldonBuilder.sfBipartidismoEncadena());
        }

        public void sfBipartidismoSale()
        {
            c.EnviarMensaje(superfaldonBuilder.sfBipartidismoSale());
        }

        #endregion

        #region Superfaldón - Ganador

        public void sfGanadorEntra()
        {
            c.EnviarMensaje(superfaldonBuilder.sfGanadorEntra());
        }

        public void sfGanadorEncadena()
        {
            c.EnviarMensaje(superfaldonBuilder.sfGanadorEncadena());
        }

        public void sfGanadorSale()
        {
            c.EnviarMensaje(superfaldonBuilder.sfGanadorSale());
        }

        #endregion
    }
}
