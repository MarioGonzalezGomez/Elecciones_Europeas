using System;
using Elecciones.src.utils;

namespace Elecciones.src.mensajes.builders
{
    internal class PrimeMensajes
    {
        public static PrimeMensajes? instance;
        // This bool helps us play first time and resume on subsequent calls.
        bool primerPlay = true;
        private readonly ConfigManager configuration;

        private PrimeMensajes()
        {
            configuration = ConfigManager.GetInstance();
            configuration.ReadConfig();
        }

        public static PrimeMensajes GetInstance()
        {
            if (instance == null)
            {
                instance = new PrimeMensajes();
            }
            return instance;
        }

        // SPECIFIC MESSAGES

        public string SubirRotulosEspeciales()
        {
            string signal = "";
            signal += CambioDeProyecto(GetProyectoPrime());
            signal += CambioParametroProyecto("Par_Posicion_GrupoXY", "2");
            return signal;
        }

        public string BajarRotulosEspeciales()
        {
            string signal = "";
            signal += CambioDeProyecto(GetProyectoPrime());
            signal += CambioParametroProyecto("Par_Posicion_GrupoXY", "1");
            return signal;
        }

        public string SubirRotulosTD()
        {
            string signal = "";
            signal += CambioDeProyecto("01_TD_2026");
            signal += CambioParametroProyecto("TickerElec", "True");
            return signal;
        }

        public string BajarRotulosTD()
        {
            string signal = "";
            signal += CambioDeProyecto("01_TD_2026");
            signal += CambioParametroProyecto("TickerElec", "False");
            return signal;
        }

        private string GetProyectoPrime()
        {
            configuration.ReadConfig();
            string proyectoPrime = configuration.GetValue("proyectoPrime");
            return string.IsNullOrWhiteSpace(proyectoPrime) ? "Especiales_ConPicto_2026" : proyectoPrime;
        }

        // GENERIC METHODS TO PARAMETERIZE SIGNALS

        public string Entra(string escena)
        {
            primerPlay = true;
            return ConstructorBase("PLAY", escena);
        }

        public string Preview(string escena)
        {
            return ConstructorBase("LOAD", escena);
        }

        public string EntraDesdePreview()
        {
            primerPlay = true;
            return $"P\\PLAY_ALL\\*\\\\\r\n";
        }

        public string Sale(string escena)
        {
            return ConstructorBase("CLEAR\\1", escena);
        }

        public string Reset()
        {
            return ConstructorBase("CLEAR_ALL\\*");
        }

        public string Accion(string escena, string accionName)
        {
            return $"P\\PLAY_ACTION\\1\\{escena}\\{accionName}\\\\\r\n";
        }

        public string CambioParametro(string escena, string parametroName, string parametroValue)
        {
            return $"P\\SCENE_PARAMETER\\*\\{escena}\\{parametroName}\\{parametroValue}\\\\\r\n";
        }

        public string CambioTexto(string escena, string objetoTextoName, string value)
        {
            return $"P\\UPDATE\\*\\{escena}\\{objetoTextoName}\\{value}\\\\\r\n";
        }

        public string UpdateData(string nombreData = "Data1")
        {
            return $"P\\COMMAND:Program\\1\\*\\{nombreData}.Update\\\\\r\n";
        }

        // GENERIC VIDEO METHODS
        public string PlayVideo(string nombreElementoClip)
        {
            string play;
            if (primerPlay)
            {
                play = $"P\\COMMAND:Program\\1\\*\\{nombreElementoClip}.Play\\\\\r\n";
                primerPlay = false;
            }
            else
            {
                play = $"P\\COMMAND:Program\\1\\*\\{nombreElementoClip}.Resume\\\\\r\n";
            }
            return play;
        }

        public string PauseVideo(string nombreElementoClip)
        {
            return $"P\\COMMAND:Program\\1\\*\\{nombreElementoClip}.Pause\\\\\r\n";
        }

        public string ReiniciarVideo(string nombreElementoClip)
        {
            return $"P\\COMMAND:Program\\1\\*\\{nombreElementoClip}.Cue\\\\\r\n";
        }

        // PROJECT-LEVEL ACTIONS
        public string CambioDeProyecto(string rutaAbsProyecto)
        {
            rutaAbsProyecto.Replace("\\", "/");
            return $"P\\CHANGE_PROJECT\\{rutaAbsProyecto}\\\\\r\n";
        }

        public string CambioParametroProyecto(string parametroName, string parametroValue)
        {
            return $"P\\PROJECT_PARAMETER\\{parametroName}\\{parametroValue}\\\\\r\n";
        }

        // BASE BUILDERS
        private string ConstructorBase(string accion, string escena)
        {
            return $"P\\{accion}\\{escena}\\\\\r\n";
        }

        private string ConstructorBase(string accion)
        {
            return $"P\\{accion}\\\\\r\n";
        }
    }
}