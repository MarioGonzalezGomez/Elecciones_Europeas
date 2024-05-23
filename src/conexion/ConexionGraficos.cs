using Elecciones_Europeas.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Elecciones_Europeas.src.conexion
{
    internal class ConexionGraficos
    {
        private static ConexionGraficos? instanceIPF;
        private static ConexionGraficos? instancePrime;
        private static ConexionGraficos? instance;
        private Socket client;
        private string _ip;
        private int _port;

        ConfigManager configuration;

        private ConexionGraficos(string programaGrafico)
        {
            configuration = ConfigManager.GetInstance();
            if (String.Equals(programaGrafico, "prime", StringComparison.OrdinalIgnoreCase))
            {
                _ip = configuration.GetValue("ipPrime");
                _port = int.Parse(configuration.GetValue("puertoPrime"));
            }
            if (String.Equals(programaGrafico, "ipf", StringComparison.OrdinalIgnoreCase))
            {
                _ip = configuration.GetValue("ipIPF");
                _port = int.Parse(configuration.GetValue("puertoIPF"));
            }
            AbrirConexion(programaGrafico);
        }
        private ConexionGraficos(string ip, int port)
        {
            configuration = ConfigManager.GetInstance();
            _ip = ip;
            _port = port;
            AbrirConexion();
        }

        public static ConexionGraficos GetInstance(string ip, int port)
        {
            if (instance == null)
            {
                instance = new ConexionGraficos(ip, port);
            }
            return instance;
        }

        public static ConexionGraficos GetInstanceIPF()
        {
            if (instanceIPF == null)
            {
                instanceIPF = new ConexionGraficos("ipf");
            }
            return instanceIPF;
        }

        public static ConexionGraficos GetInstancePrime()
        {
            if (instancePrime == null)
            {
                instancePrime = new ConexionGraficos("prime");
            }
            return instancePrime;
        }

        private void AbrirConexion(string programaGrafico = "")
        {
            Console.WriteLine("Iniciando conexión...");
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(_ip, _port);
            }
            catch (Exception ex)
            {
                if (String.Equals(programaGrafico, "prime", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Error al conectar con PRIME en la IP: {_ip}", "Error de conexión Prime", MessageBoxButton.OK, MessageBoxImage.Error);
                    configuration.SetValue("activoPrime", "0");
                }
                else if (String.Equals(programaGrafico, "ipf", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Error al conectar con BRAINSTORM en la IP: {_ip}", "Error de conexión Brainstorm", MessageBoxButton.OK, MessageBoxImage.Error);
                    configuration.SetValue("activoIPF", "0");
                }
                else
                {
                    MessageBox.Show($"Error al conectar con el programa gráfico en la IP: {_ip}", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        public void EnviarMensaje(string mensaje)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(mensaje);
            client.Send(bytes);
            Console.WriteLine($"{mensaje}");
        }

        public string RecibirMensaje(string solicitud)
        {
            try
            {
                byte[] solicitudBuffer = Encoding.UTF8.GetBytes(solicitud);
                client.Send(solicitudBuffer);

                // Establecer un tiempo de espera de 5 segundos para la operación de recepción
                client.ReceiveTimeout = 500;

                byte[] receivedBuffer = new byte[1024];
                int receivedBytes = client.Receive(receivedBuffer);
                string respuesta = Encoding.UTF8.GetString(receivedBuffer, 0, receivedBytes);
                return respuesta;
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    MessageBox.Show("Se agotó el tiempo de espera al recibir datos.", "Error de tiempo de espera", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Error al recibir mensaje itemget desde IPF: {ex.Message}", "Error Itemget", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al recibir mensaje itemget desde IPF: {ex}", "Error Itemget", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public void ReiniciarConexion(string programaGrafico)
        {
            if (String.Equals(programaGrafico, "prime", StringComparison.OrdinalIgnoreCase))
            {
                CerrarConexion("prime");
                instancePrime = GetInstancePrime();
                instancePrime._ip = configuration.GetValue("ipPrime");
                instancePrime._port = int.Parse(configuration.GetValue("puertoPrime"));
            }
            if (String.Equals(programaGrafico, "ipf", StringComparison.OrdinalIgnoreCase))
            {
                CerrarConexion("ipf");
                instanceIPF = GetInstanceIPF();
                instanceIPF._ip = configuration.GetValue("ipIPF");
                instanceIPF._port = int.Parse(configuration.GetValue("puertoIPF"));
            }
            AbrirConexion(programaGrafico);
        }

        public void CerrarConexion(string programaGrafico = "")
        {
            if (String.Equals(programaGrafico, "prime", StringComparison.OrdinalIgnoreCase))
            {
                instancePrime.client.Close();
                instancePrime = null;
            }
            else if (String.Equals(programaGrafico, "ipf", StringComparison.OrdinalIgnoreCase))
            {
                instanceIPF.client.Close();
                instanceIPF = null;
            }
            else
            {
                client.Close();
                instance = null;
            }

        }

    }
}
