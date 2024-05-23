using Elecciones_Europeas.src.controller;
using Elecciones_Europeas.src.model.IPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Elecciones_Europeas.src.logic
{
    public class Escuchador
    {
        private static List<Circunscripcion> circunscripciones;
        private CircunscripcionController cirController;
        public bool salir { get; set; }

        public Escuchador(conexion.ConexionEntityFramework conexionActiva)
        {
            cirController = CircunscripcionController.GetInstance(conexionActiva);
            circunscripciones = cirController.FindAll();
            salir = false;
            EjecutarCadaDosSegundos();
        }

        public async Task EjecutarCadaDosSegundos()
        {
            while (!salir)
            {
                try
                {
                    List<double> escrutados = circunscripciones.Select(cir => cir.escrutado).ToList();
                    List<Circunscripcion> circunscripcionesNew = cirController.FindAllFromBD();
                    List<double> escrutadosNew = circunscripcionesNew.Select(cir => cir.escrutado).ToList();

                    if (!escrutados.SequenceEqual(escrutadosNew))
                    {
                        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                        mainWindow.Update();
                        circunscripciones = circunscripcionesNew;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(2));

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"El escuchador ha sufrido un error, es posible que no se detecten los cambios en vivo \n{ex}", "Error BDD", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }

            }
        }
    }
}
