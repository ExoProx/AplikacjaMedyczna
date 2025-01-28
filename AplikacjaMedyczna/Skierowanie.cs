using System.ComponentModel;

namespace AplikacjaMedyczna
{
    public class Skierowanie : INotifyPropertyChanged
    {
        private int id;
        private string skierowanieText;
        private decimal peselPacjenta;
        private int idPersonelu;
        private string dataSkierowania;
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

        public string SkierowanieText
        {
            get => skierowanieText;
            set
            {
                if (skierowanieText != value)
                {
                    skierowanieText = value;
                    OnPropertyChanged(nameof(SkierowanieText));
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

        public string DataSkierowania
        {
            get => dataSkierowania;
            set
            {
                if (dataSkierowania != value)
                {
                    dataSkierowania = value;
                    OnPropertyChanged(nameof(DataSkierowania));
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

