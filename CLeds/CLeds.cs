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

        public struct Color
        {
            public int R {get; set;}
            public int G {get; set;}
            public int B {get; set;}
        };

        private const int numberOfLedsTypes = 3;
        public Color[] ledState { get; private set; }
        public bool[] ledChangedState { get; private set; }

        public CLeds()
        {
            ledState = new Color[numberOfLedsTypes];
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

        //zmien stan diod
        public void ChangeLedState(LedType type, Color rgb)
        {
            this.ledState[((int)type)] = rgb;
            this.ledChangedState[((int)type)] = true;
        }

        // ta funkcja miała chyba ustawiać kolor
        public void SetLedColors(LedType type, Color rgb)
        {
            this.ledState[(int)type].R = rgb.R;
            this.ledState[(int)type].G = rgb.G;
            this.ledState[(int)type].B = rgb.B;
            this.ledChangedState[(int)type] = true;
        }
    }
}
