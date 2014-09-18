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

        public double Voltage { get; private set; }
        public CPower.PowerStatus Status { get; private set; }

        public CPower()
        {
            this.Voltage = 0.0;
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

        public double GetCurrentVoltageToMaxRatio()
        {
            double ratio = this.Voltage / VoltageMaxValue;

            return ratio;
        }

        public System.Windows.Media.Brush GetColorBasedOnStatus()
        {
            System.Windows.Media.Brush brush;

            if (this.Status == CPower.PowerStatus.Critical)
            {
                brush = System.Windows.Media.Brushes.Red;
            }
            else if (this.Status == CPower.PowerStatus.Warning)
            {
                brush = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                brush = System.Windows.Media.Brushes.Green;
            }

            return brush;
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
