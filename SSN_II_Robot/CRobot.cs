using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// RS232
using CommandMessenger;
using CommandMessenger.TransportLayer;

using MAF_Robot;

namespace SSN_II_Robot
{
    // lorafen was here! Hello git!

    class CRobot
    {
        private const int numberOfButtons = 6;

        public enum RobotState : int
        {
            CriticalPower = 0,
            SequnceInProgress = 1,
            SequenceTerminating = 2,
            Idle = 3,
        };

        public CInputs Inputs { get; set; }
        public COutputs Outputs { get; set; }
        public RobotState CurrentState;

        public CRobot()
        {

            Inputs = new CInputs(numberOfButtons);
            Outputs = new COutputs();

            this.CurrentState = RobotState.Idle;

            this.InitSerialPort();
        }

        public void UpdateOutputsBasedOnInputs()
        {
            // todo - tu wstawić logikę robota
            switch (this.CurrentState)
            {
                case RobotState.CriticalPower:
                {
                    if (this.Inputs.Power.IsAtCriticalLevel())
                    {
                        this.CurrentState = RobotState.Idle;
                    }
                }
                break;

                case RobotState.Idle:
                {
                    if (this.Inputs.Power.IsAtCriticalLevel())
                    {
                        this.CurrentState = RobotState.CriticalPower;
                    }
                    /*      NIE PAMIETAM CO TU CHCIALAM WPISAC ZA WARUNEK :(
                    else if (this.Inputs.Buttons.ButtonsState == )
                    {
                        this.CurrentState = RobotState.SequnceInProgress;
                    }
                    */
                }
                break;
            }
            if (this.Inputs.Power.IsAtCriticalLevel())
            {
                this.CurrentState = RobotState.CriticalPower;
            }
        }

        #region SerialPort
        enum Command
        {
            Servo = 0,          // two ints (numer serwa + pozycja 0-100), PC -> Arduino
            MotorsPower = 1,    // two ints, PC -> Arduino
            TouchButtonMask = 2, // one bool (maska 0 - 1) PC -> Arduino
            ButtonsState = 3,   // two ints (napięcie + stan przycisków), Arduino -> PC
            LedsBottom = 4,     // three ints (składowa każdego koloru - r, g, b), PC -> Arduino
            LedsChasis = 5,     // three ints (składowa każdego koloru - r, g, b), PC -> Arduino
            LedsEyes = 6,       // three ints (składowa każdego koloru - r, g, b), PC -> Arduino            
        };

        private SerialTransport serialTransport;
        private CmdMessenger cmdMessenger;

        public void InitSerialPort()
        {
            this.serialTransport = new SerialTransport
            {
                //CurrentSerialSettings = { PortName = "COM13", BaudRate = 115200, DtrEnable = false }
                CurrentSerialSettings = { PortName = "COM66", BaudRate = 115200, DtrEnable = false }
                // setting for onboard compuetr:
                //CurrentSerialSettings = { PortName = "COM7", BaudRate = 115200, DtrEnable = false }
            };

            this.cmdMessenger = new CmdMessenger(this.serialTransport)
            {
                BoardType = BoardType.Bit16
            };

            this.cmdMessenger.Attach((int)Command.ButtonsState, OnButtonsStateReceived);

            this.cmdMessenger.StartListening();

        }

        public void SendMotorsPower()
        {
            SendCommand command = new SendCommand((int)Command.MotorsPower);

            command.AddArgument(this.Outputs.Motors.speedRightWheelDriverLevel);
            command.AddArgument(this.Outputs.Motors.speedLeftWheelDriverLevel);
            this.cmdMessenger.SendCommand(command);
        }

        public void SendTouchButtonMask(bool mask)
        {
            SendCommand command = new SendCommand((int)Command.TouchButtonMask);

            command.AddArgument(mask);
            this.cmdMessenger.SendCommand(command);
        }

        public void SendServo(CServo.ServoType servo, int position)
        {
            // todo dodać zabezpieczenie, żeby position było w zakresie 0-100
            if (position >= 0 && position <= 100)
            {
                SendCommand command = new SendCommand((int)Command.Servo);

                command.AddArgument((int)servo);
                command.AddArgument(position);
                this.cmdMessenger.SendCommand(command);
            }
        }

        public void SendServo(CServo.ServoType servo)
        {
            int newPosition = this.Outputs.Servos.servosPosition[((int)servo) - 1];
            
            SendCommand command = new SendCommand((int)Command.Servo);

            command.AddArgument((int)servo);
            command.AddArgument(newPosition);
            this.cmdMessenger.SendCommand(command);
        }

        public void SendServo()
        {
            /*
            for (int i = 0; i < this.Outputs.Servos.servosPosition.Length/2; ++i)
            {
                int newPosition = this.Outputs.Servos.servosPosition[((int)i)];

                SendCommand command = new SendCommand((int)Command.Servo);

                command.AddArgument((int)(i+1));
                command.AddArgument(newPosition);
                this.cmdMessenger.SendCommand(command);
            }
            */
            for (int i = 0; i < this.Outputs.Servos.servosChangePosition.Length; ++i)
            {
                if (this.Outputs.Servos.servosChangePosition[(int)i] == true)
                {
                    int newPosition = this.Outputs.Servos.servosPosition[((int)i)];
                    this.Outputs.Servos.servosChangePosition[(int)i] = false;
                    SendCommand command = new SendCommand((int)Command.Servo);

                    command.AddArgument((int)(i + 1));
                    command.AddArgument(newPosition);
                    this.cmdMessenger.SendCommand(command);
                }
            }
        }

        public void SendLedsBottom(int r, int g, int b)
        {
            SendCommand command = new SendCommand((int)Command.LedsBottom);

            command.AddArgument(r);
            command.AddArgument(g);
            command.AddArgument(b);
            this.cmdMessenger.SendCommand(command);
        }

        public void SendLedsChasis(int r, int g, int b)
        {
            SendCommand command = new SendCommand((int)Command.LedsChasis);

            command.AddArgument(r);
            command.AddArgument(g);
            command.AddArgument(b);
            this.cmdMessenger.SendCommand(command);
        }

        public void SendLedsEyes(int r, int g, int b)
        {
            SendCommand command = new SendCommand((int)Command.LedsEyes);

            command.AddArgument(r);
            command.AddArgument(g);
            command.AddArgument(b);
            this.cmdMessenger.SendCommand(command);

        }

        public void Dispose()
        {
            // Stop listening
            this.cmdMessenger.StopListening();

            // Dispose Command Messenger
            this.cmdMessenger.Dispose();

            // Dispose Serial Port object
            this.serialTransport.Dispose();

            this.Outputs.Sound.Dispose();

        }

        // ------------------  C A L L B A C K S ---------------------

        // Called when a received command has no attached function.

        void OnButtonsStateReceived(ReceivedCommand arguments)
        {
            int powerAsIntFromMarek = arguments.ReadInt32Arg();
            this.Inputs.Power.Update((double)powerAsIntFromMarek * (1.0/10.0));


            int buttonsState = arguments.ReadInt32Arg();
            this.Inputs.Buttons.Update((byte)buttonsState);
        }

        #endregion
    }

    public class COutputs
    {
        public CMotors Motors { get; set; }
        public CLeds LedsChassis { get; set; }

        public CLeds LedsFrontLight { get; set; }
        //public CLeds Leds

        public CServo Servos { get; set; }

        public CSound Sound { get; set; }

        public COutputs()
        {
            Motors = new CMotors();
            Servos = new CServo();
            Sound = new CSound();
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

    public class CSound : IDisposable
    {
        // czy duration tutaj? czy wówczas czas trwania dodać do wszystkich elementów wyjściowych (np ustawia się jak długo silniki mają wysyłać informację...)
        public string SoundName { get; private set; }
        public int DurationInMiliseconds { get; private set; }

        private bool flagSoundInProgress;

        private CSoundExecutor soundExecutor;

        public CSound()
        {
            SoundName = "";
            DurationInMiliseconds = -1;

            flagSoundInProgress = false;
            soundExecutor = new CSoundExecutor();
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

        public void Dispose()
        {
            this.soundExecutor.Dispose();
        }
    }

    public class CSoundExecutor : IDisposable
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


        public void Dispose()
        {
            if (this.wfr != null)
            {
                this.wfr.Dispose();
            }
            if (this.audioOutput != null)
            {
                this.audioOutput.Dispose();
            }
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

            this.speedRightWheelDriverLevel = Convert.ToByte(tempSpeedRight);
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

    public class CSerialPort
    {
        // todo sprawdzic, jakie komendy do stworzenia
        
    }
}
