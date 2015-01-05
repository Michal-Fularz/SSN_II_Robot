using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    // klasa do przechowywania sterowanego pojedynczo zbioru ledów
    public class CLeds
    {
        private int Red;
        private int Green;
        private int Blue;

        public CLeds()
        {
            this.Red = 0;
            this.Green = 0;
            this.Blue = 0;
        }
    }
}
