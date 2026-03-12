using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Elecciones.src.controller;
using Elecciones.src.logic.comparators;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.DTO.Cartones;
using Elecciones.src.model.IPF;
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
        private List<string> partidosExpandidos = new List<string>();

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
            return "";
        }

        #endregion

        #region Animaciones

        public string PrimerosResultados(bool activo)
        {
            animacionPrimeros = activo;
            return "";
        }

        public string AnimacionSondeo(bool activo)
        {
            animacionSondeo = activo;
            return "";
        }

        public string RecibirPrimerosResultados()
        {
            return "";
        }

        public string RecibirAnimacionSondeo()
        {
            return "";
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
            return "";
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
            return EventRunBuild("CuentaAtras/Entra");
        }

        public string SaleReloj()
        {
            return EventRunBuild("CuentaAtras/Sale");
        }

        public string PreparaReloj(int time)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventBuild("CuentaAtras", "TIMER_LENGTH", time.ToString(), 1));
            sb.Append(EventRunBuild("CuentaAtras/PonerEnInicio"));
            sb.Append(EventRunBuild("CuentaAtras/Entra"));
            sb.Append(EventRunBuild("CuentaAtras/Play"));
            return sb.ToString();
        }

        #endregion

        #region Ticker

        double pxTotales = 1748;
        int margin = 10;
        int posicionInicial = 0;

        private double GetTamanoFichaTicker(int partidosActivosCount)
        {
            int countSeguro = Math.Max(partidosActivosCount, 1);
            return (pxTotales - (margin * (countSeguro - 1))) / countSeguro;
        }

        private void RecolocarPartidosNoActivosAlFinal(
            StringBuilder sb,
            string tipo,
            Dictionary<string, string> partidoIdMap,
            List<PartidoDTO> partidosActivos,
            double posicionAcumuladaFinal,
            double tamanoFicha,
            int modo,
            double duracion = 0,
            double delay = 0)
        {
            if (sb == null || string.IsNullOrWhiteSpace(tipo) || partidoIdMap == null || partidoIdMap.Count == 0)
            {
                return;
            }

            HashSet<string> codigosActivos = new HashSet<string>(
                (partidosActivos ?? new List<PartidoDTO>())
                .Where(p => !string.IsNullOrWhiteSpace(p.codigo))
                .Select(p => p.codigo));

            // Colocar los partidos no activos a la derecha del ultimo activo.
            double posicionBaseNoActivos = posicionAcumuladaFinal + tamanoFicha;
            int offset = 0;

            foreach (var kvp in partidoIdMap.OrderBy(x => int.TryParse(x.Value, out int n) ? n : int.MaxValue))
            {
                if (codigosActivos.Contains(kvp.Key))
                {
                    continue;
                }

                double posicion = posicionBaseNoActivos + (offset * (tamanoFicha + margin));
                string pathSignal = $"Graficos/{tipo}/partidos/partido{kvp.Value}";

                if (modo == 1)
                {
                    sb.Append(EventBuild(pathSignal, "OBJ_DISPLACEMENT[0]", posicion.ToString(), 1));
                }
                else
                {
                    sb.Append(EventBuild(pathSignal, "OBJ_DISPLACEMENT[0]", posicion.ToString(), 2, duracion, delay));
                }

                offset++;
            }
        }

        private static readonly Dictionary<int, (int Size, int[] Positions, int LogoPos, int EscanosPos)> tickerTDLayouts = new()
        {
            {1, (1341, new[] {512}, -1125, 61)},
            {2, (660, new[] {170,855}, -765, -300)},
            {3, (435, new[] {56,512,967}, -675, -400)},
            {4, (320, new[] {0,341,683,1024}, -621, -441)},
            {5, (255, new[] {-33,240,513,787,1060}, -591, -476)},
            {6, (205, new[] {-59,169,397,626,854,1082}, -571, -495)},
            {7, (170, new[] {-35,150,335,520,705,890,1075}, -554, -474)},
            {8, (157, new[] {-73,95,263,431,599,767,935,1103}, -554, -475)},
            {9, (131, new[] {-63,82,227,372,517,662,807,952,1097}, -545, -464)},
        };

        private static (int Size, int[] Positions, int LogoPos, int EscanosPos) GetTickerTDLayout(int partidosActivos)
        {
            int countSeguro = Math.Max(partidosActivos, 1);
            return tickerTDLayouts.TryGetValue(countSeguro, out var layout) ? layout : tickerTDLayouts[1];
        }

        private int GetTickerTDPosicionNoActivosBase((int Size, int[] Positions, int LogoPos, int EscanosPos) layout, int partidosActivos)
        {
            if (layout.Positions == null || layout.Positions.Length == 0)
            {
                return layout.Size;
            }

            int indiceUltimoActivo = partidosActivos > 0
                ? Math.Min(partidosActivos - 1, layout.Positions.Length - 1)
                : layout.Positions.Length - 1;

            return layout.Positions[indiceUltimoActivo] + layout.Size;
        }

        private int GetTickerTDPasoNoActivos((int Size, int[] Positions, int LogoPos, int EscanosPos) layout)
        {
            return layout.Size + margin;
        }

        public string TickerEntra(bool oficiales, BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder sb = new StringBuilder();

            List<PartidoDTO> partidosOrdenadosPorComparer = dto.partidos.ToList();
            partidosOrdenadosPorComparer.Sort(new PartidoDTOComparerUnified(oficiales));
            partidosOrdenadosPorComparer.Reverse();

            List<PartidoDTO> partidosActivos = partidosOrdenadosPorComparer
                .Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0)
                .ToList();

            if (animacionPrimeros)
            {
                sb.Append(EventRunBuild("PrimerosResultados/Entra"));
            }

            double tamanoFicha = GetTamanoFichaTicker(partidosActivos.Count);
            sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", tamanoFicha.ToString(), 1));

            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(dto);
            double posicionAcumulada = posicionInicial;
            string tipo = oficiales ? "Escrutinio" : "Sondeo";

            for (int i = 0; i < partidosActivos.Count; i++)
            {
                PartidoDTO partido = partidosActivos[i];
                string partidoId = GetPartidoId(partidoIdMap, partido.codigo);

                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{partidoId}", "OBJ_DISPLACEMENT[0]", posicionAcumulada.ToString(), 1));

                if (i < partidosActivos.Count - 1)
                {
                    posicionAcumulada += tamanoFicha + margin;
                }
            }

            RecolocarPartidosNoActivosAlFinal(sb, tipo, partidoIdMap, partidosActivos, posicionAcumulada, tamanoFicha, 1);

            if (partidosActivos.Count > 6)
            {
                sb.Append(EventRunBuild("SaleFoto"));
            }

            sb.Append(EventRunBuild($"{tipo}/Entra"));
            return sb.ToString();
        }

        public string TickerEncadena(bool oficiales)
        {
            var main = Application.Current.MainWindow as MainWindow;
            var dto = main?.dto;
            if (dto == null) return "";
            StringBuilder sb = new StringBuilder();

            List<PartidoDTO> partidosOrdenadosPorComparer = dto.partidos.ToList();
            partidosOrdenadosPorComparer.Sort(new PartidoDTOComparerUnified(oficiales));
            partidosOrdenadosPorComparer.Reverse();

            List<PartidoDTO> partidosActivos = partidosOrdenadosPorComparer
                .Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0)
                .ToList();

            double tamanoFicha = GetTamanoFichaTicker(partidosActivos.Count);
            sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", tamanoFicha.ToString(), 2, 0.6, 0));

            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(dto);
            double posicionAcumulada = posicionInicial;
            string tipo = oficiales ? "Escrutinio" : "Sondeo";

            for (int i = 0; i < partidosActivos.Count; i++)
            {
                PartidoDTO partido = partidosActivos[i];
                string partidoId = GetPartidoId(partidoIdMap, partido.codigo);

                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{partidoId}", "OBJ_DISPLACEMENT[0]", posicionAcumulada.ToString(), 2, 0.6, 0));

                if (i < partidosActivos.Count - 1)
                {
                    posicionAcumulada += tamanoFicha + margin;
                }
            }

            RecolocarPartidosNoActivosAlFinal(sb, tipo, partidoIdMap, partidosActivos, posicionAcumulada, tamanoFicha, 2, 0.6, 0);

            if (partidosActivos.Count > 6)
            {
                sb.Append(EventRunBuild("SaleFoto"));
            }

            sb.Append(EventRunBuild($"{tipo}/Actualiza"));
            return sb.ToString();
        }

        public string TickerActualiza(BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder sb = new StringBuilder();
            string tipo = dto.oficiales ? "Escrutinio" : "Sondeo";

            if (partidosExpandidos.Count > 0)
            {
                sb.Append(EventRunBuild($"{tipo}/Actualiza"));
                return sb.ToString();
            }

            List<PartidoDTO> partidosOrdenadosPorComparer = dto.partidos.ToList();
            partidosOrdenadosPorComparer.Sort(new PartidoDTOComparerUnified(dto.oficiales));
            partidosOrdenadosPorComparer.Reverse();

            List<PartidoDTO> partidosActivos = partidosOrdenadosPorComparer
                .Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0)
                .ToList();

            double tamanoFicha = GetTamanoFichaTicker(partidosActivos.Count);
            sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", tamanoFicha.ToString(), 2, 0.3, 0));

            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(dto);
            double posicionAcumulada = posicionInicial;

            for (int i = 0; i < partidosActivos.Count; i++)
            {
                PartidoDTO partido = partidosActivos[i];
                string partidoId = GetPartidoId(partidoIdMap, partido.codigo);

                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{partidoId}", "OBJ_DISPLACEMENT[0]", posicionAcumulada.ToString(), 2, 0.3, 0));

                if (i < partidosActivos.Count - 1)
                {
                    posicionAcumulada += tamanoFicha + margin;
                }
            }

            RecolocarPartidosNoActivosAlFinal(sb, tipo, partidoIdMap, partidosActivos, posicionAcumulada, tamanoFicha, 2, 0.3, 0);

            if (partidosActivos.Count > 6)
            {
                sb.Append(EventRunBuild("SaleFoto"));
            }

            sb.Append(EventRunBuild($"{tipo}/Actualiza"));
            return sb.ToString();
        }

        public string TickerSale(bool oficial, BrainStormDTO dto = null)
        {
            StringBuilder sb = new StringBuilder();
            string tipo = oficial ? "Escrutinio" : "Sondeo";

            // Si hay partidos expandidos, cerrarlos todos antes de salir
            if (partidosExpandidos.Count > 0 && dto != null)
            {
                sb.Append(VideoOutTodos(dto));
            }

            sb.Append(EventRunBuild($"{tipo}/Sale"));
            return sb.ToString();
        }


        public string TickerVotosEntra(bool oficial)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventBuild("Datos", "MAP_STRING_PAR", "DiferenciaSale", 1));
            sb.Append(EventBuild("Datos", "MAP_STRING_PAR", "PorcentajitoEntra", 1));
            return sb.ToString();
        }

        public string TickerHistoricosEntra(bool oficial)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventBuild("Datos", "MAP_STRING_PAR", "PorcentajitoSale", 1));
            sb.Append(EventBuild("Datos", "MAP_STRING_PAR", "DiferenciaEntra", 1));
            return sb.ToString();
        }

        //NO SE USA EN ARAGÓN
        public string TickerVotosSale(bool oficial)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventBuild("Datos", "MAP_STRING_PAR", "PorcentajitoSale", 1));
            sb.Append(EventBuild("Datos", "MAP_STRING_PAR", "DiferenciaEntra", 1));
            return sb.ToString();
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
            return EventRunBuild("EntraFoto");
        }

        public string TickerFotosSale()
        {
            return EventRunBuild("SaleFoto");
        }

        #endregion

        #region Video

        public string VideoIn(BrainStormDTO dto, PartidoDTO partidoSeleccionado)
        {
            if (dto == null || partidoSeleccionado == null) return "";
            StringBuilder sb = new StringBuilder();

            // 1. Gestionar lista de partidos expandidos
            if (!partidosExpandidos.Contains(partidoSeleccionado.codigo))
            {
                partidosExpandidos.Add(partidoSeleccionado.codigo);
            }

            // 2. Filtrar partidos activos
            var partidosActivos = dto.partidos
                .Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0)
                .ToList();

            if (!partidosActivos.Any(p => p.codigo == partidoSeleccionado.codigo)) return "";

            // 3. Ordenar
            partidosActivos.Sort(new PartidoDTOComparerUnified(dto.oficiales));
            partidosActivos.Reverse(); // Descendente

            // Mapa IDs
            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(dto);

            int count = partidosActivos.Count;
            if (count == 0) return "";

            // 4. Cálculos de ancho
            // pxTotales y margin definidos en la clase
            double totalWidthAvailable = pxTotales - (margin * (count - 1));
            // double normalWidth = totalWidthAvailable / count; // No usamos normalWidth estático ya
            double minWidth = pxTotales / 5.0;

            // Contar cuántos partidos ACTIVOS están expandidos
            int numExpandidos = partidosActivos.Count(p => partidosExpandidos.Contains(p.codigo));

            // Lógica de anchos dinámica
            double widthOthers;

            if (numExpandidos > 0)
            {
                double widthRequiredForExpanded = numExpandidos * minWidth;
                double remainingSpace = totalWidthAvailable - widthRequiredForExpanded;

                int numOthers = count - numExpandidos;

                if (numOthers > 0)
                {
                    widthOthers = remainingSpace / numOthers;
                }
                else
                {
                    // Todos están expandidos (o intentan estarlo). 
                    widthOthers = 0;
                }
            }
            else
            {
                // Ninguno expandido
                widthOthers = totalWidthAvailable / count;
            }

            // 5. Posicionamiento
            double posicionAcumulada = posicionInicial;
            string tipo = dto.oficiales ? "Escrutinio" : "Sondeo";

            for (int i = 0; i < partidosActivos.Count; i++)
            {
                PartidoDTO partido = partidosActivos[i];
                string sceneObjectId = GetPartidoId(partidoIdMap, partido.codigo);

                double currentWidth;
                double scaleX = 1.0;
                double scaleZ = 1.0;

                if (partidosExpandidos.Contains(partido.codigo))
                {
                    currentWidth = minWidth; // Este es el ancho expandido (fijo)

                    // Escala X: calculada dinámicamente para que el tamaño visual sea siempre minWidth
                    // La fórmula minWidth / widthOthers garantiza que:
                    // - baseWidth (widthOthers) * scaleX = minWidth (tamaño visual constante)
                    // Esto funciona igual para 1 o múltiples expandidos
                    if (widthOthers > 0 && count > 4)
                    {
                        scaleX = minWidth / widthOthers;
                    }
                    else
                    {
                        // Para 4 o menos partidos, no hace falta escalar
                        scaleX = 1.0;
                    }

                    // Escala Y (OBJ_SCALE[2]) siempre 1.4 cuando está expandido
                    scaleZ = 1.4;
                }
                else
                {
                    currentWidth = widthOthers; // Este es el ancho reducido
                    scaleX = 1.0;
                    scaleZ = 1.0;
                }

                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}",
                                     "OBJ_DISPLACEMENT[0]",
                                     posicionAcumulada.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                     2, 0.5, 0) + "\n");

                // Aplicar escala específica a la ficha de este partido
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/fichaPartido", "OBJ_SCALE[0]", scaleX.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/fichaPartido", "OBJ_SCALE[2]", scaleZ.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");

                posicionAcumulada += currentWidth + margin;

                // Ajustes visuales para TODOS los partidos expandidos (para mantener alineación al cambiar escala)
                if (partidosExpandidos.Contains(partido.codigo))
                {
                    // CÁLCULO DINÁMICO DE POSICIONES DE TEXTO (Empírico: 150 - widthOthers)
                    double posEscanios = 150 - widthOthers;
                    double posPorcentaje = posEscanios + 40;
                    double posDiferencia = posEscanios + 4;

                    //TEXTO DIRECTO: Siempre en 40 si está expandido
                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DirectoMascara/Directo", "OBJ_DISPLACEMENT[2]", "40", 2, 0.5, 0) + "\n");

                    //CAMBIO POSICION TEXTOS Y LOGO
                    sb.Append(EventBuild($"{sceneObjectId}/Escanios", "TEXT_BLOCK_HOTPOINT[0]", posEscanios.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");
                    sb.Append(EventBuild($"{sceneObjectId}/Porcentaje1", "TEXT_BLOCK_HOTPOINT[0]", posPorcentaje.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");
                    sb.Append(EventBuild($"{sceneObjectId}/Diferencia", "TEXT_BLOCK_HOTPOINT[0]", posDiferencia.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");

                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DatosG{sceneObjectId}", "OBJ_DISPLACEMENT[2]", "25", 2, 0.5, 0) + "\n");
                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DatosP{sceneObjectId}", "OBJ_DISPLACEMENT[2]", "25", 2, 0.5, 0) + "\n");

                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/Logo", "OBJ_DISPLACEMENT[2]", "25", 2, 0.5, 0) + "\n");
                }

                // Solo si es el partido seleccionado, lanzamos el video
                if (partido.codigo == partidoSeleccionado.codigo)
                {
                    //ENTRA VIDEO
                    sb.Append(Entra($"VIDEOS/{sceneObjectId}"));
                }
            }

            // Aplicar tamaño base a las fichas no expandidas.
            // Asumimos que el usuario manejará el escalado de las expandidas, 
            // así que establecemos el tamaño base para las "normales/reducidas".
            double baseWidth = (numExpandidos == count) ? minWidth : widthOthers;
            if (count > 4)
            {
                sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", baseWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");
            }
            sb.Append(EventRunBuild("Titular/Ocultar") + "\n");
            return sb.ToString();
        }

        public string VideoOut(BrainStormDTO dto, PartidoDTO partidoSeleccionado)
        {
            if (dto == null || partidoSeleccionado == null) return "";
            StringBuilder sb = new StringBuilder();

            // 1. Quitar el partido de la lista de expandidos
            if (partidosExpandidos.Contains(partidoSeleccionado.codigo))
            {
                partidosExpandidos.Remove(partidoSeleccionado.codigo);
            }

            // 2. Filtrar partidos activos
            var partidosActivos = dto.partidos
                .Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0)
                .ToList();

            if (partidosActivos.Count == 0) return "";

            // 3. Ordenar
            partidosActivos.Sort(new PartidoDTOComparerUnified(dto.oficiales));
            partidosActivos.Reverse(); // Descendente

            // Mapa IDs
            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(dto);

            int count = partidosActivos.Count;
            string tipo = dto.oficiales ? "Escrutinio" : "Sondeo";

            // 4. Cálculos de ancho (igual que VideoIn, pero con el partido ya quitado de expandidos)
            double totalWidthAvailable = pxTotales - (margin * (count - 1));
            double minWidth = pxTotales / 5.0;

            // Contar cuántos partidos ACTIVOS siguen expandidos
            int numExpandidos = partidosActivos.Count(p => partidosExpandidos.Contains(p.codigo));

            double widthOthers;
            if (numExpandidos > 0)
            {
                double widthRequiredForExpanded = numExpandidos * minWidth;
                double remainingSpace = totalWidthAvailable - widthRequiredForExpanded;
                int numOthers = count - numExpandidos;
                widthOthers = numOthers > 0 ? remainingSpace / numOthers : 0;
            }
            else
            {
                // Ninguno expandido: todos vuelven al tamaño normal
                widthOthers = totalWidthAvailable / count;
                sb.Append(EventRunBuild("Titular/Recuperar") + "\n");
            }

            // 5. Posicionamiento y escalado
            double posicionAcumulada = posicionInicial;

            for (int i = 0; i < partidosActivos.Count; i++)
            {
                PartidoDTO partido = partidosActivos[i];
                string sceneObjectId = GetPartidoId(partidoIdMap, partido.codigo);

                double currentWidth;
                double scaleX = 1.0;
                double scaleZ = 1.0;

                if (partidosExpandidos.Contains(partido.codigo))
                {
                    // Este partido sigue expandido
                    currentWidth = minWidth;
                    if (widthOthers > 0 && count > 4)
                    {
                        scaleX = minWidth / widthOthers;
                    }
                    scaleZ = 1.4;
                }
                else
                {
                    // Este partido NO está expandido (incluye el que acabamos de cerrar)
                    currentWidth = widthOthers;
                    scaleX = 1.0;
                    scaleZ = 1.0;
                }

                // Posición
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}",
                                     "OBJ_DISPLACEMENT[0]",
                                     posicionAcumulada.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                     2, 0.5, 0) + "\n");

                // Escala
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/fichaPartido",
                    "OBJ_SCALE[0]", scaleX.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0.3) + "\n");
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/fichaPartido",
                    "OBJ_SCALE[2]", scaleZ.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0.3) + "\n");

                posicionAcumulada += currentWidth + margin;

                // Ajustes visuales para TODOS los partidos QUE QUEDAN expandidos
                if (partidosExpandidos.Contains(partido.codigo))
                {
                    // CÁLCULO DINÁMICO DE POSICIONES DE TEXTO (Empírico: 150 - widthOthers)
                    double posEscanios = 150 - widthOthers;
                    double posPorcentaje = posEscanios + 40;
                    double posDiferencia = posEscanios + 4;

                    //TEXTO DIRECTO
                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DirectoMascara/Directo", "OBJ_DISPLACEMENT[2]", "40", 2, 0.5, 0) + "\n");

                    //CAMBIO POSICION TEXTOS Y LOGO
                    sb.Append(EventBuild($"{sceneObjectId}/Escanios", "TEXT_BLOCK_HOTPOINT[0]", posEscanios.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");
                    sb.Append(EventBuild($"{sceneObjectId}/Porcentaje1", "TEXT_BLOCK_HOTPOINT[0]", posPorcentaje.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");
                    sb.Append(EventBuild($"{sceneObjectId}/Diferencia", "TEXT_BLOCK_HOTPOINT[0]", posDiferencia.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");

                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DatosG{sceneObjectId}", "OBJ_DISPLACEMENT[2]", "25", 2, 0.5, 0) + "\n");
                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DatosP{sceneObjectId}", "OBJ_DISPLACEMENT[2]", "25", 2, 0.5, 0) + "\n");

                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/Logo", "OBJ_DISPLACEMENT[2]", "25", 2, 0.5, 0) + "\n");
                }

                if (partido.codigo == partidoSeleccionado.codigo)
                {
                    //ENTRA VIDEO (Reset a 0 porque sale)
                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DirectoMascara/Directo", "OBJ_DISPLACEMENT[2]", "0", 2, 0.5, 0) + "\n");
                    //CAMBIO POSICION TEXTOS
                    sb.Append(EventBuild($"{sceneObjectId}/Escanios", "TEXT_BLOCK_HOTPOINT[0]", "0", 2, 0.3, 0.3) + "\n");
                    sb.Append(EventBuild($"{sceneObjectId}/Porcentaje1", "TEXT_BLOCK_HOTPOINT[0]", "0", 2, 0.3, 0.3) + "\n");
                    sb.Append(EventBuild($"{sceneObjectId}/Diferencia", "TEXT_BLOCK_HOTPOINT[0]", "0", 2, 0.3, 0.3) + "\n");

                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DatosG{sceneObjectId}", "OBJ_DISPLACEMENT[2]", "-7", 2, 0.3, 0.3) + "\n");
                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DatosP{sceneObjectId}", "OBJ_DISPLACEMENT[2]", "-7", 2, 0.3, 0.3) + "\n");

                    sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/Logo", "OBJ_DISPLACEMENT[2]", "-15", 2, 0.3, 0.3) + "\n");

                    //SALE VIDEO
                    sb.Append(Sale($"VIDEOS/{sceneObjectId}"));
                }
            }

            // Tamaño base para fichas no expandidas
            double baseWidth = (numExpandidos == count) ? minWidth : widthOthers;
            if (count > 4)
            {
                sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", baseWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");
            }
            return sb.ToString();
        }

        public string VideoOutTodos(BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder sb = new StringBuilder();

            // 1. Limpiar todos los expandidos
            partidosExpandidos.Clear();

            // 2. Filtrar partidos activos
            var partidosActivos = dto.partidos
                .Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0)
                .ToList();

            if (partidosActivos.Count == 0) return "";

            // 3. Ordenar
            partidosActivos.Sort(new PartidoDTOComparerUnified(dto.oficiales));
            partidosActivos.Reverse(); // Descendente

            // Mapa IDs
            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(dto);

            int count = partidosActivos.Count;
            string tipo = dto.oficiales ? "Escrutinio" : "Sondeo";

            // 4. Cálculos de ancho (todos vuelven al tamaño normal)
            double totalWidthAvailable = pxTotales - (margin * (count - 1));
            double normalWidth = totalWidthAvailable / count;

            // 5. Posicionamiento y reseteo de escalas
            double posicionAcumulada = posicionInicial;

            for (int i = 0; i < partidosActivos.Count; i++)
            {
                PartidoDTO partido = partidosActivos[i];
                string sceneObjectId = GetPartidoId(partidoIdMap, partido.codigo);

                // Posición
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}",
                                     "OBJ_DISPLACEMENT[0]",
                                     posicionAcumulada.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                     2, 0.5, 0) + "\n");

                // Resetear escala a (1, 1)
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/fichaPartido",
                    "OBJ_SCALE[0]", "1", 2, 0.5, 0.3) + "\n");
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/fichaPartido",
                    "OBJ_SCALE[2]", "1", 2, 0.5, 0.3) + "\n");

                posicionAcumulada += normalWidth + margin;

                // Resetear texto Directo (como en VideoOut)
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DirectoMascara/Directo", "OBJ_DISPLACEMENT[2]", "0", 2, 0.5, 0) + "\n");

                // Resetear posición de textos (como en VideoOut)
                sb.Append(EventBuild($"{sceneObjectId}/Escanios", "TEXT_BLOCK_HOTPOINT[0]", "0", 2, 0.3, 0.3) + "\n");
                sb.Append(EventBuild($"{sceneObjectId}/Porcentaje1", "TEXT_BLOCK_HOTPOINT[0]", "0", 2, 0.3, 0.3) + "\n");
                sb.Append(EventBuild($"{sceneObjectId}/Diferencia", "TEXT_BLOCK_HOTPOINT[0]", "0", 2, 0.3, 0.3) + "\n");

                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DatosG{sceneObjectId}", "OBJ_DISPLACEMENT[2]", "-7", 2, 0.3, 0.3) + "\n");
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/DatosP{sceneObjectId}", "OBJ_DISPLACEMENT[2]", "-7", 2, 0.3, 0.3) + "\n");

                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{sceneObjectId}/Logo", "OBJ_DISPLACEMENT[2]", "-15", 2, 0.3, 0.3) + "\n");
                // Salir video de este partido
                sb.Append(Sale($"VIDEOS/{sceneObjectId}"));
            }

            // Tamaño base para todas las fichas (todas iguales)
            if (count > 4)
            {
                sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", normalWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), 2, 0.5, 0) + "\n");
            }

            // Recuperar titular (como cuando ninguno está expandido en VideoOut)
            sb.Append(EventRunBuild("Titular/Recuperar") + "\n");

            return sb.ToString();
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
            // Ordenar partidos segun PartidoDTOComparerUnified (por escanios/votos descendente)
            List<PartidoDTO> partidosActivos = dto.partidos.ToList();
            partidosActivos.Sort(new PartidoDTOComparerUnified(true));
            partidosActivos.Reverse(); // Descendente
            partidosActivos = partidosActivos.Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();

            List<string> siglasPartidos = GetPartidosBaseParaIds(dto)
                .Select(x => x.siglas)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            foreach (string sigla in dto.partidos.Select(x => x.siglas).Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                if (!siglasPartidos.Contains(sigla))
                {
                    siglasPartidos.Add(sigla);
                }
            }

            List<string> siglasActivos = partidosActivos.Select(x => x.siglas).ToList();
            HashSet<string> siglasActivasSet = new HashSet<string>(siglasActivos);

            string signal = "";
            signal += EventBuild("NumeroEscrutado", "TEXT_STRING", $"'{dto.circunscripcionDTO.escrutado}%'", 2, 0.5, 0) + "\n";

            int n = partidosActivos?.Count ?? 0;
            var layout = GetTickerTDLayout(n);
            signal += EventBuild("Pastilla", "PRIM_BAR_LEN[0]", $"{layout.Size}", 2, 0.5, 0) + "\n";

            int posicionNoActivosBase = GetTickerTDPosicionNoActivosBase(layout, n);
            int pasoNoActivos = GetTickerTDPasoNoActivos(layout);
            int offsetNoActivos = 0;

            var activeIndex = siglasActivos
                .Select((s, i) => new { Sigla = s, Index = i })
                .ToDictionary(x => x.Sigla, x => x.Index);

            static string Esc(string s) => s?.Replace("+", "_").Replace("-", "_") ?? s;

            for (int idx = 0; idx < siglasPartidos.Count; idx++)
            {
                var siglaRaw = siglasPartidos[idx];
                var siglaObj = Esc(siglaRaw);

                // Determinar escala segun numero de partidos
                string escala = n <= 7 ? "(1,1,1)" : (n == 8 ? "(0.9,0.9,0.9)" : "(0.75,0.75,0.75)");

                if (activeIndex.TryGetValue(siglaRaw, out int posIndex) && posIndex >= 0 && posIndex < layout.Positions.Length)
                {
                    int pos = layout.Positions[posIndex];
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{pos}", 2, 0.5, 0) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Logo", "OBJ_DISPLACEMENT[0]", $"{layout.LogoPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layout.EscanosPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_SCALE", escala, 1) + "\n";
                }
                else
                {
                    int posicionNoActivo = posicionNoActivosBase + (offsetNoActivos * pasoNoActivos);
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{posicionNoActivo}", 2, 0.5, 0) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Logo", "OBJ_DISPLACEMENT[0]", $"{layout.LogoPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layout.EscanosPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_SCALE", escala, 1) + "\n";
                    offsetNoActivos++;
                }
            }

            foreach (var siglaRaw in siglasPartidos)
            {
                var siglaObj = Esc(siglaRaw);

                if (siglasActivasSet.Contains(siglaRaw))
                {
                    PartidoDTO temp = dto.partidos.FirstOrDefault(x => x.siglas == siglaRaw);
                    int escanios = temp?.escanios ?? 0;
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_CULL", "0", 2, 0.2, 0) + "\n";
                    signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{escanios}'", 2, 0.5, 0) + "\n";
                }
                else
                {
                    signal += Oculta_Desoculta(true, $"Partidos/{siglaObj}") + "\n";
                    signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", "'0'", 2, 0.5, 0) + "\n";
                }
            }

            signal += Entra("FALDON_TD");
            return signal;
        }

        public string TickerTDActualiza(BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            List<PartidoDTO> partidosActuales = dtoAnterior.partidos.ToList();
            partidosActuales.Sort(new PartidoDTOComparerUnified(true));
            partidosActuales.Reverse(); // Descendente
            partidosActuales = partidosActuales.Where(p => dtoAnterior.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();

            List<PartidoDTO> partidosActivos = dtoNuevo.partidos.ToList();
            partidosActivos.Sort(new PartidoDTOComparerUnified(true));
            partidosActivos.Reverse(); // Descendente
            partidosActivos = partidosActivos.Where(p => dtoNuevo.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();

            List<string> siglasPartidos = GetPartidosBaseParaIds(dtoNuevo)
                .Select(x => x.siglas)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            foreach (string sigla in dtoAnterior.partidos.Select(x => x.siglas).Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                if (!siglasPartidos.Contains(sigla))
                {
                    siglasPartidos.Add(sigla);
                }
            }

            foreach (string sigla in dtoNuevo.partidos.Select(x => x.siglas).Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                if (!siglasPartidos.Contains(sigla))
                {
                    siglasPartidos.Add(sigla);
                }
            }

            List<string> siglasNuevas = partidosActivos.Select(x => x.siglas).ToList();
            List<string> siglasAnteriores = partidosActuales.Select(x => x.siglas).ToList();
            HashSet<string> siglasAnterioresSet = new HashSet<string>(siglasAnteriores);

            string signal = "";
            signal += EventBuild("NumeroEscrutado", "TEXT_STRING", $"'{dtoNuevo.circunscripcionDTO.escrutado}%'", 2, 0.5, 0) + "\n";

            int nAnterior = partidosActuales?.Count ?? 0;
            int nNuevo = partidosActivos?.Count ?? 0;

            static string Esc(string s) => s?.Replace("+", "_").Replace("-", "_") ?? s;

            var siglasQueSalen = siglasAnteriores.Except(siglasNuevas).ToList();
            var siglasQueEntran = siglasNuevas.Except(siglasAnteriores).ToList();

            var layoutNuevo = GetTickerTDLayout(nNuevo);

            if (nAnterior != nNuevo)
            {
                signal += EventBuild("Pastilla", "PRIM_BAR_LEN[0]", $"{layoutNuevo.Size}", 2, 0.5, 0) + "\n";

                int posicionNoActivosBase = GetTickerTDPosicionNoActivosBase(layoutNuevo, nNuevo);
                int pasoNoActivos = GetTickerTDPasoNoActivos(layoutNuevo);
                int offsetNoActivos = 0;

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
                        bool wasActive = siglasAnterioresSet.Contains(siglaRaw);

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

                        // Determinar escala segun numero de partidos
                        string escalaNuevo = nNuevo <= 7 ? "(1,1,1)" : (nNuevo == 8 ? "(0.9,0.9,0.9)" : "(0.75,0.75,0.75)");
                        signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_SCALE", escalaNuevo, 2, 0.5, 0) + "\n";

                        PartidoDTO temp = dtoNuevo.partidos.FirstOrDefault(x => x.siglas == siglaRaw);
                        int escanios = temp?.escanios ?? 0;
                        signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{escanios}'", 2, 0.5, 0) + "\n";
                        signal += EventBuild($"Partidos/{siglaObj}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n";
                    }
                    else
                    {
                        int posicionNoActivo = posicionNoActivosBase + (offsetNoActivos * pasoNoActivos);
                        signal += EventBuild($"Partidos/{Esc(siglaRaw)}", "OBJ_DISPLACEMENT[0]", $"{posicionNoActivo}", 2, 0.5, 0) + "\n";
                        signal += EventBuild($"Partidos/{Esc(siglaRaw)}", "OBJ_CULL", "1", 2, 0.3, 0) + "\n";
                        signal += EventBuild($"Escaños/{Esc(siglaRaw)}", "TEXT_STRING", "'0'", 2, 0.5, 0) + "\n";
                        offsetNoActivos++;
                    }
                }
            }
            else
            {
                (int Size, int[] Positions, int LogoPos, int EscanosPos) layout = layoutNuevo;

                int posicionNoActivosBase = GetTickerTDPosicionNoActivosBase(layout, nNuevo);
                int pasoNoActivos = GetTickerTDPasoNoActivos(layout);
                int offsetNoActivos = 0;

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
                        int escanios = temp?.escanios ?? 0;
                        signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{escanios}'", 2, 0.5, 0) + "\n";
                    }
                }

                foreach (var siglaRaw in siglasQueSalen)
                {
                    var siglaObj = Esc(siglaRaw);
                    int posicionNoActivo = posicionNoActivosBase + (offsetNoActivos * pasoNoActivos);
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{posicionNoActivo}", 2, 0.5, 0) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_CULL", "1", 2, 0.3, 0) + "\n";
                    signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", "'0'", 2, 0.5, 0) + "\n";
                    offsetNoActivos++;
                }

                foreach (var siglaRaw in siglasQueEntran)
                {
                    var siglaObj = Esc(siglaRaw);
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_CULL", "0", 1) + "\n";

                    if (newActiveIndex.TryGetValue(siglaRaw, out int newPosIndex) && newPosIndex >= 0)
                    {
                        int newPos = layout.Positions[newPosIndex];
                        signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", $"{newPos}", 2, 0.5, 0) + "\n";

                        PartidoDTO temp = dtoNuevo.partidos.FirstOrDefault(x => x.siglas == siglaRaw);
                        int escanios = temp?.escanios ?? 0;
                        signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{escanios}'", 2, 0.5, 0) + "\n";
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

        #region Pactos Faldón

        public string pactosEntra()
        {
            bool oficiales = ResolvePactosOficiales();
            return EventRunBuild($"{GetPactometroEventRoot(oficiales)}/Entra");
        }

        public string pactosReinicio()
        {
            bool oficiales = ResolvePactosOficiales();
            string eventRoot = GetPactometroEventRoot(oficiales);
            ResetPactosState();

            StringBuilder sb = new StringBuilder();
            sb.Append(EventRunBuild($"{eventRoot}/reinicioPactometroIzq"));
            sb.Append(EventRunBuild($"{eventRoot}/reinicioPactometroDer"));
            return sb.ToString();
        }

        public string pactosSale()
        {
            bool oficiales = ResolvePactosOficiales();
            string eventRoot = GetPactometroEventRoot(oficiales);
            ResetPactosState();

            StringBuilder sb = new StringBuilder();
            sb.Append(EventRunBuild($"{eventRoot}/Sale"));
            sb.Append(EventRunBuild($"{eventRoot}/reinicioPactometroIzq"));
            sb.Append(EventRunBuild($"{eventRoot}/reinicioPactometroDer"));
            return sb.ToString();
        }

        private readonly double pxTotalesPacto = 1748;
        private bool pactoOficiales = true;
        private double acumuladoIzqDesde = 0;
        private double acumuladoIzqHasta = 0;
        private double acumuladoDchaDesde = 0;
        private double acumuladoDchaHasta = 0;
        private int acumuladoEscanosDerDesde = 0;
        private int acumuladoEscanosDerHasta = 0;
        private int acumuladoEscanosIzqDesde = 0;
        private int acumuladoEscanosIzqHasta = 0;

        private readonly List<string> partidosEnPactoDerecha = new List<string>();
        private readonly List<string> partidosEnPactoIzquierda = new List<string>();

        private static BrainStormDTO? GetMainDto()
        {
            var main = Application.Current.MainWindow as MainWindow;
            return main?.dto;
        }

        private bool ResolvePactosOficiales(BrainStormDTO? dto = null)
        {
            if (dto != null)
            {
                pactoOficiales = dto.oficiales;
                return pactoOficiales;
            }

            var mainDto = GetMainDto();
            if (mainDto != null)
            {
                pactoOficiales = mainDto.oficiales;
            }

            return pactoOficiales;
        }

        private static string GetPactometroEventRoot(bool oficiales)
        {
            return oficiales ? "Pactometro" : "PactometroSondeo";
        }

        private static string GetPactometroSceneRoot(bool oficiales)
        {
            return oficiales ? "Graficos/Pactometro" : "Graficos/PactometroSondeo";
        }

        private static string ResolveCodigoPlantilla(string codigoCircunscripcion)
        {
            if (string.IsNullOrWhiteSpace(codigoCircunscripcion))
            {
                return "";
            }

            if (codigoCircunscripcion.EndsWith("00000"))
            {
                return codigoCircunscripcion;
            }

            if (codigoCircunscripcion.StartsWith("99"))
            {
                return "9900000";
            }

            if (codigoCircunscripcion.Length >= 2)
            {
                return $"{codigoCircunscripcion.Substring(0, 2)}00000";
            }

            return codigoCircunscripcion;
        }

        private static List<PartidoDTO> GetPartidosBaseParaIds(BrainStormDTO dto)
        {
            if (dto == null || dto.partidos == null)
            {
                return new List<PartidoDTO>();
            }

            var main = Application.Current.MainWindow as MainWindow;
            if (main?.conexionActiva == null)
            {
                return dto.partidos;
            }

            string codigoPlantilla = ResolveCodigoPlantilla(dto.circunscripcionDTO?.codigo ?? "");
            if (string.IsNullOrWhiteSpace(codigoPlantilla))
            {
                return dto.partidos;
            }

            try
            {
                int avance = dto.circunscripcionDTO?.numAvance > 0 ? dto.circunscripcionDTO.numAvance : 1;
                int tipoElecciones = codigoPlantilla.StartsWith("99") ? 1 : 2;
                BrainStormController controller = BrainStormController.GetInstance(main.conexionActiva);

                BrainStormDTO plantilla = dto.oficiales
                    ? controller.FindByIdCircunscripcionOficialSinFiltrar(codigoPlantilla, avance, tipoElecciones)
                    : controller.FindByIdCircunscripcionSondeoSinFiltrar(codigoPlantilla, avance, tipoElecciones);

                if (plantilla?.partidos != null && plantilla.partidos.Count > 0)
                {
                    return plantilla.partidos;
                }
            }
            catch
            {
                // Fallback silencioso a la lista local si no se puede resolver la plantilla.
            }

            return dto.partidos;
        }

        private static Dictionary<string, string> BuildPartidoIdMap(BrainStormDTO dto)
        {
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            int nextIndex = 1;

            List<PartidoDTO> partidosBaseOrdenados = GetPartidosBaseParaIds(dto)
                .Where(p => !string.IsNullOrWhiteSpace(p.codigo))
                .GroupBy(p => p.codigo)
                .Select(g => g.First())
                .OrderBy(p => p.codigo)
                .ToList();

            foreach (PartidoDTO partido in partidosBaseOrdenados)
            {
                partidoIdMap[partido.codigo] = nextIndex.ToString("D2");
                nextIndex++;
            }

            List<PartidoDTO> partidosActualesOrdenados = (dto.partidos ?? new List<PartidoDTO>())
                .Where(p => !string.IsNullOrWhiteSpace(p.codigo))
                .GroupBy(p => p.codigo)
                .Select(g => g.First())
                .OrderBy(p => p.codigo)
                .ToList();

            foreach (PartidoDTO partido in partidosActualesOrdenados)
            {
                if (!partidoIdMap.ContainsKey(partido.codigo))
                {
                    partidoIdMap[partido.codigo] = nextIndex.ToString("D2");
                    nextIndex++;
                }
            }

            return partidoIdMap;
        }

        private static string GetPartidoId(Dictionary<string, string> partidoIdMap, string codigoPartido)
        {
            return !string.IsNullOrWhiteSpace(codigoPartido) && partidoIdMap.TryGetValue(codigoPartido, out string? id)
                ? id
                : "00";
        }

        private static int SafeEscanosTotales(BrainStormDTO dto)
        {
            return dto.circunscripcionDTO.escaniosTotales > 0 ? dto.circunscripcionDTO.escaniosTotales : 1;
        }

        private static double CalcAnchoPacto(int escanos, int totalEscanos, double pxTotales)
        {
            return (escanos * pxTotales) / totalEscanos;
        }

        private void ResetPactosState()
        {
            acumuladoIzqDesde = 0;
            acumuladoIzqHasta = 0;
            acumuladoDchaDesde = 0;
            acumuladoDchaHasta = 0;
            acumuladoEscanosDerDesde = 0;
            acumuladoEscanosDerHasta = 0;
            acumuladoEscanosIzqDesde = 0;
            acumuladoEscanosIzqHasta = 0;
            partidosEnPactoDerecha.Clear();
            partidosEnPactoIzquierda.Clear();
        }

        private void AppendPactoValoresYBarras(StringBuilder sb, bool oficiales, bool izquierda)
        {
            if (izquierda)
            {
                if (oficiales)
                {
                    sb.Append(EventBuild("Pactometro_IzqVALOR", "MAP_INT_PAR", $"{acumuladoEscanosIzqDesde}", 1));
                    sb.Append(EventBuild("BarraIzquierdas", "PRIM_RECGLO_LEN[0]", acumuladoIzqDesde.ToString(), 2, 0.3, 0));
                }
                else
                {
                    sb.Append(EventBuild("Pactometro_IzqVALOR", "MAP_STRING_PAR", $"{acumuladoEscanosIzqDesde}-{acumuladoEscanosIzqHasta}", 1));
                    sb.Append(EventBuild("BarraIzquierdasDesde", "PRIM_RECGLO_LEN[0]", acumuladoIzqDesde.ToString(), 2, 0.3, 0));
                    sb.Append(EventBuild("BarraIzquierdasHasta", "PRIM_RECGLO_LEN[0]", acumuladoIzqHasta.ToString(), 2, 0.3, 0));
                }
            }
            else
            {
                if (oficiales)
                {
                    sb.Append(EventBuild("Pactometro_DerVALOR", "MAP_INT_PAR", $"{acumuladoEscanosDerDesde}", 1));
                    sb.Append(EventBuild("BarraDerechas", "PRIM_RECGLO_LEN[0]", acumuladoDchaDesde.ToString(), 2, 0.3, 0));
                }
                else
                {
                    sb.Append(EventBuild("Pactometro_DerVALOR", "MAP_STRING_PAR", $"{acumuladoEscanosDerDesde}-{acumuladoEscanosDerHasta}", 1));
                    sb.Append(EventBuild("BarraDerechasDesde", "PRIM_RECGLO_LEN[0]", acumuladoDchaDesde.ToString(), 2, 0.3, 0));
                    sb.Append(EventBuild("BarraDerechasHasta", "PRIM_RECGLO_LEN[0]", acumuladoDchaHasta.ToString(), 2, 0.3, 0));
                }
            }
        }

        public string ActualizaPactometroFichas(BrainStormDTO dtoActualizado)
        {
            if (dtoActualizado == null) return "";
            bool oficiales = ResolvePactosOficiales(dtoActualizado);
            int totalEscanos = SafeEscanosTotales(dtoActualizado);
            StringBuilder sb = new StringBuilder();

            acumuladoEscanosDerDesde = 0;
            acumuladoEscanosDerHasta = 0;
            acumuladoDchaDesde = 0;
            acumuladoDchaHasta = 0;

            foreach (var codigo in partidosEnPactoDerecha)
            {
                var partido = dtoActualizado.partidos.FirstOrDefault(p => p.codigo == codigo);
                if (partido == null) continue;

                int escanosDesde = oficiales ? partido.escanios : partido.escaniosDesdeSondeo;
                int escanosHasta = oficiales ? partido.escanios : partido.escaniosHastaSondeo;
                acumuladoEscanosDerDesde += escanosDesde;
                acumuladoEscanosDerHasta += escanosHasta;
                acumuladoDchaDesde += CalcAnchoPacto(escanosDesde, totalEscanos, pxTotalesPacto);
                acumuladoDchaHasta += CalcAnchoPacto(escanosHasta, totalEscanos, pxTotalesPacto);
            }
            AppendPactoValoresYBarras(sb, oficiales, izquierda: false);

            acumuladoEscanosIzqDesde = 0;
            acumuladoEscanosIzqHasta = 0;
            acumuladoIzqDesde = 0;
            acumuladoIzqHasta = 0;

            foreach (var codigo in partidosEnPactoIzquierda)
            {
                var partido = dtoActualizado.partidos.FirstOrDefault(p => p.codigo == codigo);
                if (partido == null) continue;

                int escanosDesde = oficiales ? partido.escanios : partido.escaniosDesdeSondeo;
                int escanosHasta = oficiales ? partido.escanios : partido.escaniosHastaSondeo;
                acumuladoEscanosIzqDesde += escanosDesde;
                acumuladoEscanosIzqHasta += escanosHasta;
                acumuladoIzqDesde += CalcAnchoPacto(escanosDesde, totalEscanos, pxTotalesPacto);
                acumuladoIzqHasta += CalcAnchoPacto(escanosHasta, totalEscanos, pxTotalesPacto);
            }
            AppendPactoValoresYBarras(sb, oficiales, izquierda: true);

            return sb.ToString();
        }

        public string pactosEntraDerecha(BrainStormDTO dto, PartidoDTO pSeleccionado)
        {
            if (dto == null || pSeleccionado == null) return "";
            bool oficiales = ResolvePactosOficiales(dto);
            string eventRoot = GetPactometroEventRoot(oficiales);
            string sceneRoot = GetPactometroSceneRoot(oficiales);
            int totalEscanos = SafeEscanosTotales(dto);
            StringBuilder sb = new StringBuilder();

            if (!partidosEnPactoDerecha.Contains(pSeleccionado.codigo))
            {
                partidosEnPactoDerecha.Add(pSeleccionado.codigo);
            }
            bool esPrimero = partidosEnPactoDerecha.Count == 1;

            int escanosDesde = oficiales ? pSeleccionado.escanios : pSeleccionado.escaniosDesdeSondeo;
            int escanosHasta = oficiales ? pSeleccionado.escanios : pSeleccionado.escaniosHastaSondeo;
            acumuladoEscanosDerDesde += escanosDesde;
            acumuladoEscanosDerHasta += escanosHasta;
            acumuladoDchaDesde += CalcAnchoPacto(escanosDesde, totalEscanos, pxTotalesPacto);
            acumuladoDchaHasta += CalcAnchoPacto(escanosHasta, totalEscanos, pxTotalesPacto);

            AppendPactoValoresYBarras(sb, oficiales, izquierda: false);

            int escanosMayoria = oficiales ? acumuladoEscanosDerDesde : acumuladoEscanosDerHasta;
            if (escanosMayoria >= dto.circunscripcionDTO.mayoria)
            {
                sb.Append(EventRunBuild($"{eventRoot}/PasaMayoria"));
            }

            var mainDto = GetMainDto() ?? dto;
            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(mainDto);
            string id = GetPartidoId(partidoIdMap, pSeleccionado.codigo);

            sb.Append(EventBuild($"{sceneRoot}/Der/LogosDer/Logo0{partidosEnPactoDerecha.Count}", "OBJ_OVERMAT", $"'Logos/Logo{id}'", 1));

            if (esPrimero)
            {
                sb.Append(EventBuild($"{sceneRoot}/Der/LogosDer", "OBJ_GRID_JUMP_TO_END", 1));
                sb.Append(EventRunBuild($"{eventRoot}/lanzaPactometroDer"));

                if (oficiales)
                {
                    sb.Append(EventBuild($"{sceneRoot}/Der/BarraDerechas", "OBJ_OVERMAT", $"'{id}'", 1));
                }
                else
                {
                    sb.Append(EventBuild($"{sceneRoot}/Der/BarraDerechasDesde", "OBJ_OVERMAT", $"'{id}'", 1));
                    sb.Append(EventBuild($"{sceneRoot}/Der/BarraDerechasHasta", "OBJ_OVERMAT", $"'Hasta/{id}'", 1));
                }
            }
            else
            {
                sb.Append(EventBuild($"{sceneRoot}/Der/LogosDer", "OBJ_GRID_JUMP_NEXT", 1));
            }

            return sb.ToString();
        }

        public string pactosEntraIzquierda(BrainStormDTO dto, PartidoDTO pSeleccionado)
        {
            if (dto == null || pSeleccionado == null) return "";
            bool oficiales = ResolvePactosOficiales(dto);
            string eventRoot = GetPactometroEventRoot(oficiales);
            string sceneRoot = GetPactometroSceneRoot(oficiales);
            int totalEscanos = SafeEscanosTotales(dto);
            StringBuilder sb = new StringBuilder();

            if (!partidosEnPactoIzquierda.Contains(pSeleccionado.codigo))
            {
                partidosEnPactoIzquierda.Add(pSeleccionado.codigo);
            }
            bool esPrimero = partidosEnPactoIzquierda.Count == 1;

            int escanosDesde = oficiales ? pSeleccionado.escanios : pSeleccionado.escaniosDesdeSondeo;
            int escanosHasta = oficiales ? pSeleccionado.escanios : pSeleccionado.escaniosHastaSondeo;
            acumuladoEscanosIzqDesde += escanosDesde;
            acumuladoEscanosIzqHasta += escanosHasta;
            acumuladoIzqDesde += CalcAnchoPacto(escanosDesde, totalEscanos, pxTotalesPacto);
            acumuladoIzqHasta += CalcAnchoPacto(escanosHasta, totalEscanos, pxTotalesPacto);

            AppendPactoValoresYBarras(sb, oficiales, izquierda: true);

            int escanosMayoria = oficiales ? acumuladoEscanosIzqDesde : acumuladoEscanosIzqHasta;
            if (escanosMayoria >= dto.circunscripcionDTO.mayoria)
            {
                sb.Append(EventRunBuild($"{eventRoot}/PasaMayoria"));
            }

            var mainDto = GetMainDto() ?? dto;
            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(mainDto);
            string id = GetPartidoId(partidoIdMap, pSeleccionado.codigo);

            sb.Append(EventBuild($"{sceneRoot}/Izq/LogosIzq/Logo0{partidosEnPactoIzquierda.Count}", "OBJ_OVERMAT", $"'Logos/Logo{id}'", 1));

            if (esPrimero)
            {
                sb.Append(EventBuild($"{sceneRoot}/Izq/LogosIzq", "OBJ_GRID_JUMP_TO_END", 1));
                sb.Append(EventRunBuild($"{eventRoot}/lanzaPactometroIzq"));

                if (oficiales)
                {
                    sb.Append(EventBuild($"{sceneRoot}/Izq/BarraIzquierdas", "OBJ_OVERMAT", $"'{id}'", 1));
                }
                else
                {
                    sb.Append(EventBuild($"{sceneRoot}/Izq/BarraIzquierdasDesde", "OBJ_OVERMAT", $"'{id}'", 1));
                    sb.Append(EventBuild($"{sceneRoot}/Izq/BarraIzquierdasHasta", "OBJ_OVERMAT", $"'Hasta/{id}'", 1));
                }
            }
            else
            {
                sb.Append(EventBuild($"{sceneRoot}/Izq/LogosIzq", "OBJ_GRID_JUMP_NEXT", 1));
            }

            return sb.ToString();
        }

        public string pactosSaleDerecha()
        {
            if (partidosEnPactoDerecha.Count == 0) return "";
            var dto = GetMainDto();
            if (dto == null) return "";

            bool oficiales = ResolvePactosOficiales(dto);
            string sceneRoot = GetPactometroSceneRoot(oficiales);
            int totalEscanos = SafeEscanosTotales(dto);
            StringBuilder sb = new StringBuilder();

            string codigoUltimo = partidosEnPactoDerecha.Last();
            partidosEnPactoDerecha.RemoveAt(partidosEnPactoDerecha.Count - 1);

            var partido = dto.partidos.FirstOrDefault(p => p.codigo == codigoUltimo);
            if (partido != null)
            {
                int escanosDesde = oficiales ? partido.escanios : partido.escaniosDesdeSondeo;
                int escanosHasta = oficiales ? partido.escanios : partido.escaniosHastaSondeo;

                acumuladoEscanosDerDesde = Math.Max(0, acumuladoEscanosDerDesde - escanosDesde);
                acumuladoEscanosDerHasta = Math.Max(0, acumuladoEscanosDerHasta - escanosHasta);
                acumuladoDchaDesde = Math.Max(0, acumuladoDchaDesde - CalcAnchoPacto(escanosDesde, totalEscanos, pxTotalesPacto));
                acumuladoDchaHasta = Math.Max(0, acumuladoDchaHasta - CalcAnchoPacto(escanosHasta, totalEscanos, pxTotalesPacto));

                AppendPactoValoresYBarras(sb, oficiales, izquierda: false);
            }

            sb.Append(EventBuild($"{sceneRoot}/Der/LogosDer", "OBJ_GRID_JUMP_PREV", 1));
            return sb.ToString();
        }

        public string pactosSaleIzquierda()
        {
            if (partidosEnPactoIzquierda.Count == 0) return "";
            var dto = GetMainDto();
            if (dto == null) return "";

            bool oficiales = ResolvePactosOficiales(dto);
            string sceneRoot = GetPactometroSceneRoot(oficiales);
            int totalEscanos = SafeEscanosTotales(dto);
            StringBuilder sb = new StringBuilder();

            string codigoUltimo = partidosEnPactoIzquierda.Last();
            partidosEnPactoIzquierda.RemoveAt(partidosEnPactoIzquierda.Count - 1);

            var partido = dto.partidos.FirstOrDefault(p => p.codigo == codigoUltimo);
            if (partido != null)
            {
                int escanosDesde = oficiales ? partido.escanios : partido.escaniosDesdeSondeo;
                int escanosHasta = oficiales ? partido.escanios : partido.escaniosHastaSondeo;

                acumuladoEscanosIzqDesde = Math.Max(0, acumuladoEscanosIzqDesde - escanosDesde);
                acumuladoEscanosIzqHasta = Math.Max(0, acumuladoEscanosIzqHasta - escanosHasta);
                acumuladoIzqDesde = Math.Max(0, acumuladoIzqDesde - CalcAnchoPacto(escanosDesde, totalEscanos, pxTotalesPacto));
                acumuladoIzqHasta = Math.Max(0, acumuladoIzqHasta - CalcAnchoPacto(escanosHasta, totalEscanos, pxTotalesPacto));

                AppendPactoValoresYBarras(sb, oficiales, izquierda: true);
            }

            sb.Append(EventBuild($"{sceneRoot}/Izq/LogosIzq", "OBJ_GRID_JUMP_PREV", 1));
            return sb.ToString();
        }
        #endregion

        #region Sedes Faldón

        string sedeActual = "01";
        public string SedesEntra(PartidoDTO pSeleccionado)
        {
            StringBuilder sb = new StringBuilder();
            var main = Application.Current.MainWindow as MainWindow;
            var dto = main?.dto;
            if (main == null || dto == null || pSeleccionado == null) return "";

            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(dto);
            string id = GetPartidoId(partidoIdMap, pSeleccionado.codigo);

            if (main.tickerDentro)
            {
                sb.Append(EventBuild($"datosSede{sedeActual}", "MAP_STRING_PAR", $"'{id}'", 1));
                sb.Append(EventRunBuild($"Sede{sedeActual}/Entra"));
            }
            return sb.ToString();
        }

        public string SedesEncadena(PartidoDTO pSeleccionado)
        {
            StringBuilder sb = new StringBuilder();
            var main = Application.Current.MainWindow as MainWindow;
            var dto = main?.dto;
            if (main == null || dto == null || pSeleccionado == null) return "";

            Dictionary<string, string> partidoIdMap = BuildPartidoIdMap(dto);
            string id = GetPartidoId(partidoIdMap, pSeleccionado.codigo);

            string sedeSiguiente = sedeActual == "01" ? "04" : "03";
            string otraSede = sedeActual == "01" ? "02" : "01";
            sb.Append(EventBuild($"datosSede{otraSede}", "MAP_STRING_PAR", $"'{id}'", 1));
            sb.Append(EventRunBuild($"Sede{sedeSiguiente}"));
            sedeActual = otraSede;

            return sb.ToString();
        }

        public string SedesSale()
        {
            string signal = EventRunBuild($"Sede{sedeActual}/Sale");
            sedeActual = "01";
            return signal;
        }

        #endregion
    }
}

