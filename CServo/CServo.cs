﻿using System;
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
            Right1 = 0,
            Right2 = 1,
            Right3 = 2,
            Right4 = 3,
            Left1 = 4,
            Left2 = 5,
            Left3 = 6,
            Left4 = 7,
            Head = 8,
        };

        private const int numberOfServos = 9;
        public int[] servosPosition { get; private set; }
        public bool[] servosChangedPosition { get; private set; }

        public CServo()
        {
            servosPosition = new int[numberOfServos];
            servosChangedPosition = new bool[numberOfServos];

            for (int i = 0; i < numberOfServos; ++i)
            {
                servosPosition[i] = 50;
                servosChangedPosition[i] = true;
                this.SetServoPosition((ServoType)(i), 0);
            }
            // head is a special case - neutral position is 50
            this.SetServoPosition(ServoType.Head, 50);
        }

        public void ChangeServoPosition(ServoType servo, int change)
        {
            int newServoPosition = (this.servosPosition[((int)servo)] + change);

            if ((newServoPosition >= 0) && (newServoPosition <= 100))
            {
                this.servosPosition[((int)servo)] = newServoPosition;
                this.servosChangedPosition[((int)servo)] = true;
            }
        }

        public void SetServoPosition(ServoType servo, int newServoPosition)
        {
            if ((newServoPosition >= 0) && (newServoPosition <= 100))
            {
                this.servosPosition[((int)servo)] = newServoPosition;
                this.servosChangedPosition[((int)servo)] = true;
            }
        }
    }
}
