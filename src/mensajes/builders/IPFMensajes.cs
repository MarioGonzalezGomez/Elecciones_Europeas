using System.Globalization;
using System.Runtime.InteropServices;
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

            // --- Tamaño y posiciones dinámicas según número de partidos ---
            int n = dto.partidos?.Count ?? 0;

            // Mapping for 1..6 provided by the user
            var layoutByCount = new Dictionary<int, (int Size, int[] Positions, int LogoPos, int EscanosPos)>()
            {
                {1, (1341, new[] {512}, -1125, 61)},
                {2, (660, new[] {170,855}, -675, -390)},
                {3, (435, new[] {56,512,967}, -795, -279)},
                {4, (320, new[] {0,341,683,1024}, -621, -441)},
                {5, (255, new[] {-33,240,513,787,1060}, -591, -476)},
                {6, (205, new[] {-59,169,397,626,854,1082}, -571, -495)},
            };

            (int Size, int[] Positions, int LogoPos, int EscanosPos) layout;

            if (layoutByCount.ContainsKey(n))
            {
                layout = layoutByCount[n];
            }
            else if (n >= 1)
            {
                // Fallback: distribuir posiciones linealmente entre un rango aproximado
                // Rango tomado entre -60 .. 1082 (valores observados en datos) y tamaño aproximado decreciente
                int left = -60;
                int right = 1082;
                int[] positions = new int[n];
                if (n == 1)
                {
                    positions[0] = (left + right) / 2;
                }
                else
                {
                    double step = (double)(right - left) / (n - 1);
                    for (int i = 0; i < n; i++)
                    {
                        positions[i] = (int)Math.Round(left + step * i);
                    }
                }

                // approximate size: decrease with n, but clamp to reasonable range
                int approxSize = Math.Max(120, 1341 - (n - 1) * 220);
                // approximate logo/escaños offsets chosen to resemble provided data
                int approxLogo = -600 - (n - 1) * 20;
                int approxEscanos = 100 - (n - 1) * 70;

                layout = (approxSize, positions, approxLogo, approxEscanos);
            }
            else
            {
                // no parties -> default safe values
                layout = (1341, new[] { 512 }, -1125, 61);
            }

            // Aplicar tamaño de la "Pastilla" (animado)
            signal += EventBuild("Pastilla", "PRIM_BAR_LEN[0]", $"{layout.Size}", 2, 0.5, 0) + "\n";

            // Precompute mapping from sigla -> index dentro de los activos (dto.partidos)
            var activeIndex = siglasActivos
                .Select((s, i) => new { Sigla = s, Index = i })
                .ToDictionary(x => x.Sigla, x => x.Index);

            // Para cada partido (lista completa en el layout), colocar/ocultar y ajustar logo/escaños
            for (int idx = 0; idx < siglasPartidos.Count; idx++)
            {
                var siglas = siglasPartidos[idx];

                if (activeIndex.TryGetValue(siglas, out int posIndex) && posIndex >= 0 && posIndex < layout.Positions.Length)
                {
                    int pos = layout.Positions[posIndex];
                    // Posición general del contenedor del partido (animada)
                    signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[0]", $"{pos}", 2, 0.5, 0) + "\n";

                    // Logo y escaños dentro de la pastilla (colocados con itemset)
                    signal += EventBuild($"Partidos/{siglas}/Logo", "OBJ_DISPLACEMENT[0]", $"{layout.LogoPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglas}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layout.EscanosPos}", 1) + "\n";
                }
                else
                {
                    // Partido no activo: colocarlo fuera de la pantalla a la derecha y mantener offsets por defecto
                    signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[0]", "1920", 2, 0.5, 0) + "\n";
                    signal += EventBuild($"Partidos/{siglas}/Logo", "OBJ_DISPLACEMENT[0]", $"{layout.LogoPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglas}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layout.EscanosPos}", 1) + "\n";
                }

                // visible / oculto y valor de escaños ya se manejan más arriba en el bucle original,
                // aquí solo nos aseguramos de la posición y offsets.
            }

            //Fin colocación de posiciones/tamaño
            // (el resto del método original ya añadía visibilidad y textos de escaños)
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
            }
            return signal;
        }
        // public string TickerTDEncadena(bool oficial, BrainStormDTO dto)
        // {
        //     return oficial ? Encadena("TICKER") : Encadena("TICKER_SONDEO");
        // }
        public string TickerTDActualiza(BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            var main = Application.Current.MainWindow as MainWindow;
            List<string> siglasPartidos = main.dtoSinFiltrar.partidos.Select(x => x.siglas).ToList();
            List<string> siglasNuevas = dtoNuevo.partidos.Select(x => x.siglas).ToList();
            List<string> siglasAnteriores = dtoAnterior.partidos.Select(x => x.siglas).ToList();

            string signal = "";

            // Actualizar escrutado
            signal += EventBuild("NumeroEscrutado", "TEXT_STRING", $"{dtoNuevo.circunscripcionDTO.escrutado}%", 2, 0.5, 0) + "\n";

            // Comprobar si cambia el número de partidos
            int nAnterior = dtoAnterior.partidos?.Count ?? 0;
            int nNuevo = dtoNuevo.partidos?.Count ?? 0;

            if (nAnterior != nNuevo)
            {
                // Si cambia el número de partidos, recalcular layout completo
                // Mapping for 1..6 provided by the user
                var layoutByCount = new Dictionary<int, (int Size, int[] Positions, int LogoPos, int EscanosPos)>()
                {
                    {1, (1341, new[] {512}, -1125, 61)},
                    {2, (660, new[] {170,855}, -675, -390)},
                    {3, (435, new[] {56,512,967}, -795, -279)},
                    {4, (320, new[] {0,341,683,1024}, -621, -441)},
                    {5, (255, new[] {-33,240,513,787,1060}, -591, -476)},
                    {6, (205, new[] {-59,169,397,626,854,1082}, -571, -495)},
                };

                (int Size, int[] Positions, int LogoPos, int EscanosPos) layoutNuevo;

                if (layoutByCount.ContainsKey(nNuevo))
                {
                    layoutNuevo = layoutByCount[nNuevo];
                }
                else if (nNuevo >= 1)
                {
                    // Fallback: distribuir posiciones linealmente
                    int left = -60;
                    int right = 1082;
                    int[] positions = new int[nNuevo];
                    if (nNuevo == 1)
                    {
                        positions[0] = (left + right) / 2;
                    }
                    else
                    {
                        double step = (double)(right - left) / (nNuevo - 1);
                        for (int i = 0; i < nNuevo; i++)
                        {
                            positions[i] = (int)Math.Round(left + step * i);
                        }
                    }

                    int approxSize = Math.Max(120, 1341 - (nNuevo - 1) * 220);
                    int approxLogo = -600 - (nNuevo - 1) * 20;
                    int approxEscanos = 100 - (nNuevo - 1) * 70;

                    layoutNuevo = (approxSize, positions, approxLogo, approxEscanos);
                }
                else
                {
                    layoutNuevo = (1341, new[] { 512 }, -1125, 61);
                }

                // Actualizar tamaño de la pastilla
                signal += EventBuild("Pastilla", "PRIM_BAR_LEN[0]", $"{layoutNuevo.Size}", 2, 0.5, 0) + "\n";

                // Precompute mapping from sigla -> index dentro de los nuevos partidos
                var newActiveIndex = siglasNuevas
                    .Select((s, i) => new { Sigla = s, Index = i })
                    .ToDictionary(x => x.Sigla, x => x.Index);

                // Para cada partido, actualizar posiciones y visibilidad
                for (int idx = 0; idx < siglasPartidos.Count; idx++)
                {
                    var siglas = siglasPartidos[idx];

                    if (newActiveIndex.TryGetValue(siglas, out int newPosIndex) && newPosIndex >= 0 && newPosIndex < layoutNuevo.Positions.Length)
                    {
                        // Partido activo en el nuevo estado
                        int newPos = layoutNuevo.Positions[newPosIndex];
                        bool wasActive = siglasAnteriores.Contains(siglas);

                        // Primero hacer visible si no lo estaba
                        if (!wasActive)
                        {
                            signal += Oculta_Desoculta(false, $"Partidos/{siglas}") + "\n";
                        }

                        // Determinar si sube o baja de posición
                        int oldPosIndex = siglasAnteriores.IndexOf(siglas);
                        bool sube = oldPosIndex > newPosIndex; // índice menor = posición mejor

                        if (sube)
                        {
                            // El partido sube: pasar por encima (Y = -100)
                            signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[1]", "-100", 2, 0.25, 0) + "\n";
                            // Desplazar en X
                            signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0.25) + "\n";
                            // Volver a Y = 0
                            signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[1]", "0", 2, 0.25, 0.75) + "\n";
                        }
                        else
                        {
                            // El partido baja o no cambia: desplazamiento directo en X
                            signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0) + "\n";
                        }

                        // Actualizar offsets de Logo y Escaños
                        signal += EventBuild($"Partidos/{siglas}/Logo", "OBJ_DISPLACEMENT[0]", $"{layoutNuevo.LogoPos}", 1) + "\n";
                        signal += EventBuild($"Partidos/{siglas}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layoutNuevo.EscanosPos}", 1) + "\n";

                        // Actualizar texto de escaños
                        PartidoDTO temp = dtoNuevo.partidos.FirstOrDefault(x => x.siglas == siglas);
                        signal += EventBuild($"Escaños/{siglas}", "TEXT_STRING", $"{temp.escaniosHasta}", 2, 0.5, 0) + "\n";
                    }
                    else
                    {
                        // Partido no activo en el nuevo estado: ocultarlo con delay
                        signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[0]", "1920", 2, 0.5, 0) + "\n";
                        signal += Oculta_Desoculta(true, $"Partidos/{siglas}") + "\n";
                        signal += EventBuild($"Escaños/{siglas}", "TEXT_STRING", "0", 2, 0.5, 0) + "\n";
                    }
                }
            }
            else
            {
                // Si no cambia el número de partidos, solo actualizar posiciones de los que se han reordenado
                var layoutByCount = new Dictionary<int, (int Size, int[] Positions, int LogoPos, int EscanosPos)>()
                {
                    {1, (1341, new[] {512}, -1125, 61)},
                    {2, (660, new[] {170,855}, -675, -390)},
                    {3, (435, new[] {56,512,967}, -795, -279)},
                    {4, (320, new[] {0,341,683,1024}, -621, -441)},
                    {5, (255, new[] {-33,240,513,787,1060}, -591, -476)},
                    {6, (205, new[] {-59,169,397,626,854,1082}, -571, -495)},
                };

                (int Size, int[] Positions, int LogoPos, int EscanosPos) layout = layoutByCount[nNuevo];

                var newActiveIndex = siglasNuevas
                    .Select((s, i) => new { Sigla = s, Index = i })
                    .ToDictionary(x => x.Sigla, x => x.Index);

                for (int idx = 0; idx < siglasAnteriores.Count; idx++)
                {
                    var siglas = siglasAnteriores[idx];

                    if (newActiveIndex.TryGetValue(siglas, out int newPosIndex))
                    {
                        int newPos = layout.Positions[newPosIndex];
                        bool sube = idx > newPosIndex; // idx es oldPosIndex

                        if (sube)
                        {
                            // El partido sube: pasar por encima
                            signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[1]", "-100", 2, 0.25, 0) + "\n";
                            signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0.25) + "\n";
                            signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[1]", "0", 2, 0.25, 0.75) + "\n";
                        }
                        else if (idx != newPosIndex)
                        {
                            // El partido baja pero no cambia completamente
                            signal += EventBuild($"Partidos/{siglas}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0) + "\n";
                        }

                        // Actualizar escaños
                        PartidoDTO temp = dtoNuevo.partidos.FirstOrDefault(x => x.siglas == siglas);
                        signal += EventBuild($"Escaños/{siglas}", "TEXT_STRING", $"{temp.escaniosHasta}", 2, 0.5, 0) + "\n";
                    }
                }
            }

            return signal;
        }
        public string TickerTDSale()
        {
            string signal = "";
            signal += EventBuild("<>pipe", "PIPE_TYPE", "Perspective", 2, 0, 1);
            signal += EventBuild("cam1", "CAM_PV[1]", "-925", 2, 0, 1);
            //SALE
            return signal;
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
        public string fichaActualiza(bool oficiales, BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            string mode = oficiales ? "Oficiales" : "Sondeos";
            StringBuilder sb = new StringBuilder();

            // Determinar las siglas del nuevo DTO
            List<string> siglasNuevas = dtoNuevo.partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            List<string> siglasAnteriores = dtoAnterior.partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();

            // Sigla del partido que actualmente está mostrando
            string siglasYaDentro = siglas[indexCarrusel];
            int oldIndex = indexCarrusel;
            int newIndex = siglasNuevas.IndexOf(siglasYaDentro);

            // Si el partido actual ya no existe en el nuevo DTO, usar el primero del nuevo DTO
            if (newIndex == -1)
            {
                newIndex = 0;
                siglasYaDentro = siglasNuevas.Count > 0 ? siglasNuevas[0] : "";
            }

            // Determinar si es un cambio de posición significativo
            bool ganaposicion = oldIndex > newIndex; // índice menor = mejor posición
            bool pierdeposicion = oldIndex < newIndex;

            // Preparar todos los cartones fuera de pantalla (excepto los que van a ser animados)
            foreach (string sigla in siglasNuevas)
            {
                string pathPartido = $"Carton_Carrusel/{mode}/{sigla}";
                if (!sigla.Equals(siglasYaDentro))
                {
                    sb.Append(EventBuild(pathPartido, "OBJ_DISPLACEMENT[0]", "1920", 1));
                    sb.Append("\n");
                }
            }

            if (pierdeposicion)
            {
                // El partido pierde posición (baja en el ranking)
                // El que está en 0 sale hacia la derecha (1920)
                string pathSaliente = $"Carton_Carrusel/{mode}/{siglasYaDentro}";
                sb.Append(EventBuild(pathSaliente, "OBJ_DISPLACEMENT[0]", "1920", 2, 0.5, 0.0));
                sb.Append("\n");

                // El partido que ahora ocupa su posición entra de la derecha (de 1920 a 0)
                if (newIndex >= 0 && newIndex < siglasNuevas.Count)
                {
                    string siglaQueEntra = siglasNuevas[newIndex];
                    string pathEntrante = $"Carton_Carrusel/{mode}/{siglaQueEntra}";
                    sb.Append(EventBuild(pathEntrante, "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 0.0));
                    sb.Append("\n");
                }
            }
            else if (ganaposicion)
            {
                // El partido gana posición (sube en el ranking)
                // El que está en 0 sale hacia la izquierda (-1920) con Y = -0.3 para pasar por encima
                string pathSaliente = $"Carton_Carrusel/{mode}/{siglasYaDentro}";
                sb.Append(EventBuild(pathSaliente, "OBJ_DISPLACEMENT[1]", "-0.3", 2, 0.25, 0.0));
                sb.Append("\n");
                sb.Append(EventBuild(pathSaliente, "OBJ_DISPLACEMENT[0]", "-1920", 2, 0.5, 0.25));
                sb.Append("\n");

                // El partido que estaba en la posición anterior (newIndex + 1 en el orden anterior) entra
                int indexAnterior = newIndex + 1;
                if (indexAnterior >= 0 && indexAnterior < siglasAnteriores.Count)
                {
                    string siglasDelAnterior = siglasAnteriores[indexAnterior];
                    string pathEntrante = $"Carton_Carrusel/{mode}/{siglasDelAnterior}";

                    // El partido que entra también sube, así que anima Y a -0.3
                    sb.Append(EventBuild(pathEntrante, "OBJ_DISPLACEMENT[1]", "-0.3", 2, 0.25, 0.0));
                    sb.Append("\n");
                    sb.Append(EventBuild(pathEntrante, "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 0.25));
                    sb.Append("\n");
                    // Volver Y a 0
                    sb.Append(EventBuild(pathEntrante, "OBJ_DISPLACEMENT[1]", "0", 2, 0.25, 0.75));
                    sb.Append("\n");
                }

                // Volver Y a 0 del que sale
                sb.Append(EventBuild(pathSaliente, "OBJ_DISPLACEMENT[1]", "0", 2, 0.25, 0.75));
                sb.Append("\n");
            }

            // Finalizar con MAP_LLSTRING_LOAD
            string codigoMap = oficiales ? "Oficial_Codigo" : "Sondeo_Codigo";
            sb.Append(EventBuild(codigoMap, "MAP_LLSTRING_LOAD"));

            // ===== Actualizar atributos de estado =====
            // Actualizar la lista de siglas con las nuevas
            siglas = siglasNuevas;

            // Actualizar indexCarrusel con la nueva posición del partido que estaba dentro
            indexCarrusel = newIndex >= 0 ? newIndex : 0;

            // Actualizar fichaSeleccionada con el partido que ahora está dentro
            if (indexCarrusel >= 0 && indexCarrusel < dtoNuevo.partidos.Count)
            {
                fichaSeleccionada = dtoNuevo.partidos[indexCarrusel];
            }
            else
            {
                fichaSeleccionada = null;
            }

            return sb.ToString();
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

            // Helper: choose top party by escaños (escaniosHasta). On tie, compare percentage (try property "porcentaje").
            static string GetTopSiglaFromPartidos(List<PartidoDTO> partidos)
            {
                if (partidos == null || partidos.Count == 0) return "";

                // Find max escaños
                int maxEsc = partidos.Max(p => p.escaniosHasta);

                // Candidates with max escaños
                var candidates = partidos.Where(p => p.escaniosHasta == maxEsc).ToList();
                if (candidates.Count == 1)
                {
                    return candidates[0].siglas.Replace("+", "_").Replace("-", "_");
                }

                // Tie-breaker: highest percentage (try property names commonly used). Use reflection-safe read via SafeGetDouble.
                PartidoDTO best = candidates
                    .OrderByDescending(p => SafeGetDouble(p, "porcentaje", SafeGetDouble(p, "porcentajeVotos", SafeGetDouble(p, "votosPorcentaje", 0.0))))
                    .First();

                return best.siglas.Replace("+", "_").Replace("-", "_");
            }

            // Determine pages to show: 1 per up to 6, 2 for 7..12, 3 for >12
            var partidos = dto.partidos ?? new List<PartidoDTO>();
            int count = partidos.Count;
            int pagesToShow = count <= 6 ? 1 : (count <= 12 ? 2 : 3);

            // Paint maps instead of culling them.
            // If the circunscripción is an autonomía, try to get provinces; otherwise paint the single circunscripción map.
            if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo) && dto.circunscripcionDTO.codigo.EndsWith("00000"))
            {
                try
                {
                    using var con = new ConexionEntityFramework();
                    var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dto.circunscripcionDTO.nombre);
                    if (provincias != null && provincias.Count > 0)
                    {
                        // Compute top party for the autonomy once (fallback) and use it for provinces unless province-specific data is available later.
                        string topAutonomy = GetTopSiglaFromPartidos(partidos);

                        foreach (var prov in provincias)
                        {
                            // Paint the province with the most represented party.
                            // NOTE: if you later obtain province-specific results, replace topAutonomy with that result.
                            signal.Append(EventBuild($"Mayorias/{prov.nombre}", "MAT_LIST_COLOR", $"{topAutonomy}", 1) + "\n");
                        }
                    }
                    else
                    {
                        // Fallback: paint the autonomía map object itself with the top party
                        string top = GetTopSiglaFromPartidos(partidos);
                        signal.Append(EventBuild($"Mayorias/{dto.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", $"{top}", 1) + "\n");
                    }
                }
                catch (Exception)
                {
                    // On error fallback to painting the autonomía map object itself
                    string top = GetTopSiglaFromPartidos(partidos);
                    signal.Append(EventBuild($"Mayorias/{dto.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", $"{top}", 1) + "\n");
                }
            }
            else
            {
                // Not an autonomía: paint the single circunscripción selected
                string top = GetTopSiglaFromPartidos(partidos);
                signal.Append(EventBuild($"Mayorias/{dto.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", $"{top}", 1) + "\n");
            }

            // Position party fichas on the MAYORIAS map: positions 0, -100, -200, ...
            var siglas = partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            for (int i = 0; i < siglas.Count; i++)
            {
                int posicion = i * -100;
                signal.Append(EventBuild($"Mapa_Mayorias/Fichas/{siglas[i]}", "OBJ_DISPLACEMENT[2]", $"{posicion}", 1) + "\n");
                // Ensure the ficha is visible (we keep fichas visible as before)
                signal.Append(EventBuild($"Mapa_Mayorias/Fichas/{siglas[i]}", "OBJ_CULL", "0", 1) + "\n");
            }

            // Show/hide the page indicators according to number of partidos.
            // itemgo to animate OBJ_CULL to 0 (visible) or 1 (hidden) with 0.3s
            for (int page = 1; page <= 3; page++)
            {
                string value = page <= pagesToShow ? "0" : "1";
                signal.Append(EventBuild($"Mapa_Mayorias/Paginas/Pagina{page}", "OBJ_CULL", value, 2, 0.3, 0) + "\n");
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

        //PARTIDOS
        public string cartonPartidosEntra(BrainStormDTO dto)
        {
            if (dto == null) return "";

            StringBuilder sb = new StringBuilder();

            // Prepare carton
            sb.Append(Prepara("CARTON_PARTIDOS") + "\n");

            // Location text (example used Mayorias1/LugarTxt)
            sb.Append(EventBuild("Mayorias1/LugarTxt", "TEXT_STRING", $"{dto.circunscripcionDTO?.nombre ?? ""}", 1) + "\n");

            var partidos = dto.partidos ?? new List<PartidoDTO>();
            var siglas = partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();

            for (int i = 0; i < siglas.Count; i++)
            {
                string s = siglas[i];

                // X: first column x=0, second column starts at x=918 when i >= 6
                int x = i < 6 ? 0 : 918;

                // Y: step -0.2 per item (0, -0.2, -0.4, ...)
                string y = ((-0.2 * i).ToString("0.0", CultureInfo.InvariantCulture));

                // Z: first column -100 * index (0, -100, -200,...), second column resets to 0 for i=6
                int z = (i < 6) ? (-100 * i) : (-100 * (i - 6));

                // Compose tuple exactly as desired by the downstream script (preserve parentheses and commas)
                string tuple = $"({x},{y},{z})";

                sb.Append(EventBuild($"Carton_Partidos/Fichas/{s}", "OBJ_DISPLACEMENT", tuple, 1) + "\n");
                // Ensure visible
                sb.Append(EventBuild($"Carton_Partidos/Fichas/{s}", "OBJ_CULL", "0", 1) + "\n");
            }

            // Enter carton
            sb.Append(Entra("CARTON_PARTIDOS"));

            return sb.ToString();
        }
        public string cartonPartidosActualiza(BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            if (dtoNuevo == null) return "";

            StringBuilder sb = new StringBuilder();

            var anteriores = (dtoAnterior?.partidos ?? new List<PartidoDTO>())
                .Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();

            var nuevos = (dtoNuevo.partidos ?? new List<PartidoDTO>())
                .Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();

            // Animate each partido that remains or appears to its new target position (itemgo)
            for (int i = 0; i < nuevos.Count; i++)
            {
                string s = nuevos[i];

                int x = i < 6 ? 0 : 918;
                string y = ((-0.2 * i).ToString("0.0", CultureInfo.InvariantCulture));
                int z = (i < 6) ? (-100 * i) : (-100 * (i - 6));
                string tuple = $"({x},{y},{z})";

                // itemgo to new displacement (0.5s)
                sb.Append(EventBuild($"Carton_Partidos/Fichas/{s}", "OBJ_DISPLACEMENT", tuple, 2, 0.5, 0) + "\n");
                // ensure visible (instant)
                sb.Append(EventBuild($"Carton_Partidos/Fichas/{s}", "OBJ_CULL", "0", 1) + "\n");
            }

            // Any partido that existed before but no longer present -> animate out to the right and hide
            foreach (var s in anteriores.Except(nuevos))
            {
                // move out to the right and cull
                sb.Append(EventBuild($"Carton_Partidos/Fichas/{s}", "OBJ_DISPLACEMENT", "(1920,0,0)", 2, 0.5, 0) + "\n");
                sb.Append(EventBuild($"Carton_Partidos/Fichas/{s}", "OBJ_CULL", "1", 1) + "\n");
            }

            return sb.ToString();
        }
        public string cartonPartidosSale()
        {
            StringBuilder sb = new StringBuilder();

            // Try to get the master list of parties from the main DTO (fallback: do nothing if not available)
            try
            {
                var main = Application.Current.MainWindow as MainWindow;
                var todas = main?.dtoSinFiltrar?.partidos?.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList() ?? new List<string>();

                foreach (var s in todas)
                {
                    // animate out to the left and then the container will be hidden by the SALE call
                    sb.Append(EventBuild($"Carton_Partidos/Fichas/{s}", "OBJ_DISPLACEMENT", "(-1920,0,0)", 2, 0.5, 0) + "\n");
                }
            }
            catch
            {
                // ignore errors and proceed to issue the SALE
            }

            sb.Append(Sale("CARTON_PARTIDOS"));

            return sb.ToString();
        }

        //ULTIMO ESCAÑO
        // Constantes para el gráfico ULTIMO_ESCANO
        private const int TAMANO_MAXIMO_FICHA = 1756;
        private const int POS_INICIAL_IZQ = 90;
        private const int POS_INICIAL_DCH = 1844;
        private const int THRESHOLD_ANCHO_PEQUENO = 126;

        // Estado para ULTIMO_ESCANO
        private List<(bool esIzquierda, string siglas, int ancho)> ultimoEscanoPartidos = new();
        private int anchoAcumuladoIzq = 0;
        private int anchoAcumuladoDch = 0;
        private int escaniosAcumuladosIzq = 0;
        private int escaniosAcumuladosDch = 0;
        private string siglasUltimoEscano = "";
        private string siglasLuchaEscano = "";

        /// <summary>
        /// Prepara y entra el gráfico ULTIMO_ESCANO, mostrando los dos partidos que compiten por el último escaño.
        /// Solo maneja datos oficiales.
        /// </summary>
        public string ultimoEntra(BrainStormDTO dto)
        {
            if (dto == null) return "";

            StringBuilder signal = new StringBuilder();

            // Resetear estado
            ultimoEscanoPartidos.Clear();
            anchoAcumuladoIzq = 0;
            anchoAcumuladoDch = 0;
            escaniosAcumuladosIzq = 0;
            escaniosAcumuladosDch = 0;
            siglasUltimoEscano = "";
            siglasLuchaEscano = "";

            // Preparar
            signal.Append(Prepara("ULTIMO_ESCANO") + "\n");

            // Inicializar las 8 barras con ancho 0
            string[] barrasIzq = { "Barra_Izq", "Barra_Izq1", "Barra_Izq2", "Barra_Izq3" };
            string[] barrasDch = { "Barra_Dch", "Barra_Dch1", "Barra_Dch2", "Barra_Dch3" };

            foreach (var barra in barrasIzq)
            {
                signal.Append(EventBuild($"Ultimo_Escano/Barras/{barra}", "PRIM_RECGLO_LEN[0]", "0", 1) + "\n");
            }
            foreach (var barra in barrasDch)
            {
                signal.Append(EventBuild($"Ultimo_Escano/Barras/{barra}", "PRIM_RECGLO_LEN[0]", "0", 1) + "\n");
            }

            // Inicializar contadores de escaños
            signal.Append(EventBuild("Ultimo_Escano/Mayoria/Escanos_Izq", "TEXT_STRING", "0", 1) + "\n");
            signal.Append(EventBuild("Ultimo_Escano/Mayoria/Escanos_Dch", "TEXT_STRING", "0", 1) + "\n");

            // Establecer el nombre del lugar
            signal.Append(EventBuild("Ultimo_Escano/Lugar", "TEXT_STRING", $"{dto.circunscripcionDTO?.nombre ?? ""}", 1) + "\n");

            // Iluminar el mapa (similar a mayoriasEntra)
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
                            signal.Append(EventBuild($"CCAA_Carton/{prov.nombre}", "MAT_LIST_COLOR", "Blanco", 1) + "\n");
                        }
                    }
                    else
                    {
                        signal.Append(EventBuild($"CCAA_Carton/{dto.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "Blanco", 1) + "\n");
                    }
                }
                catch (Exception)
                {
                    signal.Append(EventBuild($"CCAA_Carton/{dto.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "Blanco", 1) + "\n");
                }
            }
            else if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.nombre))
            {
                signal.Append(EventBuild($"CCAA_Carton/{dto.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "Blanco", 1) + "\n");
            }

            // Obtener los datos de último escaño
            try
            {
                using var con = new ConexionEntityFramework();
                var cpList = CPController.GetInstance(con).FindByIdCircunscripcionOficial(dto.circunscripcionDTO.codigo);

                CircunscripcionPartido? cpUltimo = cpList?.FirstOrDefault(cp => cp.esUltimoEscano == 1);
                CircunscripcionPartido? cpLucha = cpList?.FirstOrDefault(cp => cp.luchaUltimoEscano == 1);
                CircunscripcionPartido? cpResto = cpList?.FirstOrDefault(cp => cp.restoVotos != -1);

                // Obtener siglas de los partidos
                if (cpUltimo != null)
                {
                    Partido? pUltimo = PartidoController.GetInstance(con).FindById(cpUltimo.codPartido);
                    if (pUltimo != null)
                    {
                        siglasUltimoEscano = pUltimo.siglas.Replace("+", "_").Replace("-", "_");
                        // El partido con último escaño: posición frontal, escala 1, escaño visible
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}", "OBJ_DISPLACEMENT", "(-94, 0, 326)", 1) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}", "OBJ_SCALE", "(1, 1, 1)", 1) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}/Escano", "OBJ_CULL", "false", 1) + "\n");
                    }
                }

                if (cpLucha != null)
                {
                    Partido? pLucha = PartidoController.GetInstance(con).FindById(cpLucha.codPartido);
                    if (pLucha != null)
                    {
                        siglasLuchaEscano = pLucha.siglas.Replace("+", "_").Replace("-", "_");
                        // El partido que lucha: posición atrás, escala menor, escaño oculto
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}", "OBJ_DISPLACEMENT", "(-154, 0.1, 0)", 1) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}", "OBJ_SCALE", "(0.86, 0.86, 0.86)", 1) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}/Escano", "OBJ_CULL", "true", 1) + "\n");
                    }
                }

                // Mostrar la diferencia de votos
                if (cpResto != null && cpResto.restoVotos != -1)
                {
                    signal.Append(EventBuild("Ultimo_Escano/Diferencia", "TEXT_STRING", $"{cpResto.restoVotos}", 1) + "\n");
                }
                else
                {
                    signal.Append(EventBuild("Ultimo_Escano/Diferencia", "TEXT_STRING", "", 1) + "\n");
                }
            }
            catch (Exception)
            {
                // Si hay error, continuar sin los datos de último escaño
            }

            // Entrar
            signal.Append(Entra("ULTIMO_ESCANO"));

            return signal.ToString();
        }
        /// <summary>
        /// Añade un partido al gráfico ULTIMO_ESCANO en el lado especificado.
        /// </summary>
        /// <param name="dto">DTO con los datos de la circunscripción</param>
        /// <param name="partido">Partido a añadir</param>
        /// <param name="esIzquierda">true para lado izquierdo, false para lado derecho</param>
        public string ultimoEntraPartido(BrainStormDTO dto, PartidoDTO partido, bool esIzquierda)
        {
            if (dto == null || partido == null) return "";

            StringBuilder signal = new StringBuilder();

            int escaniosTotales = dto.circunscripcionDTO?.escaniosTotales ?? 65;
            string siglas = partido.siglas.Replace("+", "_").Replace("-", "_");

            // Calcular ancho de la barra: (TAMANO_MAXIMO / escaniosTotales) * escaniosPartido
            int anchoPartido = (int)Math.Round((double)TAMANO_MAXIMO_FICHA / escaniosTotales * partido.escaniosHasta);

            // Determinar qué barra usar (máximo 4 por lado)
            string[] barrasIzq = { "Barra_Izq", "Barra_Izq1", "Barra_Izq2", "Barra_Izq3" };
            string[] barrasDch = { "Barra_Dch", "Barra_Dch1", "Barra_Dch2", "Barra_Dch3" };

            int indexPartido = ultimoEscanoPartidos.Count(p => p.esIzquierda == esIzquierda);
            if (indexPartido >= 4)
            {
                // Límite de 4 partidos por lado alcanzado
                return "";
            }

            string nombreBarra = esIzquierda ? barrasIzq[indexPartido] : barrasDch[indexPartido];

            // Calcular posición X
            int posX;
            if (esIzquierda)
            {
                posX = POS_INICIAL_IZQ + anchoAcumuladoIzq;
                anchoAcumuladoIzq += anchoPartido;
                escaniosAcumuladosIzq += partido.escaniosHasta;
            }
            else
            {
                posX = POS_INICIAL_DCH - anchoAcumuladoDch - anchoPartido;
                anchoAcumuladoDch += anchoPartido;
                escaniosAcumuladosDch += partido.escaniosHasta;
            }

            // Guardar en estado
            ultimoEscanoPartidos.Add((esIzquierda, siglas, anchoPartido));

            // Asignar color de la barra
            signal.Append(EventBuild($"Ultimo_Escano/Barras/{nombreBarra}", "MAT_LIST_COLOR", siglas, 1) + "\n");

            // Asignar posición X
            signal.Append(EventBuild($"Ultimo_Escano/Barras/{nombreBarra}", "OBJ_DISPLACEMENT[0]", $"{posX}", 1) + "\n");

            // Asignar ancho con animación
            signal.Append(EventBuild($"Ultimo_Escano/Barras/{nombreBarra}", "PRIM_RECGLO_LEN[0]", $"{anchoPartido}", 2, 0.5, 0) + "\n");

            // Actualizar contador de escaños
            if (esIzquierda)
            {
                signal.Append(EventBuild("Ultimo_Escano/Mayoria/Escanos_Izq", "TEXT_STRING", $"{escaniosAcumuladosIzq}", 2, 0.5, 0) + "\n");

                // Ajustar posición del texto si el primer partido de la izquierda tiene ancho pequeño
                if (indexPartido == 0)
                {
                    if (anchoPartido < THRESHOLD_ANCHO_PEQUENO)
                    {
                        signal.Append(EventBuild("OBJ_DISPLACEMENT3", "BIND_VOFFSET[0]", "120", 1) + "\n");
                    }
                    else
                    {
                        signal.Append(EventBuild("Ultimo_Escano/Mayoria_Absoluta/Barras/Barras_Izq", "OBJ_BBOX_LEN[0]", "126", 1) + "\n");
                    }
                }
            }
            else
            {
                signal.Append(EventBuild("Ultimo_Escano/Mayoria/Escanos_Dch", "TEXT_STRING", $"{escaniosAcumuladosDch}", 2, 0.5, 0) + "\n");

                // Ajustar posición del texto si el primer partido de la derecha tiene ancho pequeño
                if (indexPartido == 0)
                {
                    if (anchoPartido < THRESHOLD_ANCHO_PEQUENO)
                    {
                        signal.Append(EventBuild("OBJ_DISPLACEMENT4", "BIND_VOFFSET[0]", "-70", 1) + "\n");
                    }
                    else
                    {
                        signal.Append(EventBuild("OBJ_DISPLACEMENT4", "BIND_VOFFSET[0]", "46", 1) + "\n");
                    }
                }
            }

            return signal.ToString();
        }
        /// <summary>
        /// Actualiza el gráfico ULTIMO_ESCANO cuando cambian los datos.
        /// Detecta si los partidos que luchan se han intercambiado (el que tenía el escaño ahora lucha y viceversa).
        /// </summary>
        public string ultimoActualiza(BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            if (dtoNuevo == null) return "";

            StringBuilder signal = new StringBuilder();

            try
            {
                using var con = new ConexionEntityFramework();
                var cpList = CPController.GetInstance(con).FindByIdCircunscripcionOficial(dtoNuevo.circunscripcionDTO.codigo);

                CircunscripcionPartido? cpUltimoNuevo = cpList?.FirstOrDefault(cp => cp.esUltimoEscano == 1);
                CircunscripcionPartido? cpLuchaNuevo = cpList?.FirstOrDefault(cp => cp.luchaUltimoEscano == 1);
                CircunscripcionPartido? cpRestoNuevo = cpList?.FirstOrDefault(cp => cp.restoVotos != -1);

                string nuevoSiglasUltimo = "";
                string nuevoSiglasLucha = "";

                if (cpUltimoNuevo != null)
                {
                    Partido? p = PartidoController.GetInstance(con).FindById(cpUltimoNuevo.codPartido);
                    if (p != null) nuevoSiglasUltimo = p.siglas.Replace("+", "_").Replace("-", "_");
                }
                if (cpLuchaNuevo != null)
                {
                    Partido? p = PartidoController.GetInstance(con).FindById(cpLuchaNuevo.codPartido);
                    if (p != null) nuevoSiglasLucha = p.siglas.Replace("+", "_").Replace("-", "_");
                }

                // Detectar intercambio: el que tenía el escaño ahora lucha y viceversa
                bool intercambio = !string.IsNullOrEmpty(siglasUltimoEscano) &&
                                   !string.IsNullOrEmpty(siglasLuchaEscano) &&
                                   siglasUltimoEscano == nuevoSiglasLucha &&
                                   siglasLuchaEscano == nuevoSiglasUltimo;

                if (intercambio)
                {
                    // Animar el intercambio de posiciones (0.5 segundos)

                    // El que antes tenía el escaño (siglasUltimoEscano) ahora lucha -> mover atrás
                    signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}", "OBJ_DISPLACEMENT", "(-154, 0.1, 0)", 2, 0.5, 0) + "\n");
                    signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}", "OBJ_SCALE", "(0.86, 0.86, 0.86)", 2, 0.5, 0) + "\n");
                    signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}/Escano", "OBJ_CULL", "true", 2, 0.5, 0) + "\n");

                    // El que antes luchaba (siglasLuchaEscano) ahora tiene el escaño -> mover adelante
                    signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}", "OBJ_DISPLACEMENT", "(-94, 0, 326)", 2, 0.5, 0) + "\n");
                    signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}", "OBJ_SCALE", "(1, 1, 1)", 2, 0.5, 0) + "\n");
                    signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}/Escano", "OBJ_CULL", "false", 2, 0.5, 0) + "\n");

                    // Actualizar estado
                    siglasUltimoEscano = nuevoSiglasUltimo;
                    siglasLuchaEscano = nuevoSiglasLucha;
                }
                else if (nuevoSiglasUltimo != siglasUltimoEscano || nuevoSiglasLucha != siglasLuchaEscano)
                {
                    // Cambio diferente (no un simple intercambio), actualizar directamente
                    if (!string.IsNullOrEmpty(nuevoSiglasUltimo))
                    {
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{nuevoSiglasUltimo}", "OBJ_DISPLACEMENT", "(-94, 0, 326)", 2, 0.5, 0) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{nuevoSiglasUltimo}", "OBJ_SCALE", "(1, 1, 1)", 2, 0.5, 0) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{nuevoSiglasUltimo}/Escano", "OBJ_CULL", "false", 2, 0.5, 0) + "\n");
                    }
                    if (!string.IsNullOrEmpty(nuevoSiglasLucha))
                    {
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{nuevoSiglasLucha}", "OBJ_DISPLACEMENT", "(-154, 0.1, 0)", 2, 0.5, 0) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{nuevoSiglasLucha}", "OBJ_SCALE", "(0.86, 0.86, 0.86)", 2, 0.5, 0) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{nuevoSiglasLucha}/Escano", "OBJ_CULL", "true", 2, 0.5, 0) + "\n");
                    }

                    siglasUltimoEscano = nuevoSiglasUltimo;
                    siglasLuchaEscano = nuevoSiglasLucha;
                }

                // Actualizar diferencia de votos
                if (cpRestoNuevo != null && cpRestoNuevo.restoVotos != -1)
                {
                    signal.Append(EventBuild("Ultimo_Escano/Diferencia", "TEXT_STRING", $"{cpRestoNuevo.restoVotos}", 2, 0.5, 0) + "\n");
                }
            }
            catch (Exception)
            {
                // Si hay error, devolver vacío
            }

            return signal.ToString();
        }
        /// <summary>
        /// Encadena entre circunscripciones en el gráfico ULTIMO_ESCANO.
        /// Anima la salida, cambia los datos, y anima la entrada.
        /// </summary>
        /// <param name="dtoAnterior">DTO de la circunscripción saliente</param>
        /// <param name="dtoNuevo">DTO de la circunscripción entrante</param>
        public string ultimoEncadena(BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            if (dtoNuevo == null) return "";

            StringBuilder signal = new StringBuilder();

            // 1. Animar salida del contenedor Ultimo_Escano/Ultimo_Escano
            signal.Append(EventBuild("Ultimo_Escano/Ultimo_Escano", "OBJ_DISPLACEMENT[0]", "860", 2, 0.5, 0) + "\n");

            // 2. Resetear las barras (ancho a 0 y posición a valores por defecto) con animación
            string[] barrasIzq = { "Barra_Izq", "Barra_Izq1", "Barra_Izq2", "Barra_Izq3" };
            string[] barrasDch = { "Barra_Dch", "Barra_Dch1", "Barra_Dch2", "Barra_Dch3" };

            // Resetear barras izquierda: posición inicial 90
            for (int i = 0; i < barrasIzq.Length; i++)
            {
                signal.Append(EventBuild($"Ultimo_Escano/Barras/{barrasIzq[i]}", "PRIM_RECGLO_LEN[0]", "0", 2, 0.5, 0.5) + "\n");
                signal.Append(EventBuild($"Ultimo_Escano/Barras/{barrasIzq[i]}", "OBJ_DISPLACEMENT[0]", $"{POS_INICIAL_IZQ}", 2, 0.5, 0.5) + "\n");
            }

            // Resetear barras derecha: posición inicial 1844
            for (int i = 0; i < barrasDch.Length; i++)
            {
                signal.Append(EventBuild($"Ultimo_Escano/Barras/{barrasDch[i]}", "PRIM_RECGLO_LEN[0]", "0", 2, 0.5, 0.5) + "\n");
                signal.Append(EventBuild($"Ultimo_Escano/Barras/{barrasDch[i]}", "OBJ_DISPLACEMENT[0]", $"{POS_INICIAL_DCH}", 2, 0.5, 0.5) + "\n");
            }

            // 3. Resetear contadores de escaños a 0
            signal.Append(EventBuild("Ultimo_Escano/Mayoria/Escanos_Izq", "TEXT_STRING", "0", 2, 0.5, 0.5) + "\n");
            signal.Append(EventBuild("Ultimo_Escano/Mayoria/Escanos_Dch", "TEXT_STRING", "0", 2, 0.5, 0.5) + "\n");

            // 4. Actualizar mapas: poner las anteriores en "NoSel" y las nuevas en "Blanco"
            // Mapas de la circunscripción anterior -> NoSel
            if (dtoAnterior != null && !string.IsNullOrEmpty(dtoAnterior.circunscripcionDTO?.nombre))
            {
                if (!string.IsNullOrEmpty(dtoAnterior.circunscripcionDTO?.codigo) && dtoAnterior.circunscripcionDTO.codigo.EndsWith("00000"))
                {
                    try
                    {
                        using var con = new ConexionEntityFramework();
                        var provinciasAnt = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dtoAnterior.circunscripcionDTO.nombre);
                        if (provinciasAnt != null && provinciasAnt.Count > 0)
                        {
                            foreach (var prov in provinciasAnt)
                            {
                                signal.Append(EventBuild($"CCAA_Carton/{prov.nombre}", "MAT_LIST_COLOR", "NoSel", 2, 0.3, 0.5) + "\n");
                            }
                        }
                        else
                        {
                            signal.Append(EventBuild($"CCAA_Carton/{dtoAnterior.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "NoSel", 2, 0.3, 0.5) + "\n");
                        }
                    }
                    catch (Exception)
                    {
                        signal.Append(EventBuild($"CCAA_Carton/{dtoAnterior.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "NoSel", 2, 0.3, 0.5) + "\n");
                    }
                }
                else
                {
                    signal.Append(EventBuild($"CCAA_Carton/{dtoAnterior.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "NoSel", 2, 0.3, 0.5) + "\n");
                }
            }

            // Mapas de la circunscripción nueva -> Blanco
            if (!string.IsNullOrEmpty(dtoNuevo.circunscripcionDTO?.codigo) && dtoNuevo.circunscripcionDTO.codigo.EndsWith("00000"))
            {
                try
                {
                    using var con = new ConexionEntityFramework();
                    var provinciasNuevo = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dtoNuevo.circunscripcionDTO.nombre);
                    if (provinciasNuevo != null && provinciasNuevo.Count > 0)
                    {
                        foreach (var prov in provinciasNuevo)
                        {
                            signal.Append(EventBuild($"CCAA_Carton/{prov.nombre}", "MAT_LIST_COLOR", "Blanco", 2, 0.3, 0.5) + "\n");
                        }
                    }
                    else
                    {
                        signal.Append(EventBuild($"CCAA_Carton/{dtoNuevo.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "Blanco", 2, 0.3, 0.5) + "\n");
                    }
                }
                catch (Exception)
                {
                    signal.Append(EventBuild($"CCAA_Carton/{dtoNuevo.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "Blanco", 2, 0.3, 0.5) + "\n");
                }
            }
            else if (!string.IsNullOrEmpty(dtoNuevo.circunscripcionDTO?.nombre))
            {
                signal.Append(EventBuild($"CCAA_Carton/{dtoNuevo.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", "Blanco", 2, 0.3, 0.5) + "\n");
            }

            // 5. Actualizar el texto de la circunscripción
            signal.Append(EventBuild("Ultimo_Escano/Lugar", "TEXT_STRING", $"{dtoNuevo.circunscripcionDTO?.nombre ?? ""}", 2, 0.3, 0.5) + "\n");

            // 6. Cambiar los partidos de último escaño y lucha por último escaño
            try
            {
                using var con = new ConexionEntityFramework();
                var cpList = CPController.GetInstance(con).FindByIdCircunscripcionOficial(dtoNuevo.circunscripcionDTO.codigo);

                CircunscripcionPartido? cpUltimoNuevo = cpList?.FirstOrDefault(cp => cp.esUltimoEscano == 1);
                CircunscripcionPartido? cpLuchaNuevo = cpList?.FirstOrDefault(cp => cp.luchaUltimoEscano == 1);
                CircunscripcionPartido? cpRestoNuevo = cpList?.FirstOrDefault(cp => cp.restoVotos != -1);

                // Actualizar partido con último escaño
                if (cpUltimoNuevo != null)
                {
                    Partido? pUltimo = PartidoController.GetInstance(con).FindById(cpUltimoNuevo.codPartido);
                    if (pUltimo != null)
                    {
                        siglasUltimoEscano = pUltimo.siglas.Replace("+", "_").Replace("-", "_");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}", "OBJ_DISPLACEMENT", "(-94, 0, 326)", 1) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}", "OBJ_SCALE", "(1, 1, 1)", 1) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasUltimoEscano}/Escano", "OBJ_CULL", "false", 1) + "\n");
                    }
                }

                // Actualizar partido que lucha
                if (cpLuchaNuevo != null)
                {
                    Partido? pLucha = PartidoController.GetInstance(con).FindById(cpLuchaNuevo.codPartido);
                    if (pLucha != null)
                    {
                        siglasLuchaEscano = pLucha.siglas.Replace("+", "_").Replace("-", "_");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}", "OBJ_DISPLACEMENT", "(-154, 0.1, 0)", 1) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}", "OBJ_SCALE", "(0.86, 0.86, 0.86)", 1) + "\n");
                        signal.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasLuchaEscano}/Escano", "OBJ_CULL", "true", 1) + "\n");
                    }
                }

                // 7. Actualizar diferencia de votos
                if (cpRestoNuevo != null && cpRestoNuevo.restoVotos != -1)
                {
                    signal.Append(EventBuild("Ultimo_Escano/Diferencia", "TEXT_STRING", $"{cpRestoNuevo.restoVotos}", 1) + "\n");
                }
                else
                {
                    signal.Append(EventBuild("Ultimo_Escano/Diferencia", "TEXT_STRING", "", 1) + "\n");
                }
            }
            catch (Exception)
            {
                // Si hay error, continuar
            }

            // 8. Resetear estado de partidos añadidos
            ultimoEscanoPartidos.Clear();
            anchoAcumuladoIzq = 0;
            anchoAcumuladoDch = 0;
            escaniosAcumuladosIzq = 0;
            escaniosAcumuladosDch = 0;

            // 9. Animar entrada del contenedor Ultimo_Escano/Ultimo_Escano (después de 1 segundo para dar tiempo a los cambios)
            signal.Append(EventBuild("Ultimo_Escano/Ultimo_Escano", "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 1.0) + "\n");

            return signal.ToString();
        }
        /// <summary>
        /// Sale del gráfico ULTIMO_ESCANO y resetea el estado.
        /// </summary>
        public string ultimoSale()
        {
            // Resetear estado
            ultimoEscanoPartidos.Clear();
            anchoAcumuladoIzq = 0;
            anchoAcumuladoDch = 0;
            escaniosAcumuladosIzq = 0;
            escaniosAcumuladosDch = 0;
            siglasUltimoEscano = "";
            siglasLuchaEscano = "";

            return Sale("ULTIMO_ESCANO");
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
