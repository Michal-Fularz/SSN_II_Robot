using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Navigation;
using System.Windows.Shapes;

// gamepad
using Microsoft.Xna.Framework.Input;

// Kinect
using Microsoft.Kinect;
using LightBuzz.Vitruvius;
using Microsoft.Win32;
using System.IO;
using System.Media;
using System.Windows.Media.Imaging;

using MAF_Robot;
using System.Windows.Media;
using System.Threading;


namespace SSN_II_Robot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable 
    {
        private CRobot robot;
        private CSettings settings;

        System.Windows.Threading.DispatcherTimer mainTimer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        public void Dispose()
        {
            this.Deinit();
        }

        private void Init()
        {
            settings = new CSettings();
            settings.LoadFromFile("settings\\settings.txt");

            Thread.Sleep(100);

            kinectVitruvius = new CKinectVitruvius(vitruviusImage, vitruviusCanvas);
            kinectVitruvius.Init();
            kinectVitruvius.GestureRecognized += GestureRecognized;

            robot = new CRobot(settings.SerialPortName);

            robot.Inputs.Gamepad.OnDpadUp += new ButtonPressed(NextTabPage);
            robot.Inputs.Gamepad.OnDpadDown += new ButtonPressed(PreviousTabPage);
            //robot.Inputs.Gamepad.OnBack += new ButtonPressed(Exit);

            mainTimer = new System.Windows.Threading.DispatcherTimer();
            mainTimer.Interval = TimeSpan.FromMilliseconds(100);
            mainTimer.Tick += mainTimer_Tick;

            // KINECT
            //robot.Kinect.KinectLoaded(kinectCanvas, Image);

            // for unknown reason this was hardcoded for really long time (2016.02.09) :)
            //rtbMain.AppendText("Aplikacja rozpoczęta: 2013-03-29 11:37:52" + Environment.NewLine);
            rtbMain.AppendText("Aplikacja rozpoczęta: " + System.DateTime.Now.ToShortDateString() + System.DateTime.Now.ToShortTimeString() + Environment.NewLine);
            rtbMain.AppendText("Robot state: " + robot.CurrentState.ToString() + Environment.NewLine); ;

            mainTimer.Start();
        }

        private void Deinit()
        {
            robot.Dispose();
        }

        private void NextTabPage()
        {
            int currentIndex = tcMain.SelectedIndex;

            if (currentIndex == tcMain.Items.Count - 1)
            {
                tcMain.SelectedIndex = 0;
            }
            else
            {
                tcMain.SelectedIndex++;
            }
        }

        private void PreviousTabPage()
        {
            int currentIndex = tcMain.SelectedIndex;

            if (currentIndex == 0)
            {
                tcMain.SelectedIndex = tcMain.Items.Count - 1;
            }
            else
            {
                tcMain.SelectedIndex--;
            }
        }

        #region button handlers

        private void NextTab_Button_Click(object sender, RoutedEventArgs e)
        {
            this.NextTabPage();
        }

        private void PreviousTab_Button_Click(object sender, RoutedEventArgs e)
        {
            this.PreviousTabPage();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Deinit();
            this.Close();
        }

        /// <summary>
        /// Button to test sound and switch to another tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SoundButton(object sender, RoutedEventArgs e)
        {
            Tab5.IsSelected = true;
            robot.Outputs.Sound.Play("sound/sport.wav");
        }

        private void ServoSave(object sender, RoutedEventArgs e)
        {
            robot.Outputs.Servos.SaveData(tb_ServoName.Text);
        }

        private void ServoReset(object sender, RoutedEventArgs e)
        {
            this.robot.Outputs.Servos.ResetServos();
            this.PresentServo(robot.Outputs.Servos);
        }
        
        private void btn_LedOff_Click(object sender, RoutedEventArgs e)
        {
            this.robot.Outputs.Leds.TurnOff();
        }

        private void btn_LedSetBasedOnSliders_Click(object sender, RoutedEventArgs e)
        {
            this.robot.Outputs.Leds.SetLedColors(CLeds.LedType.Bottom, new CLeds.SColor(Convert.ToInt32(slider_R.Value), Convert.ToInt32(slider_G.Value), Convert.ToInt32(slider_B.Value)));
            this.robot.Outputs.Leds.SetLedColors(CLeds.LedType.Chasis, new CLeds.SColor(Convert.ToInt32(slider_R.Value), Convert.ToInt32(slider_G.Value), Convert.ToInt32(slider_B.Value)));
            this.robot.Outputs.Leds.SetLedColors(CLeds.LedType.Eyes, new CLeds.SColor(Convert.ToInt32(slider_R.Value), Convert.ToInt32(slider_G.Value), Convert.ToInt32(slider_B.Value)));
        }

        #endregion

        void mainTimer_Tick(object sender, EventArgs e)
        {
            CRobot.RobotState previousState = robot.CurrentState;
            robot.UpdateOutputsBasedOnInputs();
            if(previousState != robot.CurrentState)
            {
                rtbMain.AppendText("New robot state: " + robot.CurrentState.ToString() + Environment.NewLine);
            }

            PresentButtons(robot.Inputs.Buttons.ButtonsState);
            PresentPower(robot.Inputs.Power);
            PresentMotorSpeed(robot.Outputs.Motors.SpeedRightWheel, robot.Outputs.Motors.SpeedLeftWheel);
            
            PresentServo(robot.Outputs.Servos);

            //tb_alpha.Text = robot.Kinect.resultRight.ToString("000.00");
            //tb_beta.Text = robot.Kinect.resultLeft.ToString("000.00");

            // Displaying robot state on lbl
            lblRobotState.Content = robot.CurrentState;

            //bw.RunWorkerAsync(bwCounter);
        }

        private void SequenceTest(object sender, RoutedEventArgs e)
        {
            tbTest.AppendText("Zaczynamy! \r\n");
        }

        #region KOD prezentacji

        private void SliderL_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PresentMotorSpeed((int)slider_P.Value, (int)slider_L.Value);
        }

        private void SliderP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PresentMotorSpeed((int)slider_P.Value, (int)slider_L.Value);
        }

        private void PresentButtons(bool[] buttons)
        {
            SolidColorBrush greenBrush = new SolidColorBrush();
            greenBrush.Color = Colors.GreenYellow;

            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Colors.Black;

            if (buttons[0] == true)
            {
                btn_dioda_0.Background = greenBrush;
            }
            else
            {
                btn_dioda_0.Background = blackBrush;
            }

            if (buttons[1] == true)
            {
                btn_dioda_1.Background = greenBrush;
            }
            else
            {
                btn_dioda_1.Background = blackBrush;
            }

            if (buttons[2] == true)
            {
                btn_dioda_2.Background = greenBrush;
            }
            else
            {
                btn_dioda_2.Background = blackBrush;
            }

            if (buttons[3] == true)
            {
                btn_dioda_3.Background = greenBrush;
            }
            else
            {
                btn_dioda_3.Background = blackBrush;
            }

            if (buttons[4] == true)
            {
                btn_dioda_4.Background = greenBrush;
            }
            else
            {
                btn_dioda_4.Background = blackBrush;
            }

            if (buttons[5] == true)
            {
                btn_dioda_5.Background = greenBrush;
            }
            else
            {
                btn_dioda_5.Background = blackBrush;
            }
        }

        private void PresentMotorSpeed(int speedRight, int speedLeft)
        {
            const int maxHeight = 150;
            const int maxSpeed = 100;

            if (speedRight > 0)
            {
                double height = ((double)speedRight / (double)maxSpeed) * (double)maxHeight;
                rect_mR.Visibility = Visibility.Hidden;
                rect_mR_bgr.Visibility = Visibility.Visible;
                rect_mR_bgr.Height = (int)height;
            }
            else if (speedRight < 0)
            {
                double height = ((double)-speedRight / (double)maxSpeed) * (double)maxHeight;
                rect_mR_bgr.Visibility = Visibility.Hidden;
                rect_mR.Visibility = Visibility.Visible;
                rect_mR.Height = (int)height;
            }
            else
            {
                rect_mR_bgr.Visibility = Visibility.Hidden;
                rect_mR.Visibility = Visibility.Hidden;
            }

            lbl_MR.Content = speedRight;

            if (speedLeft > 0)
            {
                double height = ((double)speedLeft / (double)maxSpeed) * (double)maxHeight;
                rect_mL.Visibility = Visibility.Hidden;
                rect_mL_bgr.Visibility = Visibility.Visible;
                rect_mL_bgr.Height = (int)height;
            }
            else if (speedLeft < 0)
            {
                double height = ((double)-speedLeft / (double)maxSpeed) * (double)maxHeight;
                rect_mL_bgr.Visibility = Visibility.Hidden;
                rect_mL.Visibility = Visibility.Visible;
                rect_mL.Height = (int)height;
            }
            else
            {
                rect_mL_bgr.Visibility = Visibility.Hidden;
                rect_mL.Visibility = Visibility.Hidden;
            }

            lbl_ML.Content = speedLeft;
        }

        private void PresentPower(CPower power)
        {
            rect_Voltage.Width = power.GetCurrentVoltageToMaxRatio() * 400.0;
            rect_Voltage.Fill = power.GetColorBasedOnStatus();

            lbl_Voltage.Content = power.Voltage.ToString("00.00");
            lbl_VoltageEnum.Content = power.Status;
        }

        private void PresentServo(CServo servo)
        {
            tb_ServoH.Text = servo.servosPosition[(int)CServo.ServoType.Head].ToString();
            
            tb_ServoL1.Text = servo.servosPosition[(int)CServo.ServoType.Left1].ToString();
            tb_ServoL2.Text = servo.servosPosition[(int)CServo.ServoType.Left2].ToString();
            tb_ServoL3.Text = servo.servosPosition[(int)CServo.ServoType.Left3].ToString();
            tb_ServoL4.Text = servo.servosPosition[(int)CServo.ServoType.Left4].ToString();

            tb_ServoR1.Text = servo.servosPosition[(int)CServo.ServoType.Right1].ToString();
            tb_ServoR2.Text = servo.servosPosition[(int)CServo.ServoType.Right2].ToString();
            tb_ServoR3.Text = servo.servosPosition[(int)CServo.ServoType.Right3].ToString();
            tb_ServoR4.Text = servo.servosPosition[(int)CServo.ServoType.Right4].ToString();
        }

        #endregion

        #region SIMULATION code
        const int SIMULATION_buttonsCount = 6;
        bool[] SIMULATION_buttons = new bool[SIMULATION_buttonsCount];

        private void btn_0_Click(object sender, RoutedEventArgs e)
        {
            SIMULATION_buttons[0] = !SIMULATION_buttons[0];
        }

        private void btn_1_Click(object sender, RoutedEventArgs e)
        {
            SIMULATION_buttons[1] = !SIMULATION_buttons[1];
        }

        private void btn_2_Click(object sender, RoutedEventArgs e)
        {
            SIMULATION_buttons[2] = !SIMULATION_buttons[2];
        }

        private void btn_3_Click(object sender, RoutedEventArgs e)
        {
            SIMULATION_buttons[3] = !SIMULATION_buttons[3];
        }

        private void btn_4_Click(object sender, RoutedEventArgs e)
        {
            SIMULATION_buttons[4] = !SIMULATION_buttons[4];
        }

        private void btn_5_Click(object sender, RoutedEventArgs e)
        {
            SIMULATION_buttons[5] = !SIMULATION_buttons[5];
        }

        #endregion
        
        #region code for movie playback
        private int currentIndex = 0;
        private TimeSpan moviePosition;

        private void TabControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (currentIndex == 0)
            {
                moviePosition = meMain.Position;
            }

            if (tcMain.SelectedIndex == 0)
            {
                meMain.Position = moviePosition;
            }

            currentIndex = tcMain.SelectedIndex;
        }
        #endregion

        #region Kinect Virtivius code
        CKinectVitruvius kinectVitruvius;
        private GestureType expectedPosture = GestureType.None;
        private GestureType previouslyExpectedPosture = GestureType.None;
        private int score = 0;

        private void GestureRecognized(object sender, GestureEventArgs e)
        {
            // Display the gesture type.
            this.vitruviusStatusBarText.Text = e.Name;

            if (this.robot.CurrentState == CRobot.RobotState.Kinect)
            {
                if (this.robot.kinectSate == CRobot.KinectState.RightUp1 || this.robot.kinectSate == CRobot.KinectState.RightUp2 || this.robot.kinectSate == CRobot.KinectState.RightUp3)
                {
                    this.vitruviusStatusBarText.Text += ", waiting for RightUp";
                    if (e.Type == GestureType.RightUp)
                    {
                        this.robot.flagKinectRequiredPostionDone = true;
                        this.vitruviusStatusBarText.Text += "Found!!!";
                    }
                }
                else
                {
                    this.vitruviusStatusBarText.Text += ", waiting for LetterSmallW";
                    if (e.Type == GestureType.LetterSmallW)
                    {
                        this.robot.flagKinectRequiredPostionDone = true;
                        this.vitruviusStatusBarText.Text += "Found!!!";
                    }
                }
            }
            else
            {
                if (e.Type == expectedPosture)
                {
                    this.score += 1;
                    this.tb_vitruviusX.Text = score.ToString();

                    this.previouslyExpectedPosture = this.expectedPosture;
                    if (this.expectedPosture == GestureType.LetterW)
                    {
                        this.expectedPosture = GestureType.HandsInTheAir;
                        this.kinectVitruvius.flagDrawLetterWSkeleton = false;
                        this.kinectVitruvius.flagDrawHandsInTheAirSkeleton = true;
                    }
                    else if (this.expectedPosture == GestureType.HandsInTheAir)
                    {
                        this.expectedPosture = GestureType.LetterW;
                        this.kinectVitruvius.flagDrawLetterWSkeleton = true;
                        this.kinectVitruvius.flagDrawHandsInTheAirSkeleton = false;
                    }
                }
            }

            // Do something according to the type of the gesture.
            switch (e.Type)
            {
                case GestureType.LetterW:
                    break;
                case GestureType.HandsInTheAir:
                    break;
                case GestureType.NoGesture:
                    break;
                case GestureType.JoinedHands:
                    break;
                case GestureType.Menu:
                    break;
                case GestureType.SwipeDown:
                    break;
                case GestureType.SwipeLeft:
                    break;
                case GestureType.SwipeRight:
                    break;
                case GestureType.SwipeUp:
                    break;
                case GestureType.WaveLeft:
                    break;
                case GestureType.WaveRight:
                    break;
                case GestureType.ZoomIn:
                    break;
                case GestureType.ZoomOut:
                    break;
                default:
                    break;
            }
        }

        private void btn_VitriuviusDrawSavedSkeleton_Click(object sender, RoutedEventArgs e)
        {
            this.kinectVitruvius.flagDrawHandsInTheAirSkeleton = true;
            this.expectedPosture = GestureType.HandsInTheAir;
        }

        private void btn_VitriviusSaveLetterWSkeleton_Click(object sender, RoutedEventArgs e)
        {
            this.kinectVitruvius.SaveLetterWSkeleton();
        }

        private void btn_VitriviusSaveHandsInTheAirSkeleton_Click(object sender, RoutedEventArgs e)
        {
            this.kinectVitruvius.SaveHandsInTheAirSkeleton();
        }

        #endregion

    }
}
