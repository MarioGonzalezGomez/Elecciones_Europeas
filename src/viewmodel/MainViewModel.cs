using Elecciones_Europeas.src.conexion;
using Elecciones_Europeas.src.controller;
using Elecciones_Europeas.src.logic;
using Elecciones_Europeas.src.model.IPF;
using Elecciones_Europeas.src.model.IPF.DTO;
using Elecciones_Europeas.src.service;
using Elecciones_Europeas.src.utils;
using System.Collections.ObjectModel;
using Elecciones_Europeas.src.viewmodel.MvvmBase;

namespace Elecciones_Europeas.src.viewmodel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ConfigManager _configuration;
        private readonly INotificationService _notificationService;
        private readonly ILoggerService _loggerService;
        private ConexionEntityFramework _conexionActiva;

        public ObservableCollection<string> CircunscripcionNames { get; set; }
        public ObservableCollection<CPDataDTO> ListaDeDatos { get; set; }
        public ObservableCollection<string> AutonomiasNames { get; set; }
        public ObservableCollection<string> GraficosOptions { get; set; }
        
        private string _graficosHeader;
        public string GraficosHeader
        {
            get => _graficosHeader;
            set => SetProperty(ref _graficosHeader, value);
        }

        private List<Circunscripcion> CCAA;
        public ObservableInt EleccionSeleccionada { get; set; }

        private bool _actualizacionActiva;
        public bool ActualizacionActiva
        {
            get => _actualizacionActiva;
            set => SetProperty(ref _actualizacionActiva, value);
        }

        private int _avance;
        private bool _preparado;
        private bool _oficiales;
        private bool _regional;
        private int _tipoElecciones;

        public MainViewModel()
        {
            _configuration = ConfigManager.GetInstance();
            _notificationService = NotificationService.GetInstance();
            _loggerService = FileLoggerService.GetInstance();

            CircunscripcionNames = new ObservableCollection<string>();
            ListaDeDatos = new ObservableCollection<CPDataDTO>();
            AutonomiasNames = new ObservableCollection<string>();
            GraficosOptions = new ObservableCollection<string>();
            EleccionSeleccionada = new ObservableInt();

            InitializeVariables();
            AdaptarConexiones();
            CargarCircunscripciones();
            InitializeListView();
        }

        private void InitializeVariables()
        {
            _configuration.ReadConfig();
            ActualizacionActiva = true;
            _avance = 1;
            _preparado = false;
            _oficiales = false;
            
            _tipoElecciones = int.Parse(_configuration.GetValue("tipoElecciones"));
            EleccionSeleccionada.CambioDeElecciones += (s, e) => CambioDeElecciones();
            EleccionSeleccionada.Valor = 0;
            _regional = _configuration.GetValue("regional") == "1";
        }

        private void AdaptarConexiones()
        {
            // Logic to adapt connections based on config
             _conexionActiva = new ConexionEntityFramework(int.Parse(_configuration.GetValue($"conexionDefault{EleccionSeleccionada.Valor + 1}")), EleccionSeleccionada.Valor + 1);
        }

        private void CargarCircunscripciones()
        {
            CCAA = CircunscripcionController.GetInstance(_conexionActiva).FindAllAutonomias(_conexionActiva.db);
            if (EleccionSeleccionada.Valor == 1)
            {
                CCAA.Clear();
                CCAA.Add(CircunscripcionController.GetInstance(_conexionActiva).FindById("9900000"));
            }
            
            AutonomiasNames.Clear();
            foreach (var ccaa in CCAA)
            {
                AutonomiasNames.Add(ccaa.nombre);
            }
        }

        public void InitializeListView()
        {
            GraficosOptions.Clear();
            int tablaPrincipal = int.Parse(_configuration.GetValue("tablasGraficosPrincipal"));
            GraficosHeader = _configuration.GetValue($"headerTabla{tablaPrincipal}");
            
            switch (tablaPrincipal)
            {
                case 1:
                    GraficosOptions.Add("TICKER");
                    GraficosOptions.Add("SEDES");
                    break;
                case 2:
                    GraficosOptions.Add("PARTICIPACIÓN");
                    GraficosOptions.Add("CCAA");
                    GraficosOptions.Add("FICHAS");
                    GraficosOptions.Add("PACTÓMETRO");
                    GraficosOptions.Add("MAYORÍAS");
                    GraficosOptions.Add("SUPERFALDÓN");
                    GraficosOptions.Add("VS");
                    break;
                case 3:
                    GraficosOptions.Add("RA 1");
                    GraficosOptions.Add("SEDES");
                    GraficosOptions.Add("RA 3");
                    GraficosOptions.Add("RA 4");
                    break;
                case 4:
                    GraficosOptions.Add("PANTALLA 1");
                    GraficosOptions.Add("SEDES");
                    GraficosOptions.Add("PANTALLA 3");
                    GraficosOptions.Add("PANTALLA 4");
                    break;
            }
        }

        private void CambioDeElecciones()
        {
             _conexionActiva.CloseConection();
             // _conexionActiva.Dispose(); // Be careful with disposing if it's used elsewhere or singleton-like
             _conexionActiva = new ConexionEntityFramework(int.Parse(_configuration.GetValue($"conexionDefault{EleccionSeleccionada.Valor + 1}")), EleccionSeleccionada.Valor + 1);
             
             CargarCircunscripciones();
             CircunscripcionNames.Clear();
             ListaDeDatos.Clear();
             
             // Logic to update graphics controller if needed
             // graficos.CambioElecciones(europa);
        }
    }
}
