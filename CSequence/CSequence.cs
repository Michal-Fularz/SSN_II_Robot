using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MAF_Robot
{
    public class CSequence
    {
        public int currentTime = 0;

        public void updateTime()
        {
            this.currentTime += 1;
        }

        public bool IsFinished()
        {
            bool flagfinished = false;
            
            if(this.currentTime > this.duration)
            {
                flagfinished = true;
            }

            return flagfinished;
        }

        public int duration;

        Queue<Tuple<int, ActionBase>> ActionStandardQueue;

        public CSequence()
        {
            this.ActionStandardQueue = new Queue<Tuple<int, ActionBase>>();
        }

        public void Add(int time, ActionBase action)
        {
            this.ActionStandardQueue.Enqueue(Tuple.Create<int, ActionBase>(time, action) );
        }

        public List<ActionBase> GetCurrentActions()
        {
            List<ActionBase> listOfActions = new List<ActionBase>();

            bool flagReadNext = true;
            while (flagReadNext && (ActionStandardQueue.Count > 0))
            {
                int timeFromQueue = ActionStandardQueue.Peek().Item1;
                if(timeFromQueue <= this.currentTime)
                {
                    var actionToPerform = ActionStandardQueue.Dequeue().Item2;
                    listOfActions.Add(actionToPerform);
                }
                else
                {
                    flagReadNext = false;
                }
            }

            return listOfActions;
        }

        /// <summary>
        /// Sequence with shot sound and shake a head
        /// </summary>
        public void CreateSampleSequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            this.Add(0, new ActionMotor(40, 40));

            this.Add(10, new ActionSound("sound/shot.wav"));

            for(int i=10; i<50; ++i)
            {
                this.Add(i, new ActionServo(CServo.ServoType.Head, i));
            }  
        }

        /// <summary>
        /// Playing YMCA song and robot dancing - NOT FINISHED 
        /// </summary>
        public void CreateYMCASequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            //this.Add(0, new ActionSound("sound/YMCA.wav"));
            //this.Add(5, new ActionServo(CServo.ServoType.Left1, 40));

        }

        /// <summary>
        /// Surfing without music
        /// </summary>
        public void CreateSurferSequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            // let's play music

            // setting arms servos
            this.Add(10, new ActionServo(CServo.ServoType.Right1, 35));
            this.Add(10, new ActionServo(CServo.ServoType.Right3, 100));
            this.Add(15, new ActionServo(CServo.ServoType.Right4, 65));
            this.Add(20, new ActionServo(CServo.ServoType.Left2, 100));
            // let's the light light
            this.Add(25, new ActionLed(CLeds.LedType.Chasis, 247, 140, 45));
            // let's add some move
            this.Add(30, new ActionMotor(30, 0));
            this.Add(40, new ActionMotor(0, 30));
            this.Add(50, new ActionMotor(0, 0));
            // turn off the light
            this.Add(90, new ActionLed(CLeds.LedType.Chasis, 0, 0, 0));
            this.Add(91, new ActionServo(CServo.ServoType.Right1, 0));
            this.Add(92, new ActionServo(CServo.ServoType.Right3, 0));
            this.Add(93, new ActionServo(CServo.ServoType.Right4, 0));
            this.Add(94, new ActionServo(CServo.ServoType.Left2, 0));
        }

        /// <summary>
        /// Sequence while robot moves forearms like an athlete on gym with some nasty sounds
        /// </summary>
        public void CreateSportSequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            // sounds of sports needs to be add
            this.Add(0, new ActionSound("sound/silownia.wav"));

            this.Add(10, new ActionServo(CServo.ServoType.Left4, 90));
            this.Add(20, new ActionServo(CServo.ServoType.Left4, 0));
            this.Add(30, new ActionServo(CServo.ServoType.Right4, 90));
            this.Add(40, new ActionServo(CServo.ServoType.Right4, 0));
            this.Add(50, new ActionServo(CServo.ServoType.Left4, 90));
            this.Add(60, new ActionServo(CServo.ServoType.Left4, 0));
            this.Add(70, new ActionServo(CServo.ServoType.Right4, 90));
            this.Add(80, new ActionServo(CServo.ServoType.Right4, 0));

        }

        /// <summary>
        /// Hello sequence
        /// </summary>
        public void CreateHiSequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            this.Add(0, new ActionServo(CServo.ServoType.Right4, 90));

            // sounds of sports needs to be add
            this.Add(10, new ActionSound("sound/czesc.wav"));

            this.Add(20, new ActionServo(CServo.ServoType.Right4, 0));

        }

        /// <summary>
        /// Walk of glory of Darth Vader
        /// </summary>
        public void CreateVaderSequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            this.Add(0, new ActionSound("sound/starwars01.wav"));
            
            for (int i = 1; i < this.duration; i+=10)
            {
                this.Add(i, new ActionLed(CLeds.LedType.Bottom, 0, 255, 0));
                // zgas
                this.Add(i + 5, new ActionLed(CLeds.LedType.Bottom, 0, 0, 0));

                 //jazda do przodu
                if (i == 51)
                {
                    this.Add(i, new ActionMotor(40, 40));
                }
                if (i == 71)
                {
                    this.Add(i, new ActionMotor(0, 0));
                }

            }
        }

        public void CreateLightSequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            int l = 0;
            
            int r = 0;
            int g = 0;
            int b = 0;

            //this.Add(0, new ActionSound("sound/starwars01.wav"));
            while(l != this.duration)
            {
                this.Add(l, new ActionLed(CLeds.LedType.Chasis, r, g, b));
                this.Add(l+2, new ActionLed(CLeds.LedType.Eyes, r, g, b));
                this.Add(l+3, new ActionLed(CLeds.LedType.Bottom, r, g, b));
                
                if (r >= 120)
                {
                    r = 0;
                }

                if (g >= 120)
                {
                    g = 0;
                }

                if (b >= 120)
                {
                    b = 0;
                }
                
                r+=5;
                g+=10;
                b+=15;
                
                l+=4;
            }
            

            this.Add(94, new ActionLed(CLeds.LedType.Eyes, 0, 0, 0));
            this.Add(95, new ActionLed(CLeds.LedType.Chasis, 0, 0, 0));
            this.Add(96, new ActionLed(CLeds.LedType.Bottom, 0, 0, 0));
        }

        // NOT READY
        public void CreateTickleSequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            //this.Add(0, new ActionSound("sound/starwars01.wav"));
            //this.Add(l, new ActionLed(CLeds.LedType.Chasis, r, g, b));
        }

        public void CreatePoliceSequance()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            this.Add(0, new ActionSound("sound/police.wav"));

            for (int i = 5; i < (this.duration - 5); i+=10)
            {
                this.Add(i + 5, new ActionLed(CLeds.LedType.Chasis, 255, 0, 0));
                this.Add(i + 10, new ActionLed(CLeds.LedType.Chasis, 0, 0, 255));    
            }

            this.Add(98, new ActionLed(CLeds.LedType.Chasis, 0, 0, 0));
            
        }
    }

    public abstract class ActionBase
    {
        public enum ActionType
        {
            Servo = 0,
            Sound = 5, 
            Light = 6, 
            Movement = 7, 
            CameraMovement = 8
        };

        public ActionType actionType;
    }

    public class ActionMotor : ActionBase
    {
        public ActionMotor(int speedRightMotor, int speedLeftMotor)
        {
            this.SpeedRightMotor = speedRightMotor;
            this.SpeedLeftMotor = speedLeftMotor;
            this.actionType = ActionType.Movement;
        }

        public int SpeedRightMotor;
        public int SpeedLeftMotor;
    }

    public class ActionServo : ActionBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="position">allowed range: 0 - 100</param>
        public ActionServo(CServo.ServoType type, int position)
        {
            this.Type = type;
            this.Position = position;
            this.actionType = ActionType.Servo;
        }

        public int Position;
        public CServo.ServoType Type;
    }

    public class ActionSound : ActionBase
    {
        public ActionSound(string soundName)
        {
            this.SoundName = soundName;
            this.actionType = ActionType.Sound;
        }

        public string SoundName;
    }

    public class ActionLed : ActionBase
    {
        public ActionLed(CLeds.LedType ledsType, int r, int g, int b)
        {    
            this.Type = ledsType;
            this.Color.R = r;
            this.Color.G = g;
            this.Color.B = b;
            this.actionType = ActionType.Light;
        }

        public CLeds.SColor Color;
        public CLeds.LedType Type;
    }
}
