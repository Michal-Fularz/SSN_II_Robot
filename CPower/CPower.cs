using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class CPower
    {
        private enum PowerStatus : int
        {
            Critical = 0,
            Warning = 1,
            Normal = 2
        };

        private const double VoltageMaxValue = 15.0;
        private const double VoltageCriticalValue = 10.0;
        private const double VoltageWarningValue = 11.0;

        //public double VoltageMaxValuePresentation { get; private set; }
        private double Voltage = 0.0;

        public CPower()
        {
            //this.VoltageMaxValuePresentation = VoltageMaxValue;
            this.Voltage = 0.0;
        }

        public void Update(double voltage)
        {
            this.Voltage = voltage;
        }

        private PowerStatus Check()
        {
            PowerStatus status;

            if (Voltage <= VoltageCriticalValue)
            {
                status = PowerStatus.Critical;
            }
            else if (Voltage <= VoltageWarningValue)
            {
                status = PowerStatus.Warning;
            }
            else
            {
                status = PowerStatus.Normal;
            }

            return status;
        }

        public bool IsAtCriticalLevel()
        {
            bool isCritical = false;

            if (this.Check() == PowerStatus.Critical)
            {
                isCritical = true;
            }

            return isCritical;
        }
    }
}
