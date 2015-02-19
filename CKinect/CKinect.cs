using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Kinect visualization
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Xaml;


// Kinect
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Kinect.Toolbox;
using Kinect.Toolbox.Voice;
//using Kinect.Toolbox.Record;


// Saving data
using System.IO;
using Kinect.Toolbox.Record;

namespace MAF_Robot
{
    public class CKinect
    {
        public string AngleAlpha { get; private set; }
        public string AngleBeta { get; private set; }
        public Microsoft.Xna.Framework.Vector2 degrees { get; private set; }

        public string Status { get; private set; }

        public ImageSource Source { get; private set; }

        public void Start()
        {
            this.sensor.Start();
        }

        public void Close()
        {
            if(null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        // variables for color stream
        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Result of calculating angles between shoulder, elbow and wrist
        /// </summary>
        public double resultLeft;
        public double resultRight;

        /// <summary>
        /// Save data flag
        /// </summary>
        public bool SaveDataIsPressed = false;

        KinectSensor kinectSensor;
        readonly ColorStreamManager colorManager = new ColorStreamManager();
        readonly DepthStreamManager depthManager = new DepthStreamManager();
        SkeletonDisplayManager skeletonDisplayManager;
        private Skeleton[] skeletons;
        readonly ContextTracker contextTracker = new ContextTracker();

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            
            SkeletonPoint sp = new SkeletonPoint();
            sp.X = 0.0f;
            sp.Y = 0.0f;
            sp.Z = 0.0f;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    // Send data to file
                    SaveData(SaveDataIsPressed, skeletons);
                    
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                        
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
               
        }
    }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }

            // Calculate the angles
            this.resultRight = this.FindAngles(skeleton, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight);
            this.resultLeft = this.FindAngles(skeleton, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft);
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private System.Windows.Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Windows.Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        // kinect code for color data
        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        public void Init()
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                InitSkeletonStream();
                
                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (System.IO.IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.Status = "No Kinect Ready";// Properties.Resources.NoKinectReady;
            }
        }


        private void InitColorStream()
        {
            // Turn on the color stream to receive color frames
            this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            // Allocate space to put the pixels we'll receive
            this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
            // This is the bitmap we'll display on-screen
            this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            // Set the image we display to point to the bitmap where we'll put the image data
            this.Source = this.colorBitmap;

            // Add an event handler to be called whenever there is new color frame data
            this.sensor.ColorFrameReady += this.SensorColorFrameReady;
        }

        private void InitSkeletonStream()
        {
            // Turn on the skeleton stream to receive skeleton frames
            this.sensor.SkeletonStream.Enable();

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();
            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);
            // Display the drawing using our image control
            Source = this.imageSource;

            // Add an event handler to be called whenever there is new color frame data
            this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

        }

        private void InitSkeletonColorStream()
        {
            if (kinectSensor == null)
                return;

            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.ColorFrameReady += kinectRuntime_ColorFrameReady;

            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });
            kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;
        }

        private void InitRobotDraw()
        {
            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);
            // Display the drawing using our image control
            Source = this.imageSource;

        }

        /// <summary>
        /// Calculate angles between two joints
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="jType1">Joint 1</param>
        /// <param name="jType2">Joint 2</param>
        /// <returns></returns>
        private double FindAngles(Skeleton skeleton ,JointType jType1, JointType jType2, JointType jType3 )
        {

            Joint joint1 = skeleton.Joints[jType1];
            Joint joint2 = skeleton.Joints[jType2];
            Joint joint3 = skeleton.Joints[jType3];

            Microsoft.Xna.Framework.Vector3 vectorJoint1ToJoint2 = new Microsoft.Xna.Framework.Vector3(joint1.Position.X - joint2.Position.X, joint1.Position.Y - joint2.Position.Y, 0);
            Microsoft.Xna.Framework.Vector3 vectorJoint2ToJoint3 = new Microsoft.Xna.Framework.Vector3(joint2.Position.X - joint3.Position.X, joint2.Position.Y - joint3.Position.Y, 0);
            vectorJoint1ToJoint2.Normalize();
            vectorJoint2ToJoint3.Normalize();

            Microsoft.Xna.Framework.Vector3 crossProduct = Microsoft.Xna.Framework.Vector3.Cross(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double crossProductLength = crossProduct.Z;
            double dotProduct = Microsoft.Xna.Framework.Vector3.Dot(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double segmentAngle = Math.Atan2(crossProductLength, dotProduct);

            // Convert the result to degrees.
            double degrees = segmentAngle * (180 / Math.PI);

            // Add the angular offset.  Use modulo 360 to convert the value calculated above to a range
            // from 0 to 360.
            degrees = (degrees + 0.01) % 360;

            // Calculate whether the coordinates should be reversed to account for different sides
            //if (_ReverseCoordinates)
            //{
            //    degrees = CalculateReverseCoordinates(degrees);
            //}

            //// Shoulder Right
            //Vector3 joint1 = new Vector3(skeleton.Joints[jType1].Position.X, 
            //    skeleton.Joints[jType1].Position.Y,
            //    skeleton.Joints[jType1].Position.Z);
            //// Elbow Right
            //Vector3 joint2 = new Vector3(skeleton.Joints[jType2].Position.X,
            //    skeleton.Joints[jType2].Position.Y,
            //    skeleton.Joints[jType2].Position.Z);

            //Vector3 difference = joint1 - joint2; 
                
            //double r = Math.Sqrt(Math.Pow(difference.X, 2) + Math.Pow(difference.Y, 2));

            //Vector2 radians = new Vector2((float)Math.Atan2(difference.Y, difference.X), 
            //    (float)Math.Atan2(difference.Z, r));

            //Vector3 degrees = new Vector3((float)(radians.X * (180 / Math.PI)), 
            //    (float)(radians.Y * (180 / Math.PI)), (float)0.0);

            return degrees;
        }

        private double FindAnglesShoulderElbowXY(Skeleton skeleton)
        {
            float x = skeleton.Joints[JointType.ShoulderCenter].Position.X;
            float y = skeleton.Joints[JointType.ShoulderRight].Position.Y;
            float z = skeleton.Joints[JointType.ShoulderRight].Position.Z;

            Microsoft.Xna.Framework.Vector3 vectorJoint1 = new Microsoft.Xna.Framework.Vector3(x, y, 0);

            x = skeleton.Joints[JointType.ShoulderRight].Position.X;
            y = skeleton.Joints[JointType.ShoulderRight].Position.Y;
            z = skeleton.Joints[JointType.ShoulderRight].Position.Z;

            Microsoft.Xna.Framework.Vector3 vectorJoint2 = new Microsoft.Xna.Framework.Vector3(x, y, 0);

            x = skeleton.Joints[JointType.ElbowRight].Position.X;
            y = skeleton.Joints[JointType.ElbowRight].Position.Y;
            z = skeleton.Joints[JointType.ElbowRight].Position.Z;

            Microsoft.Xna.Framework.Vector3 vectorJoint3 = new Microsoft.Xna.Framework.Vector3(x, y, 0);

            Microsoft.Xna.Framework.Vector3 vectorJoint1ToJoint2 = vectorJoint1 - vectorJoint2;
            Microsoft.Xna.Framework.Vector3 vectorJoint2ToJoint3 = vectorJoint2 - vectorJoint3;

            vectorJoint1ToJoint2.Normalize();
            vectorJoint2ToJoint3.Normalize();

            Microsoft.Xna.Framework.Vector3 crossProduct = Microsoft.Xna.Framework.Vector3.Cross(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double crossProductLength = crossProduct.Z;
            double dotProduct = Microsoft.Xna.Framework.Vector3.Dot(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double segmentAngle = Math.Atan2(crossProductLength, dotProduct);

            // Convert the result to degrees.
            double degrees = segmentAngle * (180 / Math.PI);

            // Add the angular offset.  Use modulo 360 to convert the value calculated above to a range
            // from 0 to 360.
            //degrees = (degrees + _RotationOffset) % 360;

            return degrees;
        }

        private double FindAnglesShoulderElbowYZ(Skeleton skeleton)
        {
            float x = skeleton.Joints[JointType.ShoulderRight].Position.X;
            float y = skeleton.Joints[JointType.ShoulderRight].Position.Y;
            float z = skeleton.Joints[JointType.HipCenter].Position.Z;

            Microsoft.Xna.Framework.Vector3 vectorJoint1 = new Microsoft.Xna.Framework.Vector3(z, y, 0);

            x = skeleton.Joints[JointType.ShoulderRight].Position.X;
            y = skeleton.Joints[JointType.ShoulderRight].Position.Y;
            z = skeleton.Joints[JointType.ShoulderRight].Position.Z;

            Microsoft.Xna.Framework.Vector3 vectorJoint2 = new Microsoft.Xna.Framework.Vector3(z, y, 0);

            x = skeleton.Joints[JointType.ElbowRight].Position.X;
            y = skeleton.Joints[JointType.ElbowRight].Position.Y;
            z = skeleton.Joints[JointType.ElbowRight].Position.Z;

            Microsoft.Xna.Framework.Vector3 vectorJoint3 = new Microsoft.Xna.Framework.Vector3(z, y, 0);

            Microsoft.Xna.Framework.Vector3 vectorJoint1ToJoint2 = vectorJoint1 - vectorJoint2;
            Microsoft.Xna.Framework.Vector3 vectorJoint2ToJoint3 = vectorJoint2 - vectorJoint3;

            vectorJoint1ToJoint2.Normalize();
            vectorJoint2ToJoint3.Normalize();

            Microsoft.Xna.Framework.Vector3 crossProduct = Microsoft.Xna.Framework.Vector3.Cross(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double crossProductLength = crossProduct.Z;
            double dotProduct = Microsoft.Xna.Framework.Vector3.Dot(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double segmentAngle = Math.Atan2(crossProductLength, dotProduct);

            // Convert the result to degrees.
            double degrees = segmentAngle * (180 / Math.PI);

            // Add the angular offset.  Use modulo 360 to convert the value calculated above to a range
            // from 0 to 360.
            //degrees = (degrees + _RotationOffset) % 360;

            return degrees;
        }

        private void DrawingRobot(DrawingContext drawingContext)
        {

            Skeleton skeleton = new Skeleton();

            //skeleton.ClippedEdges = FrameEdges.None;
            //skeleton.Joints[JointType.Head].Position.X = 0.0f;


            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);


            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        public void SaveData(bool flag, Skeleton[] skeletons)
        {
            // Sending joint position datas to file 
            if (flag)
            {
                using (TextWriter writer = File.CreateText(@"Joint.txt"))
                {
                    foreach (Skeleton skeleton in skeletons)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            writer.Write("Tracking ID: " + skeleton.TrackingId + " | Joint X: | Joint Y: | Joint Z: ");
                            writer.Write(Environment.NewLine);

                            foreach (Joint joint in skeleton.Joints)
                            {
                                writer.Write(joint.JointType + ": " + joint.Position.X + ";" + joint.Position.Y + ";" + joint.Position.Z + ";");
                                writer.Write(Environment.NewLine);
                            }
                        }
                    }

                    flag = false;
                }
            }
        }

        public void KinectLoaded(System.Windows.Controls.Canvas kinectCanvas, System.Windows.Controls.Image image)
        {
            //listen to any status change for Kinects
            KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;

            //loop through all the Kinects attached to this PC, and start the first that is connected without an error.
            foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            {
                if (kinect.Status == KinectStatus.Connected)
                {
                    kinectSensor = kinect;
                    break;
                }
            }

            if (KinectSensor.KinectSensors.Count == 0)
                MessageBox.Show("No Kinect found");
            else
                Initialize(kinectCanvas, image);
        }

        private void Initialize(System.Windows.Controls.Canvas kinectCanvas, System.Windows.Controls.Image image)
        {
            if (kinectSensor == null)
                return;

            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.ColorFrameReady += kinectRuntime_ColorFrameReady;

            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });
            kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;

            skeletonDisplayManager = new SkeletonDisplayManager(kinectSensor, kinectCanvas);

            kinectSensor.Start();

            image.DataContext = colorManager;

        }

        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (kinectSensor == null)
                    {
                        kinectSensor = e.Sensor;
                        //Initialize(kinectCanvas, image);
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect was disconnected");
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect is no more powered");
                    }
                    break;
                default:
                    MessageBox.Show("Unhandled Status: " + e.Status);
                    break;
            }
        }

        private void Clean()
        {
            if (kinectSensor != null)
            {
                kinectSensor.SkeletonFrameReady -= kinectRuntime_SkeletonFrameReady;
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.Stop();
                kinectSensor = null;
            }
        }

        void kinectRuntime_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return;

                colorManager.Update(frame);
            }
        }
        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                frame.GetSkeletons(ref skeletons);

                if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;

                ProcessFrame(frame);
            }
        }
        public void ProcessFrame(ReplaySkeletonFrame frame)
        {
            Dictionary<int, string> stabilities = new Dictionary<int, string>();
            foreach (var skeleton in frame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                contextTracker.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);
                stabilities.Add(skeleton.TrackingId, contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId) ? "Stable" : "Non stable");
                if (!contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId))
                    continue;

                foreach (Joint joint in skeleton.Joints)
                {
                    if (joint.TrackingState != JointTrackingState.Tracked)
                        continue;
                }
            }

            // ustawienie opcji seated (drugi argument)
            skeletonDisplayManager.Draw(frame.Skeletons, false);
        }     



    }
}
