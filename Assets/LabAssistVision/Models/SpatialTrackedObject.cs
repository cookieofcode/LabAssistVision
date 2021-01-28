using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LabAssistVision
{
    /// <summary>
    /// Represents a tracked object reduced to its label and position in the world.
    /// </summary>
    public class SpatialTrackedObject
    {
        [NotNull] public string Label;
        public Vector3 Position;

        public SpatialTrackedObject([NotNull] string label, Vector3 position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            if (label == null) throw new ArgumentNullException(nameof(label));
            Label = label;
            Position = position;
        }

        public SpatialTrackedObject([NotNull] TrackedObject trackedObject, Vector3 position)
        {
            if (trackedObject == null) throw new ArgumentNullException(nameof(trackedObject));
            if (position == null) throw new ArgumentNullException(nameof(position));
            Label = trackedObject.Label;
            Position = position;
        }
    }
}
