using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace LabVision
{
    public interface IObjectDetector
    {
        [NotNull]
        Task<List<DetectedObject>> DetectAsync([NotNull] CameraFrame frame);
    }
    public enum DetectionStatus
    {
        Idle,
        Detecting
    }
}
