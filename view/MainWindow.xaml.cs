using Elecciones.src.conexion;
using Elecciones.src.controller;
using Elecciones.src.logic;
using Elecciones.src.mensajes;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF.DTO;
using Elecciones.src.model.IPF;
using Elecciones.src.utils;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Elecciones.src.logic.comparators;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Globalization;

namespace Elecciones
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Circunscripcion> CCAA = new();
        ObservableCollection<string> circunscripcionNames = new();
        ObservableCollection<CPDataDTO> listaDeDatos = new();
        public ConexionEntityFramework conexionActiva = null!;
        private int avance;
        public BrainStormDTO dto = null!;
        bool preparado;
        public bool oficiales;
        bool regional;
        private string autonomiasHeader = string.Empty;

        //Escuchador
        public Escuchador escuchador = null!;
        //Bool para ver si mando actualizaciÃ³n de datos o no
        public bool actualizacionActiva;

        //1 Nacionales, 2 Autonomia X
        private int tipoElecciones;

        //0 Si utilizamos datos de la DB1, 1 de la DB2...
        private ObservableInt eleccionSeleccionada = null!;

        //Si tenemos algÃºn partido seleccionado con el que se deba hacer algo
        public PartidoDTO? partidoSeleccionado;

        //Bool para hacer giro
        bool sondeoEnElAire;
        public bool tickerDentro;
        bool sedeDentro;

        //Bool cartones
        bool participacionDentro;
        bool ccaaDentro;
        bool mayoriasDentro;
        bool fichaDentro;
        public bool sfPactometroDentro;
        bool sfCarruselDentro;
        bool sfUltimoDentro;
        string? sfCodigoSedeDesplegada;
        bool cartonPartidosDentro;
        public bool ultimoEscanoDentro;
        public bool ultimoSuperfaldonDentro => sfUltimoDentro;

        //Estas conexiones serÃ¡n null si no estÃ¡n activadas por ConfiguraciÃ³n
        OrdenesIPF? ipf;
        OrdenesPrime? prime;
        GraphicController graficos = null!;

        //Ventanas adicionales
        Botonera botonera = null!;
        public Pactos? pactos;
        public Config? config;

        //Manejar datos del fichero de configuracion
        ConfigManager configuration = null!;

        //Diccionario para guardar los valores originales de escaÃ±os de sondeo
        private Dictionary<string, (int desde, int hasta)> valoresOriginalesSondeo = new Dictionary<string, (int, int)>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeVariables();
            AdaptarConexiones();
            CargarCircunscripciones();
            CargarMedios();
            InitializeListView();
            AdaptarColores();
            EscribirConexiones();
            PrepararEstructuraDeCarpetas();
            IniciarEscuchadores();
            AdaptarTablas();

            // Initialize video UI state (loads saved paths / modes)
            InitializeVideoConfigUI();

            this.Closing += WindowClosing;
        }

        private void InitializeVariables()
        {
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            actualizacionActiva = true;
            avance = 1;
            preparado = false;
            oficiales = false;
            sondeoEnElAire = false;
            tickerDentro = false;
            sedeDentro = false;
            participacionDentro = false;
            ccaaDentro = false;
            mayoriasDentro = false;
            fichaDentro = false;
            sfPactometroDentro = false;
            sfCarruselDentro = false;
            sfUltimoDentro = false;
            sfCodigoSedeDesplegada = null;
            cartonPartidosDentro = false;
            ultimoEscanoDentro = false;
            circunscripcionNames = new ObservableCollection<string>();
            listaDeDatos = new ObservableCollection<CPDataDTO>();
            tipoElecciones = int.Parse(configuration.GetValue("tipoElecciones"));
            eleccionSeleccionada = new ObservableInt();
            eleccionSeleccionada.CambioDeElecciones += CambioDeEleccionesHandler;
            eleccionSeleccionada.Valor = 0;
            regional = false;
            graficos = GraphicController.GetInstance();
        }
        private void AdaptarConexiones()
        {
            string numElecciones = configuration.GetValue("numEleccionesSimultaneas");
            if (numElecciones == "2")
            {
                btnSondeoInferior.Visibility = Visibility.Hidden;
                btnOficialesInferior.Visibility = Visibility.Visible;
            }
            if (numElecciones == "3")
            {
                btnSondeoInferior.Visibility = Visibility.Visible;
                btnOficialesInferior.Visibility = Visibility.Visible;
                btnSondeoSuperior.Visibility = Visibility.Visible;
                btnOficialesSuperior.Visibility = Visibility.Visible;
            }
            conexionActiva = new ConexionEntityFramework(int.Parse(configuration.GetValue($"conexionDefault{eleccionSeleccionada.Valor + 1}")), eleccionSeleccionada.Valor + 1);
        }

        private void AdaptarTablas()
        {
            if (configuration.GetValue("activoPrime") == "1")
            {
                prime = OrdenesPrime.GetInstance();
            }
            if (configuration.GetValue("activoIPF") == "1")
            {
                ipf = OrdenesIPF.GetInstance();
            }

            if (configuration.GetValue("regional") == "1")
            {
                regional = true;
            }

            if (configuration.GetValue("botoneraExtra") == "1")
            {
                botonera = new Botonera();
                botonera.Left = this.Width + 150;
                botonera.Top = this.Top;
                botonera.Show();
            }
        }
        private string GetContentLabelConexion()
        {
            string content = conexionActiva._tipoConexion switch
            {
                1 => "PRINCIPAL",
                2 => "RESERVA",
                3 => "LOCAL",
                _ => ""
            };
            return content;
        }
        private void CargarCircunscripciones()
        {
            if (tipoElecciones == 1) // Elecciones generales
            {
                // Cargar todas las autonomÃ­as (EspaÃ±a ya estÃ¡ incluida al principio por el mÃ©todo FindAllAutonomias)
                CCAA = CircunscripcionController.GetInstance(conexionActiva).FindAllAutonomias(eleccionSeleccionada.Valor + 1);
                autonomiasHeader = "AUTONOMÃAS";
            }
            else if (tipoElecciones == 2) // Elecciones autonÃ³micas
            {
                // Cargar solo la autonomÃ­a correspondiente (codigoRegional + 5 ceros)
                string codigoRegional = configuration.GetValue($"codigoRegionalBD{eleccionSeleccionada.Valor + 1}");
                string codigoAutonomia = $"{codigoRegional}00000";
                Circunscripcion autonomia = CircunscripcionController.GetInstance(conexionActiva).FindById(codigoAutonomia);
                CCAA = new List<Circunscripcion>();
                if (autonomia != null)
                {
                    CCAA.Add(autonomia);
                }
                autonomiasHeader = "AUTONOMÃA";
            }
        }

        private void ActualizarHeaderAutonomias()
        {
            // Buscar el GridViewColumn en el XAML y actualizar su Header
            if (autonomiasListView.View is GridView gridView && gridView.Columns.Count > 0)
            {
                gridView.Columns[0].Header = autonomiasHeader;
            }
        }

        private void CargarMedios()
        {
            try
            {
                // Guardar la selecciÃ³n actual si existe
                string? seleccionActual = cmbSondeo.SelectedItem?.ToString();

                cmbSondeo.Items.Clear();
                // Agregar opciÃ³n RTVE como primera opciÃ³n (valores originales)
                cmbSondeo.Items.Add("RTVE");

                MedioController medioController = new MedioController(conexionActiva);
                List<src.model.DTO.MedioDTO> medios = medioController.ObtenerMediosConDescripcion();

                foreach (var medio in medios)
                {
                    cmbSondeo.Items.Add(medio.descripcion);
                }

                if (cmbSondeo.Items.Count > 0)
                {
                    // Intentar restaurar la selecciÃ³n anterior
                    if (!string.IsNullOrEmpty(seleccionActual) && cmbSondeo.Items.Contains(seleccionActual))
                    {
                        cmbSondeo.SelectedItem = seleccionActual;
                    }
                    else
                    {
                        cmbSondeo.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando medios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Por ahora, se modifican manualmente, pero se podrÃ­a implementar un modo de introducir
        //los tipos de grÃ¡ficos en la ventana de configuraciÃ³n Avanzada
        public void InitializeListView()
        {
            graficosListView.Items.Clear();
            int tablaPrincipal = int.Parse(configuration.GetValue("tablasGraficosPrincipal"));
            graficosHeader.Header = configuration.GetValue($"headerTabla{tablaPrincipal}");
            switch (tablaPrincipal)
            {
                case 1:
                    graficosListView.Items.Add("CUENTA ATRÃS");
                    graficosListView.Items.Add("FICHAS");
                    graficosListView.Items.Add("SEDES");
                    break;
                case 2:
                    //graficosListView.Items.Add("PARTICIPACIÃ“N");
                    //graficosListView.Items.Add("CCAA");
                    //graficosListView.Items.Add("FICHAS");
                    //graficosListView.Items.Add("PACTÃ“METRO");
                    //graficosListView.Items.Add("MAYORÃAS");
                    //graficosListView.Items.Add("VS");
                    graficosListView.Items.Add("CARTÃ“N PARTIDOS");
                    graficosListView.Items.Add("ÃšLTIMO ESCAÃ‘O");
                    break;
                case 3:
                    graficosListView.Items.Add("ESCRUTADO");
                    graficosListView.Items.Add("CARRUSEL");
                    graficosListView.Items.Add("CCAA");
                    //graficosListView.Items.Add("PACTÃ“METRO");
                    graficosListView.Items.Add("ULTIMO");
                    break;
                case 4:
                    graficosListView.Items.Add("PANTALLA 1");
                    graficosListView.Items.Add("SEDES");
                    graficosListView.Items.Add("PANTALLA 3");
                    graficosListView.Items.Add("PANTALLA 4");
                    break;
                default:
                    break;
            }
            autonomiasListView.ItemsSource = CCAA.Select(cir => cir.nombre).ToList();
            ActualizarHeaderAutonomias();
            circunscripcionesListView.ItemsSource = circunscripcionNames;
            datosListView.ItemsSource = listaDeDatos;
        }
        private void AdaptarColores()
        {
            SolidColorBrush fondo = (SolidColorBrush)Application.Current.FindResource("PrimaryHueDarkBrush");
            Background = fondo;
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnSondeoCentral.Background = color;
            btnAvance1.Background = color;
        }
        public void EscribirConexiones()
        {
            txtConexiones.Text = GetContentLabelConexion();
        }
        private void PrepararEstructuraDeCarpetas()
        {
            string rutaDatos = configuration.GetValue("rutaArchivos");
            if (!Directory.Exists(rutaDatos))
            {
                try
                {
                    Directory.CreateDirectory(rutaDatos);
                    Directory.CreateDirectory($"{rutaDatos}\\CSV");
                    Directory.CreateDirectory($"{rutaDatos}\\JSON");
                    Directory.CreateDirectory($"{rutaDatos}\\EXCEL");
                }
                catch (Exception)
                {
                    // Manejo de errores en caso de que no se pueda crear el directorio
                    MessageBox.Show($"Se ha producido un error al intentar crear las carpetas para guardar los archivos de datos en {rutaDatos}.", "Error al crear carpetas para guardar los datos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void IniciarEscuchadores()
        {
            escuchador = new Escuchador(conexionActiva);
            escuchador.ActualizacionActiva = actualizacionActiva;
            escuchador.Iniciar();
        }

        public void SetActualizacionActiva(bool activa)
        {
            actualizacionActiva = activa;
            if (escuchador != null)
            {
                escuchador.ActualizacionActiva = activa;
            }
        }

        private string? GetSelectedCircunscripcionNombre()
        {
            return circunscripcionesListView.SelectedItem?.ToString();
        }

        private string? GetSelectedAutonomiaNombre()
        {
            return autonomiasListView.SelectedItem?.ToString();
        }

        private string GetSelectedGrafico()
        {
            return graficosListView.SelectedValue?.ToString() ?? string.Empty;
        }

        private bool IsGraficoSedesSeleccionado()
        {
            return string.Equals(GetSelectedGrafico(), "SEDES", StringComparison.Ordinal);
        }

        public void Update(bool esActualizacionManual = false)
        {
            // En modo sondeo solo permitimos refresco manual (boton Actualizar)
            if (!oficiales && !esActualizacionManual)
            {
                return;
            }

            string? elementoSeleccionado = GetSelectedCircunscripcionNombre() ?? GetSelectedAutonomiaNombre();

            if (!string.IsNullOrWhiteSpace(elementoSeleccionado))
            {
                Circunscripcion seleccionada;
                BrainStormDTO dtoAnterior;

                conexionActiva.ChangeTracker.Clear();
                conexionActiva.SaveChangesAsync();

                if (actualizacionActiva)
                {
                    dtoAnterior = new BrainStormDTO(dto);
                    seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);

                    // Always fetch the unfiltered DTO containing all parties ordered by codigo
                    dto = ObtenerDTO(elementoSeleccionado);

                    if (EsCabeceraFaldon()) { UpdateFaldones(dtoAnterior); }
                    //Add cambios por actualizacion en vivo en cartones
                    if (EsCabeceraCarton()) { UpdateCartones(dtoAnterior); }
                    if (EsCabeceraSuperfaldon()) { UpdateSuperfaldones(); }

                    // Actualizar datos en la ventana de Pactos si estÃ¡ abierta
                    if (pactos != null)
                    {
                        string tipoGrafico = GetSelectedGrafico();
                        pactos.ActualizarDatos(oficiales, tipoGrafico, avance, tipoElecciones);
                    }

                    ActualizarInfoInterfaz(seleccionada, dto);
                    EscribirFichero();
                }
            }

        }
        private void UpdateFaldones(BrainStormDTO dtoAnterior)
        {
            List<PartidoDTO> partidosQueCambian = dtoAnterior.partidos.Except(dto.partidos, new PartidoDTOComparer()).ToList();
            List<PartidoDTO> partidosQueNoEstan = dtoAnterior.partidos.Where(par => !dto.partidos.Any(par2 => par2.codigo.Equals(par.codigo))).ToList();
            if (tickerDentro)
            {
                GestionarMedioSondeo();
                graficos.TickerActualiza(dto);
            }
            if (botonera?.tickerTDIn == true)
            {
                graficos.TickerTDActualiza(dtoAnterior, dto);
            }
        }
        private void UpdateCartones(BrainStormDTO dtoAnterior)
        {
            if (ultimoEscanoDentro)
            {
                bool hayCambioUltimo = graficos.ultimoActualiza(dto);
                if (hayCambioUltimo)
                {
                    return;
                }

                return;
            }

            if (sfUltimoDentro)
            {
                if (graficos.ultimoSuperCambia(dto))
                {
                    return;
                }

                graficos.sfActualiza();
                return;
            }

            if (fichaDentro)
            {
                graficos.fichaActualiza(oficiales, dtoAnterior, dto);
            }
            if (cartonPartidosDentro)
            {
                graficos.cartonPartidosActualiza(dto);
            }
            graficos.CartonesActualiza();
        }
        private void UpdateSuperfaldones()
        {
            if (sfUltimoDentro && graficos.ultimoSuperCambia(dto))
            {
                return;
            }

            graficos.sfActualiza();
        }

        public bool EsCabeceraSuperfaldon()
        {
            string cabecera = graficosHeader?.Header?.ToString() ?? "";
            string normalizada = NormalizarCabecera(cabecera);
            return normalizada == "SUPERFALDON"
                || normalizada == "SUPERFADON";
        }

        public bool EsCabeceraFaldon()
        {
            string cabecera = graficosHeader?.Header?.ToString() ?? "";
            return NormalizarCabecera(cabecera) == "FALDON";
        }

        public bool EsCabeceraCarton()
        {
            string cabecera = graficosHeader?.Header?.ToString() ?? "";
            return NormalizarCabecera(cabecera) == "CARTON";
        }

        private static string NormalizarCabecera(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            string descompuesto = texto.Trim().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(descompuesto.Length);
            foreach (char c in descompuesto)
            {
                UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
        }

        private bool CompararOrden(BrainStormDTO anterior, BrainStormDTO actual)
        {
            List<PartidoDTO> filtrado = anterior.partidos.Where(par => (oficiales ? par.escanios : par.escaniosHastaSondeo) > 0).ToList();
            for (int i = 0; i < filtrado.Count; i++)
            {
                if (filtrado[i].codigo != actual.partidos[i].codigo) { return false; }
            }
            return true;
        }

        private void datosListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReajustarSizeTabla();
        }

        //LOGICA CONFIG
        private void imgConfig_MouseEnter(object sender, MouseEventArgs e)
        {
            // Cambiar la imagen a la versiÃ³n azul cuando el ratÃ³n entra
            imgConfig.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/tuerca_pulsada.png", UriKind.Relative));
        }
        private void imgConfig_MouseLeave(object sender, MouseEventArgs e)
        {
            imgConfig.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/tuerca.png", UriKind.Relative));
        }
        private void imgConfig_Click(object sender, MouseButtonEventArgs e)
        {
            if (config == null)
            {
                config = new Config(conexionActiva, this);
                config.Show();
            }
            else
            {
                config.Activate();
            }

        }
        public void ReiniciarParametros()
        {
            configuration.ReadConfig();
            if (configuration.GetValue("activoPrime") == "1")
            {
                if (prime == null) { prime = OrdenesPrime.GetInstance(); }
                else { prime.ReiniciarConexion(); }
            }
            else { prime = null; }
            if (configuration.GetValue("activoIPF") == "1")
            {
                if (ipf == null) { ipf = OrdenesIPF.GetInstance(); }
                else { ipf.ReiniciarConexion(); }
            }
            else { ipf = null; }

            // Invalidar todos los singletons para que se recreen con la nueva conexion
            ConexionEntityFramework.InvalidateAllSingletons();

            conexionActiva.CloseConection();
            CambioDeElecciones();

            // Actualizar el escuchador con la nueva conexion
            escuchador.ActualizarConexion(conexionActiva);

            EscribirConexiones();
            SeleccionarCircunscripcion();
        }
        public void CambioTheme()
        {
            SolidColorBrush fondo = (SolidColorBrush)Application.Current.FindResource("PrimaryHueDarkBrush");
            Background = fondo;
            SolidColorBrush letra = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightForegroundBrush");
            Foreground = letra;
            if (config != null) { config.Background = fondo; }
            if (pactos != null) { pactos.Background = fondo; }
            if (botonera != null) { botonera.Background = fondo; }
        }

        /// <summary>
        /// Actualiza el panel visible de la botonera extra segÃºn tablasGraficosPrincipal.
        /// Llamado desde Config cuando cambia el combo de grÃ¡ficos.
        /// </summary>
        public void ActualizarBotoneraGrupo()
        {
            if (botonera != null) { botonera.ConfigurarGrupoActivo(); }

            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            switch (avance)
            {
                case 1: btnAvance1.Background = color; break;
                case 2: btnAvance2.Background = color; break;
                case 3: btnAvance3.Background = color; break;
                case 4: btnAvance4.Background = color; break;
            }
            switch (eleccionSeleccionada.Valor)
            {
                case 0:
                    if (oficiales) { btnOficialesCentral.Background = color; }
                    else { btnSondeoCentral.Background = color; }
                    break;
                case 1:
                    if (oficiales) { btnOficialesInferior.Background = color; }
                    else { btnSondeoInferior.Background = color; }
                    break;
                case 2:
                    if (oficiales) { btnOficialesSuperior.Background = color; }
                    else { btnSondeoSuperior.Background = color; }
                    break;
            }
        }

        /// <summary>
        /// Inicializa los controles de vÃ­deo leyendo la configuraciÃ³n (si existe).
        /// Guarda el estado inicial en el GraphicController para que el subsistema grÃ¡fico sepa la configuraciÃ³n.
        /// </summary>
        private void InitializeVideoConfigUI()
        {
            // Use ConfigManager already available as 'configuration'
            for (int i = 1; i <= 6; i++)
            {
                string keyMode = $"video{i}_isLive";
                string keyPath = $"video{i}_path";
                string modeVal = configuration.GetValue(keyMode) ?? "0";
                string pathVal = configuration.GetValue(keyPath) ?? string.Empty;

                // find controls by name
                var chk = this.FindName($"chkVideoLive{i}") as CheckBox;
                var txt = this.FindName($"txtVideoPath{i}") as TextBox;
                var btn = this.FindName($"btnBrowse{i}") as Button;

                if (chk != null)
                {
                    bool isLive = modeVal == "1";
                    chk.IsChecked = isLive;
                }
                if (txt != null)
                {
                    txt.Text = pathVal;
                    // disable textbox and browse when live
                    if (chk != null && chk.IsChecked == true)
                    {
                        txt.IsEnabled = false;
                        if (btn != null) btn.IsEnabled = false;
                    }
                }

                // notify graphics controller
                if (graficos != null)
                {
                    bool isLiveNotify = modeVal == "1";
                    graficos.SetVideoMode(i, isLiveNotify);
                    graficos.SetVideoPath(i, pathVal);
                }
            }
        }

        // Called when user toggles a checkbox between Directo (live) and Pregrabado
        private void VideoMode_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && int.TryParse(chk.Tag?.ToString(), out int index))
            {
                SetVideoModeFromUI(index, true);
            }
        }
        private void VideoMode_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && int.TryParse(chk.Tag?.ToString(), out int index))
            {
                SetVideoModeFromUI(index, false);
            }
        }

        private void SetVideoModeFromUI(int index, bool isLive)
        {
            // enable/disable path controls
            var txt = this.FindName($"txtVideoPath{index}") as TextBox;
            var btn = this.FindName($"btnBrowse{index}") as Button;
            if (txt != null) txt.IsEnabled = !isLive;
            if (btn != null) btn.IsEnabled = !isLive;

            // persist change
            configuration.SetValue($"video{index}_isLive", isLive ? "1" : "0");
            configuration.SaveConfig();

            // notify graphic controller
            graficos?.SetVideoMode(index, isLive);
        }

        // Browse button clicked -> open file dialog, set path, persist and notify
        private void BrowseVideo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int index))
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "Video files|*.mp4;*.mov;*.mkv;*.wmv;*.avi|All files|*.*";
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    string selected = dialog.FileName;
                    var txt = this.FindName($"txtVideoPath{index}") as TextBox;
                    if (txt != null)
                    {
                        txt.Text = selected;
                    }
                    // persist
                    configuration.SetValue($"video{index}_path", selected);
                    configuration.SaveConfig();

                    // notify
                    graficos?.SetVideoPath(index, selected);
                }
            }
        }

        // Optionally expose method to update path programmatically
        private void UpdateVideoPathFromCode(int index, string path)
        {
            var txt = this.FindName($"txtVideoPath{index}") as TextBox;
            if (txt != null) txt.Text = path;
            configuration.SetValue($"video{index}_path", path);
            configuration.SaveConfig();
            graficos?.SetVideoPath(index, path);
        }

        //LOGICA CAMBIO DE ELECCIONES
        private void btnSondeoCentral_Click(object sender, RoutedEventArgs e)
        {
            oficiales = false;
            ActualizarPorOfiSondeo();
            RestaurarColorOfiSondeo();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnSondeoCentral.Background = color;
            eleccionSeleccionada.Valor = 0;
        }
        private void btnOficialesCentral_Click(object sender, RoutedEventArgs e)
        {
            oficiales = true;
            ActualizarPorOfiSondeo();
            RestaurarColorOfiSondeo();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnOficialesCentral.Background = color;
            eleccionSeleccionada.Valor = 0;
        }
        private void btnSondeoInferior_Click(object sender, RoutedEventArgs e)
        {
            oficiales = false;
            ActualizarPorOfiSondeo();
            RestaurarColorOfiSondeo();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnSondeoInferior.Background = color;
            eleccionSeleccionada.Valor = 1;
        }
        private void btnOficialesInferior_Click(object sender, RoutedEventArgs e)
        {
            oficiales = true;
            ActualizarPorOfiSondeo();
            RestaurarColorOfiSondeo();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnOficialesInferior.Background = color;
            eleccionSeleccionada.Valor = 1;
        }
        private void btnSondeoSuperior_Click(object sender, RoutedEventArgs e)
        {
            oficiales = false;
            ActualizarPorOfiSondeo();
            RestaurarColorOfiSondeo();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnSondeoSuperior.Background = color;
            eleccionSeleccionada.Valor = 2;
        }
        private void btnOficialesSuperior_Click(object sender, RoutedEventArgs e)
        {
            oficiales = true;
            ActualizarPorOfiSondeo();
            RestaurarColorOfiSondeo();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnOficialesSuperior.Background = color;
            eleccionSeleccionada.Valor = 2;
        }
        private void RestaurarColorOfiSondeo()
        {
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueMidBrush");
            btnSondeoCentral.Background = color;
            btnOficialesCentral.Background = color;
            btnSondeoInferior.Background = color;
            btnOficialesInferior.Background = color;
            btnSondeoSuperior.Background = color;
            btnOficialesSuperior.Background = color;
        }
        private void ActualizarPorOfiSondeo()
        {
            graficos.SondeoUOficial(oficiales);
            // AdaptarEntorno eliminado - funcionalidad ya no necesaria
            ActualizarDatosEnTabla();
            string? elementoSeleccionado = GetSelectedCircunscripcionNombre() ?? GetSelectedAutonomiaNombre();
            if (!string.IsNullOrWhiteSpace(elementoSeleccionado))
            {
                Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);
                ObtenerDTO(elementoSeleccionado);
                GestionarMedioSondeo();
                ActualizarInfoInterfaz(seleccionada, dto);
                preparado = false;

                if (pactos != null)
                {
                    string tipoGrafico = GetSelectedGrafico();
                    pactos.ActualizarDatos(oficiales, tipoGrafico, avance, tipoElecciones);
                }
            }
        }
        private void ActualizarDatosEnTabla()
        {
            Binding bindingCol3;
            Binding bindingCol4;
            if (oficiales)
            {
                columna3.Header = "ESCAÃ‘OS";
                bindingCol3 = new Binding(oficiales ? "escanios" : "escaniosHastaSondeo");
                columna3.DisplayMemberBinding = bindingCol3;

                columna4.Header = "DIF ESC";
                bindingCol4 = new Binding("diferenciaEscanios");
                columna4.DisplayMemberBinding = bindingCol4;

                columna6.Width = datosListView.ActualWidth / 7;
                columna7.Width = datosListView.ActualWidth / 7;
            }
            else
            {
                columna3.Header = "ESC. DESDE";
                bindingCol3 = new Binding("escaniosDesdeSondeo");
                columna3.DisplayMemberBinding = bindingCol3;

                columna4.Header = "ESC. HASTA";
                bindingCol4 = new Binding(oficiales ? "escanios" : "escaniosHastaSondeo");
                columna4.DisplayMemberBinding = bindingCol4;

                if (graficosListView.SelectedItem == null || !IsGraficoSedesSeleccionado())
                {
                    columna6.Width = 0;
                    columna7.Width = 0;
                }

            }
            ReajustarSizeTabla();
        }

        private void CambioDeEleccionesHandler(object? sender, EventArgs e)
        {
            CambioDeElecciones();
        }
        private void CambioDeElecciones()
        {
            // Forzar recreaciÃ³n de controllers/services/repos con la nueva conexiÃ³n.
            ConexionEntityFramework.InvalidateAllSingletons();

            conexionActiva.CloseConection();
            conexionActiva.Dispose();
            conexionActiva = new ConexionEntityFramework(int.Parse(configuration.GetValue($"conexionDefault{eleccionSeleccionada.Valor + 1}")), eleccionSeleccionada.Valor + 1);
            CCAA.Clear();
            CargarCircunscripciones();
            CargarMedios();
            autonomiasListView.ItemsSource = CCAA.Select(cir => cir.nombre).ToList();
            ActualizarHeaderAutonomias();
            circunscripcionNames.Clear();
            listaDeDatos.Clear();
            EscribirConexiones();
            DesplegarCircunscripciones();
            escuchador.ActualizarConexion(conexionActiva);
            bool europa = eleccionSeleccionada.Valor == 1;
            graficos.CambioElecciones(europa);
            autonomiasListView.SelectedIndex = 0;

        }

        //LOGICA BOTONES DE AVANCE
        private void btnAvance1_Click(object sender, RoutedEventArgs e)
        {
            RestaurarColorAvance();
            avance = 1;
            ActualizarPorAvance();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnAvance1.Background = color;
        }
        private void btnAvance2_Click(object sender, RoutedEventArgs e)
        {
            RestaurarColorAvance();
            avance = 2;
            ActualizarPorAvance();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnAvance2.Background = color;
        }
        private void btnAvance3_Click(object sender, RoutedEventArgs e)
        {
            RestaurarColorAvance();
            avance = 3;
            ActualizarPorAvance();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnAvance3.Background = color;
        }
        private void btnAvance4_Click(object sender, RoutedEventArgs e)
        {
            RestaurarColorAvance();
            avance = 4;
            ActualizarPorAvance();
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueLightBrush");
            btnAvance4.Background = color;
        }
        private void ActualizarPorAvance()
        {
            string? circSeleccionada = GetSelectedCircunscripcionNombre();
            if (!string.IsNullOrWhiteSpace(circSeleccionada))
            {
                Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(circSeleccionada);
                bool filtroSedes = !sedeDentro && (graficosListView.SelectedItem == null || !IsGraficoSedesSeleccionado());
                dto = ObtenerDTO(seleccionada.nombre);
                GestionarMedioSondeo();
                ActualizarInfoInterfaz(seleccionada, dto);
                preparado = false;
            }
            else
            {
                string? autonomiaSeleccionada = GetSelectedAutonomiaNombre();
                if (string.IsNullOrWhiteSpace(autonomiaSeleccionada))
                {
                    return;
                }

                Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(autonomiaSeleccionada);
                bool filtroSedes = !sedeDentro && (graficosListView.SelectedItem == null || !IsGraficoSedesSeleccionado());
                dto = ObtenerDTO(seleccionada.nombre);
                GestionarMedioSondeo();
                ActualizarInfoInterfaz(seleccionada, dto);
                preparado = false;
            }
        }
        private void RestaurarColorAvance()
        {
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueMidBrush");
            btnAvance1.Background = color;
            btnAvance2.Background = color;
            btnAvance3.Background = color;
            btnAvance4.Background = color;
        }

        //LOGICA SELECCION DE CIRCUNSCRIPCIONES
        private void autonomiasListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DesplegarCircunscripciones();
        }
        private void DesplegarCircunscripciones()
        {
            preparado = false;
            datosListView.SelectedItem = null;

            string? elementoSeleccionado = GetSelectedAutonomiaNombre();
            if (string.IsNullOrWhiteSpace(elementoSeleccionado))
            {
                return;
            }

            List<Circunscripcion> circunscripcionesSeleccionadas;
            if (regional)
            {
                circunscripcionesSeleccionadas = CircunscripcionController.GetInstance(conexionActiva).FindAllCircunscripcionesByNameAutonomiaRegional(elementoSeleccionado).ToList();
            }
            else
            {
                circunscripcionesSeleccionadas = CircunscripcionController.GetInstance(conexionActiva).FindAllCircunscripcionesByNameAutonomia(elementoSeleccionado).ToList();
            }

            circunscripcionNames.Clear();
            circunscripcionesSeleccionadas.ForEach(cir =>
            {
                circunscripcionNames.Add(cir.nombre);
            });

            Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);
            ObtenerDTO(elementoSeleccionado);
            GestionarMedioSondeo();
            ActualizarInfoInterfaz(seleccionada, dto);
        }
        private void circunscripcionesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SeleccionarCircunscripcion();
        }
        private void SeleccionarCircunscripcion()
        {
            datosListView.SelectedItem = null;
            preparado = false;

            string? elementoSeleccionado = GetSelectedCircunscripcionNombre();
            if (string.IsNullOrWhiteSpace(elementoSeleccionado))
            {
                return;
            }

            autonomiasListView.SelectedItem = null;

            Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);
            dto = ObtenerDTO(elementoSeleccionado);

            // Aplicar el medio actualmente seleccionado al nuevo DTO
            GestionarMedioSondeo();

            ActualizarInfoInterfaz(seleccionada, dto);
        }
        private void ActualizarInfoInterfaz(Circunscripcion seleccionada, BrainStormDTO dto)
        {
            lblEscanios.Content = seleccionada.escanios.ToString();
            lblEscrutado.Content = seleccionada.escrutado.ToString();
            lblParticipacion.Content = avance switch
            {
                1 => seleccionada.avance1.ToString(),
                2 => seleccionada.avance2.ToString(),
                3 => seleccionada.avance3.ToString(),
                4 => seleccionada.participacionFinal.ToString(),
                _ => seleccionada.participacionFinal.ToString()
            };

            lblParticipacionHist.Content = avance switch
            {
                1 => seleccionada.avance1Hist.ToString(),
                2 => seleccionada.avance2Hist.ToString(),
                3 => seleccionada.avance3Hist.ToString(),
                4 => seleccionada.participacionHist.ToString(),
                _ => seleccionada.participacionHist.ToString()
            };

            // Usar la misma lÃ³gica de filtrado que en ActualizarInfoInterfaz(BrainStormDTO dto)
            listaDeDatos.Clear();

            // Obtener el grÃ¡fico seleccionado
            string graficoSeleccionado = graficosListView.SelectedItem?.ToString() ?? "";

            // Obtener los datos filtrados segÃºn el grÃ¡fico y el estado (oficiales/sondeo)
            List<CPDataDTO> cpdatas = ObtenerDatosParaTabla(graficoSeleccionado, dto);

            if (partidoSeleccionado != null)
            {
                partidoSeleccionado = dto.partidos.Find(par => par.siglas.Equals(partidoSeleccionado.siglas));
            }
            cpdatas.ForEach(listaDeDatos.Add);
        }
        private void ActualizarInfoInterfaz(BrainStormDTO dto)
        {
            listaDeDatos.Clear();

            // Obtener el grÃ¡fico seleccionado
            string graficoSeleccionado = graficosListView.SelectedItem?.ToString() ?? "";

            // Obtener los datos filtrados segÃºn el grÃ¡fico y el estado (oficiales/sondeo)
            List<CPDataDTO> cpdatas = ObtenerDatosParaTabla(graficoSeleccionado, dto);

            if (partidoSeleccionado != null)
            {
                partidoSeleccionado = dto.partidos.Find(par => par.siglas.Equals(partidoSeleccionado.siglas));
            }
            cpdatas.ForEach(listaDeDatos.Add);
        }

        /// <summary>
        /// Obtiene los datos a mostrar en la tabla segÃºn el grÃ¡fico seleccionado y el estado (oficiales/sondeo)
        /// </summary>
        private List<CPDataDTO> ObtenerDatosParaTabla(string graficoSeleccionado, BrainStormDTO dto)
        {
            List<CPDataDTO> allCPDatas = CPDataDTO.FromBSDto(dto);

            return graficoSeleccionado switch
            {
                "CUENTA ATRÃS" => new List<CPDataDTO>(), // No mostrar datos
                "FICHAS" => FiltrarDatosParaFichas(allCPDatas),
                "SEDES" => FiltrarDatosParaSedes(allCPDatas),
                "CARTÃ“N PARTIDOS" => FiltrarDatosParaCartonPartidos(allCPDatas),
                "ÃšLTIMO ESCAÃ‘O" => FiltrarDatosParaUltimoEscano(allCPDatas, dto),
                _ => allCPDatas // Por defecto, mostrar todos los datos
            };
        }

        /// <summary>
        /// Filtra datos para el grÃ¡fico "FICHAS"
        /// - Sondeo: Siglas, EscaÃ±os desde Sondeo, EscaÃ±os hasta sondeo, EscaÃ±os histÃ³ricos (solo con al menos 1 escaÃ±o en Sondeo)
        /// - Oficial: Siglas, EscaÃ±os, %voto, EscaÃ±os hist, Diferencia de escaÃ±os (solo con al menos 1 escaÃ±o Oficial)
        /// </summary>
        private List<CPDataDTO> FiltrarDatosParaFichas(List<CPDataDTO> allCPDatas)
        {
            List<CPDataDTO> filtrados;

            if (oficiales)
            {
                // Filtrar solo partidos con al menos 1 escaÃ±o en datos oficiales
                filtrados = allCPDatas.Where(p => int.TryParse(p.escanios, out int esc) && esc > 0).ToList();
            }
            else
            {
                // Filtrar solo partidos con al menos 1 escaÃ±o en sondeo
                filtrados = allCPDatas.Where(p => int.TryParse(p.escaniosHastaSondeo, out int esc) && esc > 0).ToList();
                // Ordenar usando CPDataComparerSondeo (orden descendente por escaniosHastaSondeo > escaniosDesdeSondeo)
                filtrados.Sort((a, b) => -new CPDataComparerSondeo().Compare(a, b));
            }

            return filtrados;
        }

        /// <summary>
        /// Filtra datos para el grÃ¡fico "SEDES"
        /// - Muestra todos los partidos ordenados por id
        /// - Si no es oficial (oficiales==false), devuelve lista vacÃ­a
        /// - Datos mostrados: Siglas, EscaÃ±os, %voto, nÃºmero de Votantes, diferencia de votantes
        /// </summary>
        private List<CPDataDTO> FiltrarDatosParaSedes(List<CPDataDTO> allCPDatas)
        {
            // SEDES solo existe para datos oficiales
            if (!oficiales)
            {
                return new List<CPDataDTO>();
            }

            // Ya estÃ¡n ordenados por id, simplemente devolver todos
            return allCPDatas;
        }

        /// <summary>
        /// Filtra datos para el grÃ¡fico "CARTÃ“N PARTIDOS"
        /// - Solo existe para datos oficiales (devuelve lista vacÃ­a en sondeo)
        /// - Muestra todos los partidos (incluso los sin representaciÃ³n)
        /// - Datos mostrados: Siglas, EscaÃ±os, %voto, Votantes
        /// - Ordenado por CPDataComparer (mÃ¡s escaÃ±os > mÃ¡s %voto > mÃ¡s votantes)
        /// </summary>
        private List<CPDataDTO> FiltrarDatosParaCartonPartidos(List<CPDataDTO> allCPDatas)
        {
            // CARTÃ“N PARTIDOS solo existe para datos oficiales
            if (!oficiales)
            {
                return new List<CPDataDTO>();
            }

            // Devolver todos los partidos ordenados por comparer
            List<CPDataDTO> filtrados = new List<CPDataDTO>(allCPDatas);
            filtrados.Sort((a, b) => -new CPDataComparer().Compare(a, b));
            return filtrados;
        }

        /// <summary>
        /// Filtra datos para el grÃ¡fico "ÃšLTIMO ESCAÃ‘O"
        /// - Solo existe para datos oficiales (devuelve lista vacÃ­a en sondeo)
        /// - Muestra solo los dos partidos que disputan el Ãºltimo escaÃ±o
        /// - Datos mostrados: Siglas, Restos
        /// - Orden: primero el partido con esUltimoEscano=1, luego el de luchaUltimoEscano=1
        /// </summary>
        private List<CPDataDTO> FiltrarDatosParaUltimoEscano(List<CPDataDTO> allCPDatas, BrainStormDTO dto)
        {
            // ÃšLTIMO ESCAÃ‘O solo existe para datos oficiales
            if (!oficiales || dto == null)
            {
                return new List<CPDataDTO>();
            }

            List<CPDataDTO> ultimoEscano = new List<CPDataDTO>();

            // Buscar el partido que tiene el Ãºltimo escaÃ±o
            PartidoDTO? partidoUltimo = dto.partidos.Find(p => p.esUltimoEscano == 1);
            // Buscar el partido que lucha por el Ãºltimo escaÃ±o
            PartidoDTO? partidoLucha = dto.partidos.Find(p => p.luchaUltimoEscano == 1);

            // Agregar el partido con el Ãºltimo escaÃ±o primero
            if (partidoUltimo != null)
            {
                CPDataDTO? cpDataUltimo = allCPDatas.FirstOrDefault(c => c.codigo == partidoUltimo.codigo);
                if (cpDataUltimo != null)
                {
                    ultimoEscano.Add(cpDataUltimo);
                }
            }

            // Agregar el partido que lucha por el Ãºltimo escaÃ±o
            if (partidoLucha != null)
            {
                CPDataDTO? cpDataLucha = allCPDatas.FirstOrDefault(c => c.codigo == partidoLucha.codigo);
                if (cpDataLucha != null)
                {
                    ultimoEscano.Add(cpDataLucha);
                }
            }

            return ultimoEscano;
        }



        //ADAPTACION DE DATOS DEPENDIENDO DE ELEMENTO SELECCIONADO
        private void graficosListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graficosListView.SelectedIndex != -1)
            {
                string tipoGrafico = GetSelectedGrafico();
                if (string.IsNullOrWhiteSpace(tipoGrafico))
                {
                    return;
                }

                AdaptarTablaDatos(tipoGrafico);
                if (dto != null) { ActualizarInfoInterfaz(dto); }
            }
        }
        private void AdaptarTablaDatos(string tipoGrafico)
        {
            switch (tipoGrafico)
            {
                case "CUENTA ATRÃS":
                    // Ocultar la lista de datos al mostrar la cuenta atrÃ¡s
                    datosListView.Visibility = Visibility.Collapsed;
                    break;

                case "FICHAS":
                    // Restaurar visibilidad
                    datosListView.Visibility = Visibility.Visible;

                    // Ocultar columna CÃ³digo
                    columna1.Width = 0;

                    if (oficiales)
                    {
                        // Oficial: Siglas, EscaÃ±os, %voto, EscaÃ±os hist, Diferencia de escaÃ±os
                        columna3.Header = "ESCAÃ‘OS";
                        columna3.DisplayMemberBinding = new Binding("escanios");

                        columna4.Header = "% VOTO";
                        columna4.DisplayMemberBinding = new Binding("porcentajeVoto");

                        columna5.Header = "ESC. HIST";
                        columna5.DisplayMemberBinding = new Binding("escaniosHistoricos");

                        columna6.Header = "DIF ESC";
                        columna6.DisplayMemberBinding = new Binding("diferenciaEscanios");
                        columna6.Width = datosListView.ActualWidth / 7;

                        columna7.Width = 0; // Ocultar columna 7
                    }
                    else
                    {
                        // Sondeo: Siglas, EscaÃ±os desde Sondeo, EscaÃ±os hasta sondeo, EscaÃ±os histÃ³ricos
                        columna3.Header = "ESC. DESDE";
                        columna3.DisplayMemberBinding = new Binding("escaniosDesdeSondeo");

                        columna4.Header = "ESC. HASTA";
                        columna4.DisplayMemberBinding = new Binding("escaniosHastaSondeo");

                        columna5.Header = "ESC. HIST";
                        columna5.DisplayMemberBinding = new Binding("escaniosHistoricos");

                        columna6.Width = 0; // Ocultar columna 6
                        columna7.Width = 0; // Ocultar columna 7
                    }

                    if (dto != null)
                    {
                        ObtenerDTO(dto.circunscripcionDTO.nombre);
                        GestionarMedioSondeo();
                        ActualizarInfoInterfaz(dto);
                    }
                    break;

                case "SEDES":
                    // Restaurar visibilidad al volver de "CUENTA ATRÃS"
                    datosListView.Visibility = Visibility.Visible;

                    // Ocultar columna CÃ³digo
                    columna1.Width = 0;

                    if (oficiales)
                    {
                        // Mostrar: Siglas, EscaÃ±os, %voto, nÃºmero de Votantes, diferencia de votantes
                        columna3.Header = "ESCAÃ‘OS";
                        columna3.DisplayMemberBinding = new Binding("escanios");

                        columna4.Header = "% VOTO";
                        columna4.DisplayMemberBinding = new Binding("porcentajeVoto");

                        columna5.Header = "VOTANTES";
                        columna5.DisplayMemberBinding = new Binding("votantes");

                        columna6.Header = "DIF VOT";
                        columna6.DisplayMemberBinding = new Binding("diferenciaVotantes");
                        columna6.Width = datosListView.ActualWidth / 7;

                        columna7.Width = 0; // Ocultar columna 7
                    }
                    else
                    {
                        // No mostrar datos en sondeo
                        datosListView.Visibility = Visibility.Collapsed;
                    }

                    if (dto != null)
                    {
                        ObtenerDTO(dto.circunscripcionDTO.nombre);
                        GestionarMedioSondeo();
                        ActualizarInfoInterfaz(dto);
                    }
                    break;

                case "CARTÃ“N PARTIDOS":
                    // Restaurar visibilidad
                    datosListView.Visibility = Visibility.Visible;

                    // Ocultar columna CÃ³digo
                    columna1.Width = 0;

                    if (oficiales)
                    {
                        // Oficial: Siglas, EscaÃ±os, %voto, Votantes
                        columna3.Header = "ESCAÃ‘OS";
                        columna3.DisplayMemberBinding = new Binding("escanios");

                        columna4.Header = "% VOTO";
                        columna4.DisplayMemberBinding = new Binding("porcentajeVoto");
                        columna4.Width = datosListView.ActualWidth / 5; // Restaurar visibilidad

                        columna5.Header = "VOTANTES";
                        columna5.DisplayMemberBinding = new Binding("votantes");
                        columna5.Width = datosListView.ActualWidth / 5; // Restaurar visibilidad

                        columna6.Width = 0; // Ocultar columna 6
                        columna7.Width = 0; // Ocultar columna 7
                    }
                    else
                    {
                        // No mostrar datos en sondeo
                        datosListView.Visibility = Visibility.Collapsed;
                    }

                    if (dto != null)
                    {
                        dto = ObtenerDTO(dto.circunscripcionDTO.nombre);
                        GestionarMedioSondeo();
                        ActualizarInfoInterfaz(dto);
                    }
                    break;

                case "ÃšLTIMO ESCAÃ‘O":
                    // Restaurar visibilidad
                    datosListView.Visibility = Visibility.Visible;

                    // Ocultar columna CÃ³digo
                    columna1.Width = 0;

                    if (oficiales)
                    {
                        // Oficial: Siglas, Restos
                        columna3.Header = "RESTOS";
                        columna3.DisplayMemberBinding = new Binding("restos");

                        columna4.Width = 0; // Ocultar columna 4
                        columna5.Width = 0; // Ocultar columna 5
                        columna6.Width = 0; // Ocultar columna 6
                        columna7.Width = 0; // Ocultar columna 7
                    }
                    else
                    {
                        // No mostrar datos en sondeo
                        datosListView.Visibility = Visibility.Collapsed;
                    }

                    if (dto != null)
                    {
                        dto = ObtenerDTO(dto.circunscripcionDTO.nombre);
                        GestionarMedioSondeo();
                        ActualizarInfoInterfaz(dto);
                    }
                    break;

                case "INDEPENDENTISMO":
                    // Restaurar visibilidad al volver de "CUENTA ATRÃS"
                    datosListView.Visibility = Visibility.Visible;
                    ActualizarDatosEnTabla();
                    if (dto != null)
                    {
                        ObtenerDTO(dto.circunscripcionDTO.nombre);
                        GestionarMedioSondeo();
                        ActualizarInfoInterfaz(dto);
                    }
                    break;

                default:
                    // Restaurar visibilidad al volver de "CUENTA ATRÃS"
                    datosListView.Visibility = Visibility.Visible;
                    ActualizarDatosEnTabla();
                    if (dto != null)
                    {
                        ObtenerDTO(dto.circunscripcionDTO.nombre);
                        GestionarMedioSondeo();
                        ActualizarInfoInterfaz(dto);
                    }
                    break;
            }

            // Ajustar tamaÃ±o de columnas al final de AdaptarTablaDatos
            ReajustarSizeTabla();
        }
        private void ReajustarSizeTabla()
        {
            // Contar las columnas visibles (ancho > 0)
            List<GridViewColumn> columnasVisibles = new List<GridViewColumn>();
            if (columna1.Width > 0) columnasVisibles.Add(columna1);
            if (columna2.Width > 0) columnasVisibles.Add(columna2);
            if (columna3.Width > 0) columnasVisibles.Add(columna3);
            if (columna4.Width > 0) columnasVisibles.Add(columna4);
            if (columna5.Width > 0) columnasVisibles.Add(columna5);
            if (columna6.Width > 0) columnasVisibles.Add(columna6);
            if (columna7.Width > 0) columnasVisibles.Add(columna7);

            // Si no hay columnas visibles o el ancho es 0, no hacer nada
            if (columnasVisibles.Count == 0 || datosListView.ActualWidth <= 0)
                return;

            // Obtener el ancho total disponible considerando mÃ¡rgenes
            double anchoTotal = datosListView.ActualWidth;

            // Dividir el ancho total entre las columnas visibles para ocupar todo el espacio
            double anchoColumnaa = anchoTotal / columnasVisibles.Count;

            foreach (var columna in columnasVisibles)
            {
                columna.Width = anchoColumnaa;
            }
        }

        private void datosListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dto != null)
            {
                CPDataDTO? dataActual = datosListView.SelectedItem != null ? datosListView.SelectedItem as CPDataDTO : null;
                if (dataActual != null)
                {
                    partidoSeleccionado = dto.partidos.Find(par => string.Equals(par.codigo, dataActual.codigo));
                    if (graficosListView.SelectedItem != null && string.Equals(graficosListView.SelectedValue, "SEDES") && partidoSeleccionado != null && sedeDentro)
                    {
                        graficos.SedesEncadena(partidoSeleccionado);
                    }
                }
            }
        }

        private void datosListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dto == null || datosListView.SelectedItem == null)
            {
                return;
            }

            CPDataDTO? dataActual = datosListView.SelectedItem as CPDataDTO;
            if (dataActual == null)
            {
                return;
            }

            partidoSeleccionado = dto.partidos.Find(par => string.Equals(par.codigo, dataActual.codigo));
            if (partidoSeleccionado == null || string.IsNullOrWhiteSpace(partidoSeleccionado.codigo))
            {
                return;
            }

            if (!EsCabeceraSuperfaldon() || !sfCarruselDentro)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(sfCodigoSedeDesplegada))
            {
                graficos.sfDesplegarSede(partidoSeleccionado.codigo);
                sfCodigoSedeDesplegada = partidoSeleccionado.codigo;
                return;
            }

            if (string.Equals(sfCodigoSedeDesplegada, partidoSeleccionado.codigo, StringComparison.Ordinal))
            {
                graficos.sfReplegarSede();
                sfCodigoSedeDesplegada = null;
                return;
            }

            graficos.sfEncadenarSede(partidoSeleccionado.codigo);
            sfCodigoSedeDesplegada = partidoSeleccionado.codigo;
        }

        //LOGICA DE FICHEROS
        private BrainStormDTO ObtenerDTO(string circunscripcion)
        {
            // Always fetch unfiltered DTO (SinFiltrar) which contains all parties
            // ordered by the comparers but without filtering by escaÃ±os > 0
            dto = oficiales
                ? BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficialSinFiltrar(circunscripcion, avance, tipoElecciones)
                : BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeoSinFiltrar(circunscripcion, avance, tipoElecciones);

            // Guardar los valores originales de escaÃ±os de sondeo
            GuardarValoresOriginalesSondeo(dto);

            // Asegurar el orden correcto de los partidos
            if (dto != null && dto.partidos != null)
            {
                dto.partidos.Sort(new PartidoDTOComparerUnified(oficiales));
                dto.partidos.Reverse();
            }

            return dto ?? throw new InvalidOperationException("No se ha podido construir el DTO de BrainStorm.");
        }

        private void GuardarValoresOriginalesSondeo(BrainStormDTO dtoActual)
        {
            if (dtoActual != null)
            {
                foreach (var partido in dtoActual.partidos)
                {
                    // Guardar los valores originales con una clave Ãºnica por partido en esta circunscripciÃ³n
                    string clave = $"{dtoActual.circunscripcionDTO.codigo}_{partido.codigo}";
                    valoresOriginalesSondeo[clave] = (partido.escaniosDesdeSondeo, partido.escaniosHastaSondeo);
                }
            }
        }

        private List<PartidoDTO> ObtenerPartidosAlineadosParaCsv(BrainStormDTO source)
        {
            List<PartidoDTO> actuales = source.partidos
                .Where(p => !string.IsNullOrWhiteSpace(p.codigo))
                .OrderBy(p => p.codigo)
                .ToList();

            // Si estamos ya en la circunscripciÃ³n general, no hay que rellenar huecos.
            if (source.circunscripcionDTO.codigo.EndsWith("00000"))
            {
                return actuales;
            }

            try
            {
                string codigoPlantilla;
                if (tipoElecciones == 1)
                {
                    codigoPlantilla = "9900000";
                }
                else
                {
                    string codigoRegional = configuration.GetValue($"codigoRegionalBD{eleccionSeleccionada.Valor + 1}");
                    if (string.IsNullOrWhiteSpace(codigoRegional) && source.circunscripcionDTO.codigo.Length >= 2)
                    {
                        codigoRegional = source.circunscripcionDTO.codigo.Substring(0, 2);
                    }

                    codigoPlantilla = $"{codigoRegional}00000";
                }

                BrainStormDTO plantilla = oficiales
                    ? BrainStormController.GetInstance(conexionActiva).FindByIdCircunscripcionOficialSinFiltrar(codigoPlantilla, avance, tipoElecciones)
                    : BrainStormController.GetInstance(conexionActiva).FindByIdCircunscripcionSondeoSinFiltrar(codigoPlantilla, avance, tipoElecciones);

                List<PartidoDTO> partidosPlantilla = plantilla.partidos
                    .Where(p => !string.IsNullOrWhiteSpace(p.codigo))
                    .OrderBy(p => p.codigo)
                    .ToList();

                if (partidosPlantilla.Count == 0)
                {
                    return actuales;
                }

                var actualesPorCodigo = actuales
                    .GroupBy(p => p.codigo)
                    .ToDictionary(g => g.Key, g => g.First());

                var codigosPlantilla = new HashSet<string>(partidosPlantilla.Select(p => p.codigo));
                var alineados = new List<PartidoDTO>(partidosPlantilla.Count);

                foreach (PartidoDTO partidoPlantilla in partidosPlantilla)
                {
                    if (actualesPorCodigo.TryGetValue(partidoPlantilla.codigo, out PartidoDTO? partidoActual))
                    {
                        alineados.Add(partidoActual);
                    }
                    else
                    {
                        // Partido ausente en esta circunscripciÃ³n: se deja hueco en el CSV.
                        alineados.Add(new PartidoDTO());
                    }
                }

                // Si aparece algÃºn partido fuera de plantilla, se conserva al final para no perder datos.
                foreach (PartidoDTO partidoActual in actuales)
                {
                    if (!codigosPlantilla.Contains(partidoActual.codigo))
                    {
                        alineados.Add(partidoActual);
                    }
                }

                return alineados;
            }
            catch
            {
                return actuales;
            }
        }

        private bool UsarFormatoBrainStormCsvOld()
        {
            string formato = configuration.GetValue("formatoBrainStormCsv");
            return string.Equals(formato?.Trim(), "OLD", StringComparison.OrdinalIgnoreCase);
        }

        private BrainStormDTO CrearDtoNewParaCsv(BrainStormDTO source)
        {
            var ordered = new BrainStormDTO(source);
            ordered.partidos = ObtenerPartidosAlineadosParaCsv(source);
            ordered.numPartidos = ordered.partidos.Count;
            return ordered;
        }

        private BrainStormDTO CrearDtoOldParaCsv(BrainStormDTO source)
        {
            var legacy = new BrainStormDTO(source);
            bool sourceOficiales = source.oficiales;
            legacy.partidos = source.partidos
                .Where(p => !string.IsNullOrWhiteSpace(p.codigo))
                .Where(p => sourceOficiales ? p.escanios > 0 : p.escaniosHastaSondeo > 0)
                .ToList();
            legacy.partidos.Sort(new PartidoDTOComparerUnified(sourceOficiales));
            legacy.partidos.Reverse();
            legacy.numPartidos = legacy.partidos.Count;
            return legacy;
        }

        private async void EscribirFichero(bool desdeSedes = false)
        {
            if (dto != null)
            {
                try
                {
                    preparado = true;
                    bool modoOld = UsarFormatoBrainStormCsvOld();
                    string nombreSondeo = cmbSondeo.SelectedItem?.ToString() ?? "";

                    if (modoOld)
                    {
                        // Formato legacy (VersionCarmen): orden por escanos/votos y filtrado por escanos > 0.
                        var dtoOld = CrearDtoOldParaCsv(dto);
                        await dtoOld.ToCsv("BrainStorm", nombreSondeo, legacySinColumnaEscanios: true);

                        // Siempre generar tambien la version por codigo (equivalente a NEW).
                        var dtoCodigo = CrearDtoNewParaCsv(dto);
                        await dtoCodigo.ToCsv("Brainstorm_Codigo", nombreSondeo);

                        // En modo sondeo, generar copia adicional dedicada.
                        if (!dto.oficiales)
                        {
                            await dtoOld.ToCsv("Brainstorm_Sondeo_Codigo", nombreSondeo, legacySinColumnaEscanios: true);
                        }
                    }
                    else
                    {
                        var dtoNew = CrearDtoNewParaCsv(dto);
                        await dtoNew.ToCsv("BrainStorm", nombreSondeo);
                    }
                }
                catch
                {
                    // Keep behaviour non-intrusive on filesystem errors
                }

                if (partidoSeleccionado != null)
                {
                    SedesDTO sede = SedesDTO.FromPartidoDTO(partidoSeleccionado, conexionActiva);
                    await sede.ToCsv();
                }

                if (graficos.primeActivo.Valor == 1)
                {
                    // La reasignaciÃ³n de dto aquÃ­ se ha eliminado porque sobreescribÃ­a los valores
                    // de sondeo aplicados por GestionarMedioSondeo(). 
                    // await EscribirJsonPrimeAsync();
                }
                // await EscribirJsonPrimeAsync();
            }
        }
        private async Task EscribirJsonPrimeAsync()
        {
            Recuentos rs = new Recuentos();
            List<Recuentos> recuentos = rs.GetRecuentos(oficiales, avance, tipoElecciones, conexionActiva);
            await rs.ToJson(recuentos);
        }

        //LOGICA DE BOTONES GENERICOS
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (ipf != null) { ipf.Reset(); }
            if (prime != null) { prime.Reset(); }
            preparado = false;
            sondeoEnElAire = false;
            tickerDentro = false;
            sedeDentro = false;
            sfCarruselDentro = false;
            sfUltimoDentro = false;
            sfCodigoSedeDesplegada = null;
            if (pactos != null) { pactos.pactoDentro = false; }
        }
        private void btnPrepara_Click(object sender, RoutedEventArgs e)
        {
            EscribirFichero();
        }
        private void btnEntra_Click(object sender, RoutedEventArgs e)
        {
            if (!preparado) { EscribirFichero(); }
            if (EsCabeceraFaldon()) { EntraFaldon(); }
            if (EsCabeceraCarton()) { EntraCarton(); }
            if (EsCabeceraSuperfaldon()) { EntraSuperfaldon(); }
            if (string.Equals(graficosHeader.Header, "PANTALLA")) { EntraSuperfaldon(); }
            if (string.Equals(graficosHeader.Header, "REALIDAD AUMENTADA")) { EntraSuperfaldon(); }
            if (string.Equals(graficosHeader.Header, "DRON")) { EntraSuperfaldon(); }
        }
        private void btnSale_Click(object sender, RoutedEventArgs e)
        {
            if (EsCabeceraFaldon()) { SaleFaldon(); }
            if (EsCabeceraCarton()) { SaleCarton(); }
            if (EsCabeceraSuperfaldon()) { SaleSuperfaldon(); }
        }
        private void btnActualiza_Click(object sender, RoutedEventArgs e)
        {
            bool temp = actualizacionActiva;
            actualizacionActiva = true;
            Update(true);
            // Actualizar siempre la lista de medios por si hay nuevos
            CargarMedios();
            actualizacionActiva = temp;
        }
        private void btnPactos_Click(object sender, RoutedEventArgs e)
        {
            if (dto != null)
            {
                if (pactos == null)
                {
                    // Obtener el tipo de grÃ¡fico actual
                    string tipoGrafico = graficosListView.SelectedItem?.ToString() ?? "";
                    pactos = new Pactos(dto, oficiales, tipoGrafico);
                    pactos.Show();
                }
                else { pactos.Activate(); }
            }
            else
            {
                MessageBox.Show($"Seleccione alguna circunscripciÃ³n para ver su pestaÃ±a de pactos", "CircunscipciÃ³n no seleccionada", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //LOGICA PARA LOS TIPOS DE GRÃFICOS DISTINTOS
        private void EntraFaldon()
        {
            if (dto != null && graficosListView.SelectedIndex != -1)
            {
                string tipoGrafico = GetSelectedGrafico();
                switch (tipoGrafico)
                {
                    case "CUENTA ATRÃS":
                        int segundos = CalcularSegundosHastaHora();
                        // if (segundos > 0)
                        // {
                        graficos.EntraReloj(segundos);
                        graficos.SubirRotulosPrimeEsp();
                        // }
                        break;
                    case "FICHAS":
                        if (sondeoEnElAire && oficiales)
                        {
                            graficos.DeSondeoAOficiales();
                            sondeoEnElAire = false;
                        }
                        else
                        {
                            if (tickerDentro) { graficos.TickerEncadena(oficiales, dto); }
                            else
                            {
                                graficos.TickerEntra(oficiales, dto);
                                graficos.SubirRotulosPrimeEsp(1000);
                            }

                            if (!oficiales) { sondeoEnElAire = true; }
                        }
                        tickerDentro = true;
                        break;
                    case "SEDES":
                        if (partidoSeleccionado != null)
                        {
                            if (!sedeDentro)
                            {
                                graficos.SedesEntra(partidoSeleccionado);
                            }
                            else { graficos.SedesEncadena(partidoSeleccionado); }
                            sedeDentro = true;
                        }
                        break;
                    default: break;
                }
            }
        }
        private void EntraCarton()
        {
            if (dto != null && graficosListView.SelectedIndex != -1)
            {
                string tipoGrafico = GetSelectedGrafico();
                switch (tipoGrafico)
                {
                    case "PARTICIPACIÃ“N":
                        if (participacionDentro) { graficos.participacionEncadena(dto, avance); }
                        else
                        {
                            graficos.participacionEntra(dto, avance);
                            participacionDentro = true;
                        }
                        break;
                    case "FICHAS":
                        if (partidoSeleccionado == null)
                        {
                            break;
                        }

                        if (fichaDentro) { graficos.fichaEncadena(oficiales, dto, partidoSeleccionado); }
                        else
                        {
                            graficos.fichaEntra(oficiales, dto, partidoSeleccionado);
                            fichaDentro = true;
                        }
                        break;
                    case "MAYORÃAS":
                        if (mayoriasDentro) { graficos.mayoriasEncadena(dto); }
                        else
                        {
                            graficos.mayoriasEntra(dto);
                            mayoriasDentro = true;
                        }
                        break;
                    case "CCAA":
                        if (ccaaDentro) { graficos.ccaaEncadena(); }
                        else { graficos.ccaaEntra(dto); }
                        break;
                    case "SUPERFALDÃ“N":
                        graficos.superfaldonEntra(oficiales);
                        break;
                    case "VS":
                        //graficos.superfaldonEntra();
                        break;
                    case "CARTÃ“N PARTIDOS":
                        graficos.cartonPartidosEntra(dto);
                        cartonPartidosDentro = true;
                        break;
                    case "ÃšLTIMO ESCAÃ‘O":
                        if (ultimoEscanoDentro)
                        {
                            // Use a copy as "previous" if we don't have an explicit previous DTO in this context.
                            graficos.ultimoEncadena(new BrainStormDTO(dto), dto);
                        }
                        else
                        {
                            graficos.ultimoEntra(dto);
                            ultimoEscanoDentro = true;
                        }
                        break;
                    case "ÃšLTIMO SUPERFALDÃ“N":
                        graficos.ultimoSuperEntra();
                        sfUltimoDentro = true;
                        break;

                    default: break;
                }
            }
        }
        private void EntraSuperfaldon()
        {
            if (dto != null && graficosListView.SelectedIndex != -1)
            {
                string tipoGrafico = GetSelectedGrafico();
                switch (tipoGrafico)
                {
                    case "ESCRUTADO":
                        graficos.sfEscrutadoEntra();
                        break;
                    case "CARRUSEL":
                        graficos.superfaldonEntra(oficiales);
                        sfCarruselDentro = true;
                        break;
                    case "CCAA":
                        graficos.sfCCAAEntra();
                        break;
                    case "ULTIMO":
                        graficos.ultimoSuperEntra();
                        sfUltimoDentro = true;
                        break;

                    default: break;
                }
            }
        }

        private void SaleFaldon()
        {
            if (graficosListView.SelectedIndex != -1)
            {
                string tipoGrafico = GetSelectedGrafico();
                switch (tipoGrafico)
                {
                    case "CUENTA ATRÃS":
                        graficos.SaleReloj();
                        graficos.BajarRotulosPrimeEsp();
                        break;
                    case "FICHAS":
                        graficos.TickerSale(oficiales, dto);
                        graficos.BajarRotulosPrimeEsp(1000);
                        if (!oficiales) { sondeoEnElAire = false; }
                        tickerDentro = false;
                        break;
                    case "SEDES":
                        // if (partidoSeleccionado != null)
                        // {
                        //   if (partidoSeleccionado.escaniosHasta > 0)
                        //  {
                        //      graficos.SedesSale(tickerDentro);
                        //  }
                        //  else
                        //  {
                        graficos.SedesSale();
                        //   }
                        sedeDentro = false;
                        //}
                        if (dto != null)
                        {
                            ObtenerDTO(dto.circunscripcionDTO.nombre);
                            ActualizarInfoInterfaz(dto);
                        }
                        EscribirFichero();
                        break;
                    default: break;
                }
            }

        }
        private void SaleCarton()
        {
            if (graficosListView.SelectedIndex != -1)
            {
                string tipoGrafico = GetSelectedGrafico();
                switch (tipoGrafico)
                {
                    case "PARTICIPACIÃ“N":
                        graficos.participacionSale();
                        participacionDentro = false;
                        break;
                    case "CCAA":
                        graficos.ccaaSale();
                        ccaaDentro = false;
                        break;
                    case "MAYORÃAS":
                        graficos.mayoriasSale();
                        mayoriasDentro = false;
                        break;
                    case "FICHAS":
                        graficos.fichaSale(oficiales);
                        fichaDentro = false;
                        break;
                    case "SUPERFALDÃ“N":
                        graficos.superfaldonSale(oficiales);
                        break;
                    case "VS":
                        // graficos.superfaldonEntra();
                        break;
                    case "CARTÃ“N PARTIDOS":
                        graficos.cartonPartidosSale();
                        cartonPartidosDentro = false;
                        break;
                    case "ÃšLTIMO ESCAÃ‘O":
                        graficos.ultimoSale();
                        ultimoEscanoDentro = false;
                        break;
                    case "ÃšLTIMO SUPERFALDÃ“N":
                        graficos.ultimoSuperSale();
                        sfUltimoDentro = false;
                        break;
                    default: break;
                }
            }
        }
        private void SaleSuperfaldon()
        {
            if (graficosListView.SelectedIndex != -1)
            {
                string tipoGrafico = GetSelectedGrafico();
                switch (tipoGrafico)
                {
                    case "ESCRUTADO":
                        graficos.sfEscrutadoSale();
                        break;
                    case "CARRUSEL":
                        graficos.superfaldonSale(oficiales);
                        if (!string.IsNullOrWhiteSpace(sfCodigoSedeDesplegada))
                        {
                            graficos.sfReplegarSede();
                        }
                        sfCarruselDentro = false;
                        sfCodigoSedeDesplegada = null;
                        break;
                    case "CCAA":
                        graficos.sfCCAASale();
                        break;
                    case "ULTIMO":
                        graficos.ultimoSuperSale();
                        sfUltimoDentro = false;
                        break;

                    default: break;
                }
            }
        }


        // EVENTOS DE CONFIGURACIÃ“N AVANZADA
        private void chkPrimerosResultados_Checked(object sender, RoutedEventArgs e)
        {
            if (graficos != null)
            {
                graficos.PrimerosResultados(true);
            }

        }
        private void chkPrimerosResultados_Unchecked(object sender, RoutedEventArgs e)
        {
            if (graficos != null)
            {
                graficos.PrimerosResultados(false);
            }
        }
        private void chkSondeoAnimado_Checked(object sender, RoutedEventArgs e)
        {
            if (graficos != null)
            {
                graficos.AnimacionSondeo(true);
            }
        }
        private void chkSondeoAnimado_Unchecked(object sender, RoutedEventArgs e)
        {
            if (graficos != null)
            {
                graficos.AnimacionSondeo(false);
            }
        }
        private void timePickerCuentaAtras_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            CalcularSegundosHastaHora();
        }
        /// <summary>
        /// Calcula los segundos restantes hasta la hora destino seleccionada
        /// </summary>
        private int CalcularSegundosHastaHora()
        {
            if (timePickerCuentaAtras.SelectedTime.HasValue)
            {
                var horaDestino = timePickerCuentaAtras.SelectedTime.Value;
                var ahora = DateTime.Now;
                var destino = ahora.Date.Add(horaDestino.TimeOfDay);

                // Si la hora ya pasÃ³ hoy, cuenta para maÃ±ana
                if (destino <= ahora)
                    destino = destino.AddDays(1);

                var diferencia = destino - ahora;
                txtTiempoRestante.Text = $"Tiempo restante: {diferencia.Hours:00}:{diferencia.Minutes:00}:{diferencia.Seconds:00}";
                return (int)diferencia.TotalSeconds;
            }
            return 0;
        }

        //LOGICA DE CIERRE DE VENTANA
        private void WindowClosing(object? sender, CancelEventArgs e)
        {
            escuchador?.Detener();

            if (botonera != null)
            {
                botonera.Close();
            }
            if (pactos != null)
            {
                pactos.Close();
            }
            if (config != null)
            {
                config.Close();
            }
        }

        private void cmbSondeo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbSondeo.SelectedIndex >= 0 && dto != null)
                {
                    string? descripcionSondeo = cmbSondeo.SelectedItem?.ToString();
                    if (!string.IsNullOrEmpty(descripcionSondeo))
                    {
                        if (descripcionSondeo == "RTVE")
                        {
                            // Restaurar los valores originales de escaÃ±os de sondeo
                            RestaurarValoresOriginalesSondeo();
                        }
                        else
                        {
                            // Obtener el cÃ³digo del medio a partir de la descripciÃ³n
                            MedioController medioController = new MedioController(conexionActiva);
                            List<src.model.DTO.MedioDTO> medios = medioController.ObtenerMediosConDescripcion();
                            src.model.DTO.MedioDTO? medioSeleccionado = medios.FirstOrDefault(m => m.descripcion == descripcionSondeo);

                            if (medioSeleccionado != null)
                            {
                                // Obtener los datos de MedioPartido para actualizar el DTO
                                ActualizarDatosConMedio(medioSeleccionado.codigo);
                            }
                        }

                        // Si estamos en modo sondeo, actualizar la interfaz
                        if (!oficiales)
                        {
                            // Determinar si estamos en una circunscripciÃ³n o autonomÃ­a
                            if (circunscripcionesListView.SelectedItem != null)
                            {
                                // Estamos en una circunscripciÃ³n especÃ­fica
                                string? circunscripcionSeleccionada = GetSelectedCircunscripcionNombre();
                                if (!string.IsNullOrWhiteSpace(circunscripcionSeleccionada))
                                {
                                    Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(circunscripcionSeleccionada);
                                    ActualizarInfoInterfaz(seleccionada, dto);
                                }
                            }
                            else if (autonomiasListView.SelectedItem != null)
                            {
                                // Estamos en una autonomÃ­a
                                ActualizarInfoInterfaz(dto);
                            }

                            // Si hay un grÃ¡fico y circunscripciÃ³n seleccionados, exportar el CSV
                            if (graficosListView.SelectedItem != null && (circunscripcionesListView.SelectedItem != null || autonomiasListView.SelectedItem != null))
                            {
                                EscribirFichero();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error seleccionando sondeo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestaurarValoresOriginalesSondeo()
        {
            if (dto != null)
            {
                foreach (var partido in dto.partidos)
                {
                    // Buscar los valores originales con la clave Ãºnica
                    string clave = $"{dto.circunscripcionDTO.codigo}_{partido.codigo}";
                    if (valoresOriginalesSondeo.TryGetValue(clave, out var valores))
                    {
                        // Restaurar los valores originales
                        partido.escaniosDesdeSondeo = valores.desde;
                        partido.escaniosHastaSondeo = valores.hasta;
                    }
                }
            }
        }

        private void GestionarMedioSondeo()
        {
            if (cmbSondeo.SelectedItem != null && !oficiales)
            {
                string? medioSeleccionado = cmbSondeo.SelectedItem?.ToString();
                if (string.IsNullOrWhiteSpace(medioSeleccionado))
                {
                    return;
                }

                if (medioSeleccionado == "RTVE")
                {
                    RestaurarValoresOriginalesSondeo();
                }
                else
                {
                    MedioController medioController = new MedioController(conexionActiva);
                    List<src.model.DTO.MedioDTO> medios = medioController.ObtenerMediosConDescripcion();
                    src.model.DTO.MedioDTO? medio = medios.FirstOrDefault(m => m.descripcion == medioSeleccionado);
                    if (medio != null)
                    {
                        ActualizarDatosConMedio(medio.codigo);
                    }
                }
            }
        }

        private void ActualizarDatosConMedio(string codMedio)
        {
            try
            {
                MedioPartidoController medioPartidoController = new MedioPartidoController(conexionActiva);

                // Para cada partido en el DTO, actualizar los escaÃ±os de sondeo
                foreach (var partido in dto.partidos)
                {
                    // Obtener el cÃ³digo de circunscripciÃ³n del DTO
                    string codCircunscripcion = dto.circunscripcionDTO.codigo;

                    // Obtener los datos del MedioPartido
                    var medioPartido = medioPartidoController.ObtenerPorClave(codCircunscripcion, codMedio, partido.codigo);

                    if (medioPartido != null)
                    {
                        // Actualizar los valores de escaÃ±os de sondeo
                        partido.escaniosDesdeSondeo = medioPartido.escaniosDesde;
                        partido.escaniosHastaSondeo = medioPartido.escaniosHasta;
                    }
                    else
                    {
                        // Si no hay datos para este partido, establecer valores por defecto
                        partido.escaniosDesdeSondeo = 0;
                        partido.escaniosHastaSondeo = 0;
                    }
                }
                dto.numPartidos = dto.partidos.Count(p => oficiales ? p.escanios > 0 : p.escaniosDesdeSondeo > 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error actualizando datos del medio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


