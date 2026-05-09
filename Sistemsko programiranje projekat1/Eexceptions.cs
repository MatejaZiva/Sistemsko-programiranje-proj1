using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_programiranje_projekat1
{
    internal class Eexceptions : Exception
    {
        public int errorCode {get;set;}
        public Eexceptions(string message,int errorCode):base(message) 
        {
            this.errorCode = errorCode;
        }
    }
}
