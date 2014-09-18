using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Kinect visualization
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

// Kinect
using Microsoft.Kinect;

namespace MAF_Robot
{
    public class CKinect
    {
        public string AngleAlpha { get; private set; }
        public string AngleBeta { get; private set; }

        public string Status { get; private set; }

        public ImageSource Source { get; private set; }

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
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

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
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                //dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

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
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
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

        #region My Kinect code

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
                InitColorAndSkeletonStream();

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

        private void InitColorAndSkeletonStream()
        {
            // Turn on the color stream to receive color frames
            this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            // Turn on the skeleton stream to receive skeleton frames
            this.sensor.SkeletonStream.Enable();

            // Allocate space to put the pixels we'll receive
            this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
            // This is the bitmap we'll display on-screen
            this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            // Set the image we display to point to the bitmap where we'll put the image data
            this.Source = this.colorBitmap;

            this.sensor.AllFramesReady += this.ColorAndSkeletonFramesReady;
        }

        private void ColorAndSkeletonFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    Skeleton[] skeletons = new Skeleton[0];

                    using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                    {
                        if (skeletonFrame != null)
                        {
                            skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                            skeletonFrame.CopySkeletonDataTo(skeletons);
                        }
                    }

                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);

                    this.colorBitmap.Lock();

                    var bmp = new System.Drawing.Bitmap(this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight,
                                                  this.colorBitmap.BackBufferStride,
                                                  System.Drawing.Imaging.PixelFormat.Format32bppPArgb,
                                                  this.colorBitmap.BackBuffer);

                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp); // Good old Graphics

                    g.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Red), new System.Drawing.Point(5, 5), new System.Drawing.Point(50, 50)); // etc...

                    if (skeletons.Length != 0)
                    {
                        foreach (Skeleton skel in skeletons)
                        {
                            //RenderClippedEdges(skel, dc);

                            if (skel.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                //Ania_AnalizaRuchuRamion(skel, JointType.ElbowRight, JointType.WristRight, tb_alpha);
                                //Ania_AnalizaRuchuRamion(skel, JointType.ShoulderLeft, JointType.ElbowLeft, tb_beta);
                                //Ania_AnalizaRuchuRamion(skel, JointType.ElbowLeft, JointType.WristLeft, tb_gamma);

                                //tb_alpha.Text = FindAnglesShoulderElbowXY(skel).ToString("000.0");
                                //tb_beta.Text =  FindAnglesShoulderElbowYZ(skel).ToString("000.0");

                                FindAngles(skel);


                                this.DrawBonesAndJoints2(skel, g);
                            }
                            else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                            {
                                g.DrawEllipse(new System.Drawing.Pen(centerPointBrush2), (float)this.SkeletonPointToScreen(skel.Position).X, (float)this.SkeletonPointToScreen(skel.Position).Y, (float)BodyCenterThickness, (float)BodyCenterThickness);

                                //dc.DrawEllipse(this.centerPointBrush, null, this.SkeletonPointToScreen(skel.Position), BodyCenterThickness, BodyCenterThickness);
                            }
                        }
                    }

                    // ...and finally:
                    g.Dispose();
                    bmp.Dispose();
                    this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                    this.colorBitmap.Unlock();
                }
            }
        }

        private readonly System.Drawing.Pen trackedBonePen2 = new System.Drawing.Pen(System.Drawing.Brushes.Green, 6);
        private readonly System.Drawing.Pen inferredBonePen2 = new System.Drawing.Pen(System.Drawing.Brushes.Gray, 1);

        private readonly System.Drawing.Brush trackedJointBrush2 = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 68, 192, 68));
        private readonly System.Drawing.Brush inferredJointBrush2 = System.Drawing.Brushes.Yellow;

        private readonly System.Drawing.Brush centerPointBrush2 = System.Drawing.Brushes.Blue;

        private void DrawBonesAndJoints2(Skeleton skeleton, System.Drawing.Graphics graphics)
        {
            // Render Torso
            this.DrawBone2(skeleton, graphics, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone2(skeleton, graphics, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone2(skeleton, graphics, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone2(skeleton, graphics, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone2(skeleton, graphics, JointType.Spine, JointType.HipCenter);
            this.DrawBone2(skeleton, graphics, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone2(skeleton, graphics, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone2(skeleton, graphics, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone2(skeleton, graphics, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone2(skeleton, graphics, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone2(skeleton, graphics, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone2(skeleton, graphics, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone2(skeleton, graphics, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone2(skeleton, graphics, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone2(skeleton, graphics, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone2(skeleton, graphics, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone2(skeleton, graphics, JointType.HipRight, JointType.KneeRight);
            this.DrawBone2(skeleton, graphics, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone2(skeleton, graphics, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                System.Drawing.Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush2;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush2;
                }

                if (drawBrush != null)
                {
                    graphics.DrawEllipse(new System.Drawing.Pen(drawBrush), (float)this.SkeletonPointToScreen(joint.Position).X, (float)this.SkeletonPointToScreen(joint.Position).Y, (float)JointThickness, (float)JointThickness);
                    //drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        private System.Drawing.Point SkeletonPointToScreen2(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Drawing.Point(depthPoint.X, depthPoint.Y);
        }

        private void DrawBone2(Skeleton skeleton, System.Drawing.Graphics graphics, JointType jointType0, JointType jointType1)
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
            System.Drawing.Pen drawPen = this.inferredBonePen2;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen2;
            }

            graphics.DrawLine(drawPen, this.SkeletonPointToScreen2(joint0.Position), this.SkeletonPointToScreen2(joint1.Position));
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

        private double FindAngles(Skeleton skeleton)
        {
            float x1 = skeleton.Joints[JointType.ShoulderRight].Position.X;
            float y1 = skeleton.Joints[JointType.ShoulderRight].Position.Y;
            float z1 = skeleton.Joints[JointType.ShoulderRight].Position.Z;

            float x2 = skeleton.Joints[JointType.ElbowRight].Position.X;
            float y2 = skeleton.Joints[JointType.ElbowRight].Position.Y;
            float z2 = skeleton.Joints[JointType.ElbowRight].Position.Z;

            double x = x1 - x2;
            double y = y1 - y2;
            double z = z1 - z2;
            double r = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            double alpha = Math.Atan2(y, x);
            double beta = Math.Atan2(z, r);

            double alphaDegrees = alpha * (180 / Math.PI);
            double betaDegrees = beta * (180 / Math.PI);

            this.AngleAlpha = alphaDegrees.ToString("000.0");
            this.AngleBeta = betaDegrees.ToString("000.0");

            return 0;

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

        #endregion


    }
}
