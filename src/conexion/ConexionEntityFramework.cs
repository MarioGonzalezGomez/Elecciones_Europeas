using Elecciones.src.model.IPF;
using Elecciones.src.utils;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using Elecciones.src.service;

namespace Elecciones.src.conexion
{
    public class ConexionEntityFramework : DbContext
    {
        public int db;
        private string? _server;
        private string? _port;
        public string? _database { get; set; }
        private string? _user;
        private string? _pass;
        public int _tipoConexion { get; set; }
        private MySqlConnection? connection;
        private MainWindow? main;

        ConfigManager configuration;
        private readonly FileLoggerService _logger = FileLoggerService.GetInstance();

        public ConexionEntityFramework(DbContextOptions options) : base(options)
        {
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            _tipoConexion = 1;
            _server = configuration.GetValue("dataServer");
            _port = configuration.GetValue("dataPort");
            _database = configuration.GetValue("dataDB1");
            _user = configuration.GetValue("user");
            _pass = configuration.GetValue("password");
            this.main = Application.Current.MainWindow as MainWindow;
        }
        //Constructor por defecto a Principal y DB1
        public ConexionEntityFramework()
        {
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            _tipoConexion = 1;
            _server = configuration.GetValue("dataServer");
            _port = configuration.GetValue("dataPort");
            _database = configuration.GetValue("dataDB1");
            _user = configuration.GetValue("user");
            _pass = configuration.GetValue("password");
            this.main = Application.Current.MainWindow as MainWindow;
        }
        //1:Principal 2:Reserva 3:Local
        public ConexionEntityFramework(int tipoConexion, MainWindow mainWindow)
        {
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            _tipoConexion = tipoConexion;
            _server = tipoConexion switch
            {
                1 => configuration.GetValue("dataServer"),
                2 => configuration.GetValue("dataServerBackup"),
                _ => "127.0.0.1"
            };

            _port = configuration.GetValue("dataPort");
            _database = configuration.GetValue("dataDB1");
            _user = configuration.GetValue("user");
            _pass = configuration.GetValue("password");

            main = mainWindow;
        }
        //Para elegir la DB a la que conectarnos en caso de conexión múltiple
        public ConexionEntityFramework(int tipoConexion, int db)
        {
            this.db = db;
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
            _tipoConexion = tipoConexion;
            _server = tipoConexion switch
            {
                1 => configuration.GetValue("dataServer"),
                2 => configuration.GetValue("dataServerBackup"),
                _ => "127.0.0.1"
            };

            _port = configuration.GetValue("dataPort");
            _database = db switch
            {
                1 => configuration.GetValue("dataDB1"),
                2 => configuration.GetValue("dataDB2"),
                3 => configuration.GetValue("dataDB3"),
                _ => configuration.GetValue("dataDB1")
            };
            _user = configuration.GetValue("user");
            _pass = configuration.GetValue("password");
            this.main = Application.Current.MainWindow as MainWindow;
        }

        public virtual DbSet<Partido> Partidos { get; set; }
        public virtual DbSet<Circunscripcion> Circunscripciones { get; set; }
        public virtual DbSet<CircunscripcionPartido> Cps { get; set; }
        public virtual DbSet<Literal> Literales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL(GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Partido>(entity =>
            {
                entity.ToTable("partidos");

                entity.HasKey(p => p.codigo);
                entity.Property(p => p.codigo).HasColumnName("PARTIDO");
                entity.Property("codigoPadre").HasColumnName("padre");
                entity.Property("siglas").HasColumnName("sigla");
                entity.Property("nombre").HasColumnName("descripcion");
                entity.Property("independentismo").HasColumnName("tendencia");

            });
            modelBuilder.Entity<Circunscripcion>(entity =>
            {
                entity.ToTable("circunscripciones");

                entity.HasKey(c => c.codigo);
                entity.Property(c => c.codigo).HasColumnName("CIRCUNSCRIPCION");
                entity.Property("comunidad").HasColumnName("COMUNIDAD");
                entity.Property("provincia").HasColumnName("PROVINCIA");
                entity.Property("municipio").HasColumnName("MUNICIPIO");
                entity.Property("nombre").HasColumnName("descripcion");
                entity.Property("escrutado").HasColumnName("escrutado");
                entity.Property("escanios").HasColumnName("escanos");
                entity.Property("avance1").HasColumnName("avance1");
                entity.Property("avance2").HasColumnName("avance2");
                entity.Property("avance3").HasColumnName("avance3");
                entity.Property("participacionFinal").HasColumnName("participacion");
                entity.Property("votantes").HasColumnName("votantes");
                entity.Property("escaniosHistoricos").HasColumnName("escanos_hist");
                entity.Property("avance1Hist").HasColumnName("avance1_hist");
                entity.Property("avance2Hist").HasColumnName("avance2_hist");
                entity.Property("avance3Hist").HasColumnName("avance3_hist");
                entity.Property("participacionHist").HasColumnName("participacion_hist");
            });
            modelBuilder.Entity<CircunscripcionPartido>(entity =>
            {
                entity.ToTable("circunscripcion_partido");

                entity.HasKey(cp => new { cp.codCircunscripcion, cp.codPartido });
                entity.Property(cp => cp.codCircunscripcion).HasColumnName("COD_CIRCUNSCRIPCION");
                entity.Property(cp => cp.codPartido).HasColumnName("COD_PARTIDO");
                entity.Property("escaniosDesde").HasColumnName("escanos_desde");
                entity.Property("escaniosHasta").HasColumnName("escanos_hasta");
                entity.Property("porcentajeVoto").HasColumnName("votos");
                entity.Property("numVotantes").HasColumnName("votantes");
                entity.Property("escaniosDesdeHist").HasColumnName("escanos_desde_hist");
                entity.Property("escaniosHastaHist").HasColumnName("escanos_hasta_hist");
                entity.Property("porcentajeVotoHist").HasColumnName("votos_hist");
                entity.Property("numVotantesHist").HasColumnName("votantes_hist");
                entity.Property("escaniosDesdeSondeo").HasColumnName("escanos_desde_sondeo");
                entity.Property("escaniosHastaSondeo").HasColumnName("escanos_hasta_sondeo");
                entity.Property("porcentajeVotoSondeo").HasColumnName("votos_sondeo");
                entity.Property("esUltimoEscano").HasColumnName("ult_escano");
                entity.Property("luchaUltimoEscano").HasColumnName("sig_escano");
                entity.Property("restoVotos").HasColumnName("restos");
            });
            modelBuilder.Entity<Literal>(entity =>
            {
                entity.ToTable("idiomas");

                entity.HasKey(p => p.codigo);
                entity.Property(p => p.codigo).HasColumnName("ID");
                entity.Property("castellano").HasColumnName("castellano");
                entity.Property("catalan").HasColumnName("catalan");
                entity.Property("vasco").HasColumnName("vasco");
                entity.Property("gallego").HasColumnName("gallego");
                entity.Property("valenciano").HasColumnName("valenciano");
                entity.Property("mallorquin").HasColumnName("mallorquin");

            });
        }

        private string GetConnectionString()
        {
            string connectionServer = string.Empty;
            string connectionEnd = $";port={_port};uid={_user};pwd={_pass};database={_database}";

            // Helper to test TCP connectivity fast (short timeout) to avoid UI blocking
            bool TestTcpConnection(string host, int port, int timeoutMs = 1000)
            {
                try
                {
                    using var tcp = new TcpClient();
                    var task = tcp.ConnectAsync(host, port);
                    if (!task.Wait(timeoutMs))
                    {
                        return false;
                    }
                    return tcp.Connected;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"TCP test failed for {host}:{port}", ex);
                    return false;
                }
            }

            int portNumber = 3306;
            if (!int.TryParse(_port, out portNumber))
            {
                // If port parsing fails, fallback to 3306
                portNumber = 3306;
            }

            // Order of candidates: current _server (chosen tipo), then other configured servers, then localhost
            var candidates = new System.Collections.Generic.List<(string server, int tipo)>();

            // add current preference first
            if (!string.IsNullOrWhiteSpace(_server))
                candidates.Add((_server, _tipoConexion));

            // add primary and backup explicitly if different
            var primary = configuration.GetValue("dataServer");
            var backup = configuration.GetValue("dataServerBackup");

            if (!string.IsNullOrWhiteSpace(primary) && !string.Equals(primary, _server, StringComparison.OrdinalIgnoreCase))
                candidates.Add((primary, 1));
            if (!string.IsNullOrWhiteSpace(backup) && !string.Equals(backup, _server, StringComparison.OrdinalIgnoreCase))
                candidates.Add((backup, 2));

            // finally localhost
            candidates.Add(("127.0.0.1", 3));

            (string server, int tipo)? selected = null;

            foreach (var cand in candidates)
            {
                if (string.IsNullOrWhiteSpace(cand.server))
                    continue;

                if (TestTcpConnection(cand.server, portNumber, 1000))
                {
                    selected = cand;
                    break;
                }
            }

            if (selected == null)
            {
                // No server reachable
                var message = "No se ha podido conectar a la BD en ninguna de las IPs configuradas. Revise la configuración y el estado de las bases de datos.";
                _logger.LogError(message, null);

                // Show a single UI message if possible (fast, via dispatcher)
                try
                {
                    main?.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(message, "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                catch
                {
                    // ignore dispatcher failures
                }

                throw new ApplicationException("Error al conectar con la base de datos. Ninguna IP responde.");
            }

            // use selected
            _server = selected.Value.server;
            _tipoConexion = selected.Value.tipo;
            connectionServer = $"server={_server}";

            // persist chosen default for UI connections if available
            try
            {
                configuration.SetValue("conexionDefault1", _tipoConexion.ToString());
                main?.EscribirConexiones();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed persisting chosen DB connection type", ex);
            }

            return connectionServer + connectionEnd;
        }

        public void CloseConection()
        {
            if (connection != null)
            {
                try
                {
                    connection.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error closing MySql connection", ex);
                }
                finally
                {
                    connection = null;
                }
            }
        }
    }
}
