using System;
using System.ComponentModel;

namespace Elecciones.src.logic
{
    public class ObservableInt : INotifyPropertyChanged
    {
        private int _valor;

        public int Valor
        {
            get { return _valor; }
            set
            {
                if (_valor != value)
                {
                    _valor = value;
                    OnPropertyChanged(nameof(Valor));
                    OnCambioDeElecciones();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CambioDeElecciones;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnCambioDeElecciones()
        {
            CambioDeElecciones?.Invoke(this, EventArgs.Empty);
        }
    }
}
