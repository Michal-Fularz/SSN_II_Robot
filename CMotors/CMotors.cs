using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class CMotors
    {
        public int SpeedRightWheel { get; private set; }
        public int SpeedLeftWheel { get; private set; }

        public byte SpeedRightWheelDriverLevel { get; private set; }
        public byte SpeedLeftWheelDriverLevel { get; private set; }

        public CMotors()
        {
            this.SpeedRightWheel = 0;
            this.SpeedLeftWheel = 0;

            ConvertToDriverLevels();
        }

        // konwersja to wartości dla sterownika
        // może jako jeden z argumentów typ sterownika i w zależności od tego będzie używane inne przeliczenie
        // jak przesyłać wartości return vs out
        public void ConvertToDriverLevels()
        {
            // 0 - 127 - pierwsze wyjście
            // 0 - maksymalna prędkość wstecz, 127 - maksymalna prędkość do przodu, 64 - stop
            double tempSpeedRight = Math.Round((this.SpeedRightWheel + 100.0) * 127.0 / 200.0);

            // 0 - 127 - drugie wyjście
            // 0 - maksymalna prędkość wstecz, 127 - maksymalna prędkość do przodu, 64 - stop
            double tempSpeedLeft = Math.Round((this.SpeedLeftWheel + 100.0) * 127.0 / 200.0);

            if (tempSpeedLeft > 127 || tempSpeedLeft < 0)
            {
                AminExceptions.CAminExceptions.ThrowException(null, "Prędkość silnika lewego poza zakresem.");
            }
            if (tempSpeedRight > 127 || tempSpeedRight < 0)
            {
                AminExceptions.CAminExceptions.ThrowException(null, "Prędkość silnika prawego poza zakresem.");
            }

            this.SpeedRightWheelDriverLevel = Convert.ToByte(tempSpeedRight);
            this.SpeedLeftWheelDriverLevel = Convert.ToByte(tempSpeedLeft);
        }

        public void ConvertFromKeyboardInput(bool upKeyPressed, bool downKeyPressed, bool rightKeyPressed, bool leftKeyPressed)
        {
            throw new NotImplementedException();
        }

        public void ConvertFromOneStickInput(float stickValueX, float stickValueY)
        {
            double angle = Math.Atan2(stickValueY, stickValueX);
            // zamiana z radianów na stopnie
            angle = angle * 180 / Math.PI;
            double power = Math.Sqrt(Math.Pow(stickValueX, 2) + Math.Pow(stickValueY, 2));

            double motorSpeedRight = 0;
            double motorSpeedLeft = 0;

            // poprawienie najmniejszych wartości power
            /*
            // niepotrzebne, bo PID w sterowniku silników 
            if (power < 0.1)
                power = 0;
            else if (power < 0.3)
            {
                power = 0.3F;
            }
            */

            if (angle >= -170 && angle < -150)
            {
                motorSpeedRight = -1.2;
                motorSpeedLeft = -0.0;
            }
            else if (angle >= -150 && angle < -130)
            {
                motorSpeedRight = -1.2;
                motorSpeedLeft = -0.15;
            }
            else if (angle >= -130 && angle < -110)
            {
                motorSpeedRight = -1.2;
                motorSpeedLeft = -0.3;
            }
            else if (angle >= -110 && angle < -70)
            {
                motorSpeedRight = -1.0;
                motorSpeedLeft = -1.0;
            }
            else if (angle >= -70 && angle < -50)
            {
                motorSpeedRight = -0.0;
                motorSpeedLeft = -1.2;
            }
            else if (angle >= -50 && angle < -30)
            {
                motorSpeedRight = -0.15;
                motorSpeedLeft = -1.2;
            }
            else if (angle >= -30 && angle < -10)
            {
                motorSpeedRight = -0.3F;
                motorSpeedLeft = -1.2F;
            }
            // szczególny przypadek
            // skręt w lewo
            else if (angle >= -10 && angle < 10 && power > 0.1)
            {
                motorSpeedRight = 0.8;
                motorSpeedLeft = -0.8;
            }
            else if (angle >= 10 && angle < 30)
            {
                motorSpeedRight = 0.0F;
                motorSpeedLeft = 1.2F;
            }
            else if (angle >= 30 && angle < 50)
            {
                motorSpeedRight = 0.15F;
                motorSpeedLeft = 1.2F;
            }
            else if (angle >= 50 && angle < 70)
            {
                motorSpeedRight = 0.3F;
                motorSpeedLeft = 1.2F;
            }
            else if (angle >= 70 && angle < 110)
            {
                motorSpeedRight = 1;
                motorSpeedLeft = 1;
            }
            else if (angle >= 110 && angle < 130)
            {
                motorSpeedRight = 1.2F;
                motorSpeedLeft = 0.3F;
            }
            else if (angle >= 130 && angle < 150)
            {
                motorSpeedRight = 1.2F;
                motorSpeedLeft = 0.15F;
            }
            else if (angle >= 150 && angle < 170)
            {
                motorSpeedRight = 1.2F;
                motorSpeedLeft = 0.0F;
            }
            // szczególny przypadek
            // skręt w prawo
            else if (angle >= 170 || angle < -170)
            {
                motorSpeedRight = -0.8;
                motorSpeedLeft = 0.8;
            }
            else
            {
                motorSpeedRight = 0.0;
                motorSpeedLeft = 0.0;
            }

            double maxSpeed = 35;
            double minSpeed = -35;

            motorSpeedRight = power * maxSpeed * motorSpeedRight;
            motorSpeedLeft = power * maxSpeed * motorSpeedLeft;

            if (motorSpeedRight > 100)
                motorSpeedRight = 100;
            if (motorSpeedLeft > 100)
                motorSpeedLeft = 100;
            if (motorSpeedRight < -100 && motorSpeedRight != 0)
                motorSpeedRight = -100;
            if (motorSpeedLeft < -100 && motorSpeedLeft != 0)
                motorSpeedLeft = -100;

            this.SpeedRightWheel = Convert.ToInt32(motorSpeedRight);
            this.SpeedLeftWheel = Convert.ToInt32(motorSpeedLeft);
        }

        public void ConvertFromTwoStickInput(float rightStickValue, float leftStcikValue)
        {
            throw new NotImplementedException();
        }
    }
}
