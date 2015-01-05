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
            this.ActionStandardQueue.Enqueue(Tuple.Create<int, ActionBase>(time, action));
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

        public void CreateSampleSequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            this.Add(0, new ActionMotor(40, 40));

            this.Add(10, new ActionSound("sound/buziak_1.wav"));

            for(int i=10; i<50; ++i)
            {
                this.Add(i, new ActionServo(CServo.ServoType.Head, i));
            }  
        }

        public void CreateYMCASequence()
        {
            this.ActionStandardQueue.Clear();

            this.duration = 100;
            this.currentTime = 0;

            this.Add(0, new ActionSound("sound/YMCA.wav"));
            this.Add(5, new ActionServo(CServo.ServoType.Left1, 40));

        }
    }

    public abstract class ActionBase
    {
        public enum ActionType
        {
            Servo = 0,
            Sound = 5, Light = 6, Movement = 7, CameraMovement = 8
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
}
