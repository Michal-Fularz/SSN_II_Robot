using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// RS232
using CommandMessenger;
using CommandMessenger.TransportLayer;

// gamepad
using Microsoft.Xna.Framework.Input;

using MAF_Robot;

// Visualization of robot
using System.Windows.Media;

namespace SSN_II_Robot
{

    class CRobot
    {
        private const int numberOfButtons = 6;

        public enum RobotState : int
        {
            CriticalPower = 0,
            Safety = 1,
            Idle = 2,
            SequnceInProgress = 3,
            SequenceTerminating = 4,
            Kinect = 5,
            
        };

        public CInputs Inputs { get; set; }
        public COutputs Outputs { get; set; }
        public RobotState CurrentState;
        private int SameStateCounter = 0;

        public CKinect Kinect { get; set; }
        public CSequence Sequence;

        private ActionBase actionStopMotors = new ActionMotor(0, 0);

        public CRobot(string serialPortName)
        {
            Sequence = new CSequence();
            Inputs = new CInputs(numberOfButtons);
            Outputs = new COutputs();
            Kinect = new CKinect();

            this.CurrentState = RobotState.Idle;

            this.InitSerialPort(serialPortName);
        }

        private void MakeDecision()
        {
            if(this.CurrentState == RobotState.CriticalPower)
            {
                this.Perform(actionStopMotors);
                this.Outputs.Sound.StopPlayingSound();
                this.Outputs.Leds.TurnOff();
            }
            else if (this.CurrentState == RobotState.Safety)
            {
                this.Perform(actionStopMotors);
                this.Outputs.Sound.StopPlayingSound();
                this.Outputs.Leds.TurnOff();
            }
            else if (this.CurrentState == RobotState.Idle)
            {
                // control motors
                #region STEROWANIE NAPĘDAMI ZA POMOCĄ PADA
                if ((this.Inputs.Gamepad.GamepadState.Triggers.Right < 0.5) &&
                    (this.Inputs.Gamepad.GamepadState.IsButtonUp(Buttons.RightShoulder)) &&
                    (this.Inputs.Gamepad.GamepadState.Triggers.Left < 0.5) &&
                    (this.Inputs.Gamepad.GamepadState.IsButtonUp(Buttons.LeftShoulder))
                )
                {
                    this.Outputs.Motors.ConvertFromOneStickInput(this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.X, this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.Y);
                }
                #endregion

                // control servos
                #region STEROWANIE RAMIENIEM PRAWYM ZA POMOCĄ PADA
                if (this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.X >= 0.5 && (this.Inputs.Gamepad.GamepadState.Triggers.Right < 0.5) && (this.Inputs.Gamepad.GamepadState.Buttons.RightShoulder == ButtonState.Pressed))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right1, 5);
                    //this.SendServo(CServo.ServoType.Right1);
                }
                else if (this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.X <= -0.5 && (this.Inputs.Gamepad.GamepadState.Triggers.Right < 0.5) && (this.Inputs.Gamepad.GamepadState.Buttons.RightShoulder == ButtonState.Pressed))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right1, -5);
                    //this.SendServo(CServo.ServoType.Right1);
                }
                if (this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.Y >= 0.5 && (this.Inputs.Gamepad.GamepadState.Triggers.Right < 0.5) && (this.Inputs.Gamepad.GamepadState.Buttons.RightShoulder == ButtonState.Pressed))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right2, 5);
                    //this.SendServo(CServo.ServoType.Right2);
                }
                else if (this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.Y <= -0.5 && (this.Inputs.Gamepad.GamepadState.Triggers.Right < 0.5) && (this.Inputs.Gamepad.GamepadState.Buttons.RightShoulder == ButtonState.Pressed))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right2, -5);
                    //this.SendServo(CServo.ServoType.Right2);
                }
                // -------------------
                if ((this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.X >= 0.5) && (this.Inputs.Gamepad.GamepadState.Triggers.Right >= 0.5))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right3, 5);
                    //this.SendServo(CServo.ServoType.Right3);
                }
                else if ((this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.X <= -0.5) && (this.Inputs.Gamepad.GamepadState.Triggers.Right >= 0.5))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right3, -5);
                    //this.SendServo(CServo.ServoType.Right3);
                }
                if ((this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.Y >= 0.5) && (this.Inputs.Gamepad.GamepadState.Triggers.Right >= 0.5))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right4, 5);
                    //this.SendServo(CServo.ServoType.Right4);
                }
                else if ((this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.Y <= -0.5) && (this.Inputs.Gamepad.GamepadState.Triggers.Right >= 0.5))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right4, -5);
                    //this.SendServo(CServo.ServoType.Right4);
                }
                #endregion

                #region STEROWANIE RAMIENIEM LEWYM ZA POMOCĄ PADA
                if (this.Inputs.Gamepad.GamepadState.ThumbSticks.Left.X >= 0.5 && (this.Inputs.Gamepad.GamepadState.Triggers.Left < 0.5) && (this.Inputs.Gamepad.GamepadState.Buttons.LeftShoulder == ButtonState.Pressed))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left1, 5);
                    //this.SendServo(CServo.ServoType.Left1);
                }
                else if (this.Inputs.Gamepad.GamepadState.ThumbSticks.Left.X <= -0.5 && (this.Inputs.Gamepad.GamepadState.Triggers.Left < 0.5) && (this.Inputs.Gamepad.GamepadState.Buttons.LeftShoulder == ButtonState.Pressed))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left1, -5);
                    //this.SendServo(CServo.ServoType.Left1);
                }
                if (this.Inputs.Gamepad.GamepadState.ThumbSticks.Left.Y >= 0.5 && (this.Inputs.Gamepad.GamepadState.Triggers.Left < 0.5) && (this.Inputs.Gamepad.GamepadState.Buttons.LeftShoulder == ButtonState.Pressed))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left2, 5);
                    //this.SendServo(CServo.ServoType.Left2);
                }
                else if (this.Inputs.Gamepad.GamepadState.ThumbSticks.Left.Y <= -0.5 && (this.Inputs.Gamepad.GamepadState.Triggers.Left < 0.5) && (this.Inputs.Gamepad.GamepadState.Buttons.LeftShoulder == ButtonState.Pressed))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left2, -5);
                    //this.SendServo(CServo.ServoType.Left2);
                }
                // -------------------
                if ((this.Inputs.Gamepad.GamepadState.ThumbSticks.Left.X >= 0.5) && (this.Inputs.Gamepad.GamepadState.Triggers.Left >= 0.5))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left3, 5);
                    //this.SendServo(CServo.ServoType.Left3);
                }
                else if ((this.Inputs.Gamepad.GamepadState.ThumbSticks.Left.X <= -0.5) && (this.Inputs.Gamepad.GamepadState.Triggers.Left >= 0.5))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left3, -5);
                    //this.SendServo(CServo.ServoType.Left3);
                }
                if ((this.Inputs.Gamepad.GamepadState.ThumbSticks.Left.Y >= 0.5) && (this.Inputs.Gamepad.GamepadState.Triggers.Left >= 0.5))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left4, 5);
                    //this.SendServo(CServo.ServoType.Left4);
                }
                else if ((this.Inputs.Gamepad.GamepadState.ThumbSticks.Left.Y <= -0.5) && (this.Inputs.Gamepad.GamepadState.Triggers.Left >= 0.5))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left4, -5);
                    //this.SendServo(CServo.ServoType.Left4);
                }
                #endregion

                // head servo control
                #region STEROWANIE GŁOWĄ ZA POMOCĄ PADA
                if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Buttons.DPadLeft))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Head, -5);
                }
                if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Buttons.DPadRight))
                {
                    this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Head, 5);
                }
                #endregion
            }
            else if(this.CurrentState == RobotState.SequnceInProgress)
            {
                var actionsToDo = this.Sequence.GetCurrentActions();

                foreach (var item in actionsToDo)
                {
                    this.Perform(item);
                }
                this.Sequence.updateTime();
            }
            else if (this.CurrentState == RobotState.SequenceTerminating)
            {
                this.Perform(actionStopMotors);
                this.Outputs.Sound.StopPlayingSound();
            }
            else if (this.CurrentState == RobotState.Kinect)
            {
                
            }
            else
            {
                throw new NotImplementedException("Wrong robot code!");
            }

        

            // control sound

            // control leds

            // control gamepad vibrations
        }

        public void Perform(ActionBase action)
        {
            switch(action.actionType)
            {
                case ActionBase.ActionType.Movement:
                    {
                        ActionMotor a = (ActionMotor)action;
                        this.Outputs.Motors.SetSpeedDirectly(a.SpeedRightMotor, a.SpeedLeftMotor);
                    }
                    break;
                case ActionBase.ActionType.Servo:
                    {
                        ActionServo a = (ActionServo)action;
                        this.Outputs.Servos.SetServoPosition(a.Type, a.Position);
                    }
                    break;
                case ActionBase.ActionType.Sound:
                    {
                        ActionSound a = (ActionSound)action;
                        this.Outputs.Sound.Play(a.SoundName);
                    }
                    break;
                case ActionBase.ActionType.Light:
                    {
                        ActionLed a = (ActionLed)action;
                        this.Outputs.Leds.SetLedColors(a.Type, a.Color);
                    }
                    break;
            }
        }

        public void UpdateOutputsBasedOnInputs()
        {
            // 1. Critical Power - bez możliwości wyjścia z tego stanu (o ile napięcie nie wzrośnie) - wyłączyć serwa, wyłączyć silniki, ustawić wzór światełek (co trzecie z tych dookoła świeci na czerwono)
            // 2. Safety - wyłączyć serwa

            // read serial (and update of power and buttons) is done asynchronous in event from serial port
            // read gamepad
            this.Inputs.Gamepad.Update();
            // read kinect?
            //this.Kinect.Start();

            // update inputs - already done

            MakeDecision();

            // send data to robot through serial port
            // TODO - do it only if the values were changed
            this.Outputs.Motors.ConvertToDriverLevels();
            this.SendMotorsPower();

            this.SendServo();
            this.SendLeds();

            // condition for all states
            if (this.Inputs.Power.IsAtCriticalLevel())
            {
                this.CurrentState = RobotState.CriticalPower;
            }
            else
            {
                // state changes
                switch (this.CurrentState)
                {
                    case RobotState.CriticalPower:
                        {
                            if (!this.Inputs.Power.IsAtCriticalLevel())
                            {
                                this.CurrentState = RobotState.Idle;
                            }
                        }
                        break;

                    case RobotState.Idle:
                        {
                            
                            if ((this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back)) &&
                                (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)))
                            {
                                this.CurrentState = RobotState.Safety;
                            }
                            else if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Y))
                            {
                                this.Sequence.CreateHiSequence();
                                this.CurrentState = RobotState.SequnceInProgress;
                            }
                            else if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A))
                            {
                                this.Sequence.CreateSportSequence();
                                this.CurrentState = RobotState.SequnceInProgress;
                            }
                            else if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B))
                            {
                                //this.CurrentState = RobotState.Kinect;
                                this.Sequence.CreateVaderSequence();
                                this.CurrentState = RobotState.SequnceInProgress; 
                            }
                            else if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X))
                            {
                                this.Sequence.CreateLightSequence();
                                this.CurrentState = RobotState.SequnceInProgress;
                            }
                        }
                        break;

                    case RobotState.Kinect:
                        {
                            if ((this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back)) &&
                                (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)))
                            {
                                this.CurrentState = RobotState.Safety;
                            }
                        }
                        break;

                    case RobotState.Safety:
                        {
                            if ((this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back)) &&
                                (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)))
                            {
                                this.CurrentState = RobotState.Safety;
                            }
                            else if ((this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Start)) &&
                                (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)))
                            {
                                this.CurrentState = RobotState.Idle;
                            }
                        }
                        break;

                    case RobotState.SequnceInProgress:
                        {
                            if ((this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back)) &&
                                (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)))
                            {
                                this.CurrentState = RobotState.Safety;
                            }
                            else if (this.Sequence.IsFinished())
                            {
                                this.CurrentState = RobotState.SequenceTerminating;
                            }
                        }
                        break;

                    case RobotState.SequenceTerminating:
                        {
                            if ((this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back)) &&
                                (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)))
                            {
                                this.CurrentState = RobotState.Safety;
                            }

                            SameStateCounter++;

                            if (SameStateCounter >= 10)
                            {
                                SameStateCounter = 0;
                                this.CurrentState = RobotState.Idle;
                            }
                        }
                        break;

                }
            }
        }

        public void Dispose()
        {
            this.Kinect.Close();

            // Stop listening
            this.cmdMessenger.StopListening();

            // Dispose Command Messenger
            this.cmdMessenger.Dispose();

            // Dispose Serial Port object
            this.serialTransport.Dispose();

            this.Outputs.Sound.Dispose();
        }

        #region SerialPort
        private enum Command
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

        public void InitSerialPort(string serialPortName)
        {
            this.serialTransport = new SerialTransport
            {
                CurrentSerialSettings = { PortName = serialPortName, BaudRate = 115200, DtrEnable = false }
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

            command.AddArgument(this.Outputs.Motors.SpeedRightWheelDriverLevel);
            command.AddArgument(this.Outputs.Motors.SpeedLeftWheelDriverLevel);
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
            for (int i = 0; i < this.Outputs.Servos.servosChangedPosition.Length; ++i)
            {
                if (this.Outputs.Servos.servosChangedPosition[(int)i] == true)
                {
                    int newPosition = this.Outputs.Servos.servosPosition[((int)i)];
                    this.Outputs.Servos.servosChangedPosition[(int)i] = false;
                    SendCommand command = new SendCommand((int)Command.Servo);

                    command.AddArgument((int)(i + 1));
                    command.AddArgument(newPosition);
                    this.cmdMessenger.SendCommand(command);
                }
            }
        }

        public void SendLeds()
        {

            for (int i = 0; i < this.Outputs.Leds.ledChangedState.Length; ++i)
            {
                if (this.Outputs.Leds.ledChangedState[(int)i] == true)
                {
                    CLeds.SColor newColor = this.Outputs.Leds.ledState[((int)i)];
                    this.Outputs.Leds.ledChangedState[(int)i] = false;

                    SendCommand command;
                    switch (i)
                    {
                        case 0:
                            {
                                command = new SendCommand((int)Command.LedsBottom);
                            }
                            break;
                        case 1:
                            {
                                command = new SendCommand((int)Command.LedsChasis);
                            }
                            break;
                        case 2:
                            {
                                command = new SendCommand((int)Command.LedsEyes);
                            }
                            break;
                        default:
                            {
                                command = new SendCommand((int)Command.LedsChasis);
                            }
                            break;
                    }
                    command.AddArgument(newColor.R);
                    command.AddArgument(newColor.G);
                    command.AddArgument(newColor.B);
                    this.cmdMessenger.SendCommand(command);
                    // watch out for 128 value - it's stranger but this makes the leds to stop working
                    
                }
            }
        }

        // ------------------  C A L L B A C K S ---------------------
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

        public CLeds Leds { get; set; }

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

        public void Write()
        {
        }
    }

    public class CGamepadVibrations
    {
        public double VibrationRight { get; private set; }
        public double VibrationLeft { get; private set; }
    }

    public class CSerialPort
    {
        // todo sprawdzic, jakie komendy do stworzenia
        
    }
}
