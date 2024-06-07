using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.mensajes.builders;
using Elecciones_Europeas.src.model.DTO.BrainStormDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.mensajes
{
    internal class OrdenesIPF
    {
        public static OrdenesIPF instance;
        private IPFMensajes builder;
        public ConexionGraficos c;

        private OrdenesIPF()
        {
            builder = IPFMensajes.GetInstance();
            c = ConexionGraficos.GetInstanceIPF();
        }

        public static OrdenesIPF GetInstance()
        {
            if (instance == null)
            {
                instance = new OrdenesIPF();
            }
            return instance;
        }

        //ESPECIALES
        public void ReiniciarConexion()
        {
            c.ReiniciarConexion("ipf");
        }
        public void Reset()
        {
            c.EnviarMensaje(builder.Reset());
        }

        //ESPECIFICAS

        //FALDONES
        //CAMBIO ENTRE OFI Y SONDEO
        public void SondeoUOficial(bool oficiales) { c.EnviarMensaje(builder.SondeoUOficial(oficiales)); }

        //ANIMACIONES
        public void PrimerosResultados(bool activo) { c.EnviarMensaje(builder.PrimerosResultados(activo)); }
        public void AnimacionSondeo(bool activo) { c.EnviarMensaje(builder.AnimacionSondeo(activo)); }

        public string RecibirPrimerosResultados() { return c.RecibirMensaje(builder.RecibirPrimerosResultados()); }
        public string RecibirAnimacionSondeo() { return c.RecibirMensaje(builder.RecibirAnimacionSondeo()); }

        //PROYECCION
        public void Proyeccion(bool activo) { c.EnviarMensaje(builder.Proyeccion(activo)); }

        //GIROS
        public void DeSondeoAOficiales() { c.EnviarMensaje(builder.DeSondeoAOficiales()); }

        //CAMBIO DE ELECCIONES
        public void CambioElecciones(bool europa) { c.EnviarMensaje(builder.CambioElecciones(europa)); }

        //TIMER
        public void RelojEntra() { c.EnviarMensaje(builder.EntraReloj()); }
        public void RelojSale() { c.EnviarMensaje(builder.SaleReloj()); }

        //TICKER
        public void TickerEntra(bool oficial) { c.EnviarMensaje(builder.TickerEntra(oficial)); }
        public void TickerEncadena(bool oficial) { c.EnviarMensaje(builder.TickerEncadena(oficial)); }
        public void TickerActualiza() { c.EnviarMensaje(builder.TickerActualiza()); }
        public void TickerSale(bool oficial) { c.EnviarMensaje(builder.TickerSale(oficial)); }

        public void TickerActualizaEscrutado() { c.EnviarMensaje(builder.TickerActualizaEscrutado()); }
        public void TickerActualizaDatos() { c.EnviarMensaje(builder.TickerActualizaDatos()); }
        public void TickerActualizaDatosIndividualizado(List<PartidoDTO> partidos) { c.EnviarMensaje(builder.TickerActualizaDatosIndividualizado(partidos)); }

        public void TickerYaNoEstaIndividualizado(List<PartidoDTO> partidos) { c.EnviarMensaje(builder.TickerYaNoEstaIndividualizado(partidos)); }
        public void TickerActualizaPosiciones() { c.EnviarMensaje(builder.TickerActualizaPosiciones()); }
        public void TickerActualizaNumPartidos() { c.EnviarMensaje(builder.TickerActualizaNumPartidos()); }

        public void TickerVotosEntra(bool oficiales) { c.EnviarMensaje(builder.TickerVotosEntra(oficiales)); }
        public void TickerVotosSale(bool oficiales) { c.EnviarMensaje(builder.TickerVotosSale(oficiales)); }
        public void TickerHistoricosEntra(bool oficiales) { c.EnviarMensaje(builder.TickerHistoricosEntra(oficiales)); }
        public void TickerHistoricosSale(bool oficiales) { c.EnviarMensaje(builder.TickerHistoricosSale(oficiales)); }
        public void TickerMillonesEntra() { c.EnviarMensaje(builder.TickerMillonesEntra()); }
        public void TickerMillonesSale() { c.EnviarMensaje(builder.TickerMillonesSale()); }

        public void TickerFotosEntra() { c.EnviarMensaje(builder.TickerFotosEntra()); }
        public void TickerFotosSale() { c.EnviarMensaje(builder.TickerFotosSale()); }

        //PP_PSOE
        public void PP_PSOEEntra() { c.EnviarMensaje(builder.PP_PSOEEntra()); }
        public void PP_PSOESale() { c.EnviarMensaje(builder.PP_PSOESale()); }

        public void PP_PSOEaGenerales() { c.EnviarMensaje(builder.PP_PSOEaGenerales()); }
        public void PP_PSOEaEuropeas() { c.EnviarMensaje(builder.PP_PSOEaEuropeas()); }

        //DESPLIEGAS
        public void Despliega4() { c.EnviarMensaje(builder.Despliega4()); }
        public void Despliega5() { c.EnviarMensaje(builder.Despliega5()); }
        public void RecuperaTodos() { c.EnviarMensaje(builder.RecuperaTodos()); }

        //PACTOS
        public void pactosEntra() { c.EnviarMensaje(builder.pactosEntra()); }
        public void pactosReinicio() { c.EnviarMensaje(builder.pactosReinicio()); }
        public void pactosSale() { c.EnviarMensaje(builder.pactosSale()); }

        public void pactosEntraDerecha(int posicionPartido) { c.EnviarMensaje(builder.pactosEntraDerecha(posicionPartido)); }
        public void pactosEntraIzquierda(int posicionPartido) { c.EnviarMensaje(builder.pactosEntraIzquierda(posicionPartido)); }

        public void pactosSaleDerecha(int posicionPartido) { c.EnviarMensaje(builder.pactosSaleDerecha(posicionPartido)); }
        public void pactosSaleIzquierda(int posicionPartido) { c.EnviarMensaje(builder.pactosSaleIzquierda(posicionPartido)); }

        //INDEPENDENTISMO
        public void independentismoEntra() { c.EnviarMensaje(builder.independentismoEntra()); }
        public void independentismoReinicio() { c.EnviarMensaje(builder.independentismoReinicio()); }
        public void independentismoSale() { c.EnviarMensaje(builder.independentismoSale()); }

        public void independentismoEntraDerecha(int posicionPartido) { c.EnviarMensaje(builder.independentismoEntraDerecha(posicionPartido)); }
        public void independentismoEntraIzquierda(int posicionPartido) { c.EnviarMensaje(builder.independentismoEntraIzquierda(posicionPartido)); }

        public void independentismoSaleDerecha(int posicionPartido) { c.EnviarMensaje(builder.independentismoSaleDerecha(posicionPartido)); }
        public void independentismoSaleIzquierda(int posicionPartido) { c.EnviarMensaje(builder.independentismoSaleIzquierda(posicionPartido)); }

        //SEDES
        public void SedesEntra(bool tickerIn, string codPartido) { c.EnviarMensaje(builder.SedesEntra(tickerIn, codPartido)); }
        public void SedesEncadena(bool tickerIn, string codPartidoSiguiente, string codPartidoAnterior) { c.EnviarMensaje(builder.SedesEncadena(tickerIn, codPartidoSiguiente, codPartidoAnterior)); }
        public void SedesSale(bool tickerIn, string codPartido = "") { c.EnviarMensaje(builder.SedesSale(tickerIn, codPartido)); }


        //CARTONES
        //PARTICIPACION
        public void participacionEntra() { c.EnviarMensaje(builder.participacionEntra()); }
        public void participacionEncadena() { c.EnviarMensaje(builder.participacionEncadena()); }
        public void participacionSale() { c.EnviarMensaje(builder.participacionSale()); }

        //CCAA
        public void ccaaEntra() { c.EnviarMensaje(builder.ccaaEntra()); }
        public void ccaaEncadena() { c.EnviarMensaje(builder.ccaaEncadena()); }
        public void ccaaSale() { c.EnviarMensaje(builder.ccaaSale()); }

        //FICHAS DE PARTIDO
        public void fichaEntra() { c.EnviarMensaje(builder.fichaEntra()); }
        public void fichaEncadena() { c.EnviarMensaje(builder.fichaEncadena()); }
        public void fichaSale() { c.EnviarMensaje(builder.fichaSale()); }

        //PACTOMETRO
        public void pactometroEntra() { c.EnviarMensaje(builder.pactometroEntra()); }
        public void pactometroEncadena() { c.EnviarMensaje(builder.pactometroEncadena()); }
        public void pactometroSale() { c.EnviarMensaje(builder.pactometroSale()); }

        public void pactometroVictoria() { c.EnviarMensaje(builder.pactometroVictoria()); }

        //MAYORIAS
        public void mayoriasEntra() { c.EnviarMensaje(builder.mayoriasEntra()); }
        public void mayoriasEncadena() { c.EnviarMensaje(builder.mayoriasEncadena()); }
        public void mayoriasSale() { c.EnviarMensaje(builder.mayoriasSale()); }

        //SUPERFALDON
        public void superfaldonEntra() { c.EnviarMensaje(builder.superfaldonEntra()); }
        public void superfaldonSale() { c.EnviarMensaje(builder.superfaldonSale()); }

        //SEDES
        public void superfaldonSedesEntra() { c.EnviarMensaje(builder.superfaldonSedesEntra()); }
        public void superfaldonSedesEncadena() { c.EnviarMensaje(builder.superfaldonSedesEncadena()); }
        public void superfaldonSedesSale() { c.EnviarMensaje(builder.superfaldonSedesSale()); }
    }
}
