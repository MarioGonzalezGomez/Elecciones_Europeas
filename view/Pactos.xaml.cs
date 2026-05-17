using Elecciones.src.controller;
using Elecciones.src.logic;
using Elecciones.src.model;
using Elecciones.src.model.DTO.BrainStormDTO;
using Elecciones.src.model.IPF;
using Elecciones.src.model.IPF.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Elecciones
{
    /// <summary>
    /// Interaction logic for Pactos.xaml
    /// </summary>
    public partial class Pactos : Window
    {
        private BrainStormDTO dto = null!;
        private bool oficiales;
        private int mayoriaAbsoluta;
        private double totalIzq;
        private double totalDer;
        private List<CPDataDTO> partidosTotales = new();
        private ObservableCollection<CPDataDTO> partidosDisponibles = new();
        private ObservableCollection<CPDataDTO> partidosDentroIzq = new();
        private ObservableCollection<CPDataDTO> partidosDentroDer = new();
        private GraphicController graficos = null!;
        private bool preparado;
        /// <summary>
        /// CircunscripciÃ³n original del pacto - se mantiene fija durante toda la sesiÃ³n
        /// </summary>
        private string circunscripcionOriginal = string.Empty;

        public bool pactoDentro;

        ConfigManager config = null!;

        public Pactos(BrainStormDTO dto, bool oficiales)
        {
            this.dto = dto;
            this.oficiales = oficiales;
            // Guardar la circunscripciÃ³n original del pacto ANTES de InitializeVariables
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
            // Si el tipo de grÃ¡fico es "ÃšLTIMO ESCAÃ‘O", automatizar la entrada
            if (string.Equals(tipoGraficoActual, "ÃšLTIMO ESCAÃ‘O", StringComparison.OrdinalIgnoreCase))
            {
                this.Loaded += (s, e) => AutoEntrarEnPacto();
            }
        }

        /// <summary>
        /// Automatiza la entrada en el pacto simulando el click en el botÃ³n "ENTRA"
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

        private MainWindow? GetMainWindow()
        {
            return Application.Current.MainWindow as MainWindow;
        }

        private PartidoDTO? FindPartidoPorCodigo(string codigoPartido)
        {
            return dto.partidos.FirstOrDefault(par => par.codigo.Equals(codigoPartido, StringComparison.Ordinal));
        }

        private void EnviarPartidoEntraGrafico(CPDataDTO seleccionado, bool izquierda)
        {
            MainWindow? main = GetMainWindow();

            if (main?.ultimoEscanoDentro == true)
            {
                graficos.ultimoEntraPartido(dto, seleccionado, izquierda);
                return;
            }

            PartidoDTO? pSeleccionado = FindPartidoPorCodigo(seleccionado.codigo);
            if (pSeleccionado == null)
            {
                return;
            }

            if (main?.sfPactometroDentro == true)
            {
                graficos.sfPactometroPartidoEntra(dto, pSeleccionado, izquierda);
                return;
            }

            if (izquierda)
            {
                graficos.pactosEntraIzquierda(dto, pSeleccionado);
            }
            else
            {
                graficos.pactosEntraDerecha(dto, pSeleccionado);
            }
        }

        private void InitializeInfo()
        {
            lblCircunscripcion.Content = dto.circunscripcionDTO.nombre;
            lblMayoria.Content = $"MayorÃ­a absoluta: {mayoriaAbsoluta}";
            lblEscaniosIzq.Content = $"Total escaÃ±os: {totalIzq}";
            lblEscaniosDer.Content = $"Total escaÃ±os: {totalDer}";
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
                escDesdeIzq.Header = "ESCAÃ‘OS";
                Binding binding1 = new Binding("escanios");
                escDesdeIzq.DisplayMemberBinding = binding1;
                escHastaIzq.Header = "% VOTO";
                Binding binding2 = new Binding("porcentajeVoto");
                escHastaIzq.DisplayMemberBinding = binding2;

                escDesdeDentroIzq.Header = "ESCAÃ‘OS";
                escDesdeDentroIzq.DisplayMemberBinding = binding1;
                escHastaDentroIzq.Header = "% VOTO";
                escHastaDentroIzq.DisplayMemberBinding = binding2;

                escDesdeDentroDer.Header = "ESCAÃ‘OS";
                escDesdeDentroDer.DisplayMemberBinding = binding1;
                escHastaDentroDer.Header = "% VOTO";
                escHastaDentroDer.DisplayMemberBinding = binding2;

                escDesdeDer.Header = "ESCAÃ‘OS";
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
            // Filtrar solo los partidos que tienen al menos 1 escaÃ±o
            List<CPDataDTO> partidosFiltrados = FiltrarPartidosConEscanios(cpdatas);
            partidosFiltrados.ForEach(partidosDisponibles.Add);
            partidosTotales = partidosDisponibles.ToList();
        }

        /// <summary>
        /// Filtra los partidos para mostrar solo aquellos que tienen al menos 1 escaÃ±o
        /// </summary>
        private List<CPDataDTO> FiltrarPartidosConEscanios(List<CPDataDTO> partidos)
        {
            return partidos.Where(p =>
            {
                if (oficiales)
                {
                    return int.TryParse(p.escanios, out int escaniosOficiales) && escaniosOficiales > 0;
                }

                int escaniosDesde = int.TryParse(p.escaniosDesdeSondeo, out int desde) ? desde : 0;
                int escaniosHasta = int.TryParse(p.escaniosHastaSondeo, out int hasta) ? hasta : 0;
                return escaniosDesde > 0 || escaniosHasta > 0;
            }).ToList();
        }

        private int GetEscaniosParaTotal(CPDataDTO partido)
        {
            if (partido == null) return 0;

            if (oficiales)
            {
                return int.TryParse(partido.escanios, out int escanios) ? escanios : 0;
            }

            // En sondeo usamos "hasta" para mostrar un total con valor Ãºtil.
            return int.TryParse(partido.escaniosHastaSondeo, out int hasta) ? hasta : 0;
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
        /// Este mÃ©todo preserva la posiciÃ³n de los partidos en las listas (partidosDentroIzq, partidosDentroDer)
        /// mientras actualiza sus datos numÃ©ricos (escaÃ±os, votantes, porcentajes, etc.)
        /// IMPORTANTE: Solo actualiza si los datos corresponden a la circunscripciÃ³n original del pacto
        /// </summary>
        /// <param name="dtoActualizado">DTO con los datos actualizados de la circunscripciÃ³n</param>
        /// <param name="oficiales">Indica si los datos son oficiales o sondeo</param>
        /// <param name="tipoGrafico">Tipo de grÃ¡fico que estÃ¡ actualmente en emisiÃ³n (PACTÃ“METRO, MAYORÃAS, CARTÃ“N PARTIDOS, ÃšLTIMO ESCAÃ‘O, etc.)</param>
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

            // Actualizar los datos de los partidos preservando su posiciÃ³n en las listas
            ActualizarPartidosEnLista(partidosDentroIzq, cpdatasNuevasFiltradas);
            ActualizarPartidosEnLista(partidosDentroDer, cpdatasNuevasFiltradas);
            ActualizarPartidosEnLista(partidosDisponibles, cpdatasNuevasFiltradas);

            // Recalcular totales
            totalIzq = 0;
            totalDer = 0;
            foreach (var partido in partidosDentroIzq)
            {
                totalIzq += GetEscaniosParaTotal(partido);
            }
            foreach (var partido in partidosDentroDer)
            {
                totalDer += GetEscaniosParaTotal(partido);
            }

            lblEscaniosIzq.Content = $"Total escaÃ±os: {totalIzq}";
            lblEscaniosDer.Content = $"Total escaÃ±os: {totalDer}";
            lblMayoria.Content = $"MayorÃ­a absoluta: {mayoriaAbsoluta}";

            // Disparar el mÃ©todo para actualizar seÃ±ales grÃ¡ficas
            ActualizarSenalesGraficas(dtoActualizado, tipoGrafico);
        }

        /// <summary>
        /// Actualiza los datos de los partidos en una lista observable, preservando su posiciÃ³n
        /// Actualiza las propiedades del objeto en lugar de reemplazarlo, para que la UI se refresque correctamente
        /// </summary>
        private void ActualizarPartidosEnLista(ObservableCollection<CPDataDTO> lista, List<CPDataDTO> datosNuevos)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                CPDataDTO partidoActual = lista[i];
                CPDataDTO? datosActualizados = datosNuevos.FirstOrDefault(p => p.codigo == partidoActual.codigo);

                if (datosActualizados != null)
                {
                    // Actualizar los datos del partido SIN reemplazar el objeto, para que la UI se refresque
                    partidoActual.ActualizarDatos(datosActualizados);
                }
            }
        }

        /// <summary>
        /// MÃ©todo de entrada para actualizar los datos desde el exterior.
        /// Realiza su propia captura de datos utilizando el controlador y la circunscripciÃ³n original.
        /// </summary>
        public void ActualizarDatos(bool oficiales, string tipoGrafico, int avance, int tipoElecciones)
        {
            MainWindow? main = GetMainWindow();
            if (main == null || main.conexionActiva == null) return;

            try
            {
                BrainStormController controller = BrainStormController.GetInstance(main.conexionActiva);
                BrainStormDTO? dtoActualizado = null;

                if (oficiales)
                {
                    dtoActualizado = controller.FindByNameCircunscripcionOficialSinFiltrar(circunscripcionOriginal, avance, tipoElecciones);
                }
                else
                {
                    dtoActualizado = controller.FindByNameCircunscripcionSondeoSinFiltrar(circunscripcionOriginal, avance, tipoElecciones);
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
        /// MÃ©todo que dispara seÃ±ales grÃ¡ficas segÃºn el tipo de pacto en emisiÃ³n.
        /// Se ejecuta automÃ¡ticamente cuando hay una actualizaciÃ³n de datos en vivo.
        /// Mantiene los datos sincronizados con la pantalla grÃ¡fica segÃºn el tipo de pacto.
        /// </summary>
        /// <param name="dtoActualizado">DTO con la informaciÃ³n actualizada de la circunscripciÃ³n</param>
        /// <param name="tipoGrafico">Tipo de grÃ¡fico que estÃ¡ actualmente en emisiÃ³n (ej: "PACTÃ“METRO", "MAYORÃAS", etc.)</param>
        private void ActualizarSenalesGraficas(BrainStormDTO dtoActualizado, string tipoGrafico)
        {
            graficos.pactometroActualiza(dtoActualizado, tipoGrafico);
        }

        /// <summary>
        /// Obtiene el nombre de la circunscripciÃ³n actual del pacto
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
                CPDataDTO seleccionado = (CPDataDTO)partidosIzqListView.SelectedItem;
                partidosDentroIzq.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalIzq += GetEscaniosParaTotal(seleccionado);
                lblEscaniosIzq.Content = $"Total escaÃ±os: {totalIzq}";
                preparado = false;
                EnviarPartidoEntraGrafico(seleccionado, true);
            }
        }

        private void partidosIzqListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosIzqListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosIzqListView.SelectedItem;
                partidosDentroIzq.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalIzq += GetEscaniosParaTotal(seleccionado);
                lblEscaniosIzq.Content = $"Total escaÃ±os: {totalIzq}";
                EnviarPartidoEntraGrafico(seleccionado, true);
            }
        }
        private void partidosDerListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosDerListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDerListView.SelectedItem;
                partidosDentroDer.Add(seleccionado);
                partidosDisponibles.Remove(seleccionado);
                totalDer += GetEscaniosParaTotal(seleccionado);
                lblEscaniosDer.Content = $"Total escaÃ±os: {totalDer}";
                EnviarPartidoEntraGrafico(seleccionado, false);
            }
        }

        private void partidosDentroIzqListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (partidosDentroIzqListView.SelectedItem != null)
            {
                CPDataDTO seleccionado = (CPDataDTO)partidosDentroIzqListView.SelectedItem;
                partidosDentroIzq.Remove(seleccionado);
                partidosDisponibles.Add(seleccionado);
                totalIzq -= GetEscaniosParaTotal(seleccionado);
                lblEscaniosIzq.Content = $"Total escaÃ±os: {totalIzq}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                graficos.pactosSaleIzquierda();
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
                totalDer -= GetEscaniosParaTotal(seleccionado);
                lblEscaniosDer.Content = $"Total escaÃ±os: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                graficos.pactosSaleDerecha();
                ReordenarListas();
            }
        }

        private void ReordenarListas()
        {
            IComparer<CPDataDTO> comparer = oficiales
                ? new CPDataComparer()
                : new CPDataComparerSondeo();
            List<CPDataDTO> ordenados = partidosDisponibles.OrderByDescending(data => data, comparer).ToList();
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
                totalIzq -= GetEscaniosParaTotal(seleccionado);
                lblEscaniosIzq.Content = $"Total escaÃ±os: {totalIzq}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                graficos.pactosSaleIzquierda();
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
                totalDer += GetEscaniosParaTotal(seleccionado);
                lblEscaniosDer.Content = $"Total escaÃ±os: {totalDer}";
                EnviarPartidoEntraGrafico(seleccionado, false);
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
                totalDer -= GetEscaniosParaTotal(seleccionado);
                lblEscaniosDer.Content = $"Total escaÃ±os: {totalDer}";
                //Mandar mensaje de despliegue individualizado DER
                int index = partidosTotales.IndexOf(seleccionado);
                graficos.pactosSaleDerecha();
                ReordenarListas();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            partidosDentroDer.Clear();
            partidosDentroIzq.Clear();
            partidosDisponibles.Clear();
            CargarPartidos();
            totalIzq = 0;
            totalDer = 0;
            lblEscaniosIzq.Content = $"Total escaÃ±os: {totalIzq}";
            lblEscaniosDer.Content = $"Total escaÃ±os: {totalDer}";
            //HARDCODED, esto mejorar para que sea adaptable al tipo de grafico actual
            if (main != null && main.EsCabeceraSuperfaldon())
            {
                graficos.sfPactometroReinicio();
            }
            else
            {
                graficos.pactosReinicio("FICHAS");
            }

        }

        private void btnPrepara_Click(object sender, RoutedEventArgs e)
        {
            EscribirPacto();
            preparado = true;
        }
        private void btnEntra_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (!preparado)
            {
                EscribirPacto();
            }
            if (main != null && main.EsCabeceraSuperfaldon())
            {
                graficos.sfPactometroEntra();
                main.sfPactometroDentro = true;
            }
            else
            {
                graficos.pactosEntra();
            }
            pactoDentro = true;

        }
        private void btnSale_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main != null && main.EsCabeceraSuperfaldon())
            {
                graficos.sfPactometroSale();
                main.sfPactometroDentro = false;
            }
            else
            {
                graficos.pactosSale();
            }
            graficos.ultimoLimpiaPartidos();
            pactoDentro = false;
        }

        //Este metodo tambien deselecciona los partidos, para poder elegir el siguiente
        private void EscribirPacto()
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
            MainWindow? window = GetMainWindow();
            if (window != null)
            {
                window.pactos = null;
            }
        }


    }

}

