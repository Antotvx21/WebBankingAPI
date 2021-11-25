using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebBankingAPI.Model
{
    public class Bonifico
    {
        public string Iban { get; set; }
        public float Importo { get; set; }

    }
}
