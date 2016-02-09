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
        public Microsoft.Xna.Framework.Input.GamePadState GamepadState { get; set; }

        //public ButtonPressed OnBack = null;
        public ButtonPressed OnDpadUp = null;
        public ButtonPressed OnDpadDown = null;

        public CGamepad()
        {
            this.GamepadState = new GamePadState();
        }

        public bool IsConnected()
        {
            return this.GamepadState.IsConnected;
        }

        public void Update(Microsoft.Xna.Framework.Input.GamePadState gamepadState)
        {
            this.GamepadState = gamepadState;
        }

        public void Update()
        {
            this.GamepadState = Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);

            if (this.GamepadState.IsButtonDown(Buttons.Back))
            {
                //OnBack();
            }
            if (this.GamepadState.IsButtonDown(Buttons.DPadUp))
            {
                OnDpadUp();
            }
            else if(this.GamepadState.IsButtonDown(Buttons.DPadDown))
            {
                OnDpadDown();
            }
        }
    }

    public delegate void ButtonPressed();
}
