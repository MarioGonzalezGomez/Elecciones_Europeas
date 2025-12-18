using Elecciones.src.conexion;
using Elecciones.src.mensajes;
using Elecciones.src.utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for Config.xaml
    /// </summary>
    public partial class Config : Window
    {
        MainWindow main;
        ConfigManager configuration;

        public Config(ConexionEntityFramework conexionActiva, MainWindow main)
        {
            configuration = ConfigManager.GetInstance();
            InitializeComponent();
            AdaptarEntorno();
            cmbConexiones.SelectedItem = cmbConexiones.Items[0];
            InitializeDatos();
            AdaptarColores();
            this.main = main;
            this.Closing += WindowClosing;
            checkActualizacion.IsChecked = main.actualizacionActiva;
        }
        private void AdaptarEntorno()
        {
            string numElecciones = configuration.GetValue("numEleccionesSimultaneas");
            if (numElecciones == "2")
            {
                txtNombreDB3.Visibility = Visibility.Hidden;
                lblNombreDB3.Visibility = Visibility.Hidden;
            }
            if (numElecciones == "1")
            {
                txtNombreDB2.Visibility = Visibility.Hidden;
                lblNombreDB2.Visibility = Visibility.Hidden;
                txtNombreDB3.Visibility = Visibility.Hidden;
                lblNombreDB3.Visibility = Visibility.Hidden;
            }
            if (configuration.GetValue("activoPrime") == "1")
            {
                checkPrime.IsChecked = true;
                lblPuertoPrime.Visibility = Visibility.Visible;
                txtPuertoPrime.Visibility = Visibility.Visible;
            }
            if (configuration.GetValue("activoIPF") == "1")
            {
                checkIPF.IsChecked = true;
                lblDBIpf.Visibility = Visibility.Visible;
                txtDBIpf.Visibility = Visibility.Visible;
                lblPuertoIpf.Visibility = Visibility.Visible;
                txtPuertoIPF.Visibility = Visibility.Visible;
            }
            if (configuration.GetValue("regional") == "1")
            {
                checkRegional.IsChecked = true;
            }
        }

        private void AdaptarColores()
        {
            SolidColorBrush fondo = (SolidColorBrush)Application.Current.FindResource("PrimaryHueDarkBrush");
            Background = fondo;
            SolidColorBrush color = (SolidColorBrush)Application.Current.FindResource("PrimaryHueMidBrush");
            lblConexiones.Foreground = color;
            lblBD.Foreground = color;
            lblAdicionales.Foreground = color;
        }

        private void InitializeDatos()
        {
            txtIpPrincipal.Text = configuration.GetValue("dataServer");
            txtIpReserva.Text = configuration.GetValue("dataServerBackup");
            txtPuertoDB.Text = configuration.GetValue("dataPort");
            txtRuta.Text = configuration.GetValue("rutaArchivos");
            txtDBIpf.Text = configuration.GetValue("bdIPF");
            txtPuertoIPF.Text = configuration.GetValue("puertoIPF");
            txtPuertoPrime.Text = configuration.GetValue("puertoPrime");

            int theme = int.Parse(configuration.GetValue("theme"));
            cmbThemes.SelectedIndex = theme switch
            {
                1 => 0,
                2 => 1,
                _ => 0
            };

            int graficoManejado = int.Parse(configuration.GetValue("tablasGraficosPrincipal"));
            cmbGraficos.SelectedIndex = graficoManejado switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                4 => 3,
                _ => 0
            };

            RellenarComboBox();
        }

        private void RellenarComboBox()
        {
            txtNombreDB1.Text = configuration.GetValue("dataDB1");
            txtNombreDB2.Text = configuration.GetValue("dataDB2");
            txtNombreDB3.Text = configuration.GetValue("dataDB3");

            cmbConexiones.IsEnabled = true;
            cmbItem.Content = "Aplicar a todas";


            if (string.IsNullOrEmpty(txtNombreDB3.Text) && !string.IsNullOrEmpty(txtNombreDB2.Text))
            {
                cmbItem.Content = "Aplicar a todas";
                cmbConexiones.IsEnabled = true;
                for (int i = cmbConexiones.Items.Count - 1; i > 0; i--)
                {
                    cmbConexiones.Items.RemoveAt(i);
                }
            }
            if (!string.IsNullOrEmpty(txtNombreDB3.Text) && !string.IsNullOrEmpty(txtNombreDB2.Text))
            {
                for (int i = cmbConexiones.Items.Count - 1; i > 0; i--)
                {
                    cmbConexiones.Items.RemoveAt(i);
                }
            }


            if (!string.IsNullOrEmpty(txtNombreDB2.Text))
            {
                ComboBoxItem bd1 = new ComboBoxItem();
                bd1.Content = txtNombreDB1.Text;
                cmbConexiones.Items.Add(bd1);

                ComboBoxItem bd2 = new ComboBoxItem();
                bd2.Content = txtNombreDB2.Text;
                cmbConexiones.Items.Add(bd2);
            }
            else
            {
                cmbItem.Content = txtNombreDB1.Text;
                cmbConexiones.IsEnabled = false;
                for (int i = cmbConexiones.Items.Count - 1; i > 0; i--)
                {
                    cmbConexiones.Items.RemoveAt(i);
                }
            }

            if (!string.IsNullOrEmpty(txtNombreDB3.Text))
            {
                ComboBoxItem bd3 = new ComboBoxItem();
                bd3.Content = txtNombreDB3.Text;
                cmbConexiones.Items.Add(bd3);
            }
        }

        private void cmbConexiones_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cmbConexiones.Height = gridConexiones.ActualHeight / 10;
        }
        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            radio1.Height = gridConexiones.ActualHeight / 10;
            radio2.Height = gridConexiones.ActualHeight / 10;
            radio3.Height = gridConexiones.ActualHeight / 10;
        }

        private void checkActualizacion_Click(object sender, RoutedEventArgs e)
        {
            if (main != null)
            {
                bool activa = (bool)checkActualizacion.IsChecked;
                main.escuchador.salir = !activa;
                main.actualizacionActiva = activa;
            }
        }

        private void checkPrime_Click(object sender, RoutedEventArgs e)
        {
            if (checkPrime.IsChecked == true)
            {
                lblPuertoPrime.Visibility = Visibility.Visible;
                txtPuertoPrime.Visibility = Visibility.Visible;

                if (checkIPF.IsChecked == false)
                {
                    Grid.SetRow(lblPuertoPrime, 6);
                    Grid.SetRow(txtPuertoPrime, 7);
                }
                else
                {
                    Grid.SetRow(lblPuertoPrime, 10);
                    Grid.SetRow(txtPuertoPrime, 11);
                }
            }
            else
            {
                lblPuertoPrime.Visibility = Visibility.Hidden;
                txtPuertoPrime.Visibility = Visibility.Hidden;
            }
        }
        private void checkIPF_Click(object sender, RoutedEventArgs e)
        {
            if (checkIPF.IsChecked == true)
            {
                lblDBIpf.Visibility = Visibility.Visible;
                txtDBIpf.Visibility = Visibility.Visible;
                lblPuertoIpf.Visibility = Visibility.Visible;
                txtPuertoIPF.Visibility = Visibility.Visible;
                if (checkPrime.IsChecked == true)
                {
                    Grid.SetRow(lblPuertoPrime, 10);
                    Grid.SetRow(txtPuertoPrime, 11);
                }
            }
            else
            {
                lblDBIpf.Visibility = Visibility.Hidden;
                txtDBIpf.Visibility = Visibility.Hidden;
                lblPuertoIpf.Visibility = Visibility.Hidden;
                txtPuertoIPF.Visibility = Visibility.Hidden;

                if (checkPrime.IsChecked == true)
                {
                    Grid.SetRow(lblPuertoPrime, 6);
                    Grid.SetRow(txtPuertoPrime, 7);
                }
            }
        }

        private void cmbGraficos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (main != null)
            {
                configuration.SetValue("tablasGraficosPrincipal", $"{cmbGraficos.SelectedIndex + 1}");
                configuration.SetValue("bdIPF", configuration.GetValue($"headerTabla{cmbGraficos.SelectedIndex + 1}"));
                main.InitializeListView();
                main.ActualizarBotoneraGrupo();
            }
        }
        private void cmbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var app = (App)Application.Current;
            switch (cmbThemes.SelectedIndex)
            {
                case 0:
                    app.CambiarTheme(new Uri("styles/Light.xaml", UriKind.Relative));
                    break;
                case 1:
                    app.CambiarTheme(new Uri("styles/Dark.xaml", UriKind.Relative));
                    break;
                case 2:
                    app.CambiarTheme(new Uri("styles/Blue.xaml", UriKind.Relative));
                    break;
                default:
                    app.CambiarTheme(new Uri("styles/Light.xaml", UriKind.Relative));
                    break;
            }
            if (main != null)
            {
                configuration.SetValue("theme", $"{cmbThemes.SelectedIndex + 1}");
                main.CambioTheme();
                AdaptarColores();
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            configuration.SetValue("dataServer", txtIpPrincipal.Text);
            configuration.SetValue("dataServerBackup", txtIpReserva.Text);
            configuration.SetValue("dataPort", txtPuertoDB.Text);
            configuration.SetValue("dataDB1", txtNombreDB1.Text);
            configuration.SetValue("rutaArchivos", txtRuta.Text);
            configuration.SetValue("puertoIPF", txtPuertoIPF.Text);
            configuration.SetValue("puertoPrime", txtPuertoPrime.Text);

            configuration.SetValue("activoPrime", ((bool)checkPrime.IsChecked) ? "1" : "0");
            configuration.SetValue("activoIPF", ((bool)checkIPF.IsChecked) ? "1" : "0");
            configuration.SetValue("regional", ((bool)checkRegional.IsChecked) ? "1" : "0");

            string conexion;
            if (cmbConexiones.SelectedIndex == 0)
            {
                conexion = ((bool)radioPrincipal.IsChecked) ? "1" : ((bool)radioReserva.IsChecked) ? "2" : ((bool)radioLocal.IsChecked) ? "3" : configuration.GetValue("conexionDefault1");
                configuration.SetValue("conexionDefault1", conexion);
                configuration.SetValue("conexionDefault2", conexion);
                configuration.SetValue("conexionDefault3", conexion);
            }
            else
            {
                conexion = ((bool)radioPrincipal.IsChecked) ? "1" : ((bool)radioReserva.IsChecked) ? "2" : ((bool)radioLocal.IsChecked) ? "3" : configuration.GetValue($"conexionDefault{cmbConexiones.SelectedIndex}");
                configuration.SetValue($"conexionDefault{cmbConexiones.SelectedIndex}", conexion);
            }
            RellenarComboBox();
            configuration.SaveConfig();
            main.ReiniciarParametros();
        }

        private void WindowClosing(object? sender, CancelEventArgs e)
        {
            main.config = null;
        }

    }
}
