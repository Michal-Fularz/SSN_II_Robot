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

        public ButtonPressed OnRightShoulder = null;
        public ButtonPressed OnLeftShoulder = null;
        public ButtonPressed OnBack = null;

        public ButtonPressed OnA = null;
        public ButtonPressed OnB = null;
        public ButtonPressed OnX = null;
        public ButtonPressed OnY = null;

        public CGamepad()
        {
            this.GamepadState = new GamePadState();
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
                OnBack();
            }

            if (this.GamepadState.IsButtonDown(Buttons.RightShoulder))
            {
                OnRightShoulder();
            }
            if (this.GamepadState.IsButtonDown(Buttons.LeftShoulder))
            {
                OnLeftShoulder();
            }

            if(this.GamepadState.IsButtonDown(Buttons.A))
            {
                OnA();
            }
            else if (this.GamepadState.IsButtonDown(Buttons.B))
            {
                OnB();
            }
            else if (this.GamepadState.IsButtonDown(Buttons.X))
            {
                OnX();
            }
            else if (this.GamepadState.IsButtonDown(Buttons.Y))
            {
                OnY();
            }
        }
    }

    public delegate void ButtonPressed();
}
