using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.mensajes.builders
{
    internal class PrimeMensajes
    {
        public static PrimeMensajes? instance;
        //Este bool nos ayuda a hacer play la primera vez, y resume las siguientes (por si se pausase un video y se continuase)
        bool primerPlay = true;


        private PrimeMensajes() { }

        public static PrimeMensajes GetInstance()
        {
            if (instance == null)
            {
                instance = new PrimeMensajes();
            }
            return instance;
        }



        //MÉTODOS GENÉRICOS DONDE PODER ESPECIFICAR POR PARÁMETRO

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


        //MÉTODOS GENÉRICOS PARA EL MANEJO DE VIDEO
        public string PlayVideo(string nombreElementoClip)
        {
            string play;
            //Utilizo un bool para hacer play de inicio o continuando en un pause dependiendo de si es la primera vez que se ejecuta en una escena o no.
            //Por tanto, para cada nueva escena que se cargue, debería pasarse esta variable a true
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


        //ACCIONES A NIVEL DE PROYECTO

        public string CambioDeProyecto(string rutaAbsProyecto)
        {
            rutaAbsProyecto.Replace("\\", "/");
            return $"P\\CHANGE_PROJECT\\{rutaAbsProyecto}\\\\\r\n";
        }

        public string CambioParametroProyecto(string parametroName, string parametroValue)
        {
            return $"P\\PROJECT_PARAMETER\\{parametroName}\\{parametroValue}\\\\\r\n";
        }


        //CONSTRUCTORES BASE PARA LAS DIFERENTES SEÑALES

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
