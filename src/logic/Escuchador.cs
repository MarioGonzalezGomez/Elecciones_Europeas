using Elecciones.src.controller;
using Elecciones.src.model.IPF;
using Elecciones.src.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Elecciones.src.logic
{
    public class Escuchador
    {
        private readonly object _sync = new();
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);
        private readonly FileLoggerService _logger = FileLoggerService.GetInstance();

        private List<Circunscripcion> _circunscripciones = new();
        private CircunscripcionController? _cirController;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;
        private bool _actualizacionActiva = true;

        public Escuchador(conexion.ConexionEntityFramework conexionActiva)
        {
            ActualizarConexion(conexionActiva);
        }

        public bool ActualizacionActiva
        {
            get
            {
                lock (_sync)
                {
                    return _actualizacionActiva;
                }
            }
            set
            {
                lock (_sync)
                {
                    _actualizacionActiva = value;
                }
            }
        }

        public void Iniciar()
        {
            lock (_sync)
            {
                if (_listenerTask != null && !_listenerTask.IsCompleted)
                {
                    return;
                }

                _cts = new CancellationTokenSource();
                _listenerTask = Task.Run(() => EjecutarBucleAsync(_cts.Token));
            }
        }

        public async Task DetenerAsync()
        {
            Task? listenerTask;
            CancellationTokenSource? cts;

            lock (_sync)
            {
                listenerTask = _listenerTask;
                cts = _cts;
                _listenerTask = null;
                _cts = null;
            }

            if (cts == null)
            {
                return;
            }

            cts.Cancel();

            if (listenerTask != null)
            {
                try
                {
                    await listenerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Parada controlada.
                }
            }

            cts.Dispose();
        }

        public void Detener()
        {
            Task? listenerTask;
            CancellationTokenSource? cts;

            lock (_sync)
            {
                listenerTask = _listenerTask;
                cts = _cts;
                _listenerTask = null;
                _cts = null;
            }

            if (cts == null)
            {
                return;
            }

            cts.Cancel();
            cts.Dispose();

            _ = listenerTask;
        }

        private async Task EjecutarBucleAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                CircunscripcionController? controller;
                List<double> escrutadosActuales;
                bool actualizacionActiva;

                lock (_sync)
                {
                    controller = _cirController;
                    escrutadosActuales = _circunscripciones.Select(cir => cir.escrutado).ToList();
                    actualizacionActiva = _actualizacionActiva;
                }

                if (!actualizacionActiva || controller == null)
                {
                    await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                try
                {
                    List<Circunscripcion> circunscripcionesNew = controller.FindAllFromBD();
                    List<double> escrutadosNew = circunscripcionesNew.Select(cir => cir.escrutado).ToList();

                    if (!escrutadosActuales.SequenceEqual(escrutadosNew))
                    {
                        lock (_sync)
                        {
                            _circunscripciones = circunscripcionesNew;
                        }

                        await NotificarActualizacionEnUIAsync().ConfigureAwait(false);
                    }

                    await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError("El escuchador ha sufrido un error. Se reintentara automaticamente.", ex);
                    try
                    {
                        await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private static async Task NotificarActualizacionEnUIAsync()
        {
            if (Application.Current?.MainWindow is not MainWindow mainWindow)
            {
                return;
            }

            if (mainWindow.Dispatcher.CheckAccess())
            {
                mainWindow.Update();
                return;
            }

            await mainWindow.Dispatcher.InvokeAsync(mainWindow.Update);
        }

        /// <summary>
        /// Actualiza la conexion del escuchador para usar la nueva base de datos.
        /// Debe llamarse cuando cambia la configuracion de conexion.
        /// </summary>
        /// <param name="nuevaConexion">La nueva conexion a la base de datos</param>
        public void ActualizarConexion(conexion.ConexionEntityFramework nuevaConexion)
        {
            lock (_sync)
            {
                _cirController = CircunscripcionController.GetInstance(nuevaConexion);
                _circunscripciones = _cirController.FindAll();
            }
        }
    }
}
