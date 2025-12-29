using Elecciones.src.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Elecciones.src.service;

namespace Elecciones.src.conexion
{
    internal class ConexionGraficos
    {
        private static ConexionGraficos? instanceIPF;
        private static ConexionGraficos? instancePrime;
        private static ConexionGraficos? instance;
        private Socket client;
        private string _ip;
        private int _port;
        private string _programaGrafico;  // Track connection type for auto-reconnect
        public bool conectado = false;

        ConfigManager configuration;
        private INotificationService _notificationService;
        private ILoggerService _loggerService;

        private ConexionGraficos(string programaGrafico)
        {
            _notificationService = NotificationService.GetInstance();
            _loggerService = FileLoggerService.GetInstance();
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
            _programaGrafico = programaGrafico;  // Store for auto-reconnect
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
                // Use a 3-second timeout for connection attempts
                var connectTask = client.ConnectAsync(_ip, _port);
                if (!connectTask.Wait(TimeSpan.FromSeconds(2)))
                {
                    client.Close();
                    throw new TimeoutException($"Connection to {_ip}:{_port} timed out after 3 seconds");
                }
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
            try
            {
                if (!conectado)
                {
                    // Try to reconnect if not connected
                    _loggerService.LogError($"Not connected to {_programaGrafico}, attempting reconnect...", null);
                    AbrirConexion(_programaGrafico);
                }
                
                byte[] bytes = Encoding.UTF8.GetBytes(mensaje);
                client.Send(bytes);
                Console.WriteLine($"{mensaje}");
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error sending message to {_programaGrafico}, attempting reconnect...", ex);
                conectado = false;
                
                // Try to reconnect and resend once
                try
                {
                    AbrirConexion(_programaGrafico);
                    if (conectado)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(mensaje);
                        client.Send(bytes);
                        Console.WriteLine($"[RECONNECTED] {mensaje}");
                        _notificationService.ShowInfo($"Reconectado a {_programaGrafico?.ToUpper()} correctamente", "Reconexión exitosa");
                    }
                }
                catch (Exception retryEx)
                {
                    _loggerService.LogError($"Reconnection to {_programaGrafico} failed", retryEx);
                    _notificationService.ShowError($"No se pudo reconectar a {_programaGrafico?.ToUpper()}", "Error de reconexión");
                }
            }
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

        /// <summary>
        /// Envía un mensaje puntual a IPF2 (conexión bajo demanda: abrir -> enviar -> cerrar)
        /// Usado para señales específicas como TickerFotosEntra desde sfFichas
        /// </summary>
        public static void EnviarMensajePuntualIPF2(string mensaje)
        {
            var config = ConfigManager.GetInstance();
            var loggerService = FileLoggerService.GetInstance();

            string ip = config.GetValue("ipIPF2");
            string puertoStr = config.GetValue("puertoIPF2");

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(puertoStr))
            {
                loggerService.LogError("IPF2 configuration missing (ipIPF2 or puertoIPF2)", null);
                return;
            }

            try
            {
                int port = int.Parse(puertoStr);
                using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Use a 3-second timeout for connection attempts
                var connectTask = client.ConnectAsync(ip, port);
                if (!connectTask.Wait(TimeSpan.FromSeconds(2)))
                {
                    loggerService.LogError($"Connection to IPF2 at {ip}:{port} timed out after 3 seconds", null);
                    return;
                }
                client.Send(Encoding.UTF8.GetBytes(mensaje));
                client.Close();
                Console.WriteLine($"[IPF2] {mensaje}");
            }
            catch (Exception ex)
            {
                loggerService.LogError($"Error sending message to IPF2 at {ip}:{puertoStr}", ex);
            }
        }

        /// <summary>
        /// Envía TickerFotosEntra a IPF2 (para superfaldón fichas)
        /// </summary>
        public static void EnviarTickerFotosIPF2()
        {
            var config = ConfigManager.GetInstance();
            string bd = config.GetValue("bdIPF2");
            string mensaje = $"itemset('<{bd}>TICKER/FOTOS/ENTRA','EVENT_RUN');";
            EnviarMensajePuntualIPF2(mensaje);
        }
    }
}

