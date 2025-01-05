using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplikacjaMedyczna
{
    public class Recepta
    {
        public int Id { get; set; }
        public string leki { get; set; }
        public decimal peselPacjenta { get; set; }
        public int idPersonelu { get; set; }
        public DateTime dataWystawieniaRecepty { get; set; }

        public DateTime dataWaznosciRecepty { get; set; }
        public string danePersonelu { get; set; }

        public bool czyOdebrana {  get; set; }


    }
}
