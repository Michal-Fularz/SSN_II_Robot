using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class CServo
    {
        public enum ServoType
        {
            Right1 = 1,
            Right2 = 2,
            Right3 = 3,
            Right4 = 4,
            Left1 = 5,
            Left2 = 6,
            Left3 = 7,
            Left4 = 8,
            Head = 9,
        };

        private const int numberOfServos = 9;
        public int[] servosPosition;
        public bool[] servosChangePosition;

        public CServo()
        {
            servosPosition = new int[numberOfServos];
            servosChangePosition = new bool[numberOfServos];

            for (int i = 0; i < numberOfServos; ++i)
            {
                servosPosition[i] = 50;
                servosChangePosition[i] = true;
                this.SetServoPosition((ServoType)(i + 1), 0);
            }
            // head is special case - neutral position is 50
            this.SetServoPosition(ServoType.Head, 50);
        }

        public void ChangeServoPosition(ServoType servo, int change)
        {
            int newServoPosition = (this.servosPosition[((int)servo) - 1] + change);

            if ((newServoPosition >= 0) && (newServoPosition <= 100))
            {
                this.servosPosition[((int)servo) - 1] = newServoPosition;
                this.servosChangePosition[((int)servo) - 1] = true;
            }
        }

        public void SetServoPosition(ServoType servo, int newServoPosition)
        {
            if ((newServoPosition >= 0) && (newServoPosition <= 100))
            {
                this.servosPosition[((int)servo) - 1] = newServoPosition;
                this.servosChangePosition[((int)servo) - 1] = true;
            }
        }
    }
}
