using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class CInputs
    {
        public CButtons Buttons { get; set; }
        public CGamepad Gamepad { get; set; }
        public CPower Power { get; set; }

        public CInputs()
        {
            Buttons = new CButtons();
            Gamepad = new CGamepad();
            Power = new CPower();
        }

        public CInputs(int numberOfButtons)
        {
            Buttons = new CButtons(numberOfButtons);
            Gamepad = new CGamepad();
            Power = new CPower();
        }

        public void Read()
        {
            throw new NotImplementedException();
        }
    }  
}
