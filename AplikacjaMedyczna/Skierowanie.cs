using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplikacjaMedyczna
{
    public class Skierowanie
    {
        public int Id { get; set; }
        public string skierowanie { get; set; }
        public decimal peselPacjenta { get; set; }
        public int idPersonelu { get; set; }
        public DateTime dataSkierowania { get; set; }
    }
}
