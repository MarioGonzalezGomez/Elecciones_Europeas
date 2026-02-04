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
        /// <summary>
        /// Circunscripción original del pacto - se mantiene fija durante toda la sesión
        /// </summary>
        private string circunscripcionOriginal;

        public bool pactoDentro;

        ConfigManager config;

        public Pactos(BrainStormDTO dto, bool oficiales)
        {
            this.dto = dto;
            this.oficiales = oficiales;
            // Guardar la circunscripción original del pacto ANTES de InitializeVariables
            this.circunscripcionOriginal = dto.circunscripcionDTO.nombre;
            InitializeComponent();
            InitializeVariables();
            InitializeInfo();
            AdaptarTablas();
            AdaptarColores();
            this.Closing += WindowClosing;
        }

        public Pactos(BrainStormDTO dto, bool oficiales, string tipoGraficoActual) : this(dto, oficiales)
        {
            // Si el tipo de gráfico es "ÚLTIMO ESCAÑO", automatizar la entrada
            if (string.Equals(tipoGraficoActual, "ÚLTIMO ESCAÑO", StringComparison.OrdinalIgnoreCase))
            {
                this.Loaded += (s, e) => AutoEntrarEnPacto();
            }
        }

        /// <summary>
        /// Automatiza la entrada en el pacto simulando el click en el botón "ENTRA"
        /// </summary>
        private void AutoEntrarEnPacto()
        {
            if (!preparado)
            {
                EscribirPacto();
            }
            graficos.pactosEntra();
            pactoDentro = true;
        }

        private void InitializeVariables()
        {
            var main = Application.Current.MainWindow as MainWindow;
            totalIzq = 0;
            totalDer = 0;
            mayoriaAbsoluta = dto.circunscripcionDTO.mayoria;
            // circunscripcionOriginal ya se inicializa en el constructor y NO se debe cambiar
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
            lblMayoria.Content = $"Mayor�a absoluta: {mayoriaAbsoluta}";
            lblEscaniosIzq.Content = $"Total esca�os: {totalIzq}";
            lblEscaniosDer.Content = $"Total esca�os: {totalDer}";
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
                Binding binding1 = new Binding("escanios");
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
                Binding binding1 = new Binding("escaniosDesdeSondeo");
                escDesdeIzq.DisplayMemberBinding = binding1;
                escHastaIzq.Header = "HASTA";
                Binding binding2 = new Binding("escaniosHastaSondeo");
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
            // Filtrar solo los partidos que tienen al menos 1 escaño
            List<CPDataDTO> partidosFiltrados = FiltrarPartidosConEscanios(cpdatas);
            partidosFiltrados.ForEach(partidosDisponibles.Add);
            partidosTotales = partidosDisponibles.ToList();
        }

        /// <summary>
        /// Filtra los partidos para mostrar solo aquellos que tienen al menos 1 escaño
        /// </summary>
        private List<CPDataDTO> FiltrarPartidosConEscanios(List<CPDataDTO> partidos)
        {
            return partidos.Where(p =>
            {
                if (int.TryParse(p.escanios, out int escanios))
                {
                    return escanios > 0;
                }
                return false;
            }).ToList();
        }

        public void RecargarDatos(BrainStormDTO dto, bool oficiales)
        {
            this.dto = dto;
            this.oficiales = oficiales;
            InitializeVariables();
            InitializeInfo();
            AdaptarTablas();
        }

        /// <summary>
        /// Actualiza los datos del pacto en vivo manteniendo los partidos en sus listas correspondientes.
        /// Este método preserva la posición de los partidos en las listas (partidosDentroIzq, partidosDentroDer)
        /// mientras actualiza sus datos numéricos (escaños, votantes, porcentajes, etc.)
        /// IMPORTANTE: Solo actualiza si los datos corresponden a la circunscripción original del pacto
        /// </summary>
        /// <param name="dtoActualizado">DTO con los datos actualizados de la circunscripción</param>
        /// <param name="oficiales">Indica si los datos son oficiales o sondeo</param>
        /// <param name="tipoGrafico">Tipo de gráfico que está actualmente en emisión (PACTÓMETRO, MAYORÍAS, CARTÓN PARTIDOS, ÚLTIMO ESCAÑO, etc.)</param>
        public void ActualizaPacto(BrainStormDTO dtoActualizado, bool oficiales, string tipoGrafico)
        {


            // Preservar referencias a los partidos en sus listas actuales
            List<CPDataDTO> partidosEnIzq = partidosDentroIzq.ToList();
            List<CPDataDTO> partidosEnDer = partidosDentroDer.ToList();

            // Actualizar el dto con los nuevos datos
            this.dto = dtoActualizado;
            this.oficiales = oficiales;
            this.mayoriaAbsoluta = dto.circunscripcionDTO.mayoria;

            // Recargar la lista de partidos disponibles con los nuevos datos filtrados
            List<CPDataDTO> cpdatasNuevas = CPDataDTO.FromBSDto(dto);
            List<CPDataDTO> cpdatasNuevasFiltradas = FiltrarPartidosConEscanios(cpdatasNuevas);

            // Actualizar los datos de los partidos preservando su posición en las listas
            ActualizarPartidosEnLista(partidosDentroIzq, cpdatasNuevasFiltradas);
            ActualizarPartidosEnLista(partidosDentroDer, cpdatasNuevasFiltradas);
            ActualizarPartidosEnLista(partidosDisponibles, cpdatasNuevasFiltradas);

            // Recalcular totales
            totalIzq = 0;
            totalDer = 0;
            foreach (var partido in partidosDentroIzq)
            {
                if (int.TryParse(partido.escanios, out int esc))
                    totalIzq += esc;
            }
            foreach (var partido in partidosDentroDer)
            {
                if (int.TryParse(partido.escanios, out int esc))
                    totalDer += esc;
            }

            lblEscaniosIzq.Content = $"Total escaños: {totalIzq}";
            lblEscaniosDer.Content = $"Total escaños: {totalDer}";
            lblMayoria.Content = $"Mayoría absoluta: {mayoriaAbsoluta}";

            // Disparar el método para actualizar señales gráficas
            ActualizarSenalesGraficas(dtoActualizado, tipoGrafico);
        }

        /// <summary>
        /// Actualiza los datos de los partidos en una lista observable, preservando su posición
        /// Actualiza las propiedades del objeto en lugar de reemplazarlo, para que la UI se refresque correctamente
        /// </summary>
        private void ActualizarPartidosEnLista(ObservableCollection<CPDataDTO> lista, List<CPDataDTO> datosNuevos)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                CPDataDTO partidoActual = lista[i];
                CPDataDTO datosActualizados = datosNuevos.FirstOrDefault(p => p.codigo == partidoActual.codigo);

                if (datosActualizados != null)
                {
                    // Actualizar los datos del partido SIN reemplazar el objeto, para que la UI se refresque
                    partidoActual.ActualizarDatos(datosActualizados);
                }
            }
        }

        /// <summary>
        /// Método de entrada para actualizar los datos desde el exterior.
        /// Realiza su propia captura de datos utilizando el controlador y la circunscripción original.
        /// </summary>
        public void ActualizarDatos(bool oficiales, string tipoGrafico, int avance, int tipoElecciones)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main == null || main.conexionActiva == null) return;

            try
            {
                BrainStormController controller = BrainStormController.GetInstance(main.conexionActiva);
                BrainStormDTO dtoActualizado = null;

                if (oficiales)
                {
                    dtoActualizado = controller.FindByNameCircunscripcionOficial(circunscripcionOriginal, avance, tipoElecciones);
                }
                else
                {
                    dtoActualizado = controller.FindByNameCircunscripcionSondeo(circunscripcionOriginal, avance, tipoElecciones);
                }

                if (dtoActualizado != null)
                {
                    if (pactoDentro)
                    {
                        ActualizaPacto(dtoActualizado, oficiales, tipoGrafico);
                    }
                    else
                    {
                        RecargarDatos(dtoActualizado, oficiales);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exception properly
                Console.WriteLine($"Error actualizando datos de Pactos: {ex.Message}");
            }
        }

        /// <summary>
        /// Método que dispara señales gráficas según el tipo de pacto en emisión.
        /// Se ejecuta automáticamente cuando hay una actualización de datos en vivo.
        /// Mantiene los datos sincronizados con la pantalla gráfica según el tipo de pacto.
        /// </summary>
        /// <param name="dtoActualizado">DTO con la información actualizada de la circunscripción</param>
        /// <param name="tipoGrafico">Tipo de gráfico que está actualmente en emisión (ej: "PACTÓMETRO", "MAYORÍAS", etc.)</param>
        private void ActualizarSenalesGraficas(BrainStormDTO dtoActualizado, string tipoGrafico)
        {
            graficos.pactometroActualiza(dtoActualizado, tipoGrafico);
        }

        /// <summary>
        /// Obtiene el nombre de la circunscripción actual del pacto
        /// </summary>
        public string GetCircunscripcionActual()
        {
            return dto?.circunscripcionDTO?.nombre ?? "";
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
                var main = Application.Current.MainWindow as MainWindow;
                CPDataDTO seleccionado = (CPDataDTO)partidosIzqListView.SelectedItem;
                partidosDentroIzq.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalIzq += int.Parse(seleccionado.escanios);
                lblEscaniosIzq.Content = $"Total esca�os: {totalIzq}";
                preparado = false;
                //Mandar mensaje de despliegue individualizado IZQ
                if (main.ultimoEscanoDentro)
                {
                    graficos.ultimoEntraPartido(main.dto, seleccionado, true);
                }
                else if (main.sfPactometroDentro) { }
                else
                {
                    PartidoDTO pseleccionado = main.dto.partidos.FirstOrDefault(par => par.codigo.Equals(seleccionado.codigo));
                    graficos.pactosEntraIzquierda(main.dto, pseleccionado);
                }
            }
        }

        private void partidosIzqListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosIzqListView.SelectedItem != null)
            {
                var main = Application.Current.MainWindow as MainWindow;
                CPDataDTO seleccionado = (CPDataDTO)partidosIzqListView.SelectedItem;
                partidosDentroIzq.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalIzq += int.Parse(seleccionado.escanios);
                lblEscaniosIzq.Content = $"Total esca�os: {totalIzq}";
                //Mandar mensaje de despliegue individualizado IZQ
                if (main.ultimoEscanoDentro)
                {
                    graficos.ultimoEntraPartido(main.dto, seleccionado, true);
                }
                else if (main.sfPactometroDentro) { }
                else
                {
                    PartidoDTO pseleccionado = main.dto.partidos.FirstOrDefault(par => par.codigo.Equals(seleccionado.codigo));
                    graficos.pactosEntraIzquierda(main.dto, pseleccionado);
                }
            }
        }
        private void partidosDerListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosDerListView.SelectedItem != null)
            {
                var main = Application.Current.MainWindow as MainWindow;
                CPDataDTO seleccionado = (CPDataDTO)partidosDerListView.SelectedItem;
                partidosDentroDer.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalDer += int.Parse(seleccionado.escanios);
                lblEscaniosDer.Content = $"Total esca�os: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                if (main.ultimoEscanoDentro)
                {
                    graficos.ultimoEntraPartido(main.dto, seleccionado, false);
                }
                else if (main.sfPactometroDentro) { }
                else
                {
                    PartidoDTO pseleccionado = main.dto.partidos.FirstOrDefault(par => par.codigo.Equals(seleccionado.codigo));
                    graficos.pactosEntraDerecha(main.dto, pseleccionado);
                }
            }
        }

        private void partidosDentroIzqListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosDentroIzqListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDentroIzqListView.SelectedItem;
                partidosDentroIzq.Remove(seleccionado);
                partidosDisponibles.Add(seleccionado);
                totalIzq -= int.Parse(seleccionado.escanios);
                lblEscaniosIzq.Content = $"Total esca�os: {totalIzq}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                graficos.pactosSaleIzquierda(index);
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
                totalDer -= int.Parse(seleccionado.escanios);
                lblEscaniosDer.Content = $"Total esca�os: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                graficos.pactosSaleDerecha(index);
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
                totalIzq -= int.Parse(seleccionado.escanios);
                lblEscaniosIzq.Content = $"Total esca�os: {totalIzq}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                graficos.pactosSaleIzquierda(index);
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
                var main = Application.Current.MainWindow as MainWindow;
                CPDataDTO seleccionado = (CPDataDTO)partidosDerListView.SelectedItem;
                partidosDentroDer.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalDer += int.Parse(seleccionado.escanios);
                lblEscaniosDer.Content = $"Total esca�os: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                if (main.ultimoEscanoDentro)
                {
                    graficos.ultimoEntraPartido(main.dto, seleccionado, false);
                }
                else if (main.sfPactometroDentro) { }
                else
                {
                    PartidoDTO pseleccionado = main.dto.partidos.FirstOrDefault(par => par.codigo.Equals(seleccionado.codigo));
                    graficos.pactosEntraDerecha(main.dto, pseleccionado);
                }
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
                totalDer -= int.Parse(seleccionado.escanios);
                lblEscaniosDer.Content = $"Total esca�os: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                graficos.pactosSaleDerecha(index);
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
            lblEscaniosIzq.Content = $"Total escaños: {totalIzq}";
            lblEscaniosDer.Content = $"Total escaños: {totalDer}";
            //HARDCODED, esto mejorar para que sea adaptable al tipo de grafico actual
            graficos.pactosReinicio("FICHAS");

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
            graficos.pactosEntra();
            pactoDentro = true;

        }
        private void btnSale_Click(object sender, RoutedEventArgs e)
        {
            graficos.pactosSale();
            graficos.ultimoLimpiaPartidos();
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

