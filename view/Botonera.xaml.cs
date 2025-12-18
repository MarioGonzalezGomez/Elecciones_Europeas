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
        private MainWindow main;

        public Botonera()
        {
            InitializeComponent();
            InitializeVariables();
            ConfigurarGrupoActivo();
            AdaptarColores();
        }

        private void InitializeVariables()
        {
            main = Application.Current.MainWindow as MainWindow;
            gController = GraphicController.GetInstance();
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
        }

        /// <summary>
        /// Configura la visibilidad de los paneles de grupo según tablasGraficosPrincipal.
        /// Puede ser llamado externamente para actualizar dinámicamente sin reiniciar.
        /// </summary>
        public void ConfigurarGrupoActivo()
        {
            configuration.ReadConfig();
            string valorConfig = configuration.GetValue("tablasGraficosPrincipal");
            int grupo = int.TryParse(valorConfig, out int g) ? g : 1;

            // Ocultar todos los paneles
            PanelGrupo1.Visibility = Visibility.Collapsed;
            PanelGrupo2.Visibility = Visibility.Collapsed;
            PanelGrupo3.Visibility = Visibility.Collapsed;

            // Mostrar el panel correspondiente y actualizar header
            switch (grupo)
            {
                case 1:
                    PanelGrupo1.Visibility = Visibility.Visible;
                    lblGrupoActivo.Text = "GRUPO 1 - " + (configuration.GetValue("headerTabla1") ?? "FALDÓN");
                    break;
                case 2:
                    PanelGrupo2.Visibility = Visibility.Visible;
                    lblGrupoActivo.Text = "GRUPO 2 - " + (configuration.GetValue("headerTabla2") ?? "CARTÓN");
                    break;
                case 3:
                    PanelGrupo3.Visibility = Visibility.Visible;
                    lblGrupoActivo.Text = "GRUPO 3 - " + (configuration.GetValue("headerTabla3") ?? "SUPERFALDÓN");
                    break;
                default:
                    PanelGrupo1.Visibility = Visibility.Visible;
                    lblGrupoActivo.Text = "GRUPO 1 - " + (configuration.GetValue("headerTabla1") ?? "FALDÓN");
                    break;
            }
        }
        private void AdaptarColores()
        {
            SolidColorBrush fondo = (SolidColorBrush)Application.Current.FindResource("PrimaryHueDarkBrush");
            Background = fondo;
        }

        #region Área 1: Ticker - Escaños/Voto/Históricos/Millones

        private void btnEscanos_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar funcionalidad Escaños
            gController.TickerMillonesSale(); // Placeholder - usar el método existente más similar
        }

        private void btnPorcentajeVoto_Click(object sender, RoutedEventArgs e)
        {
            gController.TickerVotosEntra();
        }

        private void btnHistoricos_Click(object sender, RoutedEventArgs e)
        {
            gController.TickerHistoricosEntraInd();
        }

        private void btnMillones_Click(object sender, RoutedEventArgs e)
        {
            gController.TickerMillonesEntra();
        }

        private void btnHistoricosCom_Click(object sender, RoutedEventArgs e)
        {
            gController.TickerHistoricosEntraCom();
        }

        #endregion

        #region Área 2: Video In/Out, Entran/Salen Todos

        private void btnVideoIn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar Video In
        }

        private void btnVideoOut_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar Video Out
        }

        private void btnEntranTodos_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar Entran Todos
        }

        private void btnSalenTodos_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar Salen Todos
        }

        #endregion

        #region Área 3: TD y Especial

        private void btnEntraTD_Click(object sender, RoutedEventArgs e)
        {
            gController.SubirRotulosPrimeTD();
        }

        private void btnSaleTD_Click(object sender, RoutedEventArgs e)
        {
            gController.BajarRotulosPrimeTD();
        }

        private void btnEntraEspecial_Click(object sender, RoutedEventArgs e)
        {
            gController.SubirRotulosPrimeEsp();
        }

        private void btnSaleEspecial_Click(object sender, RoutedEventArgs e)
        {
            gController.BajarRotulosPrimeEsp();
        }

        #endregion
    }
}



