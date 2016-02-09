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
        public enum LedType
        {
            Bottom = 0,
            Chasis = 1,
            Eyes = 2,
        };

        public struct SColor
        {
            public int R;
            public int G;
            public int B;

            public SColor(int r, int g, int b)
            {
                this.R = r;
                this.G = g;
                this.B = b;
            }
        };

        private const int numberOfLedsTypes = 3;
        public SColor[] ledState { get; private set; }
        public bool[] ledChangedState { get; private set; }

        public CLeds()
        {
            ledState = new SColor[numberOfLedsTypes];
            ledChangedState = new bool[numberOfLedsTypes];

            // inicjalizacja stanu diod ustawiajac je na 0
            for (int i = 0; i < numberOfLedsTypes; i++)
            {
                ledState[i].R = 0;
                ledState[i].G = 0;
                ledState[i].B = 0;

                ledChangedState[i] = true;
            }
        }

        public void SetLedColors(LedType type, SColor rgb)
        {
            this.ledState[(int)type].R = rgb.R;
            this.ledState[(int)type].G = rgb.G;
            this.ledState[(int)type].B = rgb.B;
            this.ledChangedState[(int)type] = true;
        }

        public void TurnOff()
        {
            for (int i = 0; i < numberOfLedsTypes; i++)
            {
                SColor zero = new SColor(0, 0, 0);

                SetLedColors((LedType)i, zero);
            }
        }
    }
}
