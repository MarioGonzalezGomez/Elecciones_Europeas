using Elecciones_Europeas.src.controller;
using Elecciones_Europeas.src.model.DTO.BrainStormDTO;
using Elecciones_Europeas.src.model.IPF.DTO;
using Elecciones_Europeas.src.utils;
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

namespace Elecciones_Europeas
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
        private CPDataDTO? partidoDesplegado;

        private BrainStormDTO dto;

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
            var window = (MainWindow)Application.Current.MainWindow;
            oficial = window.oficiales;
            gController = GraphicController.GetInstance();
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            relojPausado = false;
        }

        public void AdaptarEntorno()
        {
            var window = (MainWindow)Application.Current.MainWindow;
            oficial = window.oficiales;
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
        private void AdaptarChecks()
        {
            string primerosActivo = gController.RecibirPrimerosResultados();
            Console.WriteLine($"PRIMEROS : {primerosActivo}");
            if (primerosActivo != null)
            {
                if (primerosActivo.Equals("1"))
                {
                    PrimerosResultadosCheck.IsChecked = true;
                }
                else if (primerosActivo.Equals("0"))
                {
                    PrimerosResultadosCheck.IsChecked = false;
                }
            }
            string sondeoActivo = gController.RecibirAnimacionSondeo();
            Console.WriteLine($"SONDEO : {sondeoActivo}");
            if (sondeoActivo != null)
            {
                if (sondeoActivo.Equals("1"))
                {
                    PrimerosResultadosCheck.IsChecked = true;
                }
                else if (sondeoActivo.Equals("0"))
                {
                    PrimerosResultadosCheck.IsChecked = false;
                }
            }

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

        private void DespliegaButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partido = mainWindow.datosListView.SelectedItem as CPDataDTO;
            if (partido != null)
            {
                gController.SedesEntra(true, partido.codigo);
                partidoDesplegado = partido;
            }
            else
            {
                MessageBox.Show($"Debes seleccionar un partido para desplegar sus datos", "Seleccionar Partido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RepliegaButton_Click(object sender, RoutedEventArgs e)
        {
            if (partidoDesplegado != null)
            {
                gController.SedesSale(true, partidoDesplegado.codigo);
                partidoDesplegado = null;
            }
            else
            {
                MessageBox.Show($"No hay ningún partido desplegado", "Acción no permitida", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void EncadenaButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partido = mainWindow.datosListView.SelectedItem as CPDataDTO;
            if (partido != null && partidoDesplegado != null)
            {
                gController.SedesEncadena(true, partido.codigo, partidoDesplegado.codigo);
                partidoDesplegado = partido;
            }
            else
            {
                MessageBox.Show($"Debes seleccionar un partido para poder encadenarlo", "Seleccionar Partido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EntraTimerButton_Click(object sender, RoutedEventArgs e)
        {
            gController.EntraReloj();
        }
        private void SaleTimerButton_Click(object sender, RoutedEventArgs e)
        {
            gController.SaleReloj();

        }
        private void PararTimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (relojPausado)
            {
                //Accion de continuar el reloj
                relojPausado = false;
                PararTimerButton.Content = "PAUSAR";
            }
            else
            {
                //Accion de pausar el reloj
                relojPausado = true;
                PararTimerButton.Content = "CONTINUAR";
            }
        }

        private void EntraVideoButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partido = mainWindow.datosListView.SelectedItem as CPDataDTO;
            if (partido != null)
            {
                //   gController.EsDirecto(oficial, false, partido.codigo);
                //   gController.EntraVideo(partido.codigo);
            }
            else
            {
                MessageBox.Show($"Debes seleccionar un partido para desplegar su video vivo", "Seleccionar Partido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaleVideoButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partido = mainWindow.datosListView.SelectedItem as CPDataDTO;
            //  if (partido != null) { gController.SaleVideo(partido.codigo); }
        }
        private void EntraDirectoButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partido = mainWindow.datosListView.SelectedItem as CPDataDTO;
            if (partido != null)
            {
                //   gController.EsDirecto(oficial, true, partido.codigo);
                //   gController.EntraVideo(partido.codigo);
            }
            else
            {
                MessageBox.Show($"Debes seleccionar un partido para desplegar su directo", "Seleccionar Partido", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void SaleDirectoButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partido = mainWindow.datosListView.SelectedItem as CPDataDTO;
            //  if (partido != null) { gController.SaleVideo(partido.codigo); }
        }

        private void EsVideoButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partido = mainWindow.datosListView.SelectedItem as CPDataDTO;
            if (partido != null)
            {
                if (!string.IsNullOrEmpty(VideoTextBox.Text) && VideoTextBox.Text != "*")
                {
                    //    gController.CambiaRuta(partido.codigo, $"{partido.siglas}{VideoTextBox.Text}");
                    VideoTextBox.Text = "*";
                }
                //  gController.EsDirecto(oficial, false, partido.codigo);
            }
            else { MessageBox.Show($"Debes seleccionar un partido para asignarle su video vivo", "Seleccionar Partido", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void EsDirectoButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partido = mainWindow.datosListView.SelectedItem as CPDataDTO;
            //  if (partido != null) { gController.EsDirecto(oficial, true, partido.codigo); }
            //  else { MessageBox.Show($"Debes seleccionar un partido para asignarle su video vivo", "Seleccionar Partido", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void EntraTodosButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partidos = mainWindow.datosListView.SelectedItems;
            if (partidos != null)
            {
                foreach (var p in partidos)
                {
                    CPDataDTO data = p as CPDataDTO;
                    //  gController.EntraVideo(data.codigo);
                }
            }
            else { MessageBox.Show($"Debes seleccionar almenos un partido para desplegar su video vivo", "Seleccionar Partido", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void SaleTodosButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            var partidos = mainWindow.datosListView.SelectedItems as List<CPDataDTO>;
            if (partidos != null)
            {
                foreach (var p in partidos)
                {
                    CPDataDTO data = p as CPDataDTO;
                    // gController.SaleVideo(data.codigo);
                }
            }
            else { MessageBox.Show($"Debes seleccionar almenos un partido para replegar su video vivo", "Seleccionar Partido", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void VideoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            VideoTextBox.Text = "";
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
            gController.TickerVotosEntra(oficial);
            // MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            // var desplegado = mainWindow.desplegado;
        }
        private void EntraHistButton_Click(object sender, RoutedEventArgs e)
        {
            if (oficial)
            {
                gController.TickerHistoricosEntra(oficial);
            }
            else
            {
                gController.TickerVotosSale(oficial);
            }

        }

        private void EntraFotosButton_Click(object sender, RoutedEventArgs e)
        {
            gController.TickerFotosEntra();
        }

        private void SaleFotosButton_Click(object sender, RoutedEventArgs e)
        {
            gController.TickerFotosSale();
        }
    }
}
