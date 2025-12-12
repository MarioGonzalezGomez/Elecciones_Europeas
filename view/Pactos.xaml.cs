using Elecciones.src.controller;
using Elecciones.src.logic;
using Elecciones.src.model;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using Elecciones.src.model.IPF.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Drawing;
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
using Elecciones.src.utils;
using System.ComponentModel;
using Elecciones.src.logic.comparators;
using Elecciones.src.logic.comparators;

namespace Elecciones
{
    /// <summary>
    /// Interaction logic for Pactos.xaml
    /// </summary>
    public partial class Pactos : Window
    {
        private BrainStormDTO dto;
        private bool oficiales;
        private int mayoriaAbsoluta;
        private double totalIzq;
        private double totalDer;
        private List<CPDataDTO> partidosTotales;
        private ObservableCollection<CPDataDTO> partidosDisponibles;
        private ObservableCollection<CPDataDTO> partidosDentroIzq;
        private ObservableCollection<CPDataDTO> partidosDentroDer;
        private GraphicController graficos;
        private bool preparado;

        public bool pactoDentro;
        public bool independentismo;

        ConfigManager config;

        public Pactos(BrainStormDTO dto, bool oficiales)
        {
            this.dto = dto;
            this.oficiales = oficiales;
            InitializeComponent();
            InitializeVariables();
            InitializeInfo();
            AdaptarTablas();
            AdaptarColores();
            this.Closing += WindowClosing;
        }

        private void InitializeVariables()
        {
            var main = Application.Current.MainWindow as MainWindow;
            independentismo = main.graficosListView.SelectedItem != null && string.Equals(main.graficosListView.SelectedValue, "INDEPENDENTISMO");
            totalIzq = 0;
            totalDer = 0;
            mayoriaAbsoluta = independentismo ? 50 : dto.circunscripcionDTO.mayoria;
            partidosDisponibles = new ObservableCollection<CPDataDTO>();
            partidosDentroIzq = new ObservableCollection<CPDataDTO>();
            partidosDentroDer = new ObservableCollection<CPDataDTO>();
            graficos = GraphicController.GetInstance();
            preparado = false;
            pactoDentro = false;
            partidosTotales = partidosDisponibles.ToList();
            config = ConfigManager.GetInstance();
        }

        private void InitializeInfo()
        {
            lblCircunscripcion.Content = dto.circunscripcionDTO.nombre;
            lblMayoria.Content = $"Mayoría absoluta: {mayoriaAbsoluta}";
            lblEscaniosIzq.Content = independentismo ? $"Total: {totalIzq}%" : $"Total escaños: {totalIzq}";
            lblEscaniosDer.Content = independentismo ? $"Total: {totalDer}%" : $"Total escaños: {totalDer}";
            CargarPartidos();
            partidosIzqListView.ItemsSource = partidosDisponibles;
            partidosDerListView.ItemsSource = partidosDisponibles;
            partidosDentroIzqListView.ItemsSource = partidosDentroIzq;
            partidosDentroDerListView.ItemsSource = partidosDentroDer;
        }

        private void AdaptarTablas()
        {
            if (oficiales)
            {
                escDesdeIzq.Header = "ESCAÑOS";
                Binding binding1 = new Binding("escaniosHasta");
                escDesdeIzq.DisplayMemberBinding = binding1;
                escHastaIzq.Header = "% VOTO";
                Binding binding2 = new Binding("porcentajeVoto");
                escHastaIzq.DisplayMemberBinding = binding2;

                escDesdeDentroIzq.Header = "ESCAÑOS";
                escDesdeDentroIzq.DisplayMemberBinding = binding1;
                escHastaDentroIzq.Header = "% VOTO";
                escHastaDentroIzq.DisplayMemberBinding = binding2;

                escDesdeDentroDer.Header = "ESCAÑOS";
                escDesdeDentroDer.DisplayMemberBinding = binding1;
                escHastaDentroDer.Header = "% VOTO";
                escHastaDentroDer.DisplayMemberBinding = binding2;

                escDesdeDer.Header = "ESCAÑOS";
                escDesdeDer.DisplayMemberBinding = binding1;
                escHastaDer.Header = "% VOTO";
                escHastaDer.DisplayMemberBinding = binding2;
            }
            else
            {
                escDesdeIzq.Header = "DESDE";
                Binding binding1 = new Binding("escaniosDesde");
                escDesdeIzq.DisplayMemberBinding = binding1;
                escHastaIzq.Header = "HASTA";
                Binding binding2 = new Binding("escaniosHasta");
                escHastaIzq.DisplayMemberBinding = binding2;

                escDesdeDentroIzq.Header = "DESDE";
                escDesdeDentroIzq.DisplayMemberBinding = binding1;
                escHastaDentroIzq.Header = "HASTA";
                escHastaDentroIzq.DisplayMemberBinding = binding2;

                escDesdeDentroDer.Header = "DESDE";
                escDesdeDentroDer.DisplayMemberBinding = binding1;
                escHastaDentroDer.Header = "HASTA";
                escHastaDentroDer.DisplayMemberBinding = binding2;

                escDesdeDer.Header = "DESDE";
                escDesdeDer.DisplayMemberBinding = binding1;
                escHastaDer.Header = "HASTA";
                escHastaDer.DisplayMemberBinding = binding2;
            }
        }

        private void AdaptarColores()
        {
            SolidColorBrush fondo = (SolidColorBrush)Application.Current.FindResource("PrimaryHueDarkBrush");
            Background = fondo;
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueMidBrush");
            lblCircunscripcion.Foreground = color;
        }

        private void CargarPartidos()
        {
            List<CPDataDTO> cpdatas = CPDataDTO.FromBSDto(dto);
            cpdatas.ForEach(partidosDisponibles.Add);
            partidosTotales = partidosDisponibles.ToList();
        }

        public void RecargarDatos(BrainStormDTO dto, bool oficiales)
        {
            this.dto = dto;
            this.oficiales = oficiales;
            InitializeVariables();
            InitializeInfo();
            AdaptarTablas();
        }

        private void imgFlechaEntraIzq_MouseEnter(object sender, MouseEventArgs e)
        {
            imgFlechaEntraIzq.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/flecha_pulsada.png", UriKind.Relative));
        }
        private void imgFlechaEntraIzq_MouseLeave(object sender, MouseEventArgs e)
        {
            imgFlechaEntraIzq.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/flecha.png", UriKind.Relative));
        }
        private void imgFlechaEntraIzq_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (partidosIzqListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosIzqListView.SelectedItem;
                partidosDentroIzq.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalIzq += independentismo ? double.Parse(seleccionado.porcentajeVoto) : int.Parse(seleccionado.escaniosHasta);
                lblEscaniosIzq.Content = independentismo ? $"Total: {totalIzq.ToString("F2")}%" : $"Total escaños: {totalIzq}";
                preparado = false;
                //Mandar mensaje de despliegue individualizado IZQ
                int index = partidosTotales.IndexOf(seleccionado);
                if (independentismo) { graficos.independentismoEntraIzquierda(index); } else { graficos.pactosEntraIzquierda(index); }

            }
        }

        private void partidosIzqListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosIzqListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosIzqListView.SelectedItem;
                partidosDentroIzq.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalIzq += independentismo ? double.Parse(seleccionado.porcentajeVoto) : int.Parse(seleccionado.escaniosHasta);
                lblEscaniosIzq.Content = independentismo ? $"Total: {totalIzq.ToString("F2")}%" : $"Total escaños: {totalIzq}";
                //Mandar mensaje de despliegue individualizado IZQ
                int index = partidosTotales.IndexOf(seleccionado);
                if (independentismo) { graficos.independentismoEntraIzquierda(index); } else { graficos.pactosEntraIzquierda(index); }
            }
        }
        private void partidosDerListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosDerListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDerListView.SelectedItem;
                partidosDentroDer.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalDer += independentismo ? double.Parse(seleccionado.porcentajeVoto) : int.Parse(seleccionado.escaniosHasta);
                lblEscaniosDer.Content = independentismo ? $"Total: {totalDer.ToString("F2")}%" : $"Total escaños: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                if (independentismo) { graficos.independentismoEntraDerecha(index); } else { graficos.pactosEntraDerecha(index); }
            }
        }

        private void partidosDentroIzqListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosDentroIzqListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDentroIzqListView.SelectedItem;
                partidosDentroIzq.Remove(seleccionado);
                partidosDisponibles.Add(seleccionado);
                totalIzq -= independentismo ? double.Parse(seleccionado.porcentajeVoto) : int.Parse(seleccionado.escaniosHasta);
                lblEscaniosIzq.Content = independentismo ? $"Total: {totalIzq.ToString("F2")}%" : $"Total escaños: {totalIzq}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                if (independentismo) { graficos.independentismoSaleIzquierda(index); } else { graficos.pactosSaleIzquierda(index); }
                ReordenarListas();
            }
        }
        private void partidosDentroDerListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosDentroDerListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDentroDerListView.SelectedItem;
                partidosDentroDer.Remove(seleccionado);
                partidosDisponibles.Add(seleccionado);
                totalDer -= independentismo ? double.Parse(seleccionado.porcentajeVoto) : int.Parse(seleccionado.escaniosHasta);
                lblEscaniosDer.Content = independentismo ? $"Total: {totalDer.ToString("F2")}%" : $"Total escaños: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                if (independentismo) { graficos.independentismoSaleDerecha(index); } else { graficos.pactosSaleDerecha(index); }
                ReordenarListas();
            }
        }

        private void ReordenarListas()
        {
            List<CPDataDTO> ordenados = partidosDisponibles.OrderByDescending(data => data, new CPDataComparer()).ToList();
            partidosDisponibles.Clear();
            ordenados.ForEach(partidosDisponibles.Add);
        }

        private void imgFlechaSaleIzq_MouseEnter(object sender, MouseEventArgs e)
        {
            imgFlechaSaleIzq.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/flecha_pulsada.png", UriKind.Relative));
        }
        private void imgFlechaSaleIzq_MouseLeave(object sender, MouseEventArgs e)
        {
            imgFlechaSaleIzq.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/flecha.png", UriKind.Relative));
        }
        private void imgFlechaSaleIzq_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (partidosDentroIzqListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDentroIzqListView.SelectedItem;
                partidosDentroIzq.Remove(seleccionado);
                partidosDisponibles.Add(seleccionado);
                totalIzq -= independentismo ? double.Parse(seleccionado.porcentajeVoto) : int.Parse(seleccionado.escaniosHasta);
                lblEscaniosIzq.Content = independentismo ? $"Total: {totalIzq.ToString("F2")}%" : $"Total escaños: {totalIzq}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                if (independentismo) { graficos.independentismoSaleIzquierda(index); } else { graficos.pactosSaleIzquierda(index); }
                ReordenarListas();
            }
        }

        private void partidosIzqListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            siglasIzq.Width = partidosIzqListView.ActualWidth / 3;
            escDesdeIzq.Width = partidosIzqListView.ActualWidth / 3;
            escHastaIzq.Width = partidosIzqListView.ActualWidth / 3;
        }
        private void partidosDentroIzqListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            siglasDentroIzq.Width = partidosDentroIzqListView.ActualWidth / 3;
            escDesdeDentroIzq.Width = partidosDentroIzqListView.ActualWidth / 3;
            escHastaDentroIzq.Width = partidosDentroIzqListView.ActualWidth / 3;
        }
        private void partidosDentroDerListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            siglasDentroDer.Width = partidosDentroDerListView.ActualWidth / 3;
            escDesdeDentroDer.Width = partidosDentroDerListView.ActualWidth / 3;
            escHastaDentroDer.Width = partidosDentroDerListView.ActualWidth / 3;
        }
        private void partidosDerListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            siglasDer.Width = partidosDerListView.ActualWidth / 3;
            escDesdeDer.Width = partidosDerListView.ActualWidth / 3;
            escHastaDer.Width = partidosDerListView.ActualWidth / 3;
        }

        private void imgFlechaEntraDer_MouseEnter(object sender, MouseEventArgs e)
        {
            imgFlechaEntraDer.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/flecha_pulsada.png", UriKind.Relative));
        }
        private void imgFlechaEntraDer_MouseLeave(object sender, MouseEventArgs e)
        {
            imgFlechaEntraDer.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/flecha.png", UriKind.Relative));
        }
        private void imgFlechaEntraDer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (partidosDerListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDerListView.SelectedItem;
                partidosDentroDer.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalDer += independentismo ? double.Parse(seleccionado.porcentajeVoto) : int.Parse(seleccionado.escaniosHasta);
                lblEscaniosDer.Content = independentismo ? $"Total: {totalDer.ToString("F2")}%" : $"Total escaños: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                if (independentismo) { graficos.independentismoEntraDerecha(index); } else { graficos.pactosEntraDerecha(index); }
            }
        }

        private void imgFlechaSaleDer_MouseEnter(object sender, MouseEventArgs e)
        {
            imgFlechaSaleDer.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/flecha_pulsada.png", UriKind.Relative));
        }
        private void imgFlechaSaleDer_MouseLeave(object sender, MouseEventArgs e)
        {
            imgFlechaSaleDer.Source = new BitmapImage(new Uri("/Elecciones;component/iconos/flecha.png", UriKind.Relative));
        }
        private void imgFlechaSaleDer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (partidosDentroDerListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDentroDerListView.SelectedItem;
                partidosDentroDer.Remove(seleccionado);
                partidosDisponibles.Add(seleccionado);
                totalDer -= independentismo ? double.Parse(seleccionado.porcentajeVoto) : int.Parse(seleccionado.escaniosHasta);
                lblEscaniosDer.Content = independentismo ? $"Total: {totalDer.ToString("F2")}%" : $"Total escaños: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                if (independentismo) { graficos.independentismoSaleDerecha(index); } else { graficos.pactosSaleDerecha(index); }
                ReordenarListas();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            partidosDentroDer.Clear();
            partidosDentroIzq.Clear();
            partidosDisponibles.Clear();
            CargarPartidos();
            totalIzq = 0;
            totalDer = 0;
            lblEscaniosIzq.Content = independentismo ? $"Total: {totalIzq}%" : $"Total escaños: {totalIzq}";
            lblEscaniosDer.Content = independentismo ? $"Total: {totalDer}%" : $"Total escaños: {totalDer}";

            if (independentismo) { graficos.independentismoReinicio(); } else { graficos.pactosReinicio(); }

        }

        private void btnPrepara_Click(object sender, RoutedEventArgs e)
        {
            EscribirPacto();
            preparado = true;
        }
        private void btnEntra_Click(object sender, RoutedEventArgs e)
        {
            if (!preparado)
            {
                EscribirPacto();
            }
            if (independentismo) { graficos.independentismoEntra(); } else { graficos.pactosEntra(); }
            pactoDentro = true;

        }
        private void btnSale_Click(object sender, RoutedEventArgs e)
        {
            graficos.pactosSale();
            pactoDentro = false;
            var main = Application.Current.MainWindow as MainWindow;
            main.Update();

        }

        //Este metodo tambien deselecciona los partidos, para poder elegir el siguiente
        private async void EscribirPacto()
        {
            //  if (partidosIzqListView.SelectedIndex != -1)
            //  {
            //
            //  }
            //
            //  LogicaArco arco = new LogicaArco(dto.circunscripcionDTO.escaniosTotales);
            //  await arco.ToJson(arco.GetPactos(partidosDentroIzq.ToList(), partidosDentroDer.ToList()));

            partidosDentroDerListView.SelectedIndex = -1;
            partidosDentroIzqListView.SelectedIndex = -1;
        }

        private void WindowClosing(object? sender, CancelEventArgs e)
        {
            var window = Application.Current.MainWindow as MainWindow;
            window.pactos = null;
        }


    }

}

