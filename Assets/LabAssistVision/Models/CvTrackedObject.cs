using System;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;

namespace LabAssistVision
{
    /// <summary>
    /// Represents an object tracked using an <see cref="IObjectTracker">OpenCV Tracker</see>.
    /// Keeps track of the tracked object, tracker and checks if updating the tracker failed or the rectangle did not move.
    /// </summary>
    public class CvTrackedObject
    {
        [NotNull] private TrackedObject _trackedObject;
        [NotNull] private readonly Tracker _tracker;
        [NotNull] private Rect2d _lastRect;
        private int _stale;
        private readonly int staleThreshold = 200;
        private bool _update = true;

        public CvTrackedObject([NotNull] TrackedObject trackedObject, [NotNull] Tracker tracker)
        {
            if (trackedObject == null) throw new ArgumentNullException(nameof(trackedObject));
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            _lastRect = new Rect2d();
            _trackedObject = trackedObject;
            _tracker = tracker;
        }

        public bool IsOutdated()
        {
            if (!_update) return true;
            //if (_stale >= staleThreshold) return true; // TODO: Reenable stale capability
            _stale++;
            if (_lastRect != _trackedObject.Rect) _stale = 0;
            _lastRect = _trackedObject.Rect.clone();
            return false;
        }

        public void Update([NotNull] CameraFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            Rect2d rect = UpdateTracker(frame);
            if (rect == null)
            {
                _update = false;
                return;
            }
            _update = true;
            _trackedObject = new TrackedObject(_trackedObject, rect, frame);
        }

        [CanBeNull]
        private Rect2d UpdateTracker([NotNull] CameraFrame frame)
        {
            Mat image = frame.Mat;
            Rect2d rect = new Rect2d();
            bool update = _tracker.update(image, rect);
            return update ? rect : null;
        }

        [NotNull]
        public string GetLabel()
        {
            return _trackedObject.Label;
        }

        [NotNull]
        public TrackedObject GetTrackedObject()
        {
            return _trackedObject;
        }
    }
}
