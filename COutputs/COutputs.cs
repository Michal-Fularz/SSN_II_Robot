using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class COutputs
    {
        public CMotors Motors { get; set; }

        public CLeds Leds { get; set; }

        // possible alternative - each led is different object
        //public CLeds LedsChassis { get; set; }
        //public CLeds LedsFront { get; set; }
        //public CLeds LedsBottom { get; set; }
        //public CLeds LedsHead { get; set; }

        public CServo Servos { get; set; }

        public CSound Sound { get; set; }

        public COutputs()
        {
            Motors = new CMotors();
            Servos = new CServo();
            Sound = new CSound();
            Leds = new CLeds();
        }
    }

    public class CGamepadVibrations
    {
        public double VibrationRight { get; private set; }
        public double VibrationLeft { get; private set; }
    }
}
