using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Elecciones.src.logic.comparators;
using Elecciones.src.model.DTO.BrainStormDTO;
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
        public string TickerEntra(bool oficiales, BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder sb = new StringBuilder();

            //TAMANO
            double tamanoFicha = (pxTotales - (margin * (dto.numPartidos - 1))) / dto.numPartidos;
            sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", tamanoFicha.ToString(), 1));

            // Crear partidoIdMap basado en orden por CÓDIGO (PP=00001 → partido01)
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = dto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string partidoId = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = partidoId;
            }

            // Ordenar partidos según PartidoDTOComparerUnified (por escaños/votos descendente)
            List<PartidoDTO> partidosOrdenadosPorComparer = dto.partidos.ToList();
            partidosOrdenadosPorComparer.Sort(new PartidoDTOComparerUnified(oficiales));
            partidosOrdenadosPorComparer.Reverse(); // Descendente

            // Calcular posición acumulativa para cada partido según el orden del comparador
            double posicionAcumulada = posicionInicial;
            string tipo = oficiales ? "Escrutinio" : "Sondeo";
            for (int i = 0; i < partidosOrdenadosPorComparer.Count; i++)
            {
                PartidoDTO partido = partidosOrdenadosPorComparer[i];
                string partidoId = partidoIdMap[partido.codigo];

                // Asignar posición al partido (usando su ID basado en código)
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{partidoId}", "OBJ_DISPLACEMENT[0]", posicionAcumulada.ToString(), 1));

                // Acumular para el siguiente: posición actual + tamaño ficha + margen
                if (i < partidosOrdenadosPorComparer.Count - 1)
                {
                    posicionAcumulada += tamanoFicha + margin;
                }
            }
            var partidosActivos = partidosOrdenadosPorComparer.Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();
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
            //TAMANO
            double tamanoFicha = (pxTotales - (margin * (dto.numPartidos - 1))) / dto.numPartidos;
            sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", tamanoFicha.ToString(), 2, 0.6, 0));

            // Crear partidoIdMap basado en orden por CÓDIGO (PP=00001 → partido01)
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = dto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string partidoId = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = partidoId;
            }

            // Ordenar partidos según PartidoDTOComparerUnified (por escaños/votos descendente)
            List<PartidoDTO> partidosOrdenadosPorComparer = dto.partidos.ToList();
            partidosOrdenadosPorComparer.Sort(new PartidoDTOComparerUnified(oficiales));
            partidosOrdenadosPorComparer.Reverse(); // Descendente

            // Calcular posición acumulativa para cada partido según el orden del comparador
            double posicionAcumulada = posicionInicial;
            string tipo = oficiales ? "Escrutinio" : "Sondeo";
            for (int i = 0; i < partidosOrdenadosPorComparer.Count; i++)
            {
                PartidoDTO partido = partidosOrdenadosPorComparer[i];
                string partidoId = partidoIdMap[partido.codigo];

                // Asignar posición al partido (usando su ID basado en código)
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{partidoId}", "OBJ_DISPLACEMENT[0]", posicionAcumulada.ToString(), 2, 0.6, 0));

                // Acumular para el siguiente: posición actual + tamaño ficha + margen
                if (i < partidosOrdenadosPorComparer.Count - 1)
                {
                    posicionAcumulada += tamanoFicha + margin;
                }
            }
            var partidosActivos = partidosOrdenadosPorComparer.Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();
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

            // Si hay partidos expandidos, solo enviar la señal de actualización
            // para no romper la estructura de posiciones
            if (partidosExpandidos.Count > 0)
            {
                sb.Append(EventRunBuild($"{tipo}/Actualiza"));
                return sb.ToString();
            }

            //TAMANO
            double tamanoFicha = (pxTotales - (margin * (dto.numPartidos - 1))) / dto.numPartidos;
            sb.Append(EventBuild("fichaPartido", "PRIM_RECGLO_LEN[0]", tamanoFicha.ToString(), 2, 0.3, 0));

            // Crear partidoIdMap basado en orden por CÓDIGO (PP=00001 → partido01)
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = dto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string partidoId = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = partidoId;
            }

            // Ordenar partidos según PartidoDTOComparerUnified (por escaños/votos descendente)
            List<PartidoDTO> partidosOrdenadosPorComparer = dto.partidos.ToList();
            partidosOrdenadosPorComparer.Sort(new PartidoDTOComparerUnified(dto.oficiales));
            partidosOrdenadosPorComparer.Reverse(); // Descendente

            // Calcular posición acumulativa para cada partido según el orden del comparador
            double posicionAcumulada = posicionInicial;
            for (int i = 0; i < partidosOrdenadosPorComparer.Count; i++)
            {
                PartidoDTO partido = partidosOrdenadosPorComparer[i];
                string partidoId = partidoIdMap[partido.codigo];

                // Asignar posición al partido (usando su ID basado en código)
                sb.Append(EventBuild($"Graficos/{tipo}/partidos/partido{partidoId}", "OBJ_DISPLACEMENT[0]", posicionAcumulada.ToString(), 2, 0.3, 0));

                // Acumular para el siguiente: posición actual + tamaño ficha + margen
                if (i < partidosOrdenadosPorComparer.Count - 1)
                {
                    posicionAcumulada += tamanoFicha + margin;
                }
            }
            var partidosActivos = partidosOrdenadosPorComparer.Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();
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
            sb.Append(EventRunBuild("SaleP_Escanio"));
            sb.Append(EventRunBuild("EntraP_Porcentaje"));
            return sb.ToString();
        }

        public string TickerVotosSale(bool oficial)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventRunBuild("SaleP_Porcentaje"));
            sb.Append(EventRunBuild("EntraP_Escanio"));
            return sb.ToString();
        }


        //NO SE USAN EN ESTA ELECCION ARAGON
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
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = dto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string id = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = id;
            }

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
                string sceneObjectId = partidoIdMap.ContainsKey(partido.codigo) ? partidoIdMap[partido.codigo] : "00";

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
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = dto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string id = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = id;
            }

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
                string sceneObjectId = partidoIdMap.ContainsKey(partido.codigo) ? partidoIdMap[partido.codigo] : "00";

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
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = dto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string id = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = id;
            }

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
                string sceneObjectId = partidoIdMap.ContainsKey(partido.codigo) ? partidoIdMap[partido.codigo] : "00";

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
            // Ordenar partidos según PartidoDTOComparerUnified (por escaños/votos descendente)
            List<PartidoDTO> partidosActivos = dto.partidos.ToList();
            partidosActivos.Sort(new PartidoDTOComparerUnified(true));
            partidosActivos.Reverse(); // Descendente
            partidosActivos = partidosActivos.Where(p => dto.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();


            List<string> siglasPartidos = dto.partidos.Select(x => x.siglas).ToList();
            List<string> siglasActivos = partidosActivos.Select(x => x.siglas).ToList();
            string signal = "";
            signal += EventBuild("NumeroEscrutado", "TEXT_STRING", $"'{dto.circunscripcionDTO.escrutado}%'", 2, 0.5, 0) + "\n";

            int n = partidosActivos?.Count ?? 0;

            var layoutByCount = new Dictionary<int, (int Size, int[] Positions, int LogoPos, int EscanosPos)>()
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

                // Determinar escala según número de partidos
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
                    signal += EventBuild($"Partidos/{siglaObj}", "OBJ_DISPLACEMENT[0]", "1920", 2, 0.5, 0) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Logo", "OBJ_DISPLACEMENT[0]", $"{layout.LogoPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_DISPLACEMENT[0]", $"{layout.EscanosPos}", 1) + "\n";
                    signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_SCALE", escala, 1) + "\n";
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
            List<PartidoDTO> partidosActuales = dtoAnterior.partidos.ToList();
            partidosActuales.Sort(new PartidoDTOComparerUnified(true));
            partidosActuales.Reverse(); // Descendente
            partidosActuales = partidosActuales.Where(p => dtoAnterior.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();

            List<PartidoDTO> partidosActivos = dtoNuevo.partidos.ToList();
            partidosActivos.Sort(new PartidoDTOComparerUnified(true));
            partidosActivos.Reverse(); // Descendente
            partidosActivos = partidosActivos.Where(p => dtoNuevo.oficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0).ToList();

            List<string> siglasPartidos = dtoNuevo.partidos.Select(x => x.siglas).ToList();
            List<string> siglasNuevas = partidosActivos.Select(x => x.siglas).ToList();
            List<string> siglasAnteriores = partidosActuales.Select(x => x.siglas).ToList();

            string signal = "";
            signal += EventBuild("NumeroEscrutado", "TEXT_STRING", $"'{dtoNuevo.circunscripcionDTO.escrutado}%'", 2, 0.5, 0) + "\n";

            int nAnterior = partidosActuales?.Count ?? 0;
            int nNuevo = partidosActivos?.Count ?? 0;

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
                    {7, (170, new[] {-35,150,335,520,705,890,1075}, -554, -474)},
                    {8, (157, new[] {-73,95,263,431,599,767,935,1103}, -554, -475)},
                    {9, (131, new[] {-63,82,227,372,517,662,807,952,1097}, -545, -464)},
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

                        // Determinar escala según número de partidos
                        string escalaNuevo = nNuevo <= 7 ? "(1,1,1)" : (nNuevo == 8 ? "(0.9,0.9,0.9)" : "(0.75,0.75,0.75)");
                        signal += EventBuild($"Partidos/{siglaObj}/Escaños", "OBJ_SCALE", escalaNuevo, 2, 0.5, 0) + "\n";

                        PartidoDTO temp = dtoNuevo.partidos.FirstOrDefault(x => x.siglas == siglaRaw);
                        signal += EventBuild($"Escaños/{siglaObj}", "TEXT_STRING", $"'{temp.escanios}'", 2, 0.5, 0) + "\n";
                        signal += EventBuild($"Partidos/{siglaObj}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n";
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
                    {7, (170, new[] {-35,150,335,520,705,890,1075}, -554, -474)},
                    {8, (157, new[] {-73,95,263,431,599,767,935,1103}, -554, -475)},
                    {9, (131, new[] {-63,82,227,372,517,662,807,952,1097}, -545, -464)},
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

        #region Pactos Faldón

        public string pactosEntra()
        {
            return EventRunBuild("Pactometro/Entra");
        }

        public string pactosReinicio()
        {
            acumuladoIzq = 0;
            acumuladoDcha = 0;
            acumuladoEscanosDer = 0;
            acumuladoEscanosIzq = 0;
            partidosEnPactoDerecha.Clear();
            partidosEnPactoIzquierda.Clear();
            StringBuilder sb = new StringBuilder();
            sb.Append(EventRunBuild("Pactometro/reinicioPactometroIzq"));
            sb.Append(EventRunBuild("Pactometro/reinicioPactometroDer"));
            return sb.ToString();
        }

        public string pactosSale()
        {
            acumuladoIzq = 0;
            acumuladoDcha = 0;
            acumuladoEscanosDer = 0;
            acumuladoEscanosIzq = 0;
            partidosEnPactoDerecha.Clear();
            partidosEnPactoIzquierda.Clear();
            StringBuilder sb = new StringBuilder();
            sb.Append(EventRunBuild("Pactometro/Sale"));
            sb.Append(EventRunBuild("Pactometro/reinicioPactometroIzq"));
            sb.Append(EventRunBuild("Pactometro/reinicioPactometroDer"));
            return sb.ToString();
        }

        double pxTotalesPacto = 1748;
        double acumuladoIzq = 0;
        double acumuladoDcha = 0;
        int acumuladoEscanosDer = 0;
        int acumuladoEscanosIzq = 0;

        private List<string> partidosEnPactoDerecha = new List<string>();
        private List<string> partidosEnPactoIzquierda = new List<string>();

        public string ActualizaPactometroFichas(BrainStormDTO dtoActualizado)
        {
            if (dtoActualizado == null) return "";
            StringBuilder sb = new StringBuilder();

            // RECALCULAR DERECHA
            acumuladoEscanosDer = 0;
            acumuladoDcha = 0;

            foreach (var codigo in partidosEnPactoDerecha)
            {
                var partido = dtoActualizado.partidos.FirstOrDefault(p => p.codigo == codigo);
                if (partido != null)
                {
                    acumuladoEscanosDer += partido.escanios;
                    double tamanoFicha = (partido.escanios * pxTotalesPacto) / dtoActualizado.circunscripcionDTO.escaniosTotales;
                    acumuladoDcha += tamanoFicha;
                }
            }

            sb.Append(EventBuild("Pactometro_DerVALOR", "MAP_INT_PAR", $"{acumuladoEscanosDer}", 1));
            sb.Append(EventBuild("BarraDerechas", "PRIM_RECGLO_LEN[0]", acumuladoDcha.ToString(), 2, 0.3, 0));

            // RECALCULAR IZQUIERDA
            acumuladoEscanosIzq = 0;
            acumuladoIzq = 0;

            foreach (var codigo in partidosEnPactoIzquierda)
            {
                var partido = dtoActualizado.partidos.FirstOrDefault(p => p.codigo == codigo);
                if (partido != null)
                {
                    acumuladoEscanosIzq += partido.escanios;
                    double tamanoFicha = (partido.escanios * pxTotalesPacto) / dtoActualizado.circunscripcionDTO.escaniosTotales;
                    acumuladoIzq += tamanoFicha;
                }
            }

            sb.Append(EventBuild("Pactometro_IzqVALOR", "MAP_INT_PAR", $"{acumuladoEscanosIzq}", 1));
            sb.Append(EventBuild("BarraIzquierdas", "PRIM_RECGLO_LEN[0]", acumuladoIzq.ToString(), 2, 0.3, 0));

            return sb.ToString();
        }

        public string pactosEntraDerecha(BrainStormDTO dto, PartidoDTO pSeleccionado)
        {
            if (pSeleccionado == null) return "";
            StringBuilder sb = new StringBuilder();

            // Guardar partido en lista si no existe
            if (!partidosEnPactoDerecha.Contains(pSeleccionado.codigo))
            {
                partidosEnPactoDerecha.Add(pSeleccionado.codigo);
            }

            // Detectar si es el primer partido en entrar por la derecha
            bool esPrimero = acumuladoDcha == 0;

            // NUMERO ESCANOS - suma acumulada de escaños del lado derecho
            acumuladoEscanosDer += pSeleccionado.escanios;
            sb.Append(EventBuild("Pactometro_DerVALOR", "MAP_INT_PAR", $"{acumuladoEscanosDer}", 1));

            // CRECIMIENTO BARRA
            double tamanoFicha = (pSeleccionado.escanios * pxTotalesPacto) / dto.circunscripcionDTO.escaniosTotales;
            acumuladoDcha += tamanoFicha;
            sb.Append(EventBuild("BarraDerechas", "PRIM_RECGLO_LEN[0]", acumuladoDcha.ToString(), 2, 0.3, 0));

            // LOGOS
            var main = Application.Current.MainWindow as MainWindow;
            var mainDto = main?.dto;
            // Crear partidoIdMap basado en orden por CÓDIGO (PP=00001 → partido01)
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = mainDto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string partidoId = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = partidoId;
            }
            string id = partidoIdMap[pSeleccionado.codigo];
            sb.Append(EventBuild($"Graficos/Pactometro/Der/LogosDer/Logo0{partidosEnPactoDerecha.Count}", "OBJ_OVERMAT", $"'Logos/Logo{id}'", 1));

            // Señales condicionales según si es el primero o no
            if (esPrimero)
            {
                sb.Append(EventBuild("Graficos/Pactometro/Der/LogosDer", "OBJ_GRID_JUMP_TO_END", 1));
                sb.Append(EventRunBuild("Pactometro/lanzaPactometroDer"));

                // COLOR
                sb.Append(EventBuild("Graficos/Pactometro/Der/BarraDerechas", "OBJ_OVERMAT", $"'{id}'", 1));
            }
            else
            {
                sb.Append(EventBuild("Graficos/Pactometro/Der/LogosDer", "OBJ_GRID_JUMP_NEXT", 1));
            }

            return sb.ToString();
        }

        public string pactosEntraIzquierda(BrainStormDTO dto, PartidoDTO pSeleccionado)
        {
            if (pSeleccionado == null) return "";
            StringBuilder sb = new StringBuilder();

            // Guardar partido en lista si no existe
            if (!partidosEnPactoIzquierda.Contains(pSeleccionado.codigo))
            {
                partidosEnPactoIzquierda.Add(pSeleccionado.codigo);
            }

            // Detectar si es el primer partido en entrar por la derecha
            bool esPrimero = acumuladoIzq == 0;

            // NUMERO ESCANOS - suma acumulada de escaños del lado derecho
            acumuladoEscanosIzq += pSeleccionado.escanios;
            sb.Append(EventBuild("Pactometro_IzqVALOR", "MAP_INT_PAR", $"{acumuladoEscanosIzq}", 1));

            // CRECIMIENTO BARRA
            double tamanoFicha = (pSeleccionado.escanios * pxTotalesPacto) / dto.circunscripcionDTO.escaniosTotales;
            acumuladoIzq += tamanoFicha;
            sb.Append(EventBuild("BarraIzquierdas", "PRIM_RECGLO_LEN[0]", acumuladoIzq.ToString(), 2, 0.3, 0));

            // LOGOS
            // LOGOS
            var main = Application.Current.MainWindow as MainWindow;
            var mainDto = main?.dto;
            // Crear partidoIdMap basado en orden por CÓDIGO (PP=00001 → partido01)
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = mainDto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string partidoId = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = partidoId;
            }
            string id = partidoIdMap[pSeleccionado.codigo];
            sb.Append(EventBuild($"Graficos/Pactometro/Izq/LogosIzq/Logo0{partidosEnPactoIzquierda.Count}", "OBJ_OVERMAT", $"'Logos/Logo{id}'", 1));

            // Señales condicionales según si es el primero o no
            if (esPrimero)
            {
                sb.Append(EventBuild("Graficos/Pactometro/Izq/LogosIzq", "OBJ_GRID_JUMP_TO_END", 1));
                sb.Append(EventRunBuild("Pactometro/lanzaPactometroIzq"));
                // COLOR
                sb.Append(EventBuild("Graficos/Pactometro/Izq/BarraIzquierdas", "OBJ_OVERMAT", $"'{id}'", 1));
            }
            else
            {
                sb.Append(EventBuild("Graficos/Pactometro/Izq/LogosIzq", "OBJ_GRID_JUMP_NEXT", 1));
            }

            return sb.ToString();
        }

        public string pactosSaleDerecha(int posicionPartido)
        {
            return "";
        }
        public string pactosSaleIzquierda(int posicionPartido)
        {
            return "";
        }

        #endregion

        #region Sedes Faldón

        string sedeActual = "01";
        public string SedesEntra(PartidoDTO pSeleccionado)
        {
            StringBuilder sb = new StringBuilder();
            var main = Application.Current.MainWindow as MainWindow;
            var dto = main?.dto;

            // Crear partidoIdMap basado en orden por CÓDIGO (PP=00001 → partido01)
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = dto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string partidoId = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = partidoId;
            }

            // Ordenar partidos según PartidoDTOComparerUnified (por escaños/votos descendente)
            List<PartidoDTO> partidosOrdenadosPorComparer = dto.partidos.ToList();
            partidosOrdenadosPorComparer.Sort(new PartidoDTOComparerUnified(dto.oficiales));
            partidosOrdenadosPorComparer.Reverse(); // Descendente

            string id = partidoIdMap[pSeleccionado.codigo];

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

            // Crear partidoIdMap basado en orden por CÓDIGO (PP=00001 → partido01)
            Dictionary<string, string> partidoIdMap = new Dictionary<string, string>();
            List<PartidoDTO> partidosOrdenadosPorCodigo = dto.partidos.OrderBy(p => p.codigo).ToList();
            for (int i = 0; i < partidosOrdenadosPorCodigo.Count; i++)
            {
                string partidoId = (i + 1).ToString("D2");
                partidoIdMap[partidosOrdenadosPorCodigo[i].codigo] = partidoId;
            }

            // Ordenar partidos según PartidoDTOComparerUnified (por escaños/votos descendente)
            List<PartidoDTO> partidosOrdenadosPorComparer = dto.partidos.ToList();
            partidosOrdenadosPorComparer.Sort(new PartidoDTOComparerUnified(dto.oficiales));
            partidosOrdenadosPorComparer.Reverse(); // Descendente

            string id = partidoIdMap[pSeleccionado.codigo];

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
