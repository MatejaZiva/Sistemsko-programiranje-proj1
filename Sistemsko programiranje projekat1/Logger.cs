using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemsko_programiranje_projekat1
{
    internal class Logger
    {
        private static readonly object logLock = new object();
        public static void Log(string message)
        {
            lock (logLock)
            {
                Console.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}
