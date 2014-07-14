using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class CPower
    {
        public enum PowerStatus : int
        {
            Critical = 0,
            Warning = 1,
            Normal = 2
        };

        private const double VoltageMaxValue = 15.0;
        private const double VoltageCriticalValue = 10.0;
        private const double VoltageWarningValue = 11.0;

        public double VoltageMaxValuePresentation { get; private set; }
        public double Voltage { get; private set; }
        public CPower.PowerStatus Status { get; private set; }

        public CPower()
        {
            this.VoltageMaxValuePresentation = VoltageMaxValue;
            this.Voltage = 0.0;
            this.Status = CPower.PowerStatus.Critical;
        }

        public void Update(double voltage)
        {
            Voltage = voltage;
            Anaylze();
        }

        private void Anaylze()
        {
            if (Voltage <= VoltageCriticalValue)
            {
                Status = PowerStatus.Critical;
            }
            else if (Voltage <= VoltageWarningValue)
            {
                Status = PowerStatus.Warning;
            }
            else
            {
                Status = PowerStatus.Normal;
            }
        }
    }
}
