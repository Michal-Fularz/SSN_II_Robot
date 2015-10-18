using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Kinect Vitruvius
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.WPF;

// Kinect
using Microsoft.Kinect;

// Kinect visualization
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace MAF_Robot
{
    public class CKinectVitruvius
    {
        public Image ImageToDrawOn { get; private set; }
        public Canvas CanvasToDrawOn { get; private set; }

        private GestureController _gestureController;

        private bool flagSaveLetterWSkeleton = false;
        private bool flagSaveHandsInTheAirSkeleton = false;
        public bool flagDrawLetterWSkeleton = false;
        public bool flagDrawHandsInTheAirSkeleton = false;

        private Skeleton LetterWSkeleton;
        private Skeleton HandInTheAirSkeleton;

        /// <summary>
        /// Occurs when a gesture is recognized.
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        public CKinectVitruvius(Image imageToDrawOn, Canvas canvasToDrawOn)
        {
            this.ImageToDrawOn = imageToDrawOn;
            this.CanvasToDrawOn = canvasToDrawOn;
        }

        public void Init()
        {
            //KinectSensor sensor = SensorExtensions.Default();

            KinectSensor sensor = null;
            //loop through all the Kinects attached to this PC, and start the first that is connected without an error.
            foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            {
                if (kinect.Status == KinectStatus.Connected)
                {
                    sensor = kinect;
                    break;
                }
            }


            if (sensor != null)
            {
                sensor.EnableAllStreams();
                sensor.ColorFrameReady += Sensor_ColorFrameReady;
                //sensor.DepthFrameReady += Sensor_DepthFrameReady;
                sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;

                _gestureController = new GestureController(GestureType.None);
                _gestureController.AddGesture(GestureType.HandsInTheAir);
                _gestureController.AddGesture(GestureType.LetterW);
                _gestureController.AddGesture(GestureType.LetterSmallW);
                _gestureController.AddGesture(GestureType.RightUp);
                _gestureController.AddGesture(GestureType.NoGesture);
                _gestureController.GestureRecognized += GestureController_GestureRecognized;

                sensor.Start();
            }
        }

        void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    this.ImageToDrawOn.Source = frame.ToBitmap();
                }
            }
        }

        void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    
                    {
                        this.ImageToDrawOn.Source = frame.ToBitmap();
                    }
                }
            }
        }

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    this.CanvasToDrawOn.ClearSkeletons();
                    //tblHeights.Text = string.Empty;

                    var skeletons = frame.Skeletons().Where(s => s.TrackingState == SkeletonTrackingState.Tracked);

                    foreach (var skeleton in skeletons)
                    {
                        if (skeleton != null)
                        {
                            if (this.flagSaveLetterWSkeleton)
                            {
                                this.LetterWSkeleton = DeepClone<Skeleton>(skeleton);
                                this.flagSaveLetterWSkeleton = false;
                            }
                            if (this.flagSaveHandsInTheAirSkeleton)
                            {
                                this.HandInTheAirSkeleton = DeepClone<Skeleton>(skeleton);
                                this.flagSaveHandsInTheAirSkeleton = false;
                            }

                            // Update skeleton gestures.
                            _gestureController.Update(skeleton);

                            // Draw skeleton.
                            this.CanvasToDrawOn.DrawSkeleton(skeleton);

                            if (this.flagDrawLetterWSkeleton && this.LetterWSkeleton != null)
                            {
                                this.CanvasToDrawOn.DrawSkeleton(this.LetterWSkeleton, Color.FromRgb(0, 255, 0));
                            }
                            if (this.flagDrawHandsInTheAirSkeleton && this.HandInTheAirSkeleton != null)
                            {
                                this.CanvasToDrawOn.DrawSkeleton(this.HandInTheAirSkeleton, Color.FromRgb(0, 255, 0));
                            }

                            // Display user height.
                            //tblHeights.Text += string.Format("\nUser {0}: {1}cm", skeleton.TrackingId, skeleton.Height());
                        }
                    }
                }
            }
        }

        public void SaveLetterWSkeleton()
        {
            this.flagSaveLetterWSkeleton = true;
        }

        public void SaveHandsInTheAirSkeleton()
        {
            this.flagSaveHandsInTheAirSkeleton = true;
        }

        void GestureController_GestureRecognized(object sender, GestureEventArgs e)
        {
            this.GestureRecognized(sender, e);
        }

    }
}
