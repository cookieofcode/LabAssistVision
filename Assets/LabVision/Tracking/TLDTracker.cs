using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;

namespace LabVision
{
    public class TLDTracker : CvTracker
    {

        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame.Format != ColorFormat.Grayscale) Debug.LogWarning("TLD Tracker works with grayscale, is RGB");
            return base.Initialize(frame, rect, label);
        }
        protected override Tracker CreateTracker()
        {
            Utils.setDebugMode(true);
            TrackerTLD tracker = TrackerTLD.create();
            Utils.setDebugMode(false);
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            return tracker;
        }

        protected override bool Initialize(Tracker tracker, Mat mat, Rect2d rect)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            Utils.setDebugMode(true);
            bool initialized = tracker.init(mat, rect);
            Utils.setDebugMode(false);
            return initialized;
        }
    }
}
