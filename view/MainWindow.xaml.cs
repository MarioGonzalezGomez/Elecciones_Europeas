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
using Microsoft.Win32; // <- Added for OpenFileDialog

namespace Elecciones
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Circunscripcion> CCAA;
        ObservableCollection<string> circunscripcionNames;
        ObservableCollection<CPDataDTO> listaDeDatos;
        ConexionEntityFramework conexionActiva;
        private int avance;
        BrainStormDTO dto;
        BrainStormDTO dtoSinFiltrar;
        BrainStormDTO dtoDesdeSedes;
        bool preparado;
        public bool oficiales;
        bool regional;
        private string autonomiasHeader;

        //Escuchador
        public Escuchador escuchador;
        //Bool para ver si mando actualización de datos o no
        public bool actualizacionActiva;

        //1 Nacionales, 2 Autonomia X
        private int tipoElecciones;

        //0 Si utilizamos datos de la DB1, 1 de la DB2...
        private ObservableInt eleccionSeleccionada;

        //Si tenemos algún partido seleccionado con el que se deba hacer algo
        private PartidoDTO? partidoSeleccionado;

        //Bool para hacer giro
        bool sondeoEnElAire;
        bool tickerDentro;
        bool sedeDentro;

        //Bool cartones
        bool participacionDentro;
        bool ccaaDentro;
        bool mayoriasDentro;
        bool fichaDentro;
        bool superfaldonDentro;
        bool sfFichasDentro;
        bool sfPactometroDentro;
        bool sfMayoriasDentro;
        bool sfBipartidismoDentro;
        bool sfGanadorDentro;

        //Estas conexiones serán null si no están activadas por Configuración
        OrdenesIPF? ipf;
        OrdenesPrime? prime;
        GraphicController graficos;

        //Ventanas adicionales
        Botonera botonera;
        public Pactos pactos;
        public Config? config;

        //Manejar datos del fichero de configuracion
        ConfigManager configuration;

        public MainWindow()
        {
            InitializeComponent();
            InitializeVariables();
            AdaptarConexiones();
            CargarCircunscripciones();
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
            superfaldonDentro = false;
            sfFichasDentro = false;
            sfPactometroDentro = false;
            sfMayoriasDentro = false;
            sfBipartidismoDentro = false;
            sfGanadorDentro = false;
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
                // Cargar todas las autonomías (España ya está incluida al principio por el método FindAllAutonomias)
                CCAA = CircunscripcionController.GetInstance(conexionActiva).FindAllAutonomias(eleccionSeleccionada.Valor + 1);
                autonomiasHeader = "AUTONOMÍAS";
            }
            else if (tipoElecciones == 2) // Elecciones autonómicas
            {
                // Cargar solo la autonomía correspondiente (codigoRegional + 5 ceros)
                string codigoRegional = configuration.GetValue($"codigoRegionalBD{eleccionSeleccionada.Valor + 1}");
                string codigoAutonomia = $"{codigoRegional}00000";
                Circunscripcion autonomia = CircunscripcionController.GetInstance(conexionActiva).FindById(codigoAutonomia);
                CCAA = new List<Circunscripcion>();
                if (autonomia != null)
                {
                    CCAA.Add(autonomia);
                }
                autonomiasHeader = "AUTONOMÍA";
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
        //Por ahora, se modifican manualmente, pero se podría implementar un modo de introducir
        //los tipos de gráficos en la ventana de configuración Avanzada
        public void InitializeListView()
        {
            graficosListView.Items.Clear();
            int tablaPrincipal = int.Parse(configuration.GetValue("tablasGraficosPrincipal"));
            graficosHeader.Header = configuration.GetValue($"headerTabla{tablaPrincipal}");
            switch (tablaPrincipal)
            {
                case 1:
                    graficosListView.Items.Add("CUENTA ATRÁS");
                    graficosListView.Items.Add("FICHAS");
                    graficosListView.Items.Add("SEDES");
                    //  graficosListView.Items.Add("INDEPENDENTISMO");
                    break;
                case 2:
                    graficosListView.Items.Add("PARTICIPACIÓN");
                    //graficosListView.Items.Add("CCAA");
                    graficosListView.Items.Add("FICHAS");
                    //graficosListView.Items.Add("PACTÓMETRO");
                    graficosListView.Items.Add("MAYORÍAS");
                    //graficosListView.Items.Add("VS");
                    break;
                case 3:
                    graficosListView.Items.Add("FICHAS");
                    graficosListView.Items.Add("PACTÓMETRO");
                    graficosListView.Items.Add("MAYORÍAS");
                    graficosListView.Items.Add("BIPARTIDISMO");
                    graficosListView.Items.Add("GANADOR");
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
                catch (Exception ex)
                {
                    // Manejo de errores en caso de que no se pueda crear el directorio
                    MessageBox.Show($"Se ha producido un error al intentar crear las carpetas para guardar los archivos de datos en {rutaDatos}.", "Error al crear carpetas para guardar los datos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void IniciarEscuchadores()
        {
            escuchador = new Escuchador(conexionActiva);
        }

        public void Update(bool desdeSede = false)
        {
            string elementoSeleccionado = "";
            if (circunscripcionesListView.SelectedIndex != -1)
            {
                elementoSeleccionado = circunscripcionesListView.SelectedItem.ToString();
            }
            else if (autonomiasListView.SelectedIndex != -1)
            {
                elementoSeleccionado = autonomiasListView.SelectedItem.ToString();
            }

            if (elementoSeleccionado != "")
            {
                Circunscripcion seleccionada;
                BrainStormDTO dtoAnterior;

                conexionActiva.ChangeTracker.Clear();
                conexionActiva.SaveChangesAsync();

                if (actualizacionActiva && (pactos == null || pactos.pactoDentro == false))
                {
                    if (desdeSede) { dtoAnterior = new BrainStormDTO(dtoDesdeSedes); }
                    else { dtoAnterior = new BrainStormDTO(dto); }

                    seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);

                    // Determine whether we should request the filtered DTO for UI (true) or the unfiltered one (false).
                    // Preserve previous behavior: when the current grafico is "SEDES" use unfiltered for UI; otherwise use filtered.
                    bool filtroSedes = graficosListView.SelectedItem == null || !string.Equals(graficosListView.SelectedValue, "SEDES");

                    // Use ObtenerDTO which now updates both dto and dtoSinFiltrar.
                    dto = ObtenerDTO(filtroSedes, elementoSeleccionado);

                    if (string.Equals(graficosHeader.Header, "FALDÓN")) { UpdateFaldones(dtoAnterior); }
                    //Add cambios por actualizacion en vivo en cartones
                    if (string.Equals(graficosHeader.Header, "CARTÓN")) { UpdateCartones(); }
                    if (string.Equals(graficosHeader.Header, "SUPERFADÓN")) { UpdateSuperfaldones(); }
                    if (pactos != null && pactos.pactoDentro == false) { pactos.RecargarDatos(dto, oficiales); }
                    ActualizarInfoInterfaz(seleccionada, dto);
                    EscribirFichero(desdeSede);
                }
            }

        }
        private void UpdateFaldones(BrainStormDTO dtoAnterior)
        {
            List<PartidoDTO> partidosQueCambian = dtoAnterior.partidos.Except(dto.partidos, new PartidoDTOComparer()).ToList();
            List<PartidoDTO> partidosQueNoEstan = dtoAnterior.partidos.Where(par => !dto.partidos.Any(par2 => par2.codigo.Equals(par.codigo))).ToList();
            if (partidosQueCambian.Count != 0)
            {
                if (dto.numPartidos != dtoAnterior.numPartidos || partidosQueNoEstan.Count != 0)
                {
                    graficos.TickerActualizaNumPartidos();
                    graficos.TickerYaNoEstaIndividualizado(partidosQueNoEstan);
                }
                else if (CompararOrden(dtoAnterior, dto))
                {
                    graficos.TickerActualizaDatosIndividualizado(partidosQueCambian);
                    graficos.TickerActualizaDatos();
                }
                else
                {
                    graficos.TickerActualizaPosiciones();
                }
            }
            graficos.TickerActualizaEscrutado();
            graficos.TickerActualiza(dto);
        }
        private void UpdateCartones()
        {
        }
        private void UpdateSuperfaldones()
        {
        }

        private bool CompararOrden(BrainStormDTO anterior, BrainStormDTO actual)
        {
            List<PartidoDTO> filtrado = anterior.partidos.Where(par => par.escaniosHasta > 0).ToList();
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
            // Cambiar la imagen a la versión azul cuando el ratón entra
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
            conexionActiva.CloseConection();
            CambioDeElecciones();
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
        /// Inicializa los controles de vídeo leyendo la configuración (si existe).
        /// Guarda el estado inicial en el GraphicController para que el subsistema gráfico sepa la configuración.
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
            if (botonera != null) { botonera.AdaptarEntorno(); }
            ActualizarDatosEnTabla();
            if (circunscripcionesListView.SelectedItem != null || autonomiasListView.SelectedItem != null)
            {
                string elementoSeleccionado = circunscripcionesListView.SelectedItem != null ? circunscripcionesListView.SelectedItem.ToString() : autonomiasListView.SelectedItem.ToString();
                Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);
                if (graficosListView.SelectedItem != null && string.Equals(graficosListView.SelectedValue, "SEDES"))
                {
                    ObtenerDTO(false, elementoSeleccionado);
                }
                else
                {
                    ObtenerDTO(true, elementoSeleccionado);
                }
                ActualizarInfoInterfaz(seleccionada, dto);
                preparado = false;

                if (dto != null)
                {
                    if (pactos != null)
                    {
                        pactos.RecargarDatos(dto, oficiales);
                    }
                }
            }
        }
        private void ActualizarDatosEnTabla()
        {
            Binding bindingCol3;
            Binding bindingCol4;
            if (oficiales)
            {
                columna3.Header = "ESCAÑOS";
                bindingCol3 = new Binding("escaniosHasta");
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
                bindingCol3 = new Binding("escaniosDesde");
                columna3.DisplayMemberBinding = bindingCol3;

                columna4.Header = "ESC. HASTA";
                bindingCol4 = new Binding("escaniosHasta");
                columna4.DisplayMemberBinding = bindingCol4;

                if (graficosListView.SelectedItem == null || !string.Equals(graficosListView.SelectedValue, "SEDES"))
                {
                    columna6.Width = 0;
                    columna7.Width = 0;
                }

            }
            ReajustarSizeTabla();
        }

        private void CambioDeEleccionesHandler(object sender, EventArgs e)
        {
            CambioDeElecciones();
        }
        private void CambioDeElecciones()
        {
            conexionActiva.CloseConection();
            conexionActiva.Dispose();
            conexionActiva = new ConexionEntityFramework(int.Parse(configuration.GetValue($"conexionDefault{eleccionSeleccionada.Valor + 1}")), eleccionSeleccionada.Valor + 1);
            CCAA.Clear();
            CargarCircunscripciones();
            autonomiasListView.ItemsSource = CCAA.Select(cir => cir.nombre).ToList();
            ActualizarHeaderAutonomias();
            circunscripcionNames.Clear();
            listaDeDatos.Clear();
            EscribirConexiones();
            DesplegarCircunscripciones();
            escuchador.IniciarEscuchador(conexionActiva);
            bool europa = eleccionSeleccionada.Valor == 1;
            graficos.CambioElecciones(europa);
            autonomiasListView.SelectedIndex = 0;

            if (eleccionSeleccionada.Valor == 1)
            {
                graficosListView.Items.Remove("SEDES");
            }
            else
            {
                graficosListView.Items.Add("SEDES");
            }
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
            string elementoSeleccionado;
            if (circunscripcionesListView.SelectedItem != null)
            {
                elementoSeleccionado = circunscripcionesListView.SelectedItem.ToString();
                Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);
                bool filtroSedes = graficosListView.SelectedItem == null || !string.Equals(graficosListView.SelectedValue, "SEDES");
                dto = ObtenerDTO(filtroSedes, seleccionada.nombre);
                ActualizarInfoInterfaz(seleccionada, dto);
                preparado = false;
            }
            else if (autonomiasListView.SelectedItem != null)
            {
                elementoSeleccionado = autonomiasListView.SelectedItem.ToString();
                Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);
                bool filtroSedes = graficosListView.SelectedItem == null || !string.Equals(graficosListView.SelectedValue, "SEDES");
                dto = ObtenerDTO(filtroSedes, seleccionada.nombre);
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
            if (autonomiasListView.SelectedItem != null)
            {
                //AÑADIR CIRCUNSCRIPCIONES SI LAS TIENE
                string elementoSeleccionado = autonomiasListView.SelectedItem.ToString();
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

                //PONER LA INFORMACIÓN EN LA INTERFAZ
                Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);
                if (graficosListView.SelectedItem != null && string.Equals(graficosListView.SelectedValue, "SEDES"))
                {
                    ObtenerDTO(false, elementoSeleccionado);
                }
                else
                {
                    ObtenerDTO(true, elementoSeleccionado);
                }
                ActualizarInfoInterfaz(seleccionada, dto);
            }
        }
        private void circunscripcionesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SeleccionarCircunscripcion();
        }
        private void SeleccionarCircunscripcion()
        {
            datosListView.SelectedItem = null;
            preparado = false;
            if (circunscripcionesListView.SelectedItem != null)
            {
                string elementoSeleccionado = circunscripcionesListView.SelectedItem.ToString();
                autonomiasListView.SelectedItem = null;

                //PONER LA INFORMACIÓN EN LA INTERFAZ
                Circunscripcion seleccionada = CircunscripcionController.GetInstance(conexionActiva).FindByName(elementoSeleccionado);
                if (graficosListView.SelectedItem != null && string.Equals(graficosListView.SelectedValue, "SEDES"))
                {
                    ObtenerDTO(false, elementoSeleccionado);
                }
                else
                {
                    ObtenerDTO(true, elementoSeleccionado);
                }
                ActualizarInfoInterfaz(seleccionada, dto);
            }
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
            listaDeDatos.Clear();
            List<CPDataDTO> cpdatas;
            if (graficosListView.SelectedItem != null && (string.Equals(graficosListView.SelectedValue, "SEDES") || string.Equals(graficosListView.SelectedValue, "INDEPENDENTISMO")))
            {
                dtoSinFiltrar = oficiales ? BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficialSinFiltrar(seleccionada.nombre, avance, tipoElecciones) : BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeoSinFiltrar(seleccionada.nombre, avance, tipoElecciones);
                cpdatas = CPDataDTO.FromBSDto(dtoSinFiltrar);
                if (partidoSeleccionado != null)
                {
                    partidoSeleccionado = dtoSinFiltrar.partidos.Find(par => par.siglas.Equals(partidoSeleccionado.siglas));
                }
            }
            else { cpdatas = CPDataDTO.FromBSDto(dto); }
            cpdatas.ForEach(listaDeDatos.Add);
        }
        private void ActualizarInfoInterfaz(BrainStormDTO dto)
        {
            listaDeDatos.Clear();
            List<CPDataDTO> cpdatas;
            if (graficosListView.SelectedItem != null && (string.Equals(graficosListView.SelectedValue, "SEDES") || string.Equals(graficosListView.SelectedValue, "INDEPENDENTISMO")))
            {
                dtoSinFiltrar = oficiales ? BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficialSinFiltrar(dto.circunscripcionDTO.nombre, avance, tipoElecciones) : BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeoSinFiltrar(dto.circunscripcionDTO.nombre, avance, tipoElecciones);
                EscribirFichero();
                cpdatas = CPDataDTO.FromBSDto(dtoSinFiltrar);
                if (partidoSeleccionado != null)
                {
                    partidoSeleccionado = dtoSinFiltrar.partidos.Find(par => par.siglas.Equals(partidoSeleccionado.siglas));
                }
            }
            else { cpdatas = CPDataDTO.FromBSDto(dto); }
            cpdatas.ForEach(listaDeDatos.Add);
        }


        //ADAPTACION DE DATOS DEPENDIENDO DE ELEMENTO SELECCIONADO
        private void graficosListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graficosListView.SelectedIndex != -1)
            {
                AdaptarTablaDatos(graficosListView.SelectedValue.ToString());
                if (dto != null) { ActualizarInfoInterfaz(dto); }
            }
        }
        private void AdaptarTablaDatos(string tipoGrafico)
        {
            switch (tipoGrafico)
            {
                case "CUENTA ATRÁS":
                    // Ocultar la lista de datos al mostrar la cuenta atrás
                    datosListView.Visibility = Visibility.Collapsed;
                    break;
                case "SEDES":
                    // Restaurar visibilidad al volver de "CUENTA ATRÁS"
                    datosListView.Visibility = Visibility.Visible;

                    columna3.Header = "ESCAÑOS";
                    Binding binding3 = new Binding("escaniosHasta");
                    columna3.DisplayMemberBinding = binding3;
                    columna4.Header = "DIF ESC";
                    Binding binding4 = new Binding("diferenciaEscanios");
                    columna4.DisplayMemberBinding = binding4;
                    columna5.Header = "% VOTO";
                    Binding binding5 = new Binding("porcentajeVoto");
                    columna5.DisplayMemberBinding = binding5;
                    if (dto != null)
                    {
                        ObtenerDTO(false, dto.circunscripcionDTO.nombre);
                        ActualizarInfoInterfaz(dto);
                    }
                    break;
                case "INDEPENDENTISMO":
                    // Restaurar visibilidad al volver de "CUENTA ATRÁS"
                    datosListView.Visibility = Visibility.Visible;

                    ActualizarDatosEnTabla();
                    if (dto != null)
                    {
                        ObtenerDTO(false, dto.circunscripcionDTO.nombre);
                        ActualizarInfoInterfaz(dto);
                    }
                    break;

                default:
                    // Restaurar visibilidad al volver de "CUENTA ATRÁS"
                    datosListView.Visibility = Visibility.Visible;

                    ActualizarDatosEnTabla();
                    if (dto != null)
                    {
                        ObtenerDTO(true, dto.circunscripcionDTO.nombre);
                        ActualizarInfoInterfaz(dto);
                    }
                    break;
            }
        }
        private void ReajustarSizeTabla()
        {
            if (oficiales)
            {
                columna1.Width = datosListView.ActualWidth / 7;
                columna2.Width = datosListView.ActualWidth / 7;
                columna3.Width = datosListView.ActualWidth / 7;
                columna4.Width = datosListView.ActualWidth / 7;
                columna5.Width = datosListView.ActualWidth / 7;
                columna6.Width = datosListView.ActualWidth / 7;
                columna7.Width = datosListView.ActualWidth / 7;
            }
            else
            {
                columna1.Width = datosListView.ActualWidth / 5;
                columna2.Width = datosListView.ActualWidth / 5;
                columna3.Width = datosListView.ActualWidth / 5;
                columna4.Width = datosListView.ActualWidth / 5;
                columna5.Width = datosListView.ActualWidth / 5;
                columna6.Width = 0;
                columna7.Width = 0;
            }
        }

        private void datosListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dto != null)
            {
                CPDataDTO? dataActual = datosListView.SelectedItem != null ? datosListView.SelectedItem as CPDataDTO : null;
                if (dataActual != null)
                {
                    if (graficosListView.SelectedItem != null && string.Equals(graficosListView.SelectedValue, "SEDES") && dtoSinFiltrar != null)
                    {
                        partidoSeleccionado = dtoSinFiltrar.partidos.Find(par => par.codigo.Equals(dataActual.codigo));
                    }
                    else
                    {
                        partidoSeleccionado = dto.partidos.Find(par => string.Equals(par.codigo, dataActual.codigo));
                    }
                }
                preparado = false;
            }
        }

        //LOGICA DE FICHEROS
        private BrainStormDTO ObtenerDTO(bool filtrado, string circunscripcion)
        {
            // Always update both dto (used by UI when 'filtrado' requested or not) and dtoSinFiltrar (complete list).
            // This ensures dtoSinFiltrar reflects the latest data whenever dto is refreshed.
            if (filtrado)
            {
                // dto is filtered (UI-friendly), dtoSinFiltrar keeps the full unfiltered dataset
                dto = oficiales
                    ? BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficial(circunscripcion, avance, tipoElecciones)
                    : BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeo(circunscripcion, avance, tipoElecciones);

                try
                {
                    dtoSinFiltrar = oficiales
                        ? BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficialSinFiltrar(circunscripcion, avance, tipoElecciones)
                        : BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeoSinFiltrar(circunscripcion, avance, tipoElecciones);
                }
                catch
                {
                    // If unfiltered fetch fails, keep dtoSinFiltrar as a defensive copy of dto to avoid null usage elsewhere.
                    dtoSinFiltrar = new BrainStormDTO(dto);
                }
            }
            else
            {
                // dto is the unfiltered dataset; make dtoSinFiltrar a copy of it so both reflect the same full data.
                dto = oficiales
                    ? BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficialSinFiltrar(circunscripcion, avance, tipoElecciones)
                    : BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeoSinFiltrar(circunscripcion, avance, tipoElecciones);

                // Keep dtoSinFiltrar consistent with dto (full)
                dtoSinFiltrar = new BrainStormDTO(dto);
            }

            return dto;
        }

        private async void EscribirFichero(bool desdeSedes = false)
        {
            if (dto != null)
            {
                preparado = true;

                // Read ordering mode from config (non-boolean so it can be extended later)
                string ordenSetting = configuration.GetValue("ordenPartidos") ?? "0";

                // Local helper: create a new BrainStormDTO copy ordered by partido.codigo.
                // Uses the provided source (so we can pass an unfiltered DTO when needed).
                BrainStormDTO CreateOrderedDtoCopyFrom(BrainStormDTO source)
                {
                    var ordered = new BrainStormDTO(source);
                    ordered.partidos = source.partidos.OrderBy(p => p.codigo).ToList();
                    // For the code-ordered files we want the full list count (they contain all parties).
                    ordered.numPartidos = ordered.partidos.Count;
                    return ordered;
                }

                // If config requests code ordering we must obtain the full (unfiltered) DTO from controller
                // so the _Codigo files contain the complete list of parties. Do this without mutating the UI dto.
                BrainStormDTO dtoToWriteMain = dto; // which dto will be used for the main filenames

                if (ordenSetting == "1")
                {
                    // Fetch unfiltered DTO directly from controller (do not call ObtenerDTO to avoid changing UI state).
                    dtoSinFiltrar = oficiales
                        ? BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficialSinFiltrar(dto.circunscripcionDTO.nombre, avance, tipoElecciones)
                        : BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeoSinFiltrar(dto.circunscripcionDTO.nombre, avance, tipoElecciones);

                    // If the configuration indicates that the main CSVs should use code-ordering,
                    // use the ordered full DTO for the main files as well.
                    dtoToWriteMain = CreateOrderedDtoCopyFrom(dtoSinFiltrar);
                }

                // Write the main CSV (behaviour preserved; main file will use code-ordered DTO if ordenSetting == "1")
                if (graficosListView.SelectedItem != null && string.Equals(graficosListView.SelectedValue, "SEDES"))
                {
                    if (desdeSedes)
                    {
                        if (oficiales)
                        {
                            if (ordenSetting == "1") { await dtoToWriteMain.ToCsv(); }
                            else { await dto.ToCsv(); }
                        }
                        else
                        {
                            if (ordenSetting == "1") { await dtoToWriteMain.ToCsv("Brainstorm_Sondeo"); }
                            else { await dto.ToCsv("Brainstorm_Sondeo"); }
                        }
                    }
                }
                else
                {
                    if (oficiales)
                    {
                        if (ordenSetting == "1") { await dtoToWriteMain.ToCsv(); }
                        else { await dto.ToCsv(); }
                    }
                    else
                    {
                        if (ordenSetting == "1") { await dtoToWriteMain.ToCsv("Brainstorm_Sondeo"); }
                        else { await dto.ToCsv("Brainstorm_Sondeo"); }
                    }
                }

                // Always also write the new code-ordered files that must contain the complete list.
                // Ensure we have a full DTO to build them from (fetch if not already fetched).
                try
                {
                    if (dtoSinFiltrar == null)
                    {
                        dtoSinFiltrar = oficiales
                            ? BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficialSinFiltrar(dto.circunscripcionDTO.nombre, avance, tipoElecciones)
                            : BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeoSinFiltrar(dto.circunscripcionDTO.nombre, avance, tipoElecciones);
                    }

                    var dtoCodigoOrdered = CreateOrderedDtoCopyFrom(dtoSinFiltrar);

                    if (oficiales)
                    {
                        await dtoCodigoOrdered.ToCsv("Brainstorm_Codigo");
                    }
                    else
                    {
                        await dtoCodigoOrdered.ToCsv("Brainstorm_Sondeo_Codigo");
                    }
                }
                catch
                {
                    // Keep behaviour non-intrusive on filesystem errors (existing code uses minimal handling).
                }

                if (partidoSeleccionado != null)
                {
                    SedesDTO sede = SedesDTO.FromPartidoDTO(partidoSeleccionado, conexionActiva);
                    await sede.ToCsv();
                }

                if (graficos.primeActivo.Valor == 1)
                {
                    if (oficiales) { dto = BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionOficialSinFiltrar(dto.circunscripcionDTO.nombre, avance, tipoElecciones); }
                    else { dto = BrainStormController.GetInstance(conexionActiva).FindByNameCircunscripcionSondeoSinFiltrar(dto.circunscripcionDTO.nombre, avance, tipoElecciones); }
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
            if (pactos != null) { pactos.pactoDentro = false; }
        }
        private void btnPrepara_Click(object sender, RoutedEventArgs e)
        {
            EscribirFichero();
        }
        private void btnEntra_Click(object sender, RoutedEventArgs e)
        {
            if (!preparado) { EscribirFichero(); }
            if (string.Equals(graficosHeader.Header, "FALDÓN")) { EntraFaldon(); }
            if (string.Equals(graficosHeader.Header, "CARTÓN")) { EntraCarton(); }
            if (string.Equals(graficosHeader.Header, "SUPERFALDÓN")) { EntraSuperfaldon(); }
            if (string.Equals(graficosHeader.Header, "PANTALLA")) { EntraSuperfaldon(); }
            if (string.Equals(graficosHeader.Header, "REALIDAD AUMENTADA")) { EntraSuperfaldon(); }
            if (string.Equals(graficosHeader.Header, "DRON")) { EntraSuperfaldon(); }
        }
        private void btnSale_Click(object sender, RoutedEventArgs e)
        {
            if (string.Equals(graficosHeader.Header, "FALDÓN")) { SaleFaldon(); }
            if (string.Equals(graficosHeader.Header, "CARTÓN")) { SaleCarton(); }
            if (string.Equals(graficosHeader.Header, "SUPERFALDÓN")) { SaleSuperfaldon(); }
        }
        private void btnActualiza_Click(object sender, RoutedEventArgs e)
        {
            bool temp = actualizacionActiva;
            actualizacionActiva = true;
            if (graficosListView.SelectedItem != null && (string.Equals(graficosListView.SelectedValue, "SEDES") || string.Equals(graficosListView.SelectedValue, "INDEPENDENTISMO")))
            {
                Update(true);
            }
            else { Update(); }

            actualizacionActiva = temp;
        }
        private void btnPactos_Click(object sender, RoutedEventArgs e)
        {
            if (dto != null)
            {
                if (pactos == null)
                {
                    if (graficosListView.SelectedItem != null && string.Equals(graficosListView.SelectedValue, "INDEPENDENTISMO"))
                    {
                        pactos = new Pactos(dtoSinFiltrar, oficiales);
                    }
                    else
                    {
                        pactos = new Pactos(dto, oficiales);
                    }
                    pactos.Show();
                }
                else { pactos.Activate(); }
            }
            else
            {
                MessageBox.Show($"Seleccione alguna circunscripción para ver su pestaña de pactos", "Circunscipción no seleccionada", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //LOGICA PARA LOS TIPOS DE GRÁFICOS DISTINTOS
        private void EntraFaldon()
        {
            if (dto != null && graficosListView.SelectedIndex != -1)
            {
                switch (graficosListView.SelectedValue.ToString())
                {
                    case "CUENTA ATRÁS":
                        int segundos = CalcularSegundosHastaHora();
                        if (segundos > 0)
                        {
                            graficos.EntraReloj(segundos);
                        }
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
                            else { graficos.TickerEntra(oficiales, dto); }
                            if (!oficiales) { sondeoEnElAire = true; }
                        }
                        tickerDentro = true;
                        break;
                    case "INDEPENDENTISMO":
                        if (tickerDentro)
                        {
                            graficos.independentismoEntra();
                            dtoDesdeSedes = new BrainStormDTO(dto);
                        }
                        break;
                    case "SEDES":
                        if (partidoSeleccionado != null)
                        {
                            if (!sedeDentro)
                            {
                                graficos.SedesEntra(false, partidoSeleccionado.codigo);
                                dtoDesdeSedes = new BrainStormDTO(dto);
                            }
                            else { graficos.SedesEncadena(false, partidoSeleccionado.codigo); }
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
                switch (graficosListView.SelectedValue.ToString())
                {
                    case "PARTICIPACIÓN":
                        if (participacionDentro) { graficos.participacionEncadena(dtoSinFiltrar, avance); }
                        else
                        {
                            graficos.participacionEntra(dtoSinFiltrar, avance);
                            participacionDentro = true;
                        }
                        break;
                    case "FICHAS":
                        if (fichaDentro) { graficos.fichaEncadena(oficiales, dtoSinFiltrar, partidoSeleccionado); }
                        else
                        {
                            graficos.fichaEntra(oficiales, dtoSinFiltrar, partidoSeleccionado);
                            fichaDentro = true;
                        }
                        break;
                    case "MAYORÍAS":
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
                    case "SUPERFALDÓN":
                        if (superfaldonDentro) { graficos.superfaldonEntra(); }
                        else { graficos.superfaldonEntra(); }
                        break;
                    case "VS":
                        //graficos.superfaldonEntra();
                        break;

                    default: break;
                }
            }
        }
        private void EntraSuperfaldon()
        {
            if (dto != null && graficosListView.SelectedIndex != -1)
            {
                switch (graficosListView.SelectedValue.ToString())
                {
                    case "FICHAS":
                        if (sfFichasDentro) { graficos.sfFichasEncadena(); }
                        else { graficos.sfFichasEntra(); }
                        sfFichasDentro = true;
                        break;
                    case "PACTÓMETRO":
                        if (sfPactometroDentro) { graficos.sfPactometroEncadena(); }
                        else { graficos.sfPactometroEntra(); }
                        sfPactometroDentro = true;
                        break;
                    case "MAYORÍAS":
                        if (sfMayoriasDentro) { graficos.sfMayoriasEncadena(); }
                        else { graficos.sfMayoriasEntra(); }
                        sfMayoriasDentro = true;
                        break;
                    case "BIPARTIDISMO":
                        if (sfBipartidismoDentro) { graficos.sfBipartidismoEncadena(); }
                        else { graficos.sfBipartidismoEntra(); }
                        sfBipartidismoDentro = true;
                        break;
                    case "GANADOR":
                        if (sfGanadorDentro) { graficos.sfGanadorEncadena(); }
                        else { graficos.sfGanadorEntra(); }
                        sfGanadorDentro = true;
                        break;

                    default: break;
                }
            }
        }

        private void SaleFaldon()
        {
            if (graficosListView.SelectedIndex != -1)
            {
                switch (graficosListView.SelectedValue.ToString())
                {
                    case "CUENTA ATRÁS":
                        graficos.SaleReloj();
                        break;
                    case "FICHAS":
                        graficos.TickerSale(oficiales);
                        if (!oficiales) { sondeoEnElAire = false; }
                        tickerDentro = false;
                        break;
                    case "SEDES":
                        if (partidoSeleccionado != null)
                        {
                            graficos.SedesSale(false, partidoSeleccionado.codigo);
                            sedeDentro = false;
                            if (tickerDentro)
                            {
                                Update(true);
                            }
                        }
                        break;
                    case "INDEPENDENTISMO":
                        graficos.independentismoSale();
                        if (tickerDentro)
                        {
                            Update(true);
                        }
                        break;
                    default: break;
                }
            }
        }
        private void SaleCarton()
        {
            if (graficosListView.SelectedIndex != -1)
            {
                switch (graficosListView.SelectedValue.ToString())
                {
                    case "PARTICIPACIÓN":
                        graficos.participacionSale();
                        participacionDentro = false;
                        break;
                    case "CCAA":
                        graficos.ccaaSale();
                        ccaaDentro = false;
                        break;
                    case "MAYORÍAS":
                        graficos.mayoriasSale();
                        mayoriasDentro = false;
                        break;
                    case "FICHAS":
                        graficos.fichaSale(oficiales);
                        fichaDentro = false;
                        break;
                    case "SUPERFALDÓN":
                        graficos.superfaldonSale();
                        break;
                    case "VS":
                        // graficos.superfaldonEntra();
                        break;

                    default: break;
                }
            }
        }
        private void SaleSuperfaldon()
        {
            if (graficosListView.SelectedIndex != -1)
            {
                switch (graficosListView.SelectedValue.ToString())
                {
                    case "FICHAS":
                        graficos.sfFichasSale();
                        sfFichasDentro = false;
                        break;
                    case "PACTÓMETRO":
                        graficos.sfPactometroSale();
                        sfPactometroDentro = false;
                        break;
                    case "MAYORÍAS":
                        graficos.sfMayoriasSale();
                        sfMayoriasDentro = false;
                        break;
                    case "BIPARTIDISMO":
                        graficos.sfBipartidismoSale();
                        sfBipartidismoDentro = false;
                        break;
                    case "GANADOR":
                        graficos.sfGanadorSale();
                        sfGanadorDentro = false;
                        break;

                    default: break;
                }
            }
        }


        // EVENTOS DE CONFIGURACIÓN AVANZADA
        private bool primerosResultadosActivo = true;
        private bool sondeoAnimadoActivo = true;
        private void chkPrimerosResultados_Checked(object sender, RoutedEventArgs e)
        {
            primerosResultadosActivo = true;
        }
        private void chkPrimerosResultados_Unchecked(object sender, RoutedEventArgs e)
        {
            primerosResultadosActivo = false;
        }
        private void chkSondeoAnimado_Checked(object sender, RoutedEventArgs e)
        {
            sondeoAnimadoActivo = true;
        }
        private void chkSondeoAnimado_Unchecked(object sender, RoutedEventArgs e)
        {
            sondeoAnimadoActivo = false;
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

                // Si la hora ya pasó hoy, cuenta para mañana
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
    }
}