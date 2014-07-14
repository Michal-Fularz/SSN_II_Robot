using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class CButtons
    {
        public bool[] ButtonsState { get; set; }

        public CButtons()
        {
            ButtonsState = new bool[0];
        }

        public CButtons(int numberOfButtons)
        {
            ButtonsState = new bool[numberOfButtons];
        }

        public int GetNumberOfButtons()
        {
            return ButtonsState.Length;
        }

        public void Update(byte buttonsState)
        {
            for (int i = 0; i < ButtonsState.Length; ++i)
            {
                if ((buttonsState & (1 << i)) != 0)
                {
                    ButtonsState[i] = true;
                }
                else
                {
                    ButtonsState[i] = false;
                }
            }
        }

        public void Update(bool[] buttons)
        {
            for (int i = 0; i < ButtonsState.Length; ++i)
            {
                this.ButtonsState[i] = buttons[i];
            }
        }
    }
}
