// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/*
 * Adapted from https://github.com/microsoft/MixedReality-SpectatorView/blob/7796da6acb0ae41bed1b9e0e9d1c5c683b4b8374/src/SpectatorView.Unity/Assets/PhotoCapture/Scripts/CameraIntrinsics.cs
 * Modified by cookieofcode (cookieofcode@gmail.com)
 */

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LabAssistVision
{
    /// <summary>
    /// Contains information on camera intrinsic parameters.
    /// Note: This class wraps logic from Windows.Media.Devices.Core.CameraIntrinsics to use in Unity.
    /// </summary>
    [Serializable]
    public class CameraIntrinsic
    {
#if ENABLE_WINMD_SUPPORT
        /// <summary>
        /// Holds the <see cref="CameraIntrinsic"/> of Windows UWP to use at <see cref="UnprojectAtUnitDepth"/>.
        /// </summary>
        // TODO: https://stackoverflow.com/questions/51272055/opencv-unproject-2d-points-to-3d-with-known-depth-z
        [CanBeNull] public readonly Windows.Media.Devices.Core.CameraIntrinsics WindowsCameraIntrinsics;
#endif

        /// <summary>
        /// Gets the focal length of the camera.
        /// </summary>
        public readonly Vector2 FocalLength;

        /// <summary>
        /// Gets the image height of the camera, in pixels.
        /// </summary>
        public readonly uint ImageHeight;

        /// <summary>
        /// Gets the image width of the camera, in pixels.
        /// </summary>
        public readonly uint ImageWidth;

        /// <summary>
        /// Gets the principal point of the camera.
        /// </summary>
        public readonly Vector2 PrincipalPoint;

        /// <summary>
        /// Gets the radial distortion coefficient of the camera.
        /// </summary>
        public readonly Vector3 RadialDistortion;

        /// <summary>
        /// Gets the tangential distortion coefficient of the camera.
        /// </summary>
        public readonly Vector2 TangentialDistortion;

        /// <summary>
        /// Gets a matrix that transforms a 3D point to video frame pixel coordinates without
        /// compensating for the distortion model of the camera.The 2D point resulting from
        /// this transformation will not accurately map to the pixel coordinate in a video
        /// frame unless the app applies its own distortion compensation. This is useful
        /// for apps that choose to implement GPU-based distortion compensation instead of
        /// using UndistortPoint, which uses the CPU to compute the distortion compensation.
        /// </summary>
        public readonly Matrix4x4 UndistortedProjectionTransform;

        /// <param name="focalLength">focal length for the camera</param>
        /// <param name="imageWidth">image width in pixels</param>
        /// <param name="imageHeight">image height in pixels</param>
        /// <param name="principalPoint">principal point for the camera </param>
        /// <param name="radialDistortion">radial distortion for the camera</param>
        /// <param name="tangentialDistortion">tangential distortion for the camera</param>
        /// <param name="undistortedProjectionTransform">Undistorted projection transform for the camera</param>
        public CameraIntrinsic(
            Vector2 focalLength,
            uint imageWidth,
            uint imageHeight,
            Vector2 principalPoint,
            Vector3 radialDistortion,
            Vector2 tangentialDistortion,
            Matrix4x4 undistortedProjectionTransform)
        {
            FocalLength = focalLength;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            PrincipalPoint = principalPoint;
            RadialDistortion = radialDistortion;
            TangentialDistortion = tangentialDistortion;
            UndistortedProjectionTransform = undistortedProjectionTransform;
        }

        /// <summary>
        /// Constructor using default values.
        /// </summary>
        [Obsolete]
        public CameraIntrinsic()
        {
            FocalLength = Vector2.zero;
            ImageWidth = 0;
            ImageHeight = 0;
            PrincipalPoint = Vector2.zero;
            RadialDistortion = Vector3.zero;
            TangentialDistortion = Vector2.zero;
            UndistortedProjectionTransform = Matrix4x4.identity;
        }

        /// <summary>
        /// Constructor using default values except the project transformation.
        /// </summary>
        /// <param name="undistortedProjectionTransform">Undistorted projection transform for the camera</param>
        public CameraIntrinsic(Matrix4x4 undistortedProjectionTransform)
        {
            FocalLength = Vector2.zero;
            ImageWidth = 0;
            ImageHeight = 0;
            PrincipalPoint = Vector2.zero;
            RadialDistortion = Vector3.zero;
            TangentialDistortion = Vector2.zero;
            UndistortedProjectionTransform = undistortedProjectionTransform;
        }

        /// <summary>
        /// Copy Constructor (shallow). On Windows UWP, values of the original camera intrinsics are used.
        /// </summary>
        public CameraIntrinsic([NotNull] CameraIntrinsic intrinsic)
        {
            if (intrinsic == null) throw new ArgumentNullException(nameof(intrinsic));
#if ENABLE_WINMD_SUPPORT
            WindowsCameraIntrinsics = intrinsic.WindowsCameraIntrinsics;
            if (WindowsCameraIntrinsics == null) throw new NullReferenceException(nameof(WindowsCameraIntrinsics));
            FocalLength = new Vector2(WindowsCameraIntrinsics.FocalLength.X, WindowsCameraIntrinsics.FocalLength.Y);
            ImageWidth = WindowsCameraIntrinsics.ImageWidth;
            ImageHeight = WindowsCameraIntrinsics.ImageHeight;
            PrincipalPoint = new Vector2(WindowsCameraIntrinsics.PrincipalPoint.X, WindowsCameraIntrinsics.PrincipalPoint.Y);
            RadialDistortion = new Vector3(WindowsCameraIntrinsics.RadialDistortion.X, WindowsCameraIntrinsics.RadialDistortion.Y, WindowsCameraIntrinsics.RadialDistortion.Z);
            TangentialDistortion = new Vector2(WindowsCameraIntrinsics.TangentialDistortion.X, WindowsCameraIntrinsics.TangentialDistortion.Y);
            UndistortedProjectionTransform = WindowsCameraIntrinsics.UndistortedProjectionTransform.ToUnity();
#else
            FocalLength = intrinsic.FocalLength;
            ImageWidth = intrinsic.ImageWidth;
            ImageHeight = intrinsic.ImageHeight;
            PrincipalPoint = intrinsic.PrincipalPoint;
            RadialDistortion = intrinsic.RadialDistortion;
            TangentialDistortion = intrinsic.TangentialDistortion;
            UndistortedProjectionTransform = intrinsic.UndistortedProjectionTransform;
#endif
        }

#if ENABLE_WINMD_SUPPORT
        public CameraIntrinsic([NotNull] Windows.Media.Devices.Core.CameraIntrinsics cameraIntrinsics)
        {
            if (cameraIntrinsics == null) throw new ArgumentNullException(nameof(cameraIntrinsics));
            FocalLength = new Vector2(cameraIntrinsics.FocalLength.X, cameraIntrinsics.FocalLength.Y);
            ImageWidth = cameraIntrinsics.ImageWidth;
            ImageHeight = cameraIntrinsics.ImageHeight;
            PrincipalPoint = new Vector2(cameraIntrinsics.PrincipalPoint.X, cameraIntrinsics.PrincipalPoint.Y);
            RadialDistortion = new Vector3(cameraIntrinsics.RadialDistortion.X, cameraIntrinsics.RadialDistortion.Y, cameraIntrinsics.RadialDistortion.Z);
            TangentialDistortion = new Vector2(cameraIntrinsics.TangentialDistortion.X, cameraIntrinsics.TangentialDistortion.Y);
            UndistortedProjectionTransform = cameraIntrinsics.UndistortedProjectionTransform.ToUnity();
            WindowsCameraIntrinsics = cameraIntrinsics;
        }
        
        /// <summary>
        /// Unprojects pixel coordinates into a camera space ray from the camera origin, expressed as a X, Y coordinates on a plane one meter from the camera.
        /// </summary>
        /// <param name="pixelCoordinate">The point to unproject. Points in Windows UWP use a different coordinate system than OpenCV</param>
        public Vector2 UnprojectAtUnitDepth(Windows.Foundation.Point pixelCoordinate)
        {
            if (WindowsCameraIntrinsics == null) throw new NotImplementedException("Unprojection without UWP is not implemented yet.");
            System.Numerics.Vector2 unprojected = WindowsCameraIntrinsics.UnprojectAtUnitDepth(pixelCoordinate);
            return unprojected.ToUnity();
        }
#endif

        public override string ToString()
        {
            return $"Focal Length:{FocalLength.ToString("G4")}, Principal Point:{PrincipalPoint.ToString("G4")}, Image Width:{ImageWidth:G4}, Image Height:{ImageHeight:G4}, Radial Distortion:{RadialDistortion.ToString("G4")}, Tangential Distortion:{TangentialDistortion.ToString("G4")}";
        }
    }
}
