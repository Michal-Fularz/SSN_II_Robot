﻿using System;
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

using MAF_Robot;
using System.Windows.Media;


namespace SSN_II_Robot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private CRobot robot;
        private CSettings settings;

        System.Windows.Threading.DispatcherTimer mainTimer;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            Deinit();
        }

        private void Init()
        {
            settings = new CSettings();
            settings.LoadFromFile("settings\\settings.txt");

            robot = new CRobot(settings.SerialPortName);

            robot.Inputs.Gamepad.OnDpadUp += new ButtonPressed(NextTabPage);
            robot.Inputs.Gamepad.OnDpadDown += new ButtonPressed(PreviousTabPage);
            //robot.Inputs.Gamepad.OnBack += new ButtonPressed(Exit);

            robot.Inputs.Gamepad.OnA += new ButtonPressed(PlaySampleSound);

            mainTimer = new System.Windows.Threading.DispatcherTimer();
            mainTimer.Interval = TimeSpan.FromMilliseconds(100);
            mainTimer.Tick += mainTimer_Tick;

            bw = new System.ComponentModel.BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;

            // KINECT - image 1
            robot.Kinect.Init();
            this.Image.Source = robot.Kinect.Source;
            
            // Drawing robot on image 2
            this.RobotImage.Source = robot.Kinect.Source;

            rtbMain.AppendText("Aplikacja rozpoczęta: 2013-03-29 11:37:52" + Environment.NewLine);
            rtbMain.AppendText("Robot state: " + robot.CurrentState.ToString() + Environment.NewLine); ;

            mainTimer.Start();

            //meMain.LoadedBehavior = MediaState.Manual;
        }

        private void Deinit()
        {
            robot.Dispose();
            this.bw.Dispose();
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

        private void Exit()
        {
            Deinit();
            Close();
        }

        private void PlaySampleSound()
        {
            this.robot.Outputs.Sound.Play("sound/buziak_1.wav");
        }

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

            tb_alpha.Text = robot.Kinect.resultRight.ToString("000.00");
            tb_beta.Text = robot.Kinect.resultLeft.ToString("000.00");

            // Displaying robot state on lbl
            lblRobotState.Content = robot.CurrentState;

            //bw.RunWorkerAsync(bwCounter);
        }

        #region Background worker code

        System.ComponentModel.BackgroundWorker bw;

        int bwCounter = 0;

        void bw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            int value = (int)e.Result;

            tb_gamma.Text = "Done " + value.ToString() + "!";

            bwCounter = value + 1;
        }

        void bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int value = (int)e.Argument;

            e.Result = value + 1;
        }
        
        #endregion


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

        private void PresentSound(string filename, int dlugosc)
        {
            lbl_nazwaUtworu.Content = filename;
            lbl_dlugosc.Content = dlugosc;
        }

        private void SoundButton(object sender, RoutedEventArgs e)
        {
            string filename = "Kasia Kowalska";
            int dlugosc = 15000;

            PresentSound(filename, dlugosc);
        }

        private void PresentPower(CPower power)
        {
            // TODO - move this logic to CPower class and return appropriate length and status

            /*
            double width = power.Voltage / robot.Inputs.Power.VoltageMaxValuePresentation * 400;

            rect_Voltage.Width = width;

            if (power.Status == CPower.PowerStatus.Critical)
            {
                rect_Voltage.Fill = Brushes.Red;
            }
            else if (power.Status == CPower.PowerStatus.Warning)
            {
                rect_Voltage.Fill = Brushes.Orange;
            }
            else
            {
                rect_Voltage.Fill = Brushes.Green;
            }

            lbl_Voltage.Content = power.Voltage.ToString("00.00");
            lbl_VoltageEnum.Content = power.Status;
             * */
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

        private void PresentAngle(CKinect skeleton)
        {
            tb_alpha.Text =  skeleton.degrees.X.ToString();
            tb_beta.Text = skeleton.degrees.Y.ToString();
        }

        #endregion

        #region PRZYCISKI KIERUNKOWE NA STRONACH

        private void MenuPrev_Button_Click(object sender, RoutedEventArgs e)
        {
            Tab4.IsSelected = true;
        }

        private void MenuNext_Button_Click(object sender, RoutedEventArgs e)
        {
            Tab2.IsSelected = true;
        }

        private void Tab2Prev_Button_Click(object sender, RoutedEventArgs e)
        {
            Tab1.IsSelected = true;
        }

        private void Tab2Next_Button_Click(object sender, RoutedEventArgs e)
        {
            Tab3.IsSelected = true;
        }

        private void Tab3Prev_Button_Click(object sender, RoutedEventArgs e)
        {
            Tab2.IsSelected = true;
        }

        private void Tab3Next_Button_Click(object sender, RoutedEventArgs e)
        {
            Tab4.IsSelected = true;
        }

        private void Tab4Prev_Button_Click(object sender, RoutedEventArgs e)
        {
            Tab3.IsSelected = true;
        }

        private void Tab4Next_Button_Click(object sender, RoutedEventArgs e)
        {
            Tab1.IsSelected = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            robot.Dispose();
            Close();
        }

        #endregion


        // TODO - zrobić coś z tym kodem!
        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Deinit();
        }


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
      
        #region SIMULATION code
        const int SIMULATION_buttonsCount = 6;
        bool[] SIMULATION_buttons = new bool[SIMULATION_buttonsCount];

        private void btn_0_Click(object sender, RoutedEventArgs e)
        {
            //SIMULATION_buttons[0] = !SIMULATION_buttons[0];
        }

        private void btn_1_Click(object sender, RoutedEventArgs e)
        {
            //SIMULATION_buttons[1] = !SIMULATION_buttons[1];
        }

        private void btn_2_Click(object sender, RoutedEventArgs e)
        {
            //SIMULATION_buttons[2] = !SIMULATION_buttons[2];
        }

        private void btn_3_Click(object sender, RoutedEventArgs e)
        {
            //SIMULATION_buttons[3] = !SIMULATION_buttons[3];
        }

        private void btn_4_Click(object sender, RoutedEventArgs e)
        {
            //SIMULATION_buttons[4] = !SIMULATION_buttons[4];
        }

        private void btn_5_Click(object sender, RoutedEventArgs e)
        {
            //SIMULATION_buttons[5] = !SIMULATION_buttons[5];
        }

        #endregion
  
        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left1, 50);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left2, 50);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right3, 50);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right4, 50);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left3, 50);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left4, 50);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right1, 50);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right2, 50);
           
            robot.SendServo();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // watch out for 128 value - it's stranger but this makes the leds to stop working
            robot.SendLedsBottom(196, 0, 0);
            robot.SendLedsChasis(0, 128, 0);
            robot.SendLedsEyes(0, 0, 128);
        }

        private void testButtonLed_01_Click(object sender, RoutedEventArgs e)
        {
            testLed_1.Visibility = System.Windows.Visibility.Hidden;
            System.Threading.Thread.Sleep(2000);
            testLed_1.Visibility = System.Windows.Visibility.Visible;
            System.Threading.Thread.Sleep(2000);
            testLed_1.Visibility = System.Windows.Visibility.Hidden;

            //for (int i = 0; i < 10; i++)
            //{

            //    testLed_1.Visibility = System.Windows.Visibility.Hidden;
            //    testLed_2.Visibility = System.Windows.Visibility.Hidden;
            //    testLed_3.Visibility = System.Windows.Visibility.Hidden;
                
                
            //    if (i % 2 == 0)
            //    {
            //        tbTest.AppendText("Weszlo!\r\n");
            //        testLed_1.Visibility = System.Windows.Visibility.Hidden;
            //        System.Threading.Thread.Sleep(1000);
            //    }
            //    else
            //    {
            //        tbTest.AppendText("Weszlo, ale...\r\n");
            //        testLed_1.Visibility = System.Windows.Visibility.Visible;
            //        System.Threading.Thread.Sleep(1000);
            //    }
            //}
            
        }

        private void testButtonLed_02_Click(object sender, RoutedEventArgs e)
        {
            testLed_1.Visibility = System.Windows.Visibility.Visible;
       
        }

        private void SequenceTest(object sender, RoutedEventArgs e)
        {
            tbTest.AppendText("Zaczynamy! \r\n");
        }

        /// <summary>
        /// Action for SaveData button on Spine screen, change flag to true if it's pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SaveKinect_Click(object sender, RoutedEventArgs e)
        {
            robot.Kinect.SaveDataIsPressed = true;
            if (robot.Kinect.SaveDataIsPressed)
            {
                lbl_SaveDataState.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ServoSave(object sender, RoutedEventArgs e)
        {
            
            robot.Outputs.Servos.SaveData(tb_ServoName.Text);
        }

        private void ServoReset(object sender, RoutedEventArgs e)
        {
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left1, 0);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left2, 0);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left3, 0);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Left4, 0);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right1, 0);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right2, 0);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right3, 0);
            robot.Outputs.Servos.ChangeServoPosition(CServo.ServoType.Right4, 0);
        }
    

        
    }
}
