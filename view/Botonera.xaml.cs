using Elecciones.src.controller;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF.DTO;
using Elecciones.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Elecciones
{
    /// <summary>
    /// Interaction logic for Botonera.xaml
    /// </summary>
    public partial class Botonera : Window
    {
        GraphicController gController;
        ConfigManager configuration;

        private bool oficial;
        private bool relojPausado;

        private BrainStormDTO dto;
        private PartidoDTO partidoSeleccionado;
        private MainWindow main;

        public Botonera()
        {
            InitializeComponent();
            InitializeVariables();
            AdaptarEntorno();
            AdaptarColores();
            // AdaptarChecks();
        }

        private void InitializeVariables()
        {
            main = Application.Current.MainWindow as MainWindow;
            oficial = main?.oficiales ?? false;
            gController = GraphicController.GetInstance();
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            relojPausado = false;
        }

        public void AdaptarEntorno()
        {
            var window = Application.Current.MainWindow as MainWindow;
            oficial = window?.oficiales ?? false;
            Brush color;
            if (oficial)
            {
                color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cb96f8"));
                EntraHistButton.Background = color;
                EntraHistButton.Content = "ENTRA HISTÓRICO";
                // EntraMillonesButton.Visibility = Visibility.Visible;
                // EntraEscanosButton.Visibility = Visibility.Visible;
            }
            else
            {
                color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff7979"));
                EntraHistButton.Background = color;
                EntraHistButton.Content = "SALE % VOTO";
                // EntraMillonesButton.Visibility = Visibility.Hidden;
                // EntraEscanosButton.Visibility = Visibility.Hidden;
            }
        }
        private void AdaptarColores()
        {
            SolidColorBrush fondo = (SolidColorBrush)Application.Current.FindResource("PrimaryHueDarkBrush");
            Background = fondo;
        }

        private void PrimerosResultadosCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (gController != null && gController.ipfActivo.Valor == 1) { gController.PrimerosResultados(true); }
        }
        private void PrimerosResultadosCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (gController != null && gController.ipfActivo.Valor == 1) { gController.PrimerosResultados(false); }
        }

        private void SondeoAnimadoCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (gController != null && gController.ipfActivo.Valor == 1) { gController.AnimacionSondeo(true); }
        }
        private void SondeoAnimadoCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (gController != null && gController.ipfActivo.Valor == 1) { gController.AnimacionSondeo(false); }
        }

        private void EntraMillonesButton_Click(object sender, RoutedEventArgs e)
        {
            if (oficial)
                gController.TickerMillonesEntra();
        }
        private void EntraEscanosButton_Click(object sender, RoutedEventArgs e)
        {
            if (oficial)
                gController.TickerMillonesSale();
        }
        private void EntraPVotoButton_Click(object sender, RoutedEventArgs e)
        {
            gController.TickerVotosEntra();
            // MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            // var desplegado = mainWindow.desplegado;
        }
        private void EntraHistButton_Click(object sender, RoutedEventArgs e)
        {
            if (oficial)
            {
                gController.TickerHistoricosEntraInd();
            }
            else
            {
                gController.TickerVotosSale();
            }

        }

        private void EntraPP_PSOEButton_Click(object sender, RoutedEventArgs e)
        {
            gController.PP_PSOEEntra();
        }
        private void SalePP_PSOEButton_Click(object sender, RoutedEventArgs e)
        {
            gController.PP_PSOESale();
        }

        private void btnSubirRotulosTd_Click(object sender, RoutedEventArgs e)
        {
            // Enviar SubirRotulos a Prime para TeleDirecciones
            gController.SubirRotulosPrimeTD();
        }

        private void btnBajarRotulosTd_Click(object sender, RoutedEventArgs e)
        {
            // Enviar BajarRotulos a Prime para TeleDirecciones
            gController.BajarRotulosPrimeTD();
        }

        private void btnSubirRotulosEsp_Click(object sender, RoutedEventArgs e)
        {
            gController.SubirRotulosPrimeEsp();
        }

        private void btnBajarRotulosEsp_Click(object sender, RoutedEventArgs e)
        {
            gController.BajarRotulosPrimeEsp();
        }
    }
}



