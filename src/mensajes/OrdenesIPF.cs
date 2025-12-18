using Elecciones.src.conexion;
using Elecciones.src.mensajes.builders;
using Elecciones.src.model.DTO.BrainStormDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones.src.mensajes
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
        public void SondeoUOficial(bool oficiales)
        {
            c.EnviarMensaje(builder.SondeoUOficial(oficiales));
        }

        //ANIMACIONES
        public void PrimerosResultados(bool activo)
        {
            builder.PrimerosResultados(activo);
        }
        public void AnimacionSondeo(bool activo)
        {
            builder.AnimacionSondeo(activo);
        }

        public string RecibirPrimerosResultados()
        {
            return c.RecibirMensaje(builder.RecibirPrimerosResultados());
        }
        public string RecibirAnimacionSondeo()
        {
            return c.RecibirMensaje(builder.RecibirAnimacionSondeo());
        }

        //PROYECCION
        public void Proyeccion(bool activo)
        {
            c.EnviarMensaje(builder.Proyeccion(activo));
        }

        //GIROS
        public void DeSondeoAOficiales()
        {
            c.EnviarMensaje(builder.DeSondeoAOficiales());
        }

        //CAMBIO DE ELECCIONES
        public void CambioElecciones(bool europa)
        {
            c.EnviarMensaje(builder.CambioElecciones(europa));
        }

        //TIMER
        public void RelojEntra(int segundos)
        {
            c.EnviarMensaje(builder.EntraReloj(segundos));
        }
        public void RelojSale()
        {
            c.EnviarMensaje(builder.SaleReloj());
        }

        //TICKER
        public void TickerEntra(bool oficial, BrainStormDTO dto)
        {
            c.EnviarMensaje(builder.TickerEntra(oficial, dto));
        }
        public void TickerEncadena(bool oficial, BrainStormDTO dto)
        {
            c.EnviarMensaje(builder.TickerEncadena(oficial, dto));
        }
        public void TickerActualiza(BrainStormDTO dto)
        {
            c.EnviarMensaje(builder.TickerActualiza(dto));
        }
        public void TickerSale(bool oficial)
        {
            c.EnviarMensaje(builder.TickerSale(oficial));
        }

        public void TickerActualizaEscrutado()
        {
            c.EnviarMensaje(builder.TickerActualizaEscrutado());
        }
        public void TickerActualizaDatos()
        {
            c.EnviarMensaje(builder.TickerActualizaDatos());
        }
        public void TickerActualizaDatosIndividualizado(List<PartidoDTO> partidos)
        {
            c.EnviarMensaje(builder.TickerActualizaDatosIndividualizado(partidos));
        }

        public void TickerYaNoEstaIndividualizado(List<PartidoDTO> partidos)
        {
            c.EnviarMensaje(builder.TickerYaNoEstaIndividualizado(partidos));
        }
        public void TickerActualizaPosiciones()
        {
            c.EnviarMensaje(builder.TickerActualizaPosiciones());
        }
        public void TickerActualizaNumPartidos()
        {
            c.EnviarMensaje(builder.TickerActualizaNumPartidos());
        }

        public void TickerEscanosEntra()
        {
            c.EnviarMensaje(builder.TickerEscanosEntra());
        }
        public void TickerEscanosSale()
        {
            c.EnviarMensaje(builder.TickerEscanosSale());
        }
        public void TickerVotosEntra()
        {
            c.EnviarMensaje(builder.TickerVotosEntra());
        }
        public void TickerVotosSale()
        {
            c.EnviarMensaje(builder.TickerVotosSale());
        }
        public void TickerHistoricosEntraInd()
        {
            c.EnviarMensaje(builder.TickerHistoricosEntraInd());
        }
        public void TickerHistoricosSaleInd()
        {
            c.EnviarMensaje(builder.TickerHistoricosSaleInd());
        }
        public void TickerHistoricosEntraCom()
        {
            c.EnviarMensaje(builder.TickerHistoricosEntraCom());
        }
        public void TickerHistoricosSaleCom()
        {
            c.EnviarMensaje(builder.TickerHistoricosSaleCom());
        }
        public void TickerMillonesEntra()
        {
            c.EnviarMensaje(builder.TickerMillonesEntra());
        }
        public void TickerMillonesSale()
        {
            c.EnviarMensaje(builder.TickerMillonesSale());
        }

        public void TickerFotosEntra()
        {
            c.EnviarMensaje(builder.TickerFotosEntra());
        }
        public void TickerFotosSale()
        {
            c.EnviarMensaje(builder.TickerFotosSale());
        }

        //TICKER TD
        public void TickerTDEntra(BrainStormDTO dto)
        {
            c.EnviarMensaje(builder.TickerTDEntra(dto));
        }
        public void TickerTDSale()
        {
            c.EnviarMensaje(builder.TickerTDSale());
        }

        //PP_PSOE
        public void PP_PSOEEntra()
        {
            c.EnviarMensaje(builder.PP_PSOEEntra());
        }
        public void PP_PSOESale()
        {
            c.EnviarMensaje(builder.PP_PSOESale());
        }


        //DESPLIEGAS
        public void Despliega4()
        {
            c.EnviarMensaje(builder.Despliega4());
        }
        public void Despliega5()
        {
            c.EnviarMensaje(builder.Despliega5());
        }
        public void RecuperaTodos()
        {
            c.EnviarMensaje(builder.RecuperaTodos());
        }

        //PACTOS
        public void pactosEntra()
        {
            c.EnviarMensaje(builder.pactosEntra());
        }
        public void pactosReinicio()
        {
            c.EnviarMensaje(builder.pactosReinicio());
        }
        public void pactosSale()
        {
            c.EnviarMensaje(builder.pactosSale());
        }

        public void pactosEntraDerecha(int posicionPartido)
        {
            c.EnviarMensaje(builder.pactosEntraDerecha(posicionPartido));
        }
        public void pactosEntraIzquierda(int posicionPartido)
        {
            c.EnviarMensaje(builder.pactosEntraIzquierda(posicionPartido));
        }

        public void pactosSaleDerecha(int posicionPartido)
        {
            c.EnviarMensaje(builder.pactosSaleDerecha(posicionPartido));
        }
        public void pactosSaleIzquierda(int posicionPartido)
        {
            c.EnviarMensaje(builder.pactosSaleIzquierda(posicionPartido));
        }

        //INDEPENDENTISMO
        public void independentismoEntra()
        {
            c.EnviarMensaje(builder.independentismoEntra());
        }
        public void independentismoReinicio()
        {
            c.EnviarMensaje(builder.independentismoReinicio());
        }
        public void independentismoSale()
        {
            c.EnviarMensaje(builder.independentismoSale());
        }

        public void independentismoEntraDerecha(int posicionPartido)
        {
            c.EnviarMensaje(builder.independentismoEntraDerecha(posicionPartido));
        }
        public void independentismoEntraIzquierda(int posicionPartido)
        {
            c.EnviarMensaje(builder.independentismoEntraIzquierda(posicionPartido));
        }

        public void independentismoSaleDerecha(int posicionPartido)
        {
            c.EnviarMensaje(builder.independentismoSaleDerecha(posicionPartido));
        }
        public void independentismoSaleIzquierda(int posicionPartido)
        {
            c.EnviarMensaje(builder.independentismoSaleIzquierda(posicionPartido));
        }

        //SEDES
        public void SedesEntra(bool tickerIn, BrainStormDTO dto, PartidoDTO seleccionado)
        {
            c.EnviarMensaje(builder.SedesEntra(tickerIn, dto, seleccionado));
        }
        public void SedesEncadena(bool tickerIn, string codPartidoSiguiente, string codPartidoAnterior)
        {
            c.EnviarMensaje(builder.SedesEncadena(tickerIn, codPartidoSiguiente, codPartidoAnterior));
        }
        public void SedesSale(bool tickerIn)
        {
            c.EnviarMensaje(builder.SedesSale(tickerIn));
        }


        //CARTONES
        //PARTICIPACION
        public void participacionEntra(BrainStormDTO dto, int avance)
        {
            c.EnviarMensaje(builder.participacionEntra(dto, avance));
        }
        public void participacionEncadena(BrainStormDTO dto, int avance)
        {
            c.EnviarMensaje(builder.participacionEncadena(dto, avance));
        }
        public void participacionSale()
        {
            c.EnviarMensaje(builder.participacionSale());
        }

        //CCAA
        public void ccaaEntra(BrainStormDTO dto)
        {
            c.EnviarMensaje(builder.ccaaEntra(dto));
        }
        public void ccaaEncadena()
        {
            c.EnviarMensaje(builder.ccaaEncadena());
        }
        public void ccaaSale()
        {
            c.EnviarMensaje(builder.ccaaSale());
        }

        //FICHAS DE PARTIDO
        public void fichaEntra(bool oficiales, BrainStormDTO dto, PartidoDTO partido)
        {
            c.EnviarMensaje(builder.fichaEntra(oficiales, dto, partido));
        }
        public void fichaEncadena(bool oficiales, BrainStormDTO dto, PartidoDTO partido)
        {
            c.EnviarMensaje(builder.fichaEncadena(oficiales, dto, partido));
        }
        public void fichaActualiza(bool oficiales, BrainStormDTO dto, BrainStormDTO dtoAnterior)
        {
            c.EnviarMensaje(builder.fichaActualiza(oficiales, dto, dtoAnterior));
        }
        public void fichaSale(bool oficiales)
        {
            c.EnviarMensaje(builder.fichaSale(oficiales));
        }

        //PACTOMETRO
        public void pactometroEntra()
        {
            c.EnviarMensaje(builder.pactometroEntra());
        }
        public void pactometroEncadena()
        {
            c.EnviarMensaje(builder.pactometroEncadena());
        }
        public void pactometroSale()
        {
            c.EnviarMensaje(builder.pactometroSale());
        }

        public void pactometroVictoria()
        {
            c.EnviarMensaje(builder.pactometroVictoria());
        }

        //MAYORIAS
        public void mayoriasEntra(BrainStormDTO dto)
        {
            c.EnviarMensaje(builder.mayoriasEntra(dto));
        }
        public void mayoriasEncadena(BrainStormDTO dto)
        {
            c.EnviarMensaje(builder.mayoriasEncadena(dto));
        }
        public void mayoriasSale()
        {
            c.EnviarMensaje(builder.mayoriasSale());
        }

        //SUPERFALDON
        public void superfaldonEntra()
        {
            c.EnviarMensaje(builder.superfaldonEntra());
        }
        public void superfaldonSale()
        {
            c.EnviarMensaje(builder.superfaldonSale());
        }

        //SEDES
        public void superfaldonSedesEntra()
        {
            c.EnviarMensaje(builder.superfaldonSedesEntra());
        }
        public void superfaldonSedesEncadena()
        {
            c.EnviarMensaje(builder.superfaldonSedesEncadena());
        }
        public void superfaldonSedesSale()
        {
            c.EnviarMensaje(builder.superfaldonSedesSale());
        }

        //SUPERFALDON - FICHAS
        public void sfFichasEntra()
        {
            c.EnviarMensaje(builder.sfFichasEntra());
        }
        public void sfFichasEncadena()
        {
            c.EnviarMensaje(builder.sfFichasEncadena());
        }
        public void sfFichasSale()
        {
            c.EnviarMensaje(builder.sfFichasSale());
        }

        //SUPERFALDON - PACTOMETRO
        public void sfPactometroEntra()
        {
            c.EnviarMensaje(builder.sfPactometroEntra());
        }
        public void sfPactometroEncadena()
        {
            c.EnviarMensaje(builder.sfPactometroEncadena());
        }
        public void sfPactometroSale()
        {
            c.EnviarMensaje(builder.sfPactometroSale());
        }

        //SUPERFALDON - MAYORIAS
        public void sfMayoriasEntra()
        {
            c.EnviarMensaje(builder.sfMayoriasEntra());
        }
        public void sfMayoriasEncadena()
        {
            c.EnviarMensaje(builder.sfMayoriasEncadena());
        }
        public void sfMayoriasSale()
        {
            c.EnviarMensaje(builder.sfMayoriasSale());
        }

        //SUPERFALDON - BIPARTIDISMO
        public void sfBipartidismoEntra()
        {
            c.EnviarMensaje(builder.sfBipartidismoEntra());
        }
        public void sfBipartidismoEncadena()
        {
            c.EnviarMensaje(builder.sfBipartidismoEncadena());
        }
        public void sfBipartidismoSale()
        {
            c.EnviarMensaje(builder.sfBipartidismoSale());
        }

        //SUPERFALDON - GANADOR
        public void sfGanadorEntra()
        {
            c.EnviarMensaje(builder.sfGanadorEntra());
        }
        public void sfGanadorEncadena()
        {
            c.EnviarMensaje(builder.sfGanadorEncadena());
        }
        public void sfGanadorSale()
        {
            c.EnviarMensaje(builder.sfGanadorSale());
        }
    }
}
