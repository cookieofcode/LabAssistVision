using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.VideoModule;
using Rect = OpenCVForUnity.CoreModule.Rect;

namespace LabAssistVision
{
    /// <summary>
    /// Experimental. Mean Shift Tracker from: https://docs.opencv.org/4.5.0/d7/d00/tutorial_meanshift.html.
    /// Warning: Requires to override methods in Tracker, add virtual to class files.
    /// </summary>
    /*
    public class CamShiftTracker : CvTracker
    {

        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {

            if (frame.Format != ColorFormat.RGB) throw new ArgumentException("Only RGB supported");
            //if (frame.Format != ColorFormat.Grayscale) Debug.LogWarning("Boosting Tracker uses Grayscale");
            return base.Initialize(frame, rect, label);
        }

        protected override Tracker CreateTracker()
        {
            TrackerCamShift tracker = new TrackerCamShift();
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            return tracker;
        }

        protected override bool Initialize(Tracker tracker, Mat mat, Rect2d rect)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            bool initialized = tracker.init(mat, rect);
            return initialized;
        }
    }
    public class TrackerCamShift : Tracker
    {
        protected internal TrackerCamShift(IntPtr addr) : base(addr)
        {
            throw new NotSupportedException();
        }

        public TrackerCamShift() : base(new IntPtr())
        {

        }

        private TermCriteria term_crit;
        private MatOfInt channels;
        private Rect track_window;
        private Mat roi_hist;
        private MatOfFloat range;
        public override bool init(Mat frame, Rect2d rect)
        {
            Utils.setDebugMode(true, true);
            return init(frame, new Rect((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
        }

        private bool init(Mat frame, Rect rect)
        {
            Utils.setDebugMode(true, true);
            //Mat hsv_roi = new Mat(roi.rows(), roi.cols(), CvType.CV_8UC3);
            Mat hsv_roi = new Mat();
            Mat mask = new Mat();
            track_window = rect;
            Mat roi = new Mat(frame, track_window);
            Imgproc.cvtColor(roi, hsv_roi, Imgproc.COLOR_RGB2HSV);
            //Core.inRange(hsv_roi, new Scalar(0, 60, 32), new Scalar(180, 255, 255), mask);
            Core.inRange(hsv_roi, new Scalar(0, 0, 0), new Scalar(255, 255, 255), mask);

            range = new MatOfFloat(0, 180);
            roi_hist = new Mat();
            MatOfInt histSize = new MatOfInt(180);
            channels = new MatOfInt(0);
            List<Mat> hsv_roi_list = new List<Mat>();
            hsv_roi_list.Add(hsv_roi);
            Imgproc.calcHist(hsv_roi_list, channels, mask, roi_hist, histSize, range, false);
            Core.normalize(roi_hist, roi_hist, 0, 255, Core.NORM_MINMAX);

            term_crit = new TermCriteria(TermCriteria.EPS | TermCriteria.COUNT, 10, 1);
            return true;
        }

        public override bool update(Mat frame, Rect2d rect)
        {
            Utils.setDebugMode(true, true);
            Mat hsv = new Mat();
            Mat dst = new Mat();
            Imgproc.cvtColor(frame, hsv, Imgproc.COLOR_RGB2HSV);
            List<Mat> hsv_list = new List<Mat> {hsv};
            Imgproc.calcBackProject(hsv_list, channels, roi_hist, dst, range, 1);
            //Video.meanShift(dst, track_window, term_crit);
            RotatedRect rot_rect = Video.CamShift(dst, track_window, term_crit);
            Rect boundingRect = rot_rect.boundingRect();
            rect.x = boundingRect.x;
            rect.y = boundingRect.y;
            rect.height = boundingRect.height;
            rect.width = boundingRect.width;
            return true;
        }
    }
    */
}
