using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplikacjaMedyczna
{
    public static class SharedData
    {
        public static string pesel { get; set; }  // patient pesel
        public static string id { get; set; }  // worker id
        public static string rola { get; set; }  // worker role
    }
}
