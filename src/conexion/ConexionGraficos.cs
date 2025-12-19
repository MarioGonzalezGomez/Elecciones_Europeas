using Elecciones_Europeas.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Elecciones_Europeas.src.service;

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
        public bool conectado = false;

        ConfigManager configuration;
        private INotificationService _notificationService;
        private ILoggerService _loggerService;

        private ConexionGraficos(string programaGrafico)
        {
            _notificationService = NotificationService.GetInstance();
            _loggerService = FileLoggerService.GetInstance();
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
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
            _notificationService = NotificationService.GetInstance();
            _loggerService = FileLoggerService.GetInstance();
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
                conectado = true;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error connecting to {programaGrafico} at {_ip}:{_port}", ex);
                if (String.Equals(programaGrafico, "prime", StringComparison.OrdinalIgnoreCase))
                {
                    _notificationService.ShowError($"Error al conectar con PRIME en la IP: {_ip}", "Error de conexión Prime");
                    configuration.SetValue("activoPrime", "0");
                    conectado = false;
                }
                else if (String.Equals(programaGrafico, "ipf", StringComparison.OrdinalIgnoreCase))
                {
                    _notificationService.ShowError($"Error al conectar con BRAINSTORM en la IP: {_ip}", "Error de conexión Brainstorm");
                    configuration.SetValue("activoIPF", "0");
                    conectado = false;
                }
                else
                {
                    _notificationService.ShowError($"Error al conectar con el programa gráfico en la IP: {_ip}", "Error de conexión");
                    conectado = false;
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
                _loggerService.LogError("SocketException in RecibirMensaje", ex);
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    _notificationService.ShowError("Se agotó el tiempo de espera al recibir datos.", "Error de tiempo de espera");
                }
                else
                {
                    _notificationService.ShowError($"Error al recibir mensaje itemget desde IPF: {ex.Message}", "Error Itemget");
                }
                return null;
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Exception in RecibirMensaje", ex);
                _notificationService.ShowError($"Error al recibir mensaje itemget desde IPF: {ex}", "Error Itemget");
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
