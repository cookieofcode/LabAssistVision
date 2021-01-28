using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit;
using OpenCVForUnity.CoreModule;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LabAssistVision
{
    /// <summary>
    /// Represents a tracked object on the video stream.
    /// </summary>
    public class TrackedObject : ICloneable
    {
        [NotNull] public Rect2d Rect;
        [NotNull] public string Label;
        [NotNull] public CameraIntrinsic Intrinsic;
        [NotNull] public CameraExtrinsic Extrinsic;
        public int FrameHeight;

        public TrackedObject([NotNull] Rect2d rect, [NotNull] string label, [NotNull] CameraIntrinsic intrinsic, [NotNull] CameraExtrinsic extrinsic, int frameHeigth)
        {
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            if (label == null) throw new ArgumentNullException(nameof(label));
            if (intrinsic == null) throw new ArgumentNullException(nameof(intrinsic));
            if (extrinsic == null) throw new ArgumentNullException(nameof(extrinsic));
            if (frameHeigth == 0) throw new ArgumentOutOfRangeException(nameof(frameHeigth));
            Label = label;
            Rect = rect;
            Intrinsic = intrinsic;
            Extrinsic = extrinsic;
            FrameHeight = frameHeigth;
        }

        public TrackedObject([NotNull] TrackedObject trackedObject)
        {
            if (trackedObject == null) throw new ArgumentNullException(nameof(trackedObject));
            if (trackedObject.FrameHeight == 0) throw new ArgumentOutOfRangeException(nameof(trackedObject.FrameHeight));
            Rect = trackedObject.Rect.clone();
            Label = string.Copy(trackedObject.Label);
            Intrinsic = new CameraIntrinsic(trackedObject.Intrinsic);
            Extrinsic = new CameraExtrinsic(trackedObject.Extrinsic);
            FrameHeight = trackedObject.FrameHeight;
        }

        public TrackedObject([NotNull] TrackedObject trackedObject, [NotNull] Rect2d rect, [NotNull] CameraFrame frame) : this(rect, trackedObject.Label, frame.Intrinsic, frame.Extrinsic, frame.Height) { }

        public TrackedObject([NotNull] DetectedObject detectedObject)
        {
            if (detectedObject == null) throw new ArgumentNullException(nameof(detectedObject));
            Rect = detectedObject.Rect;
            Label = detectedObject.Label;
            CameraFrame frame = detectedObject.Frame;
            Intrinsic = frame.Intrinsic;
            Extrinsic = frame.Extrinsic;
            FrameHeight = frame.Height;
        }

        public object Clone()
        {
            return new TrackedObject(this);
        }

        /// <summary>
        /// Algorithm to approximate the vertical point in the bounding box regarding the user's position.
        /// </summary>
        public Point GetBoundingBoxTarget()
        {
            var cameraForward = Extrinsic.viewFromWorld.GetColumn(2);
            var cameraToGroundAngle = Vector3.Angle(cameraForward, Vector3.down);
            var offsetFactor = 0f;
            if (cameraToGroundAngle <= 90)
            {
                offsetFactor = 0.5f + cameraToGroundAngle / 180;
            }
            return new Point(Rect.x + Rect.width / 2, Rect.y + Rect.height * offsetFactor);
        }

#if ENABLE_WINMD_SUPPORT
        /// <summary>
        /// OpenCV uses Row-major order (top-left is 0,0).
        /// Windows UWP uses Cartesian coordinate system (bottom left is 0,0).
        /// </summary>
        private Windows.Foundation.Point Convert(Point point)
        {
            return new Windows.Foundation.Point(point.x, FrameHeight - point.y);
        }
#endif

        /// <summary>
        /// Returns the unprojected forward vector. Fallback to default forward vector if UWP is not available.
        /// <see cref="VisualizationManager"/> may override fallback if main camera is available (only available on main thread).
        /// Adapted from https://github.com/abist-co-ltd/hololens-opencv-laserpointer/blob/master/Assets/Script/HololensLaserPointerDetection.cs.
        /// </summary>
        public Vector3 GetLayForward(Vector2 unprojectionOffset)
        {
#if ENABLE_WINMD_SUPPORT
            Windows.Foundation.Point target = Convert(GetBoundingBoxTarget());
            Vector2 unprojection = Intrinsic.UnprojectAtUnitDepth(target);
            Vector3 correctedUnprojection = new Vector3(unprojection.x + unprojectionOffset.x, unprojection.y + unprojectionOffset.y, 1.0f);
            Vector4 forward = -Extrinsic.Forward;
            Vector4 upwards = Extrinsic.Upwards;
            Quaternion rotation = Quaternion.LookRotation(forward, upwards);
            Vector3 layForward = Vector3.Normalize(rotation * correctedUnprojection);
#else
            // Fallback if using Mono. Main camera needs to be executed on main thread.
            Vector3 layForward = GetDefaultLayForward();
#endif
            if (layForward == Vector3.forward) Debug.LogWarning("Lay forward is forward vector.");
            if (layForward == Vector3.zero) Debug.LogWarning("Lay forward is zero vector.");
            return layForward;
        }

        public Vector3 GetCameraPosition()
        {
            Vector3 cameraPosition = Extrinsic.Position;
            if (cameraPosition == Vector3.forward) Debug.LogWarning("Camera position is forward vector.");
            if (cameraPosition == Vector3.zero) Debug.LogWarning("Camera position is zero vector.");
            return cameraPosition;
        }

        /// <summary>
        /// In the Unity Editor, the extrinsic, intrinsic and UWP features are not available.
        /// The forward vector of the main camera provides an approximation to use for Holographic Remoting.
        /// The main camera is only available on the main thread.
        /// </summary>
        /// <returns>The forward vector of the main camera</returns>
        public Vector3 GetDefaultLayForward()
        {
#if ENABLE_WINMD_SUPPORT
            Debug.LogWarning("Get default lay forward. This is not necessary UWP.");
#endif
            // TODO: Check and ensure main thread
            Debug.Log("Using main camera instead of unprojection.");
            var mainCamera = Camera.main;
            if (mainCamera != null) return mainCamera.transform.forward * -1f; // TODO: Check -1
            Debug.LogWarning("No main camera available. Use forward vector.");
            return Vector3.forward;
        }
    }
}