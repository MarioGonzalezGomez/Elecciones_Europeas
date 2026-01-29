using System.Globalization;
using System.Text;
using System.Windows;
using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.DTO.Cartones;
using Elecciones.src.model.IPF;
using Elecciones.src.model.IPF.DTO;
using Microsoft.Extensions.Primitives;

namespace Elecciones.src.mensajes.builders
{
    /// <summary>
    /// Clase especializada para señales de Cartón (pantalla completa).
    /// Incluye: Participación, CCAA, Fichas, Mayorías, Pactómetro, UltimoEscaño.
    /// </summary>
    internal class CartonMensajes : IPFMensajesBase
    {
        private static CartonMensajes? instance;

        // Estado para FICHAS
        private int indexCarrusel = 0;
        private List<string> siglas = new();
        private PartidoDTO? fichaSeleccionada = null;

        // Estado para ULTIMO_ESCANO
        private const int TAMANO_MAXIMO_FICHA = 1756;
        private const int POS_INICIAL_IZQ = 90;
        private const int POS_INICIAL_DCH = 1844;
        private const int THRESHOLD_ANCHO_PEQUENO = 126;
        private List<(bool esIzquierda, string siglas, int ancho)> ultimoEscanoPartidos = new();
        private int anchoAcumuladoIzq = 0;
        private int anchoAcumuladoDch = 0;
        private int escaniosAcumuladosIzq = 0;
        private int escaniosAcumuladosDch = 0;
        private string siglasUltimoEscano = "";
        private string siglasLuchaEscano = "";

        private CartonMensajes() : base() { }

        public static CartonMensajes GetInstance()
        {
            instance ??= new CartonMensajes();
            return instance;
        }

        #region Cartones Actualiza

        public string CartonesActualiza()
        {
            string signal = "";
            signal += EventBuild("Oficial_Codigo", "MAP_LLSTRING_LOAD") + "\n";
            signal += EventBuild("UltimoEscanoCSV", "MAP_LLSTRING_LOAD") + "\n";
            return signal;
        }

        #endregion

        #region Participación
        // Los métodos participacionEntra, participacionEncadena, participacionSale
        // se mantienen con la lógica original - ver archivo original líneas 884-1211
        public string participacionEntra(BrainStormDTO dto, int avance)
        {
            StringBuilder signal = new StringBuilder();
            if (dto == null) return "";

            signal.Append(Prepara("PARTICIPACION") + "\n");

            try
            {
                var main = Application.Current.MainWindow as MainWindow;
                var con = main?.conexionActiva;
                Circunscripcion? circ = null;

                if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo))
                    circ = CircunscripcionController.GetInstance(con).FindById(dto.circunscripcionDTO.codigo);
                if (circ == null && !string.IsNullOrEmpty(dto.circunscripcionDTO?.nombre))
                    circ = CircunscripcionController.GetInstance(con).FindByName(dto.circunscripcionDTO.nombre);

                var nombreParaMap = circ?.nombre ?? dto.circunscripcionDTO?.nombre ?? "";
                var codigoParaMap = circ?.codigo ?? dto.circunscripcionDTO?.codigo ?? "";

                if (!string.IsNullOrEmpty(codigoParaMap) && codigoParaMap.EndsWith("00000"))
                {
                    var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(nombreParaMap);
                    if (provincias?.Count > 0)
                        foreach (var prov in provincias)
                            signal.Append(EventBuild($"Participacion/Mapa/{prov.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                    else
                        signal.Append(EventBuild($"Participacion/Mapa/{nombreParaMap}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                }
                else
                    signal.Append(EventBuild($"Participacion/Mapa/{nombreParaMap}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");

                signal.Append(CambiaTexto("Participacion/LugarTxt", circ?.nombre ?? dto.circunscripcionDTO?.nombre ?? "") + "\n");
                signal.Append(EventBuild("Participacion_Barra_Izq", "MAP_FLOAT_PAR", "0", 1) + "\n");
                signal.Append(EventBuild("Participacion_Barra_Dch", "MAP_FLOAT_PAR", "0", 1) + "\n");

                // Hour labels from config
                string horaAv1 = configuration.GetValue("horaAvance1") ?? "";
                string horaAv2 = configuration.GetValue("horaAvance2") ?? "";
                string horaAv3 = configuration.GetValue("horaAvance3") ?? "";
                string horaFinal = configuration.GetValue("horaParticipacion") ?? "";
                string horaAv1Hist = configuration.GetValue("horaAvance1Historico") ?? "";
                string horaAv2Hist = configuration.GetValue("horaAvance2Historico") ?? "";
                string horaAv3Hist = configuration.GetValue("horaAvance3Historico") ?? "";
                string horaFinalHist = configuration.GetValue("horaParticipacionHistorico") ?? "";

                double leftValue = 0.0, rightValue = 0.0;
                string leftTime = "", rightTime = "";
                double finalParticipation = circ?.participacionFinal ?? 0.0;

                switch (avance)
                {
                    case 1:
                        leftTime = horaAv1Hist; rightTime = horaAv1;
                        leftValue = circ?.avance1Hist ?? 0; rightValue = circ?.avance1 != 0 ? circ.avance1 : finalParticipation;
                        break;
                    case 2:
                        leftTime = horaAv2Hist; rightTime = horaAv2;
                        leftValue = circ?.avance2Hist ?? 0; rightValue = circ?.avance2 != 0 ? circ.avance2 : finalParticipation;
                        break;
                    case 3:
                        leftTime = horaAv3Hist; rightTime = horaAv3;
                        leftValue = circ?.avance3Hist ?? 0; rightValue = circ?.avance3 != 0 ? circ.avance3 : finalParticipation;
                        break;
                    default:
                        leftTime = horaFinalHist; rightTime = horaFinal;
                        leftValue = circ?.participacionHist ?? 0; rightValue = finalParticipation;
                        break;
                }

                signal.Append(EventBuild("Participacion/Hora_Izq", "TEXT_STRING", leftTime, 1) + "\n");
                signal.Append(EventBuild("Participacion/Hora_Dch", "TEXT_STRING", rightTime, 1) + "\n");
                signal.Append(EventBuild("Participacion_Barra_Izq", "MAP_FLOAT_PAR", leftValue.ToString("F2", CultureInfo.InvariantCulture), 2, 0.5, 0.6) + "\n");
                signal.Append(EventBuild("Participacion_Barra_Dch", "MAP_FLOAT_PAR", rightValue.ToString("F2", CultureInfo.InvariantCulture), 2, 0.5, 0.6) + "\n");

                if (rightValue < 25)
                {
                    signal.Append(EventBuild("Participacion/Barras/Barra_Dch/Txt_Dch", "OBJ_OFFSET[2]", "322", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Dch", "TEXT_FONT", "Heavy_Naranja", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Izq", "TEXT_FONT", "Heavy", 1) + "\n");
                }
                else
                {
                    signal.Append(EventBuild("Participacion/Barras/Barra_Dch/Txt_Dch", "OBJ_OFFSET[2]", "190", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Dch", "TEXT_FONT", "Heavy_Negro", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Izq", "TEXT_FONT", "Heavy_Negro", 1) + "\n");
                }
            }
            catch { }

            signal.Append(Entra("PARTICIPACION"));
            return signal.ToString();
        }

        public string participacionEncadena(BrainStormDTO dto, int avance)
        {
            if (dto == null) return "";
            StringBuilder signal = new StringBuilder();
            signal.Append(Encadena("PARTICIPACION") + "\n");

            try
            {
                var main = Application.Current.MainWindow as MainWindow;
                var con = main?.conexionActiva;

                if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo) && dto.circunscripcionDTO.codigo.EndsWith("00000"))
                {
                    var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dto.circunscripcionDTO.nombre);
                    if (provincias?.Count > 0)
                        foreach (var prov in provincias)
                            signal.Append(EventBuild($"Participacion/Mapa/{prov.nombre}", "OBJ_CULL", "0", 2, 0.3, 0.3) + "\n");
                    else
                        signal.Append(EventBuild($"Participacion/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0.3) + "\n");
                }
                else
                    signal.Append(EventBuild($"Participacion/Mapa/{dto.circunscripcionDTO?.nombre}", "OBJ_CULL", "0", 2, 0.3, 0.3) + "\n");

                signal.Append(CambiaTexto("Participacion/LugarTxt", dto.circunscripcionDTO?.nombre ?? "") + "\n");

                string horaAv1 = configuration.GetValue("horaAvance1") ?? "";
                string horaAv2 = configuration.GetValue("horaAvance2") ?? "";
                string horaAv3 = configuration.GetValue("horaAvance3") ?? "";
                string horaFinal = configuration.GetValue("horaParticipacion") ?? "";
                string horaAv1Hist = configuration.GetValue("horaAvance1Historico") ?? "";
                string horaAv2Hist = configuration.GetValue("horaAvance2Historico") ?? "";
                string horaAv3Hist = configuration.GetValue("horaAvance3Historico") ?? "";
                string horaFinalHist = configuration.GetValue("horaParticipacionHistorico") ?? "";

                var cDto = dto.circunscripcionDTO;
                double leftValue = 0, rightValue = 0;

                switch (avance)
                {
                    case 1:
                        leftValue = SafeGetDouble(cDto, "avance1Hist", 0);
                        rightValue = SafeGetDouble(cDto, "avance1", SafeGetDouble(cDto, "participacion", 0));
                        signal.Append(EventBuild("Participacion/Hora_Izq", "TEXT_STRING", horaAv1Hist, 1) + "\n");
                        signal.Append(EventBuild("Participacion/Hora_Dch", "TEXT_STRING", horaAv1, 1) + "\n");
                        break;
                    case 2:
                        leftValue = SafeGetDouble(cDto, "avance2Hist", 0);
                        rightValue = SafeGetDouble(cDto, "avance2", SafeGetDouble(cDto, "participacion", 0));
                        signal.Append(EventBuild("Participacion/Hora_Izq", "TEXT_STRING", horaAv2Hist, 1) + "\n");
                        signal.Append(EventBuild("Participacion/Hora_Dch", "TEXT_STRING", horaAv2, 1) + "\n");
                        break;
                    case 3:
                        leftValue = SafeGetDouble(cDto, "avance3Hist", 0);
                        rightValue = SafeGetDouble(cDto, "avance3", SafeGetDouble(cDto, "participacion", 0));
                        signal.Append(EventBuild("Participacion/Hora_Izq", "TEXT_STRING", horaAv3Hist, 1) + "\n");
                        signal.Append(EventBuild("Participacion/Hora_Dch", "TEXT_STRING", horaAv3, 1) + "\n");
                        break;
                    default:
                        leftValue = SafeGetDouble(cDto, "participacionHistorica", SafeGetDouble(cDto, "participacionHist", 0));
                        rightValue = SafeGetDouble(cDto, "participacion", 0);
                        signal.Append(EventBuild("Participacion/Hora_Izq", "TEXT_STRING", horaFinalHist, 1) + "\n");
                        signal.Append(EventBuild("Participacion/Hora_Dch", "TEXT_STRING", horaFinal, 1) + "\n");
                        break;
                }

                if (rightValue < 25)
                {
                    signal.Append(EventBuild("Participacion/Barras/Barra_Dch/Txt_Dch", "OBJ_OFFSET[2]", "322", 2, 0.5) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Dch", "TEXT_FONT", "Heavy_Naranja", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Izq", "TEXT_FONT", "Heavy", 1) + "\n");
                }
                else
                {
                    signal.Append(EventBuild("Participacion/Barras/Barra_Dch/Txt_Dch", "OBJ_OFFSET[2]", "190", 2, 0.5) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Dch", "TEXT_FONT", "Heavy_Negro", 1) + "\n");
                    signal.Append(EventBuild("Participacion/Txt_Izq", "TEXT_FONT", "Heavy_Negro", 1) + "\n");
                }

                signal.Append(EventBuild("Participacion_Barra_Dch", "MAP_FLOAT_PAR", rightValue.ToString("F2", CultureInfo.InvariantCulture), 2, 0.5, 0.3) + "\n");
                signal.Append(EventBuild("Participacion_Barra_Izq", "MAP_FLOAT_PAR", leftValue.ToString("F2", CultureInfo.InvariantCulture), 2, 0.5, 0.3) + "\n");
            }
            catch { }

            return signal.ToString();
        }

        public string participacionSale() => Sale("PARTICIPACION");

        #endregion

        #region CCAA

        public string ccaaEntra(BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder signal = new StringBuilder();
            signal.Append(Prepara("CCAA_CARTONES") + "\n");
            signal.Append(EventBuild("CCAA_Carton/LugarTxt", "TEXT_STRING", dto.circunscripcionDTO?.nombre ?? "", 1) + "\n");

            try
            {
                var main = Application.Current.MainWindow as MainWindow;
                var con = main?.conexionActiva;
                if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo) && dto.circunscripcionDTO.codigo.EndsWith("00000"))
                {
                    var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dto.circunscripcionDTO.nombre);
                    if (provincias?.Count > 0)
                        foreach (var prov in provincias)
                            signal.Append(EventBuild($"CCAA_Carton/Mapa/{prov.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                    else
                        signal.Append(EventBuild($"CCAA_Carton/Mapa/{dto.circunscripcionDTO.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
                }
                else
                    signal.Append(EventBuild($"CCAA_Carton/Mapa/{dto.circunscripcionDTO?.nombre}", "OBJ_CULL", "0", 2, 0.3, 0) + "\n");
            }
            catch { }

            var partidos = dto.partidos ?? new List<PartidoDTO>();
            var siglasLocal = partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            for (int i = 0; i < siglasLocal.Count; i++)
            {
                signal.Append(EventBuild($"CCAA_Carton/Fichas/{siglasLocal[i]}", "OBJ_DISPLACEMENT[2]", $"{i * -110}", 1) + "\n");
                signal.Append(EventBuild($"CCAA_Carton/Fichas/{siglasLocal[i]}", "OBJ_CULL", "0", 1) + "\n");
            }

            signal.Append(Entra("CCAA_CARTONES"));
            return signal.ToString();
        }

        public string ccaaEncadena() => EventRunBuild("CCAA/CAMBIA");
        public string ccaaSale() => Sale("CCAA");

        public string ccaaBaja(BrainStormDTO dto)
        {
            var partidos = dto?.partidos ?? new List<PartidoDTO>();
            return partidos.Count > 7 ? EventRunBuild("CCAA_CARTONES/BAJA", 0.0, 3.0) + "\n" : "";
        }

        public string ccaaSube(BrainStormDTO dto)
        {
            var partidos = dto?.partidos ?? new List<PartidoDTO>();
            return partidos.Count > 7 ? EventRunBuild("CCAA_CARTONES/SUBE", 0.0, 3.0) + "\n" : "";
        }

        #endregion

        #region Fichas de Partido

        public string fichaEntra(bool oficiales, BrainStormDTO dto, PartidoDTO? seleccionado = null)
        {
            fichaSeleccionada = seleccionado;
            siglas = dto.partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            string mode = oficiales ? "Oficiales" : "Sondeos";
            StringBuilder sb = new StringBuilder();

            sb.Append(EventRunBuild($"CARRUSEL/{(oficiales ? "OFICIAL" : "SONDEO")}") + "\n");

            foreach (string sigla in siglas)
                sb.Append(EventBuild($"Carton_Carrusel/{mode}/{sigla}", "OBJ_DISPLACEMENT[0]", "1920", 1) + "\n");

            string siglaSel = seleccionado?.siglas ?? siglas[0];
            sb.Append(EventBuild($"Carton_Carrusel/{mode}/{siglaSel}", "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 0) + "\n");
            indexCarrusel = siglas.IndexOf(siglaSel);

            sb.Append(Entra("CARRUSEL"));
            return sb.ToString();
        }

        public string fichaEncadena(bool oficiales, BrainStormDTO dto, PartidoDTO? seleccionado = null)
        {
            fichaSeleccionada = seleccionado;
            string mode = oficiales ? "Oficiales" : "Sondeos";
            StringBuilder sb = new StringBuilder();
            string siglasYaDentro = siglas[indexCarrusel];

            foreach (string sigla in siglas.Where(s => s != siglasYaDentro))
                sb.Append(EventBuild($"Carton_Carrusel/{mode}/{sigla}", "OBJ_DISPLACEMENT[0]", "1920", 1) + "\n");

            sb.Append(EventBuild($"Carton_Carrusel/{mode}/{siglasYaDentro}", "OBJ_DISPLACEMENT[0]", "-1920", 2, 0.5, 0) + "\n");

            string siglaEntra = seleccionado?.siglas ?? siglas[indexCarrusel + 1];
            sb.Append(EventBuild($"Carton_Carrusel/{mode}/{siglaEntra}", "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 0) + "\n");
            indexCarrusel = siglas.IndexOf(siglaEntra);

            return sb.ToString();
        }

        public string fichaActualiza(bool oficiales, BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            string mode = oficiales ? "Oficiales" : "Sondeos";
            StringBuilder sb = new StringBuilder();

            List<string> siglasNuevas = dtoNuevo.partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            string siglasYaDentro = siglas[indexCarrusel];
            int newIndex = siglasNuevas.IndexOf(siglasYaDentro);
            if (newIndex == -1) { newIndex = 0; siglasYaDentro = siglasNuevas.FirstOrDefault() ?? ""; }

            foreach (string sigla in siglasNuevas.Where(s => s != siglasYaDentro))
                sb.Append(EventBuild($"Carton_Carrusel/{mode}/{sigla}", "OBJ_DISPLACEMENT[0]", "1920", 1) + "\n");

            sb.Append(EventBuild(oficiales ? "Oficial_Codigo" : "Sondeo_Codigo", "MAP_LLSTRING_LOAD"));
            siglas = siglasNuevas;
            indexCarrusel = newIndex >= 0 ? newIndex : 0;
            fichaSeleccionada = indexCarrusel < dtoNuevo.partidos.Count ? dtoNuevo.partidos[indexCarrusel] : null;

            return sb.ToString();
        }

        public string fichaSale(bool oficiales)
        {
            StringBuilder sb = new StringBuilder();
            string mode = oficiales ? "Oficiales" : "Sondeos";
            if (siglas?.Count > 0 && indexCarrusel >= 0 && indexCarrusel < siglas.Count)
                sb.Append(EventBuild($"Carton_Carrusel/{mode}/{siglas[indexCarrusel]}", "OBJ_DISPLACEMENT[0]", "-1920", 2, 0.5, 0));
            sb.Append(Sale("CARRUSEL"));
            indexCarrusel = 0; siglas = new(); fichaSeleccionada = null;
            return sb.ToString();
        }

        #endregion

        #region Pactómetro Cartón

        public string pactometroEntra() => Entra("PACTOMETRO");
        public string pactometroEncadena() => EventRunBuild("PACTOMETRO/POSICIONES");
        public string pactometroSale() => Sale("PACTOMETRO");
        public string pactometroVictoria() => EventRunBuild("PACTOMETRO/VICTORIA");

        #endregion

        #region Mayorías

        public string mayoriasEntra(BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder signal = new StringBuilder();
            signal.Append(Prepara("MAYORIAS") + "\n");
            signal.Append(EventBuild("Mayorias/LugarTxt1", "TEXT_STRING", dto.circunscripcionDTO?.nombre ?? "", 1) + "\n");

            var partidos = dto.partidos ?? new List<PartidoDTO>();
            string topSigla = partidos.OrderByDescending(p => p.escanios).FirstOrDefault()?.siglas.Replace("+", "_").Replace("-", "_") ?? "";

            try
            {
                var main = Application.Current.MainWindow as MainWindow;
                var con = main?.conexionActiva;
                if (!string.IsNullOrEmpty(dto.circunscripcionDTO?.codigo) && dto.circunscripcionDTO.codigo.EndsWith("00000"))
                {
                    var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dto.circunscripcionDTO.nombre);
                    if (provincias?.Count > 0)
                        foreach (var prov in provincias)
                            signal.Append(EventBuild($"Mayorias/{prov.nombre}", "MAT_LIST_COLOR", topSigla, 1) + "\n");
                    else
                        signal.Append(EventBuild($"Mayorias/{dto.circunscripcionDTO.nombre}", "MAT_LIST_COLOR", topSigla, 1) + "\n");
                }
                else
                    signal.Append(EventBuild($"Mayorias/{dto.circunscripcionDTO?.nombre}", "MAT_LIST_COLOR", topSigla, 1) + "\n");
            }
            catch { }

            var siglasLocal = partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            for (int i = 0; i < siglasLocal.Count; i++)
            {
                signal.Append(EventBuild($"Mapa_Mayorias/Fichas/{siglasLocal[i]}", "OBJ_DISPLACEMENT[2]", $"{i * -100}", 1) + "\n");
                signal.Append(EventBuild($"Mapa_Mayorias/Fichas/{siglasLocal[i]}", "OBJ_CULL", "0", 2, 0, 0.5) + "\n");
            }

            int pagesToShow = partidos.Count <= 6 ? 1 : (partidos.Count <= 12 ? 2 : 3);
            for (int page = 1; page <= 3; page++)
                signal.Append(EventBuild($"Mapa_Mayorias/Paginas/Pagina{page}", "OBJ_CULL", page <= pagesToShow ? "0" : "1", 2, 0.3, 0) + "\n");

            signal.Append(Entra("MAYORIAS"));
            return signal.ToString();
        }

        public string mayoriasEncadena(BrainStormDTO dto) => EventRunBuild("MAYORIAS/CAMBIA");
        public string mayoriasSale() => Sale("MAYORIAS");

        public string mayoriasBaja(BrainStormDTO dto)
        {
            var partidos = dto?.partidos ?? new List<PartidoDTO>();
            return partidos.Count > 6 ? EventRunBuild("MAYORIAS/BAJA", 0.0, 3.0) + "\n" : "";
        }

        public string mayoriasSube(BrainStormDTO dto)
        {
            var partidos = dto?.partidos ?? new List<PartidoDTO>();
            return partidos.Count > 6 ? EventRunBuild("MAYORIAS/SUBE", 0.0, 3.0) + "\n" : "";
        }

        #endregion

        #region Cartón Partidos

        public string cartonPartidosEntra(BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder sb = new StringBuilder();
            // sb.Append(Prepara("CARTON_PARTIDOS") + "\n");

            var partidos = dto.partidos ?? new List<PartidoDTO>();
            var siglasLocal = partidos.Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();
            for (int i = 0; i < siglasLocal.Count; i++)
            {
                int x = i < 6 ? 0 : 918;
                string y = ((-0.2 * i).ToString("0.0", CultureInfo.InvariantCulture));
                int z = i < 6 ? (-100 * i) : (-100 * (i - 6));
                sb.Append(EventBuild($"Carton_Partidos/Fichas/{siglasLocal[i]}", "OBJ_DISPLACEMENT", $"({x},{y},{z})", 1) + "\n");
            }
            sb.Append(CartonesActualiza());
            sb.Append(Entra("CARTON_PARTIDOS"));
            return sb.ToString();
        }

        public string cartonPartidosActualiza(BrainStormDTO dto)
        {
            if (dto == null) return "";
            StringBuilder sb = new StringBuilder();

            var siglas = (dto.partidos ?? new List<PartidoDTO>()).Select(p => p.siglas.Replace("+", "_").Replace("-", "_")).ToList();

            for (int i = 0; i < siglas.Count; i++)
            {
                int x = i < 6 ? 0 : 918;
                string y = ((-0.2 * i).ToString("0.0", CultureInfo.InvariantCulture));
                int z = i < 6 ? (-100 * i) : (-100 * (i - 6));
                sb.Append(EventBuild($"Carton_Partidos/Fichas/{siglas[i]}", "OBJ_DISPLACEMENT", $"({x},{y},{z})", 2, 0.5, 0) + "\n");
            }
            sb.Append(EventBuild("Oficial_Codigo", "MAP_LLSTRING_LOAD"));

            return sb.ToString();
        }

        public string cartonPartidosSale()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Sale("CARTON_PARTIDOS"));
            return sb.ToString();
        }

        #endregion

        #region Último Escaño

        public string ultimoEntra(BrainStormDTO dto)
        {
            if (dto == null) return "";
            var main = Application.Current.MainWindow as MainWindow;
            var con = main?.conexionActiva;
            StringBuilder sb = new StringBuilder();
            sb.Append(Prepara("ULTIMO_ESCANO"));
            var provincias = CircunscripcionController.GetInstance(con).FindAllCircunscripcionesByNameAutonomia(dto.circunscripcionDTO.nombre);
            //PARTE COLOR
            if (dto.circunscripcionDTO.codigo.EndsWith("00000") && provincias?.Count > 0)
            {
                foreach (var prov in provincias)
                {
                    sb.Append(EventBuild($"CCAA_Carton/{prov.nombre}", "MAT_COLOR_HSV[2]", "1", 2, 0.5, 0));
                }
            }
            else
            {
                sb.Append(EventBuild($"CCAA_Carton/{dto.circunscripcionDTO.nombre}", "MAT_COLOR_HSV[2]", "1", 2, 0.5, 0));
            }
            //TERMINA PARTE COLOR
            //PARTE PARTIDOS DESTACADOS
            PartidoDTO ultimo = dto.partidos.Find(p => p.esUltimoEscano == 1);
            PartidoDTO siguiente = dto.partidos.Find(p => p.luchaUltimoEscano == 1);
            if (ultimo != null)
            {
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{ultimo.siglas}", "OBJ_DISPLACEMENT", "(-94, 0, 326)", 1));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{ultimo.siglas}", "OBJ_SCALE", "(1, 1, 1)", 1));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{ultimo.siglas}/Escano", "OBJ_CULL", "0", 2, 0, 0.1));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{ultimo.siglas}", "OBJ_CULL", "0", 2, 0, 0.1));
            }
            if (siguiente != null)
            {
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siguiente.siglas}", "OBJ_DISPLACEMENT", "(-154, 0, 0)", 1));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siguiente.siglas}", "OBJ_SCALE", "(0.86, 0.86, 0.86)", 1));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siguiente.siglas}/Escano", "OBJ_CULL", "1", 2, 0, 0.1));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siguiente.siglas}", "OBJ_CULL", "0", 2, 0, 0.1));
            }
            sb.Append(CartonesActualiza());
            sb.Append(Entra("ULTIMO_ESCANO"));
            return sb.ToString();
        }

        public string ultimoEntraPartido(BrainStormDTO dto, CPDataDTO partido, bool esIzquierda)
        {
            if (dto == null || partido == null) return "";
            StringBuilder signal = new StringBuilder();

            int escaniosTotales = dto.circunscripcionDTO?.escaniosTotales ?? 65;
            string siglasP = partido.siglas.Replace("+", "_").Replace("-", "_");
            int anchoPartido = (int)Math.Round((double)TAMANO_MAXIMO_FICHA / escaniosTotales * int.Parse(partido.escanios));

            string[] barrasIzq = { "Barra_Izq", "Barra_Izq1", "Barra_Izq2", "Barra_Izq3" };
            string[] barrasDch = { "Barra_Dch", "Barra_Dch1", "Barra_Dch2", "Barra_Dch3" };

            int indexPartido = ultimoEscanoPartidos.Count(p => p.esIzquierda == esIzquierda);
            if (indexPartido >= 4) return "";

            string nombreBarra = esIzquierda ? barrasIzq[indexPartido] : barrasDch[indexPartido];
            string nombreGrupo = esIzquierda ? "Barras_Izq" : "Barras_Dch";

            int posX;
            if (esIzquierda) { posX = POS_INICIAL_IZQ + anchoAcumuladoIzq; anchoAcumuladoIzq += anchoPartido; escaniosAcumuladosIzq += int.Parse(partido.escanios); }
            else { posX = POS_INICIAL_DCH - anchoAcumuladoDch; anchoAcumuladoDch += anchoPartido; escaniosAcumuladosDch += int.Parse(partido.escanios); }

            ultimoEscanoPartidos.Add((esIzquierda, siglasP, anchoPartido));

            signal.Append(EventBuild($"Ultimo_Escano/Barras/{nombreBarra}", "MAT_LIST_COLOR", siglasP, 1) + "\n");
            signal.Append(EventBuild($"Ultimo_Escano/Mayoria_Absoluta/Barras/{nombreGrupo}/{nombreBarra}", "OBJ_DISPLACEMENT[0]", $"{posX}", 1) + "\n");
            signal.Append(EventBuild($"Ultimo_Escano/Barras/{nombreBarra}", "PRIM_RECGLO_LEN[0]", $"{anchoPartido}", 2, 0.5, 0) + "\n");

            if (esIzquierda)
            {
                signal.Append(EventBuild("Num_Izq", "MAP_FLOAT_PAR", $"{escaniosAcumuladosIzq}", 2, 0.5, 0) + "\n");
                signal.Append(EventBuild("OBJ_DISPLACEMENT3", "BIND_VOFFSET[0]", anchoAcumuladoIzq < THRESHOLD_ANCHO_PEQUENO ? "120" : "0", 2, 0.3, 0.1) + "\n");
            }
            else
            {
                signal.Append(EventBuild("Num_Dch", "MAP_FLOAT_PAR", $"{escaniosAcumuladosDch}", 2, 0.5, 0) + "\n");
                signal.Append(EventBuild("OBJ_DISPLACEMENT4", "BIND_VOFFSET[0]", anchoAcumuladoDch < THRESHOLD_ANCHO_PEQUENO ? "-50" : "46", 2, 0.3, 0.1) + "\n");
            }

            return signal.ToString();
        }

        public string ultimoLimpiaPartidos()
        {
            anchoAcumuladoIzq = anchoAcumuladoDch = escaniosAcumuladosIzq = escaniosAcumuladosDch = 0;
            ultimoEscanoPartidos.Clear();
            return EventBuild("ULTIMO_ESCANO/SALE_BARRAS", "EVENT_RUN");
        }

        public string ultimoActualiza(BrainStormDTO dto)
        {
            if (dto == null) return "";
            var main = Application.Current.MainWindow as MainWindow;
            var con = main?.conexionActiva;
            StringBuilder sb = new StringBuilder();

            // PARTE PARTIDOS DESTACADOS
            PartidoDTO ultimo = dto.partidos.Find(p => p.esUltimoEscano == 1);
            PartidoDTO siguiente = dto.partidos.Find(p => p.luchaUltimoEscano == 1);

            if (ultimo != null)
            {
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{ultimo.siglas}", "OBJ_DISPLACEMENT", "(-94, 0, 326)", 2, 0.5, 0));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{ultimo.siglas}", "OBJ_SCALE", "(1, 1, 1)", 2, 0.5, 0));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{ultimo.siglas}/Escano", "OBJ_CULL", "0", 2, 0.5, 0));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{ultimo.siglas}", "OBJ_CULL", "0", 2, 0.5, 0));
            }

            if (siguiente != null)
            {
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siguiente.siglas}", "OBJ_DISPLACEMENT", "(-154, 0, 0)", 2, 0.5, 0));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siguiente.siglas}", "OBJ_SCALE", "(0.86, 0.86, 0.86)", 2, 0.5, 0));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siguiente.siglas}/Escano", "OBJ_CULL", "1", 2, 0.5, 0));
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siguiente.siglas}", "OBJ_CULL", "0", 2, 0.5, 0));
            }

            // Ocultar todos los partidos que no sean último ni siguiente
            foreach (var partido in dto.partidos)
            {
                // Saltar si es el partido último o el siguiente
                if ((ultimo != null && partido.codigo == ultimo.codigo) || 
                    (siguiente != null && partido.codigo == siguiente.codigo))
                {
                    continue;
                }

                // Ocultar el partido
                string siglasOcultar = partido.siglas.Replace("+", "_").Replace("-", "_");
                sb.Append(EventBuild($"Ultimo_Escano/Ultimo_Escano/{siglasOcultar}", "OBJ_CULL", "1", 2, 0.25, 0) + "\n");
            }

            sb.Append(CartonesActualiza());
            return sb.ToString();
        }

        public string ActualizaPactometroUltimoEscano(BrainStormDTO dtoNuevo)
        {
            if (dtoNuevo == null) return "";
            StringBuilder signal = new StringBuilder();

            int escaniosTotales = dtoNuevo.circunscripcionDTO?.escaniosTotales ?? 65;
            anchoAcumuladoIzq = anchoAcumuladoDch = escaniosAcumuladosIzq = escaniosAcumuladosDch = 0;
            var nuevaLista = new List<(bool esIzquierda, string siglas, int ancho)>();

            string[] barrasIzq = { "Barra_Izq", "Barra_Izq1", "Barra_Izq2", "Barra_Izq3" };
            string[] barrasDch = { "Barra_Dch", "Barra_Dch1", "Barra_Dch2", "Barra_Dch3" };
            int idxIzq = 0, idxDch = 0;

            foreach (var item in ultimoEscanoPartidos)
            {
                var partidoNuevo = dtoNuevo.partidos.FirstOrDefault(p => p.siglas.Replace("+", "_").Replace("-", "_") == item.siglas);
                int nuevosEscanios = partidoNuevo?.escanios ?? 0;
                int nuevoAncho = (int)Math.Round((double)TAMANO_MAXIMO_FICHA / escaniosTotales * nuevosEscanios);

                string nombreBarra = item.esIzquierda ? (idxIzq < barrasIzq.Length ? barrasIzq[idxIzq++] : "") : (idxDch < barrasDch.Length ? barrasDch[idxDch++] : "");
                string nombreGrupo = item.esIzquierda ? "Barras_Izq" : "Barras_Dch";

                if (!string.IsNullOrEmpty(nombreBarra))
                {
                    int posX = item.esIzquierda ? POS_INICIAL_IZQ + anchoAcumuladoIzq : POS_INICIAL_DCH - anchoAcumuladoDch;
                    if (item.esIzquierda) { anchoAcumuladoIzq += nuevoAncho; escaniosAcumuladosIzq += nuevosEscanios; }
                    else { anchoAcumuladoDch += nuevoAncho; escaniosAcumuladosDch += nuevosEscanios; }

                    signal.Append(EventBuild($"Ultimo_Escano/Mayoria_Absoluta/Barras/{nombreGrupo}/{nombreBarra}", "OBJ_DISPLACEMENT[0]", $"{posX}", 2, 0.5, 0) + "\n");
                    signal.Append(EventBuild($"Ultimo_Escano/Barras/{nombreBarra}", "PRIM_RECGLO_LEN[0]", $"{nuevoAncho}", 2, 0.5, 0) + "\n");
                }
                nuevaLista.Add((item.esIzquierda, item.siglas, nuevoAncho));
            }

            ultimoEscanoPartidos = nuevaLista;
            signal.Append(EventBuild("Num_Izq", "MAP_FLOAT_PAR", $"{escaniosAcumuladosIzq}", 2, 0.5, 0) + "\n");
            signal.Append(EventBuild("Num_Dch", "MAP_FLOAT_PAR", $"{escaniosAcumuladosDch}", 2, 0.5, 0) + "\n");

            return signal.ToString();
        }

        public string ultimoEncadena(BrainStormDTO dtoAnterior, BrainStormDTO dtoNuevo)
        {
            if (dtoNuevo == null) return "";
            StringBuilder signal = new StringBuilder();

            signal.Append(EventBuild("Ultimo_Escano/Ultimo_Escano", "OBJ_DISPLACEMENT[0]", "860", 2, 0.5, 0) + "\n");
            signal.Append(EventRunBuild("ULTIMO_ESCANO/OCULTA_MAPA") + "\n");
            signal.Append(EventBuild("Ultimo_Escano/Lugar", "TEXT_STRING", dtoNuevo.circunscripcionDTO?.nombre ?? "", 1) + "\n");

            ultimoEscanoPartidos.Clear();
            anchoAcumuladoIzq = anchoAcumuladoDch = escaniosAcumuladosIzq = escaniosAcumuladosDch = 0;

            signal.Append(EventBuild("Ultimo_Escano/Ultimo_Escano", "OBJ_DISPLACEMENT[0]", "0", 2, 0.5, 1.0) + "\n");
            signal.Append(CartonesActualiza() + "\n");

            return signal.ToString();
        }

        public string ultimoSale()
        {
            ultimoEscanoPartidos.Clear();
            anchoAcumuladoIzq = anchoAcumuladoDch = escaniosAcumuladosIzq = escaniosAcumuladosDch = 0;
            siglasUltimoEscano = siglasLuchaEscano = "";
            return Sale("ULTIMO_ESCANO");
        }



        #endregion
    }
}
