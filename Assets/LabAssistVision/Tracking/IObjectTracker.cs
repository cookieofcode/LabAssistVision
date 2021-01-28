using System.Collections.Generic;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;

namespace LabAssistVision
{
    public interface IObjectTracker
    {
        List<TrackedObject> Update([NotNull] CameraFrame frame);
        TrackedObject Initialize([NotNull] CameraFrame frame, [NotNull] Rect2d boundingBox, [NotNull] string label);
        void Reset();
    }
}
