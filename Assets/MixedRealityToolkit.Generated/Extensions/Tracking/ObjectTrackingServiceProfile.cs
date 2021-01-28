using System;
using LabAssistVision;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
    [MixedRealityServiceProfile(typeof(IObjectTrackingService))]
    [CreateAssetMenu(fileName = "ObjectTrackingServiceProfile", menuName = "MixedRealityToolkit/ObjectTrackingService Configuration Profile")]
    public class ObjectTrackingServiceProfile : BaseMixedRealityProfile
    {
        /// <summary>
        /// Offset of unprojected pixel coordinates.
        /// </summary>
        [Tooltip("Set offset of unprojected pixel coordinates. Offset unit is meter.")]
        public Vector2 unprojectionOffset = new Vector2(0, 0);

        /// <summary>
        /// The Tracker to use
        /// </summary>
        [Tooltip("Select the tracker to use. MOSSE performs best regarding to speed.")]
        public Tracker tracker;

        [Tooltip("Enforces a fixed amount of trackers. Used for performance measurement.")]
        public bool forceFixedTrackerCount;
        [Tooltip("Specifies the amount of fixed trackers. Only works in combination with forceFixedTrackerCount.")]
        public int fixedTrackerCount;
    }

    public enum Tracker
    {
        MosseTracker,
        BoostingTracker,
        CSRTTracker,
        KCFTracker,
        MedianFlowTracker,
        TLDTracker,
        MILTracker,
        TestTracker
    }
}