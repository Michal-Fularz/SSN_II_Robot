using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// gamepad
using Microsoft.Xna.Framework.Input;

namespace SSN_II_RobotPokazowy
{
    class CRobot
    {
        private const int numberOfButtons = 6;

        public CInputs Inputs { get; set; }
        public COutputs Outputs { get; set; }

        public CRobot()
        {
            Inputs = new CInputs(numberOfButtons);
            Outputs = new COutputs();
        }

        public void UpdateOutputsBasedOnInputs()
        {
            this.Outputs.Motors.ConvertFromOneStickInput(this.Inputs.GamePad.GamePadState.ThumbSticks.Right.X, this.Inputs.GamePad.GamePadState.ThumbSticks.Right.Y);
        }
    }


    public class CInputs
    {
        public CButtons Buttons { get; set; }
        public CGamePad GamePad { get; set; }
        public CPower Power { get; set; }

        public CInputs()
        {
            Buttons = new CButtons();
            GamePad = new CGamePad();
            Power = new CPower();
        }

        public CInputs(int numberOfButtons)
        {
            Buttons = new CButtons(numberOfButtons);
            GamePad = new CGamePad();
            Power = new CPower();
        }

        public void Read()
        {
            throw new NotImplementedException();
        }
    }

    public class CPower
    {
        public enum PowerStatus : int
        {
            Critical = 0,
            Warning = 1,
            Normal = 2
        };

        public readonly double VoltageMaxValue;
        private const double VoltageCriticalValue = 9;
        private const double VoltageWarningValue = 15;

        public double Voltage { get; private set; }
        public CPower.PowerStatus Status;

        public CPower()
        {
            this.VoltageMaxValue = 24.0;
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

    public class CButtons
    {
        public bool[] ButtonsState { get; set; }

        public CButtons()
        {
            ButtonsState = new bool[0];
        }

        public CButtons(int numberOfButtons)
        {
            ButtonsState = new bool[numberOfButtons];
        }

        public int GetNumberOfButtons()
        {
            return ButtonsState.Length;
        }

        public void Update(byte buttonsState)
        {
            for (int i = 0; i < ButtonsState.Length; ++i)
            {
                if ((buttonsState & (1 << i)) != 0)
                {
                    ButtonsState[i] = true;
                }
                else
                {
                    ButtonsState[i] = false;
                }
            }
        }

        public void Update(bool[] buttons)
        {
            for (int i = 0; i < ButtonsState.Length; ++i)
            {
                this.ButtonsState[i] = buttons[i];
            }
        }
    }

    public class CGamePad
    {
        public Microsoft.Xna.Framework.Input.GamePadState GamePadState { get; set; }

        public CGamePad()
        {
            GamePadState = new GamePadState();
        }

        public void Update(Microsoft.Xna.Framework.Input.GamePadState gamePadState)
        {
            GamePadState = gamePadState;
        }
    }

    public class COutputs
    {
        public CMotors Motors { get; set; }
        public CLeds LedsChassis { get; set; }

        public CLeds LedsFrontLight { get; set; }
        //public CLeds Leds

        public CSound Sound { get; set; }

        public COutputs()
        {
            Motors = new CMotors();
        }

        public void Write()
        {
        }
    }

    public class CGamepadVibrations
    {
        public double VibrationRight { get; private set; }
        public double VibrationLeft { get; private set; }
    }

    public class CServo
    {
    }

    public class CSound
    {
        // czy duration tutaj? czy wówczas czas trwania dodać do wszystkich elementów wyjściowych (np ustawia się jak długo silniki mają wysyłać informację...)
        public string SoundName { get; private set; }
        public int DurationInMiliseconds { get; private set; }

        private bool flagSoundInProgress;

        public CSound()
        {
            SoundName = "";
            DurationInMiliseconds = -1;

            flagSoundInProgress = false;
        }

        public bool IsSoundInProgress()
        {
            return flagSoundInProgress;
        }

        public void MarkSoundAsInProgress()
        {
            flagSoundInProgress = true;
        }

        public void MarkSoundAsFinished()
        {
            SoundName = "-";
            DurationInMiliseconds = -1;

            flagSoundInProgress = false;
        }
    }

    public class CSoundExecutor
    {
        private NAudio.Wave.WaveFileReader wfr;
        private NAudio.Wave.WaveChannel32 wc;
        private NAudio.Wave.WaveOutEvent audioOutput;

        public bool FlagSoundPlaying { get; private set; }
        
        public CSoundExecutor()
        {
            this.FlagSoundPlaying = false;
        }

        public void PlaySound(string filename)
        {
            if (this.FlagSoundPlaying)
            {
                this.StopPlayingSound();
            }

            try
            {
                this.wfr = new NAudio.Wave.WaveFileReader(filename);
                this.wc = new NAudio.Wave.WaveChannel32(this.wfr) { PadWithZeroes = false };

                this.audioOutput = new NAudio.Wave.WaveOutEvent();
                this.audioOutput.Init(wc);
                this.audioOutput.Play();
                this.audioOutput.PlaybackStopped += PlaybackStopped;

                this.FlagSoundPlaying = true;
            }
            catch (Exception e)
            {
                AminExceptions.CAminExceptions.ThrowException(e, "Błąd przy próbie odtwarzania pliku dźwiękowego.");
                throw;
            }

        }

        public void StopPlayingSound()
        {
            try
            {
                this.audioOutput.Stop();
                this.FlagSoundPlaying = false;
            }
            catch (Exception e)
            {
                AminExceptions.CAminExceptions.ThrowException(e, "Błąd przy próbie zatrzymania odtwarzania pliku dźwiękowego.");
            }
        }

        private void PlaybackStopped(Object sender, NAudio.Wave.StoppedEventArgs args)
        {
            this.StopPlayingSound();
        }


    }

    // klasa do przechowywania pojedynczej (sterowanego pojedynczo zbioru ledów) czy wszystkich na robocie?
    public class CLeds
    {
        private int Red;
        private int Green;
        private int Blue;

        public CLeds()
        {
        }
    }

    public class CMotors
    {
        public int SpeedRightWheel { get; private set; }
        public int SpeedLeftWheel { get; private set; }

        public byte speedRightWheelDriverLevel { get; set; }
        public byte speedLeftWheelDriverLevel { get; set; }

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
            // 1 - 127 - pierwsze wyjście
            // 1 - maksymalna prędkość wstecz, 127 - maksymalna prędkość do przodu, 64 - stop
            double tempSpeedRight = Math.Round((this.SpeedRightWheel + 100.0) * 127.0 / 201.0);

            this.speedRightWheelDriverLevel = Convert.ToByte(tempSpeedRight);


            // 128 - 255 - drugie wyjście
            // 128 - maksymalna prędkość wstecz, 255 - maksymalna prędkość do przodu, 192 - stop
            double tempSpeedLeft = Math.Round((this.SpeedLeftWheel + 100.0) * 127.0 / 200.0) + 128.0;

            if (tempSpeedLeft == 256)
            {
                tempSpeedLeft--;
            }

            this.speedLeftWheelDriverLevel = Convert.ToByte(tempSpeedLeft);
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
                motorSpeedRight = 0.4;
                motorSpeedLeft = -0.4;
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
                motorSpeedRight = -0.4;
                motorSpeedLeft = 0.4;
            }
            else
            {
                motorSpeedRight = 0.0;
                motorSpeedLeft = 0.0;
            }

            double maxSpeed = 80;
            double minSpeed = -80;

            motorSpeedRight = power * maxSpeed * motorSpeedRight;
            motorSpeedLeft = power * maxSpeed * motorSpeedLeft;

            /*
            if (motorSpeedRight > maxSpeed)
                motorSpeedRight = maxSpeed;
            if (motorSpeedLeft > maxSpeed)
                motorSpeedLeft = maxSpeed;
            if (motorSpeedRight < minSpeed && motorSpeedRight != 0)
                motorSpeedRight = minSpeed;
            if (motorSpeedLeft < minSpeed && motorSpeedLeft != 0)
                motorSpeedLeft = minSpeed;
             * */

            this.SpeedRightWheel = Convert.ToInt32(motorSpeedRight);
            this.SpeedLeftWheel = Convert.ToInt32(motorSpeedLeft);
        }

        public void ConvertFromTwoStickInput(float rightStickValue, float leftStcikValue)
        {
            throw new NotImplementedException();
        }
    }
}
