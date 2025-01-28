using System.ComponentModel;

namespace AplikacjaMedyczna
{
    public class Recepta : INotifyPropertyChanged
    {
        private int id;
        private string leki;
        private decimal peselPacjenta;
        private int idPersonelu;
        private string dataWystawieniaRecepty;
        private string dataWaznosciRecepty;
        private string danePersonelu;
        private bool czyOdebrana;

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

        public string Leki
        {
            get => leki;
            set
            {
                if (leki != value)
                {
                    leki = value;
                    OnPropertyChanged(nameof(Leki));
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

        public string DataWystawieniaRecepty
        {
            get => dataWystawieniaRecepty;
            set
            {
                if (dataWystawieniaRecepty != value)
                {
                    dataWystawieniaRecepty = value;
                    OnPropertyChanged(nameof(DataWystawieniaRecepty));
                }
            }
        }

        public string DataWaznosciRecepty
        {
            get => dataWaznosciRecepty;
            set
            {
                if (dataWaznosciRecepty != value)
                {
                    dataWaznosciRecepty = value;
                    OnPropertyChanged(nameof(DataWaznosciRecepty));
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

        public bool CzyOdebrana
        {
            get => czyOdebrana;
            set
            {
                if (czyOdebrana != value)
                {
                    czyOdebrana = value;
                    OnPropertyChanged(nameof(CzyOdebrana));
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
