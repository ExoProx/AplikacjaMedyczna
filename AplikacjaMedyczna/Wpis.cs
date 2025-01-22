using System;
using System.ComponentModel;

namespace AplikacjaMedyczna
{
    public class Wpis : INotifyPropertyChanged
    {
        private int id;
        private string wpisText;
        private decimal peselPacjenta;
        private int idPersonelu;
        private DateTime dataWpisu;
        private string danePersonelu;

        public int Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string WpisText
        {
            get => wpisText;
            set
            {
                if (wpisText != value)
                {
                    wpisText = value;
                    OnPropertyChanged(nameof(WpisText));
                }
            }
        }

        public decimal PeselPacjenta
        {
            get => peselPacjenta;
            set
            {
                if (peselPacjenta != value)
                {
                    peselPacjenta = value;
                    OnPropertyChanged(nameof(PeselPacjenta));
                }
            }
        }

        public int IdPersonelu
        {
            get => idPersonelu;
            set
            {
                if (idPersonelu != value)
                {
                    idPersonelu = value;
                    OnPropertyChanged(nameof(IdPersonelu));
                }
            }
        }

        public DateTime DataWpisu
        {
            get => dataWpisu;
            set
            {
                if (dataWpisu != value)
                {
                    dataWpisu = value;
                    OnPropertyChanged(nameof(DataWpisu));
                }
            }
        }

        public string DanePersonelu
        {
            get => danePersonelu;
            set
            {
                if (danePersonelu != value)
                {
                    danePersonelu = value;
                    OnPropertyChanged(nameof(DanePersonelu));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
