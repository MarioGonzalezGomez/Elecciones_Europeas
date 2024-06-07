using Elecciones_Europeas.src.model.DTO.BrainStormDTO;
using Elecciones_Europeas.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Elecciones_Europeas.src.mensajes.builders
{
    internal class IPFMensajes
    {
        public static IPFMensajes? instance;
        private string _bd;
        ConfigManager configuration;

        private IPFMensajes()
        {
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            _bd = configuration.GetValue("bdIPF");
        }

        public static IPFMensajes GetInstance()
        {
            if (instance == null)
            {
                instance = new IPFMensajes();
            }
            return instance;
        }



        //MENSAJES ESPECIFICOS

        //FALDONES
        //CAMBIO ENTRE OFI Y SONDEO
        public string SondeoUOficial(bool oficiales) { return oficiales ? EventBuild("Sondeo_Oficiales", "MAP_INT_PAR", "1", 1) : EventBuild("Sondeo_Oficiales", "MAP_INT_PAR", "0", 1); }

        //ANIMACIONES
        public string PrimerosResultados(bool activo) { return activo ? EventBuild("PRIMEROS", "MAP_INT_PAR", "1", 1) : EventBuild("PRIMEROS", "MAP_INT_PAR", "0", 1); }
        public string AnimacionSondeo(bool activo) { return activo ? EventBuild("SONDEO", "MAP_INT_PAR", "1", 1) : EventBuild("SONDEO", "MAP_INT_PAR", "0", 1); }

        public string RecibirPrimerosResultados() { return EventBuild("PRIMEROS", "MAP_INT_PAR", 3); }
        public string RecibirAnimacionSondeo() { return EventBuild("SONDEO", "MAP_INT_PAR", 3); }

        //PROYECCION
        public string Proyeccion(bool activo) { return activo ? EventBuild("TICKER/PROYECCION", "MAP_INT_PAR", "1", 1) : EventBuild("TICKER/PROYECCION", "MAP_INT_PAR", "0", 1); }

        //GIROS
        public string DeSondeoAOficiales() { return EventRunBuild("SONDEOaOFICIALES"); }

        //CAMBIO DE ELECCIONES
        public string CambioElecciones(bool europa) { return europa ? EventBuild("TICKER/EUROPA", "MAP_INT_PAR", "1", 1) : EventBuild("TICKER/EUROPA", "MAP_INT_PAR", "0", 1); }

        //RELOJ
        public string EntraReloj() { return EventRunBuild("EntraReloj"); }
        public string SaleReloj() { return EventRunBuild("SaleReloj"); }
        public string PreparaReloj(string time) { return EventBuild("OBJETO", "TEXT_STRING", time, 1); }

        //TICKER
        public string TickerEntra(bool oficial) { return oficial ? EventRunBuild("TICKER/ENTRA") : EventRunBuild("TICKER_SONDEO/ENTRA"); }
        public string TickerEncadena(bool oficial) { return oficial ? Encadena("TICKER") : Encadena("TICKER_SONDEO"); }
        public string TickerActualiza() { return EventRunBuild("TICKER/ACTUALIZO"); }
        public string TickerSale(bool oficial) { return oficial ? EventRunBuild("TICKER/SALE") : EventRunBuild("TICKER_SONDEO/SALE"); }

        public string TickerActualizaEscrutado() { return EventBuild("TICKER/CambiaEscrutado", "MAP_INT_PAR", "1", 1); }
        public string TickerActualizaDatos() { return EventBuild("TICKER/CambiaResultado", "MAP_INT_PAR", "1", 1); }
        public string TickerActualizaDatosIndividualizado(List<PartidoDTO> partidos)
        {
            StringBuilder signal = new StringBuilder();
            foreach (var part in partidos)
            {
                string cod = string.Equals(part.codigo, "11103") ? "00103" : part.codigo;
                signal.Append(EventBuild($"TICKER/{cod}/HaCambiado", "MAP_INT_PAR", "1", 1));
            }
            return signal.ToString();
        }

        public string TickerYaNoEstaIndividualizado(List<PartidoDTO> partidos)
        {
            StringBuilder signal = new StringBuilder();
            List<string> codigos = partidos.Select(par => par.codigo).ToList();
            codigos.ForEach(cod =>
            {
                string code = string.Equals(cod, "11103") ? "00103" : cod;
                signal.Append(EventBuild($"TICKER/{code}/YaNoEsta", "MAP_INT_PAR", "1", 1));
            }
                );
            return signal.ToString();
        }
        public string TickerActualizaPosiciones() { return EventBuild("TICKER/CambiaOrden", "MAP_INT_PAR", "1", 1); }
        public string TickerActualizaNumPartidos() { return EventBuild("TICKER/CambiaNPartidos", "MAP_INT_PAR", "1", 1); }

        public string TickerVotosEntra(bool oficial) { return oficial ? EventRunBuild("TICKER/VOTOS/ENTRA") : EventRunBuild("TICKER_SONDEO/VOTOS/ENTRA"); }
        public string TickerVotosSale(bool oficial) { return oficial ? EventRunBuild("TICKER/VOTOS/SALE") : EventRunBuild("TICKER_SONDEO/VOTOS/SALE"); }
        public string TickerHistoricosEntra(bool oficial) { return oficial ? EventRunBuild("TICKER/HISTORICOS/ENTRA") : EventRunBuild("TICKER_SONDEO/HISTORICOS/ENTRA"); }
        public string TickerHistoricosSale(bool oficial) { return oficial ? EventRunBuild("TICKER/HISTORICOS/SALE") : EventRunBuild("TICKER_SONDEO/HISTORICOS/SALE"); }
        public string TickerMillonesEntra() { return EventRunBuild("TICKER/MILLONES/ENTRA"); }
        public string TickerMillonesSale() { return EventRunBuild("TICKER/MILLONES/SALE"); }

        public string TickerFotosEntra() { return EventRunBuild("TICKER/FOTOS/ENTRA"); }
        public string TickerFotosSale() { return EventRunBuild("TICKER/FOTOS/SALE"); }

        //PP_PSOE
        public string PP_PSOEEntra() { return Entra("TICKER/PP_PSOE"); }
        public string PP_PSOESale() { return Sale("TICKER/PP_PSOE"); }

        public string PP_PSOEaGenerales() { return EventRunBuild("TICKER/PP_PSOE/A_GENERALES"); }
        public string PP_PSOEaEuropeas() { return EventRunBuild("TICKER/PP_PSOE/A_EUROPEAS"); }

        //DESPLIEGAS
        public string Despliega4() { return EventRunBuild("TICKER/DESPLIEGA_4"); }
        public string Despliega5() { return EventRunBuild("TICKER/DESPLIEGA_5"); }

        public string RecuperaTodos() { return EventRunBuild("TICKER/RECUPERO_TODOS"); }

        //PACTOMETRO
        public string pactosEntra() { return Entra("PACTOMETRO"); }
        public string pactosReinicio() { return EventRunBuild("PACTOMETRO/INICIO"); }
        public string pactosSale() { return Sale("PACTOMETRO"); }

        public string pactosEntraDerecha(int posicionPartido)
        {
            string signal = EventBuild("PACTOMETRO/CualDcha", "MAP_INT_PAR", $"{posicionPartido + 1}", 1);
            signal += EventBuild("PACTOMETRO/PongoQuitoDcha", "MAP_INT_PAR", "1", 1);
            signal += EventRunBuild("PACTOMETRO/SumaPorDcha");
            return signal;
        }
        public string pactosEntraIzquierda(int posicionPartido)
        {
            string signal = EventBuild("PACTOMETRO/CualIzda", "MAP_INT_PAR", $"{posicionPartido + 1}", 1);
            signal += EventBuild("PACTOMETRO/PongoQuitoIzda", "MAP_INT_PAR", "1", 1);
            signal += EventRunBuild("PACTOMETRO/SumaPorIzda");
            return signal;
        }

        public string pactosSaleDerecha(int posicionPartido)
        {
            string signal = EventBuild("PACTOMETRO/CualDcha", "MAP_INT_PAR", $"{posicionPartido + 1}", 1);
            signal += EventBuild("PACTOMETRO/PongoQuitoDcha", "MAP_INT_PAR", "0", 1);
            signal += EventRunBuild("PACTOMETRO/SumaPorDcha");
            return signal;
        }
        public string pactosSaleIzquierda(int posicionPartido)
        {
            string signal = EventBuild("PACTOMETRO/CualIzda", "MAP_INT_PAR", $"{posicionPartido + 1}", 1);
            signal += EventBuild("PACTOMETRO/PongoQuitoIzda", "MAP_INT_PAR", "0", 1);
            signal += EventRunBuild("PACTOMETRO/SumaPorIzda");
            return signal;
        }

        //INDEPENDENTISMO
        public string independentismoEntra() { return Entra("PACTOMETRO_IND"); }
        public string independentismoReinicio() { return EventRunBuild("PACTOMETRO_IND/INICIO"); }
        public string independentismoSale() { return Sale("PACTOMETRO_IND"); }

        public string independentismoEntraDerecha(int posicionPartido)
        {
            string signal = EventBuild("PACTOMETRO_IND/CualDcha", "MAP_INT_PAR", $"{posicionPartido + 1}", 1);
            signal += EventBuild("PACTOMETRO_IND/PongoQuitoDcha", "MAP_INT_PAR", "1", 1);
            signal += EventRunBuild("PACTOMETRO_IND/SumaPorDcha");
            return signal;
        }
        public string independentismoEntraIzquierda(int posicionPartido)
        {
            string signal = EventBuild("PACTOMETRO_IND/CualIzda", "MAP_INT_PAR", $"{posicionPartido + 1}", 1);
            signal += EventBuild("PACTOMETRO_IND/PongoQuitoIzda", "MAP_INT_PAR", "1", 1);
            signal += EventRunBuild("PACTOMETRO_IND/SumaPorIzda");
            return signal;
        }

        public string independentismoSaleDerecha(int posicionPartido)
        {
            string signal = EventBuild("PACTOMETRO_IND/CualDcha", "MAP_INT_PAR", $"{posicionPartido + 1}", 1);
            signal += EventBuild("PACTOMETRO_IND/PongoQuitoDcha", "MAP_INT_PAR", "0", 1);
            signal += EventRunBuild("PACTOMETRO_IND/SumaPorDcha");
            return signal;
        }
        public string independentismoSaleIzquierda(int posicionPartido)
        {
            string signal = EventBuild("PACTOMETRO_IND/CualIzda", "MAP_INT_PAR", $"{posicionPartido + 1}", 1);
            signal += EventBuild("PACTOMETRO_IND/PongoQuitoIzda", "MAP_INT_PAR", "0", 1);
            signal += EventRunBuild("PACTOMETRO_IND/SumaPorIzda");
            return signal;
        }

        //SEDES
        public string SedesEntra(bool tickerIn, string codPartido)
        {
            string signal;
            if (tickerIn)
            {
                signal = EventBuild("TOTAL/CualPartidoATotal", "MAP_STRING_PAR", $"'{codPartido}'", 1);
                signal += EventBuild($"TOTAL/FCN_INI", "MAP_EXE", 1);
                signal += EventBuild($"TOTAL/FCN_Total", "MAP_EXE", 1);
            }
            else
            {
                signal = EventBuild($"SEDES/Partido1", "MAP_STRING_PAR", $"'{codPartido}'", 1);
                signal += Entra($"SEDES");
            }
            return signal;
        }
        public string SedesEncadena(bool tickerIn, string codPartidoSiguiente, string codPartidoAnterior)
        {
            string signal;
            if (tickerIn)
            {
                signal = EventBuild("TOTAL/CualPartidoAEncadenar", "MAP_STRING_PAR", $"'{codPartidoSiguiente}'", 1);
                signal += EventBuild($"TOTAL/FCN_Sube", "MAP_EXE", 1);
                signal += EventBuild($"TOTAL/FCN_Encad", "MAP_EXE", 1);
            }
            else
            {
                signal = EventBuild("SEDES/Partido2", "MAP_STRING_PAR", $"'{codPartidoSiguiente}'", 1);
                signal += EventRunBuild($"SEDES/ENCADENO");
            }
            return signal;
        }
        public string SedesSale(bool tickerIn, string codPartido = "")
        {
            string signal = tickerIn ? EventBuild($"TOTAL/FCN_Vuelve", "MAP_EXE", 1) : Sale("SEDES");
            return signal;
        }


        //CARTONES
        //PARTICIPACION
        public string participacionEntra() { return Entra("PARTICIPACION"); }
        public string participacionEncadena() { return EventRunBuild("PARTICIPACION/CAMBIA"); }
        public string participacionSale() { return Sale("PARTICIPACION"); }

        //CCAA
        public string ccaaEntra() { return Entra("CCAA"); }
        public string ccaaEncadena() { return EventRunBuild("CCAA/CAMBIA"); }
        public string ccaaSale() { return Sale("CCAA"); }

        //FICHAS DE PARTIDO
        public string fichaEntra() { return Entra("CARTONES"); }
        public string fichaEncadena() { return EventRunBuild("CARTONES/CAMBIA"); }
        public string fichaSale() { return Sale("CARTONES"); }

        //PACTOMETRO
        public string pactometroEntra() { return Entra("PACTOMETRO"); }
        public string pactometroEncadena() { return EventRunBuild("PACTOMETRO/POSICIONES"); }
        public string pactometroSale() { return Sale("PACTOMETRO"); }

        public string pactometroVictoria() { return EventRunBuild("PACTOMETRO/VICTORIA"); }

        //MAYORIAS
        public string mayoriasEntra() { return Entra("MAYORIAS"); }
        public string mayoriasEncadena() { return EventRunBuild("MAYORIAS/CAMBIA"); }
        public string mayoriasSale() { return Sale("MAYORIAS"); }

        //SUPERFALDON
        public string superfaldonEntra() { return Entra("SUPERFALDON"); }
        public string superfaldonSale() { return Sale("SUPERFALDON"); }

        //SEDES
        public string superfaldonSedesEntra() { return Entra("SEDES"); }
        public string superfaldonSedesEncadena() { return Entra("SEDES/ENCADENA"); }
        public string superfaldonSedesSale() { return Sale("SEDES"); }


        //CONSTRUCTORES

        //Para construir la señal necesitaría el objeto o evento al que llamo, la propiedad a cambiar,
        //el valor o valores que cambian y el tipo: 1 para itemset y 2 para itemgo
        private string EventBuild(string objeto, string propiedad, string values, int tipoItem)
        {
            string setOgo = "";
            if (tipoItem == 1)
            {
                setOgo = "itemset('";
            }
            if (tipoItem == 2)
            {
                setOgo = "itemgo('";
            }
            return $"{setOgo}<{_bd}>{objeto}','{propiedad}',{values});";
        }
        private string EventBuild(string objeto, string propiedad, int tipoItem)
        {
            string setOgo = "";
            if (tipoItem == 1)
            {
                setOgo = "itemset('";
            }
            if (tipoItem == 2)
            {
                setOgo = "itemgo('";
            }
            if (tipoItem == 3)
            {
                setOgo = "itemget('";
            }
            return $"{setOgo}<{_bd}>{objeto}','{propiedad}');";
        }
        private string EventRunBuild(string objeto, double delay = 0.0)
        {
            return delay != 0.0 ? $"itemgo('<{_bd}>{objeto}','EVENT_RUN',0,{delay});" : $"itemset('<{_bd}>{objeto}','EVENT_RUN');";
        }

        //MENSAJES COMUNES
        public string Reset()
        {
            return EventRunBuild("RESET");
        }
        public string Entra(string objeto)
        {
            return EventRunBuild($"{objeto}/ENTRA");
        }
        public string Encadena(string objeto)
        {
            return EventRunBuild($"{objeto}/ENCADENA");
        }
        public string Sale(string objeto)
        {
            return EventRunBuild($"{objeto}/SALE");
        }

    }
}
