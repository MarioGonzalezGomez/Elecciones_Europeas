using System.Globalization;
using System.Text;
using System.Windows;
using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using Elecciones.src.utils;

namespace Elecciones.src.mensajes.builders
{
    internal class IPFMensajes
    {
        public static IPFMensajes? instance;
        private string _bd;
        ConfigManager configuration;

        private bool animacionPrimeros = true;
        private bool animacionSondeo = true;

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

        // --- Helpers añadidos: leer modo/ruta de vídeo bajo demanda ---
        /// <summary>
        /// Devuelve true si la fuente de vídeo del slot indicado está en modo DIRECTO (live).
        /// </summary>
        public bool IsVideoLive(int index)
        {
            if (index < 1 || index > 6) return false;
            var val = configuration.GetValue($"video{index}_isLive");
            if (string.IsNullOrEmpty(val)) return false;
            return val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Devuelve la ruta persistida para el slot de vídeo indicado (vacío si no existe).
        /// </summary>
        public string GetVideoPath(int index)
        {
            if (index < 1 || index > 6) return string.Empty;
            return configuration.GetValue($"video{index}_path") ?? string.Empty;
        }
        // --- Fin helpers ---

        //MENSAJES ESPECIFICOS

        //FALDONES
        //CAMBIO ENTRE OFI Y SONDEO
        public string SondeoUOficial(bool oficiales)
        {
            return oficiales ? EventBuild("Sondeo_Oficiales", "MAP_INT_PAR", "1", 1) : EventBuild("Sondeo_Oficiales", "MAP_INT_PAR", "0", 1);
        }

        //ANIMACIONES
        public void PrimerosResultados(bool activo)
        {
            animacionPrimeros = activo;
            //return activo ? EventBuild("PRIMEROS", "MAP_INT_PAR", "1", 1) : EventBuild("PRIMEROS", "MAP_INT_PAR", "0", 1);
        }
        public void AnimacionSondeo(bool activo)
        {
            animacionSondeo = activo;
            //  return activo ? EventBuild("SONDEO", "MAP_INT_PAR", "1", 1) : EventBuild("SONDEO", "MAP_INT_PAR", "0", 1);
        }

        public string RecibirPrimerosResultados()
        {
            return EventBuild("PRIMEROS", "MAP_INT_PAR", 3);
        }
        public string RecibirAnimacionSondeo()
        {
            return EventBuild("SONDEO", "MAP_INT_PAR", 3);
        }

        //PROYECCION
        public string Proyeccion(bool activo)
        {
            return activo ? EventBuild("TICKER/PROYECCION", "MAP_INT_PAR", "1", 1) : EventBuild("TICKER/PROYECCION", "MAP_INT_PAR", "0", 1);
        }

        //GIROS
        public string DeSondeoAOficiales()
        {
            return EventRunBuild("SONDEOaOFICIALES");
        }

        //CAMBIO DE ELECCIONES
        public string CambioElecciones(bool europa)
        {
            return europa ? EventBuild("TICKER/EUROPA", "MAP_INT_PAR", "1", 1) : EventBuild("TICKER/EUROPA", "MAP_INT_PAR", "0", 1);
        }

        //RELOJ
        public string EntraReloj(int segundos)
        {
            string signal = "";
            // Fix: pasar el valor numérico como string y tipoItem explícito
            signal += EventBuild("cuentaAtras", "TIMER_LENGTH", $"{segundos}", 1) + "\n";
            signal += Entra("cuentaAtras");
            return signal;
        }
        public string SaleReloj()
        {
            return Sale("cuentaAtras");
        }
        public string PreparaReloj(string time)
        {
            return EventBuild("OBJETO", "TEXT_STRING", time, 1);
        }

        //TICKER
        public string TickerEntra(bool oficial, BrainStormDTO dto)
        {
            string tipo = oficial ? "Resultados" : "Sondeo";
            string entra = (oficial && animacionPrimeros || !oficial && animacionSondeo) ? "EntraPorPrimeraVez" : "ENTRA";
            string signal = "";
            // Fix: pasar count como string y tipoItem explícito
            signal += EventBuild("nPartidosConEscanio", "MAP_INT_PAR", $"{dto.partidos.Count}", 1) + "\n";
            signal += Entra(tipo);
            return signal;
        }
        public string TickerEncadena(bool oficial, BrainStormDTO dto)
        {
            return oficial ? Encadena("TICKER") : Encadena("TICKER_SONDEO");
        }
        public string TickerActualiza(BrainStormDTO dto)
        {
            // Fix: pasar count como string y tipoItem explícito
            return EventBuild("nPartidosConEscanio", "MAP_INT_PAR", $"{dto.partidos.Count}", 1);
        }
        public string TickerSale(bool oficial)
        {
            string tipo = oficial ? "Resultados" : "Sondeo";
            return Sale(tipo);
        }

        public string TickerActualizaEscrutado()
        {
            return EventBuild("TICKER/CambiaEscrutado", "MAP_INT_PAR", "1", 1);
        }
        public string TickerActualizaDatos()
        {
            return EventBuild("TICKER/CambiaResultado", "MAP_INT_PAR", "1", 1);
        }
        public string TickerActualizaDatosIndividualizado(List<PartidoDTO> partidos)
        {
            StringBuilder signal = new StringBuilder();
            foreach (var part in partidos)
            {
                signal.Append(EventBuild($"TICKER/{part.codigo}/HaCambiado", "MAP_INT_PAR", "1", 1));
            }
            return signal.ToString();
        }

        public string TickerYaNoEstaIndividualizado(List<PartidoDTO> partidos)
        {
            StringBuilder signal = new StringBuilder();
            List<string> codigos = partidos.Select(par => par.codigo).ToList();
            codigos.ForEach(cod =>
            {
                signal.Append(EventBuild($"TICKER/{cod}/YaNoEsta", "MAP_INT_PAR", "1", 1));
            }
                );
            return signal.ToString();
        }
        public string TickerActualizaPosiciones()
        {
            return EventBuild("TICKER/CambiaOrden", "MAP_INT_PAR", "1", 1);
        }
        public string TickerActualizaNumPartidos()
        {
            return EventBuild("TICKER/CambiaNPartidos", "MAP_INT_PAR", "1", 1);
        }

        public string TickerEscanosEntra()
        {
            // Datos por separado -> General + Escanios Entra
            string signal = "";
            signal += EventRunBuild("DatosPorSeparado/General") + "\n";
            signal += EventRunBuild("DatosPorSeparado/Escanios/ENTRA");
            return signal;
        }
        public string TickerEscanosSale()
        {
            // Escanios Sale (solo la señal específica)
            return EventRunBuild("DatosPorSeparado/Escanios/SALE");
        }
        public string TickerVotosEntra()
        {
            // Datos por separado -> General + '100' (votos) Entra
            string signal = "";
            signal += EventRunBuild("DatosPorSeparado/General") + "\n";
            signal += EventRunBuild("DatosPorSeparado/100/ENTRA");
            return signal;
        }
        public string TickerVotosSale()
        {
            // Votos Sale (solo la señal específica)
            return EventRunBuild("DatosPorSeparado/100/SALE");
        }

        public string TickerHistoricosEntraInd()
        {
            // Datos por separado -> General + HST Entra (individualizados)
            string signal = "";
            signal += EventRunBuild("DatosPorSeparado/General") + "\n";
            signal += EventRunBuild("DatosPorSeparado/HST/ENTRA");
            return signal;
        }
        public string TickerHistoricosSaleInd()
        {
            // HST Sale (individualizados)
            return EventRunBuild("DatosPorSeparado/HST/SALE");
        }

        public string TickerHistoricosEntraCom()
        {
            // Datos combinados -> General + HST Entra (combinados)
            string signal = "";
            signal += EventRunBuild("DatosCombinados/General") + "\n";
            signal += EventRunBuild("DatosCombinados/HST/ENTRA");
            return signal;
        }
        public string TickerHistoricosSaleCom()
        {
            // HST Sale (combinados)
            return EventRunBuild("DatosCombinados/HST/SALE");
        }

        public string TickerMillonesEntra()
        {
            // Datos combinados -> General + MLLNS Entra (millones)
            string signal = "";
            signal += EventRunBuild("DatosCombinados/General") + "\n";
            signal += EventRunBuild("DatosCombinados/MLLNS/ENTRA");
            return signal;
        }
        public string TickerMillonesSale()
        {
            // MLLNS Sale (millones)
            return EventRunBuild("DatosCombinados/MLLNS/SALE");
        }

        public string TickerFotosEntra()
        {
            // Fix: pasar 1 como string
            return EventBuild("siOcultoCarasSegunNPartidos", "MAP_INT_PAR", "1", 1);
        }
        public string TickerFotosSale()
        {
            // Fix: pasar 0 como string
            return EventBuild("siOcultoCarasSegunNPartidos", "MAP_INT_PAR", "0", 1);
        }

        public string TickerVideoDespliega()
        {
            //itemset("SiVideo/EntraVideoGeneral", "EVENT_RUN")
            //itemset("SiVideo/" + siglas + "/EntraVideo" + siglas, "EVENT_RUN")
            return "";
        }
        public string TickerVideoOculta()
        {
            //itemset("SiVideo/" + siglas + "/SaleVideo" + siglas, "EVENT_RUN")
            //itemset("SiVideo/SaleVideoGeneral", "EVENT_RUN")
            return "";
        }

        //FALDON TD
        public string TickerTDEntra(BrainStormDTO dto)
        {
            var main = Application.Current.MainWindow as MainWindow;
            List<string> siglasPartidos = main.dtoSinFiltrar.partidos.Select(x => x.siglas).ToList();
            List<string> siglasActivos = dto.partidos.Select(x => x.siglas).ToList();
            string signal = "";
            //Poner camara en su sitio
            signal += EventBuild("<>pipe", "PIPE_TYPE", "Orthogonal", 1) + "\n";
            signal += EventBuild("cam1", "CAM_PV[1]", "-535", 1) + "\n";

            signal += EventBuild("NumeroEscrutado", "TEXT_STRING", $"{dto.circunscripcionDTO.escrutado}%", 2, 0.5, 0) + "\n";

            //Tamano
            //itemgo("Pastilla", "PRIM_BAR_LEN[0]", 1341, 0.5, 0) 

            foreach (var siglas in siglasPartidos)
            {

                if (siglasActivos.Contains(siglas))
                {
                    PartidoDTO temp = dto.partidos.FirstOrDefault(x => x.siglas == siglas);
                    signal += Oculta_Desoculta(false, $"Partidos/{siglas}") + "\n";
                    signal += EventBuild($"Escaños/{siglas}", "TEXT_STRING", $"{temp.escaniosHasta}", 2, 0.5, 0) + "\n";
                }
                else
                {
                    signal += Oculta_Desoculta(true, $"Partidos/{siglas}") + "\n";
                    signal += EventBuild($"Escaños/{siglas}", "TEXT_STRING", $"0", 2, 0.5, 0) + "\n";
                }
                //Posiciones
                //For de posiciones de fichas
                //itemgo("Partidos/{sigla}", "OBJ_DISPLACEMENT[0]", 512, 0.5, 0)

                //Posicion elementos dentro pastilla con for para cada elemento
                //itemset("Partidos/{sigla}/Logo", "OBJ_DISPLACEMENT[0]", -1125)
                //itemset("Partidos/{sigla}/Escaños", "OBJ_DISPLACEMENT[0]", 61)

            }

            return signal;
        }
        public string TickerTDEncadena(bool oficial, BrainStormDTO dto)
        {
            return oficial ? Encadena("TICKER") : Encadena("TICKER_SONDEO");
        }
        public string TickerTDActualiza(BrainStormDTO dto)
        {
            // Fix: pasar count como string y tipoItem explícito
            return EventBuild("nPartidosConEscanio", "MAP_INT_PAR", $"{dto.partidos.Count}", 1);
        }
        public string TickerTDSale(bool oficial)
        {
            string tipo = oficial ? "Resultados" : "Sondeo";
            return Sale(tipo);
        }

        //PP_PSOE
        public string PP_PSOEEntra()
        {
            return Entra("TICKER/PP_PSOE");
        }
        public string PP_PSOESale()
        {
            return Sale("TICKER/PP_PSOE");
        }

        //DESPLIEGAS
        public string Despliega4()
        {
            return EventRunBuild("TICKER/DESPLIEGO_4");
        }
        public string Despliega5()
        {
            return EventRunBuild("TICKER/DESPLIEGO_5");
        }

        public string RecuperaTodos()
        {
            return EventRunBuild("TICKER/RECUPERO_TODOS");
        }

        //PACTOMETRO
        public string pactosEntra()
        {
            return Entra("PACTOMETRO");
        }
        public string pactosReinicio()
        {
            return EventRunBuild("PACTOMETRO/INICIO");
        }
        public string pactosSale()
        {
            return Sale("PACTOMETRO");
        }

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
        public string independentismoEntra()
        {
            return Entra("PACTOMETRO_IND");
        }
        public string independentismoReinicio()
        {
            return EventRunBuild("PACTOMETRO_IND/INICIO");
        }
        public string independentismoSale()
        {
            return Sale("PACTOMETRO_IND");
        }

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
        public string SedesEntra(bool tickerIn, BrainStormDTO dto, PartidoDTO seleccionado = null)
        {
            string signal = "";
            signal += EventBuild("nSede", "MAP_INT_PAR", $"{dto.partidos.IndexOf(seleccionado) + 1}", 1) + "\n";
            if (tickerIn)
            {
                signal += EventBuild("Sedes", "MAP_EXE");
                signal += Entra("Sedes/DesdePartido");
            }
            else
            {
                signal += Entra("Sedes/Desde0");
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
        public string SedesSale(bool tickerIn)
        {
            string signal = tickerIn ? Sale("Sedes/DesdePartido") : Sale("Sedes/Desde0");
            return signal;
        }


        //CARTONES

        //PARTICIPACION
        public string participacionEntra(BrainStormDTO dto, int avance)
        {
            StringBuilder signal = new StringBuilder();
            if (dto != null)
            {
                signal.Append(Prepara("PARTICIPACION") + "\n");

                // Try to load the full Circunscripcion model (contains avance1/2/3 and sus históricos).
                Circunscripcion? circ = null;
                try
                {
                    using var con = new ConexionEntityFramework();
                    // Prefer lookup by código si existe, otherwise by nombre.
                    if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo))
                    {
                        circ = CircunscripcionController.GetInstance(con).FindById(dto.circunscripcionDTO.codigo);
                    }
                    if (circ == null && !string.IsNullOrEmpty(dto.circunscripcionDTO?.nombre))
                    {
                        circ = CircunscripcionController.GetInstance(con).FindByName(dto.circunscripcionDTO.nombre);
                    }

                    // Reveal map(s) for autonomía / provincias using the Circunscripcion data when available.
                    var nombreParaMap = circ?.nombre ?? dto.circunscripcionDTO?.nombre ?? string.Empty;
                    var codigoParaMap = circ?.codigo ?? dto.circunscripcionDTO?.codigo ?? string.Empty;

                    if (!string.IsNullOrEmpty(codigoParaMap) && codigoParaMap.EndsWith("00000"))
                    {
                        var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(nombreParaMap);
                        if (provincias != null && provincias.Count > 0)
                        {
                            foreach (var prov in provincias)
                            {
                                signal.Append(EventBuild($"Participacion/Mapa/{prov.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                            }
                        }
                        else
                        {
                            signal.Append(EventBuild($"Participacion/Mapa/{nombreParaMap}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                        }
                    }
                    else
                    {
                        signal.Append(EventBuild($"Participacion/Mapa/{nombreParaMap}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                    }
                }
                catch (Exception)
                {
                    // DB error or lookup failure: fallback to DTO name usage
                    if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.nombre))
                    {
                        signal.Append(EventBuild($"Participacion/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                    }
                }

                // Location text
                string lugarTxt = circ?.nombre ?? dto.circunscripcionDTO?.nombre ?? "";
                signal.Append(CambiaTexto("Participacion/LugarTxt", $"{lugarTxt}") + "\n");

                // Default zero values (will be overwritten below with the proper advance values)
                signal.Append(EventBuild("Participacion_Barra_Izq", "MAP_FLOAT_PAR", "0", 1) + "\n");
                signal.Append(EventBuild("Participacion_Barra_Dch", "MAP_FLOAT_PAR", "0", 1) + "\n");

                // Configurable hour labels (editable in config.ini)
                string horaAv1 = configuration.GetValue("horaAvance1") ?? "";
                string horaAv2 = configuration.GetValue("horaAvance2") ?? "";
                string horaAv3 = configuration.GetValue("horaAvance3") ?? "";
                string horaFinal = configuration.GetValue("horaParticipacion") ?? "";

                // Historical hour labels
                string horaAv1Hist = configuration.GetValue("horaAvance1Historico") ?? "";
                string horaAv2Hist = configuration.GetValue("horaAvance2Historico") ?? "";
                string horaAv3Hist = configuration.GetValue("horaAvance3Historico") ?? "";
                string horaFinalHist = configuration.GetValue("horaParticipacionHistorico") ?? "";

                // Determine left/right values and labels using the full Circunscripcion when available,
                // otherwise fall back to values present in dto.circunscripcionDTO.
                double leftValue = 0.0;
                double rightValue = 0.0;
                string leftTime = "";
                string rightTime = "";

                // Helpers to read fallback values from DTO safely
                var cDto = dto.circunscripcionDTO;
                // For final participation fallback: try model.participacionFinal, else DTO's participacion (if present)
                double finalParticipation = 0.0;
                if (circ != null)
                {
                    finalParticipation = circ.participacionFinal;
                }
                else if (cDto != null)
                {
                    // some DTOs use 'participacion' as the final value
                    try { finalParticipation = Convert.ToDouble(cDto.GetType().GetProperty("participacion")?.GetValue(cDto) ?? 0.0); } catch { finalParticipation = 0.0; }
                }

                // New mapping: do NOT cross advances.
                // Show historical value for the same advance on the left, current value for the same advance on the right.
                switch (avance)
                {
                    case 1:
                        leftTime = string.IsNullOrWhiteSpace(horaAv1Hist) ? "" : horaAv1Hist;
                        rightTime = string.IsNullOrWhiteSpace(horaAv1) ? "" : horaAv1;
                        if (circ != null)
                        {
                            leftValue = circ.avance1Hist;
                            rightValue = circ.avance1 != 0.0 ? circ.avance1 : finalParticipation;
                        }
                        else
                        {
                            try { leftValue = Convert.ToDouble(cDto?.GetType().GetProperty("avance1Hist")?.GetValue(cDto) ?? 0.0); } catch { leftValue = 0.0; }
                            try { rightValue = Convert.ToDouble(cDto?.GetType().GetProperty("avance1")?.GetValue(cDto) ?? finalParticipation); } catch { rightValue = finalParticipation; }
                        }
                        break;

                    case 2:
                        leftTime = string.IsNullOrWhiteSpace(horaAv2Hist) ? "" : horaAv2Hist;
                        rightTime = string.IsNullOrWhiteSpace(horaAv2) ? "" : horaAv2;
                        if (circ != null)
                        {
                            leftValue = circ.avance2Hist;
                            rightValue = circ.avance2 != 0.0 ? circ.avance2 : finalParticipation;
                        }
                        else
                        {
                            try { leftValue = Convert.ToDouble(cDto?.GetType().GetProperty("avance2Hist")?.GetValue(cDto) ?? 0.0); } catch { leftValue = 0.0; }
                            try { rightValue = Convert.ToDouble(cDto?.GetType().GetProperty("avance2")?.GetValue(cDto) ?? finalParticipation); } catch { rightValue = finalParticipation; }
                        }
                        break;

                    case 3:
                        leftTime = string.IsNullOrWhiteSpace(horaAv3Hist) ? "" : horaAv3Hist;
                        rightTime = string.IsNullOrWhiteSpace(horaAv3) ? "" : horaAv3;
                        if (circ != null)
                        {
                            leftValue = circ.avance3Hist;
                            rightValue = circ.avance3 != 0.0 ? circ.avance3 : finalParticipation;
                        }
                        else
                        {
                            try { leftValue = Convert.ToDouble(cDto?.GetType().GetProperty("avance3Hist")?.GetValue(cDto) ?? 0.0); } catch { leftValue = 0.0; }
                            try { rightValue = Convert.ToDouble(cDto?.GetType().GetProperty("avance3")?.GetValue(cDto) ?? finalParticipation); } catch { rightValue = finalParticipation; }
                        }
                        break;

                    case 4:
                    default:
                        // Final participation: compare historical participation with final participation
                        leftTime = string.IsNullOrWhiteSpace(horaFinalHist) ? "" : horaFinalHist;
                        rightTime = string.IsNullOrWhiteSpace(horaFinal) ? "" : horaFinal;
                        if (circ != null)
                        {
                            leftValue = circ.participacionHist;
                            rightValue = finalParticipation;
                        }
                        else
                        {
                            // Some DTOs may have 'participacionHistorica' or 'participacionHist'
                            try { leftValue = Convert.ToDouble(cDto?.GetType().GetProperty("participacionHistorica")?.GetValue(cDto) ?? cDto?.GetType().GetProperty("participacionHist")?.GetValue(cDto) ?? 0.0); } catch { leftValue = 0.0; }
                            rightValue = finalParticipation;
                        }
                        break;
                }

                // Send hour labels
                signal.Append(EventBuild("Participacion/Hora_Izq", "TEXT_STRING", $"{leftTime}", 1) + "\n");
                signal.Append(EventBuild("Participacion/Hora_Dch", "TEXT_STRING", $"{rightTime}", 1) + "\n");

                // Send bar values with animation (itemgo)
                signal.Append(EventBuild("Participacion_Barra_Izq", "MAP_FLOAT_PAR", $"{leftValue.ToString("F2", CultureInfo.InvariantCulture)}", 2, 0.5, 0.6) + "\n");
                signal.Append(EventBuild("Participacion_Barra_Dch", "MAP_FLOAT_PAR", $"{rightValue.ToString("F2", CultureInfo.InvariantCulture)}", 2, 0.5, 0.6) + "\n");

                // Adjust fonts/offset depending on the right value (current participation)
                double reference = rightValue;
                if (reference < 25)
                {
                    signal.Append(EventBuild("Participacion/Barras/Barra_Dch/Txt_Dch", "OBJ_OFFSET[2]", "322", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Dch", "TEXT_FONT", "Heavy_Naranja", 1) + "\n");
                    signal.Append(EventBuild("OBJ_DISPLACEMENT2", "BIND_VOFFSET[0]", "322", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Izq", "TEXT_FONT", "Heavy", 1) + "\n");
                }
                else
                {
                    signal.Append(EventBuild("Participacion/Barras/Barra_Dch/Txt_Dch", "OBJ_OFFSET[2]", "190", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Dch", "TEXT_FONT", "Heavy_Negro", 1) + "\n");
                    signal.Append(EventBuild("OBJ_DISPLACEMENT2", "BIND_VOFFSET[0]", "190", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Izq", "TEXT_FONT", "Heavy_Negro", 1) + "\n");
                }

                // Finalmente, entrar en PARTICIPACION
                signal.Append(Entra("PARTICIPACION"));
            }

            return signal.ToString();
        }
        public string participacionEncadena(BrainStormDTO dto, int avance)
        {
            string signal = "";
            if (dto != null)
            {
                signal += Encadena("PARTICIPACION") + "\n";

                // If the selected circunscripción is an autonomía (code ends with 00000)
                // then hide (Oculta_Desoculta(false,...)) every province belonging to that autonomía.
                // We use CircunscripcionController.FindAllCircunscripcionesByNameAutonomia(nombreAutonomia)
                // which returns the provinces (codes ending with "000") for the given autonomía name.
                if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo) && dto.circunscripcionDTO.codigo.EndsWith("00000"))
                {
                    try
                    {
                        using var con = new ConexionEntityFramework();
                        var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dto.circunscripcionDTO.nombre);
                        if (provincias != null && provincias.Count > 0)
                        {
                            foreach (var prov in provincias)
                            {
                                signal += EventBuild($"Participacion/Mapa/{prov.nombre}", "OBJ_CULL", "0", 2, 0.3, 0.3) + "\n";
                            }
                        }
                        else
                        {
                            // Fallback: if no provinces found, hide the autonomía map object itself
                            signal += EventBuild($"Participacion/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0.3) + "\n";
                        }
                    }
                    catch (Exception)
                    {
                        // On any error (DB unavailable, etc.) fallback to hiding the autonomía object itself
                        signal += EventBuild($"Participacion/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0.3) + "\n";
                    }
                }
                else
                {
                    // Not an autonomía: hide the single circunscripción selected
                    signal += EventBuild($"Participacion/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0.3) + "\n";
                }

                signal += CambiaTexto("Participacion/LugarTxt", $"{dto.circunscripcionDTO.nombre}") + "\n";

                // Read hour labels (including historical)
                string horaAv1 = configuration.GetValue("horaAvance1") ?? "";
                string horaAv2 = configuration.GetValue("horaAvance2") ?? "";
                string horaAv3 = configuration.GetValue("horaAvance3") ?? "";
                string horaFinal = configuration.GetValue("horaParticipacion") ?? "";
                string horaAv1Hist = configuration.GetValue("horaAvance1Historico") ?? "";
                string horaAv2Hist = configuration.GetValue("horaAvance2Historico") ?? "";
                string horaAv3Hist = configuration.GetValue("horaAvance3Historico") ?? "";
                string horaFinalHist = configuration.GetValue("horaParticipacionHistorico") ?? "";

                // Determine left/right values using DTO only (encadenar doesn't query DB)
                double leftValue = 0.0;
                double rightValue = 0.0;
                var cDto = dto.circunscripcionDTO;
                switch (avance)
                {
                    case 1:
                        leftValue = SafeGetDouble(cDto, "avance1Hist", 0.0);
                        rightValue = SafeGetDouble(cDto, "avance1", SafeGetDouble(cDto, "participacion", 0.0));
                        signal += EventBuild("Participacion/Hora_Izq", "TEXT_STRING", $"{(string.IsNullOrWhiteSpace(horaAv1Hist) ? "" : horaAv1Hist)}", 1) + "\n";
                        signal += EventBuild("Participacion/Hora_Dch", "TEXT_STRING", $"{(string.IsNullOrWhiteSpace(horaAv1) ? "" : horaAv1)}", 1) + "\n";
                        break;
                    case 2:
                        leftValue = SafeGetDouble(cDto, "avance2Hist", 0.0);
                        rightValue = SafeGetDouble(cDto, "avance2", SafeGetDouble(cDto, "participacion", 0.0));
                        signal += EventBuild("Participacion/Hora_Izq", "TEXT_STRING", $"{(string.IsNullOrWhiteSpace(horaAv2Hist) ? "" : horaAv2Hist)}", 1) + "\n";
                        signal += EventBuild("Participacion/Hora_Dch", "TEXT_STRING", $"{(string.IsNullOrWhiteSpace(horaAv2) ? "" : horaAv2)}", 1) + "\n";
                        break;
                    case 3:
                        leftValue = SafeGetDouble(cDto, "avance3Hist", 0.0);
                        rightValue = SafeGetDouble(cDto, "avance3", SafeGetDouble(cDto, "participacion", 0.0));
                        signal += EventBuild("Participacion/Hora_Izq", "TEXT_STRING", $"{(string.IsNullOrWhiteSpace(horaAv3Hist) ? "" : horaAv3Hist)}", 1) + "\n";
                        signal += EventBuild("Participacion/Hora_Dch", "TEXT_STRING", $"{(string.IsNullOrWhiteSpace(horaAv3) ? "" : horaAv3)}", 1) + "\n";
                        break;
                    default:
                        leftValue = SafeGetDouble(cDto, "participacionHistorica", SafeGetDouble(cDto, "participacionHist", 0.0));
                        rightValue = SafeGetDouble(cDto, "participacion", 0.0);
                        signal += EventBuild("Participacion/Hora_Izq", "TEXT_STRING", $"{(string.IsNullOrWhiteSpace(horaFinalHist) ? "" : horaFinalHist)}", 1) + "\n";
                        signal += EventBuild("Participacion/Hora_Dch", "TEXT_STRING", $"{(string.IsNullOrWhiteSpace(horaFinal) ? "" : horaFinal)}", 1) + "\n";
                        break;
                }

                // Fonts/offset depending on rightValue
                if (rightValue < 25)
                {
                    signal += EventBuild("Participacion/Barras/Barra_Dch/Txt_Dch", "OBJ_OFFSET[2]", "322", 2, 0.5) + "\n";
                    signal += EventBuild("Participacion/Txt_Dch", "TEXT_FONT", "Heavy_Naranja", 1) + "\n";
                    signal += EventBuild("OBJ_DISPLACEMENT2", "BIND_VOFFSET[0]", "322", 2, 0.5) + "\n";
                    signal += EventBuild("Participacion/Txt_Izq", "TEXT_FONT", "Heavy", 1) + "\n";
                }
                else
                {
                    signal += EventBuild("Participacion/Barras/Barra_Dch/Txt_Dch", "OBJ_OFFSET[2]", "190", 2, 0.5) + "\n";
                    signal += EventBuild("Participacion/Txt_Dch", "TEXT_FONT", "Heavy_Negro", 1) + "\n";
                    signal += EventBuild("OBJ_DISPLACEMENT2", "BIND_VOFFSET[0]", "190", 2, 0.5) + "\n";
                    signal += EventBuild("Participacion/Txt_Izq", "TEXT_FONT", "Heavy_Negro", 1) + "\n";
                }

                signal += EventBuild("Participacion_Barra_Dch", "MAP_FLOAT_PAR", $"{rightValue.ToString("F2", CultureInfo.InvariantCulture)}", 2, 0.5, 0.3) + "\n";
                signal += EventBuild("Participacion_Barra_Izq", "MAP_FLOAT_PAR", $"{leftValue.ToString("F2", CultureInfo.InvariantCulture)}", 2, 0.5, 0.3) + "\n";

            }

            return signal;
        }

        // small helper to safely read double properties from DTO via reflection
        private static double SafeGetDouble(object? dto, string propName, double defaultValue)
        {
            if (dto == null) return defaultValue;
            try
            {
                var prop = dto.GetType().GetProperty(propName);
                if (prop == null) return defaultValue;
                var val = prop.GetValue(dto);
                if (val == null) return defaultValue;
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }
        public string participacionSale()
        {
            return Sale("PARTICIPACION");
        }

        //CCAA
        public string ccaaEntra(BrainStormDTO dto)
        {
            StringBuilder signal = new StringBuilder();

            if (dto == null)
            {
                return "";
            }

            // Prepare CCAA cartones
            signal.Append(Prepara("CCAA_CARTONES") + "\n");

            // Set location text
            signal.Append(EventBuild("CCAA_Carton/LugarTxt", "TEXT_STRING", $"{dto.circunscripcionDTO.nombre}", 1) + "\n");

            // Reveal map(s) for the selected circunscripción (autonomía -> reveal provinces)
            if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo) && dto.circunscripcionDTO.codigo.EndsWith("00000"))
            {
                try
                {
                    using var con = new ConexionEntityFramework();
                    var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dto.circunscripcionDTO.nombre);
                    if (provincias != null && provincias.Count > 0)
                    {
                        foreach (var prov in provincias)
                        {
                            signal.Append(EventBuild($"CCAA_Carton/Mapa/{prov.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                        }
                    }
                    else
                    {
                        // Fallback: reveal the autonomía map object itself
                        signal.Append(EventBuild($"CCAA_Carton/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                    }
                }
                catch (Exception)
                {
                    // On error fallback to revealing the autonomía map object itself
                    signal.Append(EventBuild($"CCAA_Carton/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                }
            }
            else
            {
                // Not an autonomía: reveal the single circunscripción selected
                signal.Append(EventBuild($"CCAA_Carton/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
            }

            // Position fichas: 0, -110, -220, ...
            var partidos = dto.partidos ?? new List<PartidoDTO>();
            var siglasLocal = partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            for (int i = 0; i < siglasLocal.Count; i++)
            {
                int posicion = i * -110;
                signal.Append(EventBuild($"CCAA_Carton/Fichas/{siglasLocal[i]}", "OBJ_DISPLACEMENT[2]", $"{posicion}", 1) + "\n");
                // Make visible
                signal.Append(EventBuild($"CCAA_Carton/Fichas/{siglasLocal[i]}", "OBJ_CULL", "0", 1) + "\n");
            }

            // Enter CCAA cartones
            signal.Append(Entra("CCAA_CARTONES"));

            return signal.ToString();
        }
        public string ccaaEncadena()
        {
            return EventRunBuild("CCAA/CAMBIA");
        }
        public string ccaaSale()
        {
            return Sale("CCAA");
        }
        public string ccaaBaja(BrainStormDTO dto)
        {
            StringBuilder signal = new StringBuilder();
            var partidos = dto.partidos ?? new List<PartidoDTO>();
            if (partidos.Count > 7)
            {
                signal.Append(EventRunBuild("CCAA_CARTONES/BAJA", 0.0, 3.0) + "\n");
            }
            return signal.ToString();
        }
        public string ccaaSube(BrainStormDTO dto)
        {
            StringBuilder signal = new StringBuilder();
            var partidos = dto.partidos ?? new List<PartidoDTO>();
            if (partidos.Count > 7)
            {
                signal.Append(EventRunBuild("CCAA_CARTONES/SUBE", 0.0, 3.0) + "\n");
            }
            return signal.ToString();
        }

        //FICHAS DE PARTIDO
        private int indexCarrusel = 0;
        List<string> siglas;
        PartidoDTO fichaSeleccionada = null;
        public string fichaEntra(bool oficiales, BrainStormDTO dto, PartidoDTO seleccionado = null)
        {
            fichaSeleccionada = seleccionado;
            siglas = dto.partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            string mode = oficiales ? "Oficiales" : "Sondeos";
            string path = $"Carton_Carrusel/{mode}/{siglas}";
            StringBuilder sb = new StringBuilder();

            string oficicialOSondeo = oficiales ? "OFICIAL" : "SONDEO";
            sb.Append(EventRunBuild($"CARRUSEL/{oficicialOSondeo}"));
            sb.Append("\n");
            // prepare offscreen to the right
            foreach (string sigla in siglas)
            {
                string pathPartido = $"Carton_Carrusel/{mode}/{sigla}";
                sb.Append(EventBuild(pathPartido, "OBJ_DISPLACEMENT[0]", "1920", 1));
                sb.Append("\n");
            }
            if (seleccionado != null)
            {
                string pathSeleccionado = $"Carton_Carrusel/{mode}/{seleccionado.siglas}";
                sb.Append(EventBuild(pathSeleccionado, "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 0.0));
                sb.Append("\n");
                indexCarrusel = siglas.IndexOf(seleccionado.siglas);
            }
            else
            {
                string pathSeleccionado = $"Carton_Carrusel/{mode}/{siglas[0]}";
                sb.Append(EventBuild(pathSeleccionado, "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 0.0));
                indexCarrusel = 0;
            }

            sb.Append(Entra("CARRUSEL"));

            return sb.ToString();

        }
        // Encadena: animate an outgoing carton (siglasSalir) to the left and bring the incoming (siglasEntrar) from the right.
        // If you only want to animate a single carton out, pass siglasEntrar = null or empty.
        public string fichaEncadena(bool oficiales, BrainStormDTO dto, PartidoDTO seleccionado = null)
        {
            fichaSeleccionada = seleccionado;
            string mode = oficiales ? "Oficiales" : "Sondeos";
            string path = $"Carton_Carrusel/{mode}/{siglas}";
            StringBuilder sb = new StringBuilder();
            string siglasYaDentro = siglas[indexCarrusel];

            // prepare offscreen to the right
            foreach (string sigla in siglas)
            {
                string pathPartido = $"Carton_Carrusel/{mode}/{sigla}";
                if (!sigla.Equals(siglasYaDentro))
                {
                    sb.Append(EventBuild(pathPartido, "OBJ_DISPLACEMENT[0]", "1920", 1));
                    sb.Append("\n");
                }
            }
            string pathSaliente = $"Carton_Carrusel/{mode}/{siglasYaDentro}";
            sb.Append(EventBuild(pathSaliente, "OBJ_DISPLACEMENT[0]", "-1920", 2, 0.5, 0.0));
            sb.Append("\n");

            if (seleccionado != null)
            {
                string pathEntrante = $"Carton_Carrusel/{mode}/{seleccionado.siglas}";
                sb.Append(EventBuild(pathEntrante, "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 0.0));
                sb.Append("\n");
                indexCarrusel = siglas.IndexOf(seleccionado.siglas);
            }
            else
            {
                string pathSeleccionado = $"Carton_Carrusel/{mode}/{siglas[indexCarrusel + 1]}";
                sb.Append(EventBuild(pathSeleccionado, "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 0.0));
                indexCarrusel += 1;
            }

            return sb.ToString();
        }
        public string fichaActualiza(bool oficiales, BrainStormDTO dto, BrainStormDTO dtoAnterior)
        {
            //-Posición cartón que pasa por delante: itemset("Carton_Carrusel/Oficiales/Cs", "OBJ_DISPLACEMENT", (0, -0.3, 0))
            //- Posición cartón que pasa por detrás: itemset("Carton_Carrusel/Oficiales/Cs", "OBJ_DISPLACEMENT", (0, 0, 0))
            string signal = "";
            if (oficiales)
            {
                signal += EventBuild("Oficial_Codigo", "MAP_LLSTRING_LOAD");
            }
            else
            {
                signal += EventBuild("Sondeo_Codigo", "MAP_LLSTRING_LOAD");
            }
            return signal;
        }
        public string fichaSale(bool oficiales)
        {
            StringBuilder sb = new StringBuilder();
            string mode = oficiales ? "Oficiales" : "Sondeos";
            if (siglas != null && indexCarrusel >= 0)
            {
                string siglasYaDentro = siglas[indexCarrusel];
                string pathSaliente = $"Carton_Carrusel/{mode}/{siglasYaDentro}";
                sb.Append(EventBuild(pathSaliente, "OBJ_DISPLACEMENT[0]", "-1920", 2, 0.5, 0.0));
            }
            sb.Append(Sale("CARRUSEL"));
            indexCarrusel = 0;
            siglas = new List<string>();
            fichaSeleccionada = null;
            return sb.ToString();
        }

        //PACTOMETRO
        public string pactometroEntra()
        {
            return Entra("PACTOMETRO");
        }
        public string pactometroEncadena()
        {
            return EventRunBuild("PACTOMETRO/POSICIONES");
        }
        public string pactometroSale()
        {
            return Sale("PACTOMETRO");
        }

        public string pactometroVictoria()
        {
            return EventRunBuild("PACTOMETRO/VICTORIA");
        }

        //MAYORIAS
        public string mayoriasEntra(BrainStormDTO dto)
        {
            StringBuilder signal = new StringBuilder();

            if (dto == null)
            {
                return "";
            }

            signal.Append(Prepara("MAYORIAS") + "\n");

            // Set location text
            signal.Append(EventBuild("Mayorias/LugarTxt1", "TEXT_STRING", $"{dto.circunscripcionDTO.nombre}", 1) + "\n");

            // Reveal map(s) for the selected circunscripción.
            if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo) && dto.circunscripcionDTO.codigo.EndsWith("00000"))
            {
                try
                {
                    using var con = new ConexionEntityFramework();
                    var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dto.circunscripcionDTO.nombre);
                    if (provincias != null && provincias.Count > 0)
                    {
                        foreach (var prov in provincias)
                        {
                            signal.Append(EventBuild($"Mapa_Mayorias/Mapa/{prov.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                        }
                    }
                    else
                    {
                        // Fallback: reveal the autonomía map object itself
                        signal.Append(EventBuild($"Mapa_Mayorias/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                    }
                }
                catch (Exception)
                {
                    // On error fallback to revealing the autonomía map object itself
                    signal.Append(EventBuild($"Mapa_Mayorias/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                }
            }
            else
            {
                // Not an autonomía: reveal the single circunscripción selected
                signal.Append(EventBuild($"Mapa_Mayorias/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
            }

            var partidos = dto.partidos ?? new List<PartidoDTO>();
            // Position party fichas on the MAYORIAS map: positions 0, -100, -200, ...
            var siglas = partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            for (int i = 0; i < siglas.Count; i++)
            {
                int posicion = i * -100;
                signal.Append(EventBuild($"Mapa_Mayorias/Fichas/{siglas[i]}", "OBJ_DISPLACEMENT[2]", $"{posicion}", 1) + "\n");
                // Ensure the ficha is visible
                signal.Append(EventBuild($"Mapa_Mayorias/Fichas/{siglas[i]}", "OBJ_CULL", "0", 1) + "\n");
            }

            // Enter MAYORIAS
            signal.Append(Entra("MAYORIAS"));

            return signal.ToString();
        }
        public string mayoriasEncadena(BrainStormDTO dto)
        {
            return EventRunBuild("MAYORIAS/CAMBIA");
        }
        public string mayoriasSale()
        {
            return Sale("MAYORIAS");
        }
        public string mayoriasBaja(BrainStormDTO dto)
        {
            StringBuilder signal = new StringBuilder();
            var partidos = dto.partidos ?? new List<PartidoDTO>();
            if (partidos.Count > 6)
            {
                signal.Append(EventRunBuild("MAYORIAS/BAJA", 0.0, 3.0) + "\n");
            }
            return signal.ToString();
        }
        public string mayoriasSube(BrainStormDTO dto)
        {
            StringBuilder signal = new StringBuilder();
            var partidos = dto.partidos ?? new List<PartidoDTO>();
            if (partidos.Count > 6)
            {
                signal.Append(EventRunBuild("MAYORIAS/SUBE", 0.0, 3.0) + "\n");
            }
            return signal.ToString();
        }

        //SUPERFALDON
        public string superfaldonEntra()
        {
            return Entra("SUPERFALDON");
        }
        public string superfaldonSale()
        {
            return Sale("SUPERFALDON");
        }

        //SEDES
        public string superfaldonSedesEntra()
        {
            return Entra("SEDES");
        }
        public string superfaldonSedesEncadena()
        {
            return Entra("SEDES/ENCADENA");
        }
        public string superfaldonSedesSale()
        {
            return Sale("SEDES");
        }

        //SUPERFALDON - FICHAS
        // TODO: Construir señal para entrada del gráfico FICHAS en SUPERFALDÓN
        public string sfFichasEntra()
        {
            return "";
        }
        // TODO: Construir señal para encadenar entre gráficos FICHAS en SUPERFALDÓN
        public string sfFichasEncadena()
        {
            return "";
        }
        // TODO: Construir señal para salida del gráfico FICHAS en SUPERFALDÓN
        public string sfFichasSale()
        {
            return "";
        }

        //SUPERFALDON - PACTOMETRO
        // TODO: Construir señal para entrada del gráfico PACTÓMETRO en SUPERFALDÓN
        public string sfPactometroEntra()
        {
            return "";
        }
        // TODO: Construir señal para encadenar entre gráficos PACTÓMETRO en SUPERFALDÓN
        public string sfPactometroEncadena()
        {
            return "";
        }
        // TODO: Construir señal para salida del gráfico PACTÓMETRO en SUPERFALDÓN
        public string sfPactometroSale()
        {
            return "";
        }

        //SUPERFALDON - MAYORIAS
        // TODO: Construir señal para entrada del gráfico MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasEntra()
        {
            return "";
        }
        // TODO: Construir señal para encadenar entre gráficos MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasEncadena()
        {
            return "";
        }
        // TODO: Construir señal para salida del gráfico MAYORÍAS en SUPERFALDÓN
        public string sfMayoriasSale()
        {
            return "";
        }

        //SUPERFALDON - BIPARTIDISMO
        // TODO: Construir señal para entrada del gráfico BIPARTIDISMO en SUPERFALDÓN
        public string sfBipartidismoEntra()
        {
            return "";
        }
        // TODO: Construir señal para encadenar entre gráficos BIPARTIDISMO en SUPERFALDÓN
        public string sfBipartidismoEncadena()
        {
            return "";
        }
        // TODO: Construir señal para salida del gráfico BIPARTIDISMO en SUPERFALDÓN
        public string sfBipartidismoSale()
        {
            return "";
        }

        //SUPERFALDON - GANADOR
        // TODO: Construir señal para entrada del gráfico GANADOR en SUPERFALDÓN
        public string sfGanadorEntra()
        {
            return "";
        }
        // TODO: Construir señal para encadenar entre gráficos GANADOR en SUPERFALDÓN
        public string sfGanadorEncadena()
        {
            return "";
        }
        // TODO: Construir señal para salida del gráfico GANADOR en SUPERFALDÓN
        public string sfGanadorSale()
        {
            return "";
        }


        //CONSTRUCTORES

        //Para construir la señal necesitaría el objeto o evento al que llamo, la propiedad a cambiar,
        //el valor o valores que cambian y el tipo: 1 para itemset y 2 para itemgo
        /// <summary>
        /// General constructor for itemset/itemgo/itemget. If using itemgo, pass tipoItem = 2 and provide animTime & delay.
        /// values may be null for itemget or event-run itemset.
        /// </summary>
        private string EventBuild(string objeto, string propiedad, string? values, int tipoItem, double animTime = 0.0, double delay = 0.0)
        {
            // Ensure decimal point invariant formatting for doubles
            string animStr = animTime.ToString(CultureInfo.InvariantCulture);
            string delayStr = delay.ToString(CultureInfo.InvariantCulture);

            // Local helper: determine if values is numeric/boolean/already-quoted; otherwise wrap in single quotes.
            static string FormatValue(string raw)
            {
                if (raw == null) return "0";
                var trimmed = raw.Trim();

                // Already quoted with single or double quotes -> keep as is
                if (trimmed.Length >= 2 &&
                    ((trimmed[0] == '\'' && trimmed[^1] == '\'') || (trimmed[0] == '\"' && trimmed[^1] == '\"')))
                {
                    return trimmed;
                }

                // Numeric (int)
                if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return trimmed;
                }

                // Numeric (double/float)
                if (double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _))
                {
                    // Ensure invariant decimal separator already present in trimmed if any
                    return trimmed;
                }

                // Boolean
                if (bool.TryParse(trimmed, out var b))
                {
                    // script likely expects lowercase true/false
                    return b ? "true" : "false";
                }

                // Looks like an expression or special token (e.g. contains parentheses, [, ], / or .) - keep as is
                if (trimmed.IndexOfAny(new[] { '(', ')', '[', ']', '{', '}', '/', '.', ',' }) >= 0)
                {
                    return trimmed;
                }

                // Default: treat as string and wrap in single quotes
                return $"'{trimmed}'";
            }

            if (tipoItem == 1) // itemset
            {
                if (!string.IsNullOrEmpty(values))
                {
                    var formatted = FormatValue(values);
                    return $"itemset('<{_bd}>{objeto}','{propiedad}',{formatted});";
                }
                else
                {
                    return $"itemset('<{_bd}>{objeto}','{propiedad}');";
                }
            }

            if (tipoItem == 2) // itemgo (animation)
            {
                // If values is null, use 0 as placeholder (common for EVENT_RUN usage)
                var val = string.IsNullOrEmpty(values) ? "0" : values;
                var formatted = FormatValue(val);
                return $"itemgo('<{_bd}>{objeto}','{propiedad}',{formatted},{animStr},{delayStr});";
            }

            if (tipoItem == 3) // itemget
            {
                return $"itemget('<{_bd}>{objeto}','{propiedad}');";
            }

            // Fallback to itemset without value
            return $"itemset('<{_bd}>{objeto}','{propiedad}');";
        }

        // Backwards-compatible overloads that call the general constructor
        private string EventBuild(string objeto, string propiedad, string values, int tipoItem)
        {
            return EventBuild(objeto, propiedad, (string?)values, tipoItem, 0.0, 0.0);
        }
        private string EventBuild(string objeto, string propiedad, int tipoItem)
        {
            return EventBuild(objeto, propiedad, null, tipoItem, 0.0, 0.0);
        }
        private string EventBuild(string objeto, string propiedad)
        {
            return $"itemset('<{_bd}>{objeto}','{propiedad}');";
        }

        /// <summary>
        /// Build an EVENT_RUN signal. If animTime or delay are non-zero the signal will be an itemgo with placeholders,
        /// otherwise it will be a simple itemset acting as a "play" button.
        /// </summary>
        private string EventRunBuild(string objeto, double animTime = 0.0, double delay = 0.0)
        {
            if (animTime != 0.0 || delay != 0.0)
            {
                // Use itemgo for animated event run; values placeholder 0
                return EventBuild(objeto, "EVENT_RUN", "0", 2, animTime, delay);
            }
            else
            {
                return EventBuild(objeto, "EVENT_RUN", null, 1);
            }
        }

        //MENSAJES COMUNES
        public string Reset()
        {
            return EventRunBuild("RESET");
        }
        public string Prepara(string objeto)
        {
            return EventRunBuild($"{objeto}/PREPARA");
        }
        public string Entra(string objeto)
        {
            return EventRunBuild($"{objeto}/ENTRA");
        }
        public string Encadena(string objeto)
        {
            return EventRunBuild($"{objeto}/ENCADENA");
        }
        public string Oculta_Desoculta(bool ocultar, string objeto)
        {
            return ocultar ? EventBuild(objeto, "OBJ_CULL", "1", 1) : EventBuild(objeto, "OBJ_CULL", "0", 1);
        }
        public string CambiaTexto(string objeto, string texto)
        {
            return EventBuild(objeto, "TEXT_STRING", $"{texto}", 1);
        }
        public string Sale(string objeto)
        {
            return EventRunBuild($"{objeto}/SALE");
        }

    }
}
