using System;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;

namespace LabAssistVision
{
    /// <summary>
    /// Represents a detected object.
    /// </summary>
    public class DetectedObject
    {
        [NotNull] public Rect2d Rect;
        [NotNull] public string Label;
        public double Probability;
        [NotNull] public CameraFrame Frame;

        public DetectedObject([NotNull] Rect2d rect, [NotNull] string label, double probability, [NotNull] CameraFrame frame)
        {
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            if (label == null) throw new ArgumentNullException(nameof(label));
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            Rect = rect;
            Label = label;
            Probability = probability;
            Frame = frame;
        }

        public DetectedObject([NotNull] DetectedObject detectedObject)
        {
            Rect = detectedObject.Rect.clone();
            Label = detectedObject.Label;
            Probability = detectedObject.Probability;
            Frame = detectedObject.Frame;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: [Label: {Label}], [Probability: {Probability}], [Rect: x:{Rect.x}, y:{Rect.y}, width:{Rect.width}, height:{Rect.height}]";
        }
    }
}
