using System.Text;
using System.Windows;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF.DTO;

namespace Elecciones.src.mensajes.builders
{
    /// <summary>
    /// Clase especializada para señales de Faldón (parte inferior de pantalla).
    /// Incluye: Ticker, Video, Pactos, Independentismo, Sedes, Reloj, etc.
    /// </summary>
    internal class FaldonMensajes : IPFMensajesBase
    {
        private static FaldonMensajes? instance;
        private bool animacionPrimeros = true;
        private bool animacionSondeo = true;

        private FaldonMensajes() : base() { }

        public static FaldonMensajes GetInstance()
        {
            if (instance == null)
            {
                instance = new FaldonMensajes();
            }
            return instance;
        }

        #region Cambio Sondeo/Oficial

        public string SondeoUOficial(bool oficiales)
        {
            return oficiales ? EventBuild("Sondeo_Oficiales", "MAP_INT_PAR", "1", 1) : EventBuild("Sondeo_Oficiales", "MAP_INT_PAR", "0", 1);
        }

        #endregion

        #region Animaciones

        public string PrimerosResultados(bool activo)
        {
            animacionPrimeros = activo;
            return activo ? EventBuild("PRIMEROS", "MAP_INT_PAR", "1", 1) : EventBuild("PRIMEROS", "MAP_INT_PAR", "0", 1);
        }

        public string AnimacionSondeo(bool activo)
        {
            animacionSondeo = activo;
            return activo ? EventBuild("SONDEO", "MAP_INT_PAR", "1", 1) : EventBuild("SONDEO", "MAP_INT_PAR", "0", 1);
        }

        public string RecibirPrimerosResultados()
        {
            return EventBuild("PRIMEROS", "MAP_INT_PAR", 3);
        }

        public string RecibirAnimacionSondeo()
        {
            return EventBuild("SONDEO", "MAP_INT_PAR", 3);
        }

        #endregion

        #region Proyección

        public string Proyeccion(bool activo)
        {
            return activo ? EventBuild("TICKER/PROYECCION", "MAP_INT_PAR", "1", 1) : EventBuild("TICKER/PROYECCION", "MAP_INT_PAR", "0", 1);
        }

        #endregion

        #region Giros

        public string DeSondeoAOficiales()
        {
            return EventRunBuild("SONDEOaOFICIALES");
        }

        #endregion

        #region Cambio de Elecciones

        public string CambioElecciones(bool europa)
        {
            return europa ? EventBuild("TICKER/EUROPA", "MAP_INT_PAR", "1", 1) : EventBuild("TICKER/EUROPA", "MAP_INT_PAR", "0", 1);
        }

        #endregion

        #region Reloj

        public string EntraReloj()
        {
            return EventRunBuild("EntraReloj");
        }

        public string SaleReloj()
        {
            return EventRunBuild("SaleReloj");
        }

        public string PreparaReloj(string time)
        {
            return EventBuild("OBJETO", "TEXT_STRING", time, 1);
        }

        #endregion

        #region Ticker

        double pxTotales = 1748;
        int margin = 10;
        int posicionInicial = 0;
        public string TickerEntra(bool oficiales, BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder sb = new StringBuilder();

            //TAMANO
            double tamanoFicha = (pxTotales - (margin * (dto.numPartidos - 1))) / dto.numPartidos;
            sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", tamanoFicha.ToString(), 1));

            //POSICION
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            for (int i = 0; i < dto.partidos.Count; i++)
            {
                string partidoId = (i + 1).ToString("D2");
                partidoIdMap[dto.partidos[i].codigo] = partidoId;
            }

            // Ordenar partidos según PartidoDTOComparer (por escaños descendente)
            List<PartidoDTO> partidosOrdenados = dto.partidos.OrderByDescending(p => oficiales ? p.escanios : p.escaniosHastaSondeo).ToList();

            // Calcular posición acumulativa para cada partido según el orden del comparador
            double posicionAcumulada = posicionInicial;
            for (int i = 0; i < partidosOrdenados.Count; i++)
            {
                PartidoDTO partido = partidosOrdenados[i];
                string partidoId = partidoIdMap[partido.codigo];

                // Asignar posición al partido
                sb.Append(EventBuild($"partido{partidoId}", "OBJ_DISPLACEMENT[0]", posicionAcumulada.ToString(), 1));

                // Acumular para el siguiente: posición actual + tamaño ficha + margen
                if (i < partidosOrdenados.Count - 1)
                {
                    posicionAcumulada += tamanoFicha + margin;
                }
            }

            return sb.ToString();
            //return oficial ? EventRunBuild("TICKER/ENTRA") : EventRunBuild("TICKER_SONDEO/ENTRA");
        }

        public string TickerEncadena(bool oficial)
        {
            return oficial ? Encadena("TICKER") : Encadena("TICKER_SONDEO");
        }

        public string TickerActualiza()
        {
            return EventRunBuild("TICKER/ACTUALIZO");
        }

        public string ActualizaPactometroFichas(BrainStormDTO dtoActualizado)
        {
            //TODO
            return EventRunBuild("PACTOMETRO/ACTUALIZA_FICHAS");
        }

        public string TickerSale(bool oficial)
        {
            return oficial ? EventRunBuild("TICKER/SALE") : EventRunBuild("TICKER_SONDEO/SALE");
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
                signal.Append(EventBuild($"TICKER/{part.padre}/HaCambiado", "MAP_INT_PAR", "1", 1));
            }
            return signal.ToString();
        }

        public string TickerYaNoEstaIndividualizado(List<PartidoDTO> partidos)
        {
            StringBuilder signal = new StringBuilder();
            List<string> codigos = partidos.Select(par => par.padre).ToList();
            codigos.ForEach(cod =>
            {
                signal.Append(EventBuild($"TICKER/{cod}/YaNoEsta", "MAP_INT_PAR", "1", 1));
                signal.Append(EventBuild($"TICKER/PARTIDOS/{cod}", "OBJ_CULL", "1", 2, 0.2, 0));
            });
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

        public string TickerVotosEntra(bool oficial)
        {
            return oficial ? EventRunBuild("TICKER/VOTOS/ENTRA") : EventRunBuild("TICKER_SONDEO/VOTOS/ENTRA");
        }

        public string TickerVotosSale(bool oficial)
        {
            return oficial ? EventRunBuild("TICKER/VOTOS/SALE") : EventRunBuild("TICKER_SONDEO/VOTOS/SALE");
        }

        public string TickerHistoricosEntra(bool oficial)
        {
            return oficial ? EventRunBuild("TICKER/HISTORICOS/ENTRA") : EventRunBuild("TICKER_SONDEO/HISTORICOS/ENTRA");
        }

        public string TickerHistoricosSale(bool oficial)
        {
            return oficial ? EventRunBuild("TICKER/HISTORICOS/SALE") : EventRunBuild("TICKER_SONDEO/HISTORICOS/SALE");
        }

        public string TickerMillonesEntra()
        {
            return EventRunBuild("TICKER/MILLONES/ENTRA");
        }

        public string TickerMillonesSale()
        {
            return EventRunBuild("TICKER/MILLONES/SALE");
        }

        public string TickerFotosEntra()
        {
            return EventRunBuild("TICKER/FOTOS/ENTRA");
        }

        public string TickerFotosSale()
        {
            return EventRunBuild("TICKER/FOTOS/SALE");
        }

        #endregion

        #region Video

        public string VideoIn(BrainStormDTO dto, PartidoDTO partidoSeleccionado)
        {
            string signal = "";
            signal += EventBuild($"TICKER/FNC_Video{partidoSeleccionado.codigo}", "MAP_EXE");
            return signal;
        }

        public string VideoOut(BrainStormDTO dto, PartidoDTO partidoSeleccionado)
        {
            string signal = "";
            signal += EventBuild($"TICKER/FNC_CierroVideo{partidoSeleccionado.codigo}", "MAP_EXE");
            return signal;
        }

        public string VideoOutTodos(BrainStormDTO dto)
        {
            string signal = "";
            foreach (var partido in dto.partidos)
            {
                signal += EventBuild($"TICKER/FNC_CierroVideo{partido.codigo}", "MAP_EXE") + "\n";
            }
            return signal;
        }

        public string VideoInTodos(BrainStormDTO dto)
        {
            string signal = "";
            foreach (var partido in dto.partidos)
            {
                signal += EventBuild($"TICKER/FNC_Video{partido.codigo}", "MAP_EXE") + "\n";
            }
            return signal;
        }

        #endregion

        #region Ticker TD

        public string TickerTDEntra(BrainStormDTO dto)
        {
            List<string> siglasPartidos = dto.partidos.Select(x => x.siglas).ToList();
            List<string> siglasActivos = dto.partidos.Select(x => x.siglas).ToList();
            string signal = "";
            signal += EventBuild("NumeroEscrutado", "TEXT_STRING", $"'{dto.circunscripcionDTO.escrutado}%'", 2, 0.5, 0) + "\n";

            int n = dto.partidos?.Count ?? 0;

            var layoutByCount = new Dictionary<int, (int Size, int[] Positions, int LogoPos, int EscanosPos)>()
            {
                {1, (1341, new[] {512}, -1125, 61)},
                {2, (660, new[] {170,855}, -765, -300)},
                {3, (435, new[] {56,512,967}, -675, -400)},
                {4, (320, new[] {0,341,683,1024}, -621, -441)},
                {5, (255, new[] {-33,240,513,787,1060}, -591, -476)},
                {6, (205, new[] {-59,169,397,626,854,1082}, -571, -495)},
            };

            var layout = layoutByCount.ContainsKey(n) ? layoutByCount[n] : layoutByCount[1];
            signal += EventBuild("Pastilla", "PRIM_BAR_LEN[0]", $"{layout.Size}", 2, 0.5, 0) + "\n";

            var activeIndex = siglasActivos
                .Select((s, i) => new { Sigla = s, Index = i })
                .ToDictionary(x => x.Sigla, x => x.Index);

            static string Esc(string s) => s?.Replace("+", "_") ?? s;

            for (int idx = 0; idx < siglasPartidos.Count; idx++)
            {
                var siglaRaw = siglasPartidos[idx];
                var siglaObj = Esc(siglaRaw);

                if (activeIndex.TryGetValue(siglaRaw, out int posIndex) && posIndex >= 0 && posIndex < layout.Positions.Length)
                {
                    int pos = layout.Positions[posIndex];
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{pos}", 2, 0.5, 0) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Logo", "OBJ_DISPLACEMENT[0]", $"{layout.LogoPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layout.EscanosPos}", 1) + "\n";
                }
                else
                {
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", "1920", 2, 0.5, 0) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Logo", "OBJ_DISPLACEMENT[0]", $"{layout.LogoPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layout.EscanosPos}", 1) + "\n";
                }
            }

            foreach (var siglaRaw in siglasPartidos)
            {
                var siglaObj = Esc(siglaRaw);

                if (siglasActivos.Contains(siglaRaw))
                {
                    PartidoDTO temp = dto.partidos.FirstOrDefault(x => x.siglas == siglaRaw);
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_CULL", "0", 2, 0.2, 0) + "\n";
                    signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{temp.escanios}'", 2, 0.5, 0) + "\n";
                }
                else
                {
                    signal += Oculta_Desoculta(true, $"Partidos/{siglaObj}") + "\n";
                    signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'0'", 2, 0.5, 0) + "\n";
                }
            }

            signal += Entra("FALDON_TD");
            return signal;
        }

        public string TickerTDActualiza(BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            var main = Application.Current.MainWindow as MainWindow;
            List<string> siglasPartidos = main.dto.partidos.Select(x => x.siglas).ToList();
            List<string> siglasNuevas = dtoNuevo.partidos.Select(x => x.siglas).ToList();
            List<string> siglasAnteriores = dtoAnterior.partidos.Select(x => x.siglas).ToList();

            string signal = "";
            signal += EventBuild("NumeroEscrutado", "TEXT_STRING", $"'{dtoNuevo.circunscripcionDTO.escrutado}%'", 2, 0.5, 0) + "\n";

            int nAnterior = dtoAnterior.partidos?.Count ?? 0;
            int nNuevo = dtoNuevo.partidos?.Count ?? 0;

            static string Esc(string s) => s?.Replace("+", "_") ?? s;

            var siglasQueSalen = siglasAnteriores.Except(siglasNuevas).ToList();
            var siglasQueEntran = siglasNuevas.Except(siglasAnteriores).ToList();

            if (nAnterior != nNuevo)
            {
                var layoutByCount = new Dictionary<int, (int Size, int[] Positions, int LogoPos, int EscanosPos)>()
                {
                    {1, (1341, new[] {512}, -1125, 61)},
                    {2, (660, new[] {170,855}, -765, -300)},
                    {3, (435, new[] {56,512,967}, -675, -400)},
                    {4, (320, new[] {0,341,683,1024}, -621, -441)},
                    {5, (255, new[] {-33,240,513,787,1060}, -591, -476)},
                    {6, (205, new[] {-59,169,397,626,854,1082}, -571, -495)},
                };

                var layoutNuevo = layoutByCount.ContainsKey(nNuevo) ? layoutByCount[nNuevo] : layoutByCount[1];
                signal += EventBuild("Pastilla", "PRIM_BAR_LEN[0]", $"{layoutNuevo.Size}", 2, 0.5, 0) + "\n";

                var newActiveIndex = siglasNuevas
                    .Select((s, i) => new { Sigla = s, Index = i })
                    .ToDictionary(x => x.Sigla, x => x.Index);

                for (int idx = 0; idx < siglasPartidos.Count; idx++)
                {
                    var siglaRaw = siglasPartidos[idx];
                    var siglaObj = Esc(siglaRaw);

                    if (newActiveIndex.TryGetValue(siglaRaw, out int newPosIndex) && newPosIndex >= 0 && newPosIndex < layoutNuevo.Positions.Length)
                    {
                        int newPos = layoutNuevo.Positions[newPosIndex];
                        bool wasActive = siglasAnteriores.Contains(siglaRaw);

                        if (!wasActive)
                        {
                            signal += Oculta_Desoculta(false, $"Partidos/{siglaObj}") + "\n";
                        }

                        int oldPosIndex = siglasAnteriores.IndexOf(siglaRaw);
                        bool sube = oldPosIndex > newPosIndex;

                        if (sube)
                        {
                            signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[1]", "-100", 2, 0.25, 0) + "\n";
                            signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0.25) + "\n";
                            signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[1]", "0", 2, 0.25, 0.75) + "\n";
                        }
                        else
                        {
                            signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0) + "\n";
                        }

                        signal += EventBuild($"Partidos/{siglaObj}/Logo", "OBJ_DISPLACEMENT[0]", $"{layoutNuevo.LogoPos}", 1) + "\n";
                        signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layoutNuevo.EscanosPos}", 1) + "\n";

                        PartidoDTO temp = dtoNuevo.partidos.FirstOrDefault(x => x.siglas == siglaRaw);
                        signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{temp.escanios}'", 2, 0.5, 0) + "\n";
                    }
                    else
                    {
                        signal += EventBuild($"Partidos/{Esc(siglaRaw)}", "OBJ_DISPLACEMENT[0]", "1920", 2, 0.5, 0) + "\n";
                        signal += EventBuild($"Partidos/{Esc(siglaRaw)}", "OBJ_CULL", "1", 2, 0.3, 0) + "\n";
                        signal += EventBuild($"Escaños/{Esc(siglaRaw)}", "TEXT_STRING", "'0'", 2, 0.5, 0) + "\n";
                    }
                }
            }
            else
            {
                var layoutByCount = new Dictionary<int, (int Size, int[] Positions, int LogoPos, int EscanosPos)>()
                {
                    {1, (1341, new[] {512}, -1125, 61)},
                    {2, (660, new[] {170,855}, -765, -300)},
                    {3, (435, new[] {56,512,967}, -675, -400)},
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
                    var siglaRaw = siglasAnteriores[idx];
                    var siglaObj = Esc(siglaRaw);

                    if (newActiveIndex.TryGetValue(siglaRaw, out int newPosIndex))
                    {
                        int newPos = layout.Positions[newPosIndex];
                        bool sube = idx > newPosIndex;

                        if (sube)
                        {
                            signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[1]", "-100", 2, 0.25, 0) + "\n";
                            signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0.25) + "\n";
                            signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[1]", "0", 2, 0.25, 0.75) + "\n";
                        }
                        else if (idx != newPosIndex)
                        {
                            signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0) + "\n";
                        }

                        PartidoDTO temp = dtoNuevo.partidos.FirstOrDefault(x => x.siglas == siglaRaw);
                        signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{temp.escanios}'", 2, 0.5, 0) + "\n";
                    }
                }

                foreach (var siglaRaw in siglasQueSalen)
                {
                    var siglaObj = Esc(siglaRaw);
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", "1920", 2, 0.5, 0) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_CULL", "1", 2, 0.3, 0) + "\n";
                    signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", "'0'", 2, 0.5, 0) + "\n";
                }

                foreach (var siglaRaw in siglasQueEntran)
                {
                    var siglaObj = Esc(siglaRaw);
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_CULL", "0", 1) + "\n";

                    if (newActiveIndex.TryGetValue(siglaRaw, out int newPosIndex) && newPosIndex >= 0)
                    {
                        var layout2 = layoutByCount[nNuevo];
                        int newPos = layout2.Positions[newPosIndex];
                        signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0) + "\n";

                        PartidoDTO temp = dtoNuevo.partidos.FirstOrDefault(x => x.siglas == siglaRaw);
                        signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{temp.escanios}'", 2, 0.5, 0) + "\n";
                    }
                }
            }

            return signal;
        }

        public string TickerTDSale()
        {
            return Sale("FALDON_TD");
        }

        #endregion

        #region PP_PSOE

        public string PP_PSOEEntra()
        {
            return Entra("TICKER/PP_PSOE");
        }

        public string PP_PSOESale()
        {
            return Sale("TICKER/PP_PSOE");
        }

        #endregion

        #region Despliegas

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

        #endregion

        #region Pactos Faldón

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

        #endregion

        #region Sedes Faldón

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
            return Sale("SEDES");
        }

     

        #endregion
    }
}
