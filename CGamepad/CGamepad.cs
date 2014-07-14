using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// gamepad
using Microsoft.Xna.Framework.Input;

namespace MAF_Robot
{
    public class CGamepad
    {
        public Microsoft.Xna.Framework.Input.GamePadState GamePadState { get; set; }

        public CGamepad()
        {
            GamePadState = new GamePadState();
        }

        public void Update(Microsoft.Xna.Framework.Input.GamePadState gamePadState)
        {
            GamePadState = gamePadState;
        }
    }
}
