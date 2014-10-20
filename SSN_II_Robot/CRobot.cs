﻿using System;
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

        public CKinect Kinect { get; set; }

        public CRobot(string serialPortName)
        {

            Inputs = new CInputs(numberOfButtons);
            Outputs = new COutputs();
            Kinect = new CKinect();

            this.CurrentState = RobotState.Idle;

            this.InitSerialPort(serialPortName);
        }

        private void MakeDecision()
        {
            // control motors
            // TODO - add dependancy from robotState
            #region STEROWANIE NAPĘDAMI

            if ((this.Inputs.Gamepad.GamepadState.Triggers.Right < 0.5) &&
                (this.Inputs.Gamepad.GamepadState.IsButtonUp(Buttons.RightShoulder)) &&
                (this.Inputs.Gamepad.GamepadState.Triggers.Left < 0.5) &&
                (this.Inputs.Gamepad.GamepadState.IsButtonUp(Buttons.LeftShoulder))
            )
            {
                this.Outputs.Motors.ConvertFromOneStickInput(this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.X, this.Inputs.Gamepad.GamepadState.ThumbSticks.Right.Y);
                this.Outputs.Motors.ConvertToDriverLevels();
            }

            #endregion

            // control servos
            // TODO - add dependancy from robotState
            #region STEROWANIE RAMIENIEM PRAWYM
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

            #region STEROWANIE RAMIENIEM LEWYM
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
            if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Buttons.DPadLeft))
            {
                this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Head, -5);
            }
            if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Buttons.DPadRight))
            {
                this.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Head, 5);
            }

            // control sound

            // control leds

            // control gamepad vibrations
        }

        public void UpdateOutputsBasedOnInputs()
        {
            // 1. Critical Power - bez możliwości wyjścia z tego stanu (o ile napięcie nie wzrośnie) - wyłączyć serwa, wyłączyć silniki, ustawić wzór światełek (co trzecie z tych dookoła świeci na czerwono)
            // 2. Safety - wyłączyć serwa

            // read serial (and update of power and buttons) is done asynchronous in event from serial port
            // read gamepad
            this.Inputs.Gamepad.Update();
            // read kinect?
            this.Kinect.Start();

            // update inputs - already done

            MakeDecision();

            // send data to robot through serial port
            // TODO - do it only if the values were changed
            this.SendMotorsPower();

            this.SendServo();

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
                    if (this.Inputs.Power.IsAtCriticalLevel())
                    {
                        this.CurrentState = RobotState.CriticalPower;
                    }
                    else if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Y))
                    {
                        this.CurrentState = RobotState.SequnceInProgress;
                    }
                    else if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B))
                    {
                        this.CurrentState = RobotState.Kinect;
                    }
                }
                break;

                case RobotState.Kinect:
                {
                    if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                        this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back))
                    {
                        this.CurrentState = RobotState.Idle;
                    }
                    else if (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X) &&
                        this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back))
                    {
                        this.CurrentState = RobotState.Safety;
                    }
                    else if (this.Inputs.Power.IsAtCriticalLevel())
                    {
                        this.CurrentState = RobotState.CriticalPower;
                    }
                }
                break;

                case RobotState.Safety:
                {
                    if ( (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)) &&
                        (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Start)) )
                    {
                        this.CurrentState = RobotState.Idle;
                    }
                    else if (this.Inputs.Power.IsAtCriticalLevel())
                    {
                        this.CurrentState = RobotState.CriticalPower;
                    }
                }
                break;

                case RobotState.SequnceInProgress:
                {
                    if ( (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back)) &&
                        (this.Inputs.Gamepad.GamepadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)) )
                    {
                        this.CurrentState = RobotState.Safety;
                    }
                    else if (this.Inputs.Power.IsAtCriticalLevel())
                    {
                        this.CurrentState = RobotState.CriticalPower;
                    }
                }
                break;

            }
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
            this.Kinect.Close();

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

    public class CSerialPort
    {
        // todo sprawdzic, jakie komendy do stworzenia
        
    }
}
