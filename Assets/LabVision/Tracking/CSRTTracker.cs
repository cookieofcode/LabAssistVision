using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.UnityUtils;

namespace LabVision
{
    public class CSRTTracker : CvTracker
    {
        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame.Format != ColorFormat.RGB) throw new ArgumentException("CSRT Tracker requires RGB format (OpenCVForUnity)");
            return base.Initialize(frame, rect, label);
        }
       
        protected override Tracker CreateTracker()
        {
            Utils.setDebugMode(true);
            TrackerCSRT tracker = TrackerCSRT.create();
            Utils.setDebugMode(false);
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            return tracker;
        }

        protected override bool Initialize(Tracker tracker, Mat mat, Rect2d rect)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            Mat bgr = new Mat(mat.rows(), mat.cols(), CvType.CV_8UC3);
            Imgproc.cvtColor(mat, bgr, Imgproc.COLOR_YUV2BGR_NV12);
            Utils.setDebugMode(true);
            bool initialized = tracker.init(bgr, rect);
            Utils.setDebugMode(false);
            return initialized;
        }
    }
}
