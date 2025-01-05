using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplikacjaMedyczna
{
    public class Wpis
    {
       
            public int Id { get; set; }
            public string wpis { get; set; }
            public decimal peselPacjenta { get; set; }
            public int idPersonelu { get; set; }
            public DateTime dataWpisu { get; set; }
            public string danePersonelu { get; set; }
    }
}
