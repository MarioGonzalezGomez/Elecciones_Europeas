using Elecciones_Europeas.src.model.IPF;
using Elecciones_Europeas.src.utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;

namespace Elecciones_Europeas.src.conexion
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
        private MySqlConnection connection;
        private MainWindow main;

        ConfigManager configuration;

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
            this.main = (MainWindow)Application.Current.MainWindow;
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
            this.main = (MainWindow)Application.Current.MainWindow;
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
            this.main = (MainWindow)Application.Current.MainWindow;
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
            string connectionServer;
            string connectionEnd = $";port={_port};uid={_user};pwd={_pass};database={_database}";

            try
            {
                connectionServer = $"server ={_server}";
                TestConnection(connectionServer + connectionEnd);
            }
            catch (Exception exMain)
            {
                MessageBox.Show($"Error al conectar con la base de datos en la ip {_server}. Buscando nueva conexión.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                try
                {
                    if (_tipoConexion != 1)
                    {
                        _server = configuration.GetValue("dataServer");
                        connectionServer = $"server ={_server}";
                        TestConnection(connectionServer + connectionEnd);
                        _tipoConexion = 1;
                        configuration.SetValue("conexionDefault1", "1");
                        main.EscribirConexiones();
                    }
                    else
                    {
                        _server = configuration.GetValue("dataServerBackup");
                        connectionServer = $"server ={_server}";
                        TestConnection(connectionServer + connectionEnd);
                        _tipoConexion = 2;
                        configuration.SetValue("conexionDefault1", "2");
                        main.EscribirConexiones();
                    }

                }
                catch (Exception exBackup)
                {
                    MessageBox.Show($"Error al conectar con la base de datos en la ip {_server}. Buscando nueva conexión.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                    try
                    {
                        if (_tipoConexion == 3)
                        {
                            _server = configuration.GetValue("dataServerBackup");
                            connectionServer = $"server ={_server}";
                            TestConnection(connectionServer + connectionEnd);
                            _tipoConexion = 2;
                            configuration.SetValue("conexionDefault1", "2");
                            main.EscribirConexiones();
                        }
                        else
                        {
                            _server = "127.0.0.1";
                            connectionServer = $"server ={_server}";
                            TestConnection(connectionServer + connectionEnd);
                            _tipoConexion = 3;
                            configuration.SetValue("conexionDefault1", "3");
                            main.EscribirConexiones();
                        }
                    }
                    catch (Exception exLocal)
                    {
                        // Maneja el error o lanza una excepción si no se puede establecer ninguna conexión
                        MessageBox.Show($"No se ha podido conectar a la BD de ninguna de las IP facilitadas. Revise que están en funcionamiento y que las IP están bien escritas en el fichero de configuración", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                        throw new ApplicationException("Error al conectar con la base de datos.", exLocal);
                    }
                }
            }

            return connectionServer + connectionEnd;
        }

        private void TestConnection(string connectionString)
        {
            using (connection = new MySqlConnection(connectionString))
            {
                connection.Open();
            }
        }

        public void CloseConection()
        {
            if (connection != null)
                connection.Close();
        }
    }
}
