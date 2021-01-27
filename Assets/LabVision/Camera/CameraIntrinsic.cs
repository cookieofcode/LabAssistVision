// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/*
 * Adapted from https://github.com/microsoft/MixedReality-SpectatorView/blob/7796da6acb0ae41bed1b9e0e9d1c5c683b4b8374/src/SpectatorView.Unity/Assets/PhotoCapture/Scripts/CameraIntrinsics.cs
 * Modified by cookieofcode (cookieofcode@gmail.com)
 */

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LabVision
{
    /// <summary>
    /// Contains information on camera intrinsic parameters.
    /// Note: This class wraps logic in Windows.Media.Devices.Core.CameraIntrinsics for use in Unity.
    /// </summary>
    [Serializable]
    public class CameraIntrinsic
    {
#if ENABLE_WINMD_SUPPORT
        // TODO: https://stackoverflow.com/questions/51272055/opencv-unproject-2d-points-to-3d-with-known-depth-z
        [CanBeNull] public Windows.Media.Devices.Core.CameraIntrinsics WindowsCameraIntrinsics;
#endif

        /// <summary>
        /// Gets the focal length of the camera.
        /// </summary>
        public Vector2 focalLength;

        /// <summary>
        /// Gets the image height of the camera, in pixels.
        /// </summary>
        public uint imageHeight;

        /// <summary>
        /// Gets the image width of the camera, in pixels.
        /// </summary>
        public uint imageWidth;

        /// <summary>
        /// Gets the principal point of the camera.
        /// </summary>
        public Vector2 principalPoint;

        /// <summary>
        /// Gets the radial distortion coefficient of the camera.
        /// </summary>
        public Vector3 radialDistortion;

        /// <summary>
        /// Gets the tangential distortion coefficient of the camera.
        /// </summary>
        public Vector2 tangentialDistortion;

        /// <summary>
        ///     Gets a matrix that transforms a 3D point to video frame pixel coordinates without
        ///     compensating for the distortion model of the camera.The 2D point resulting from
        ///     this transformation will not accurately map to the pixel coordinate in a video
        ///     frame unless the app applies its own distortion compensation.This is useful
        ///     for apps that choose to implement GPU-based distortion compensation instead of
        ///     using UndistortPoint, which uses the CPU to compute the distortion compensation.
        /// </summary>
        public Matrix4x4 undistortedProjectionTransform;

        /// <summary>
        /// CameraIntrinsics constructor
        /// </summary>
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
            this.focalLength = focalLength;
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.principalPoint = principalPoint;
            this.radialDistortion = radialDistortion;
            this.tangentialDistortion = tangentialDistortion;
            this.undistortedProjectionTransform = undistortedProjectionTransform;
        }

        public CameraIntrinsic()
        {
            focalLength = Vector2.zero;
            imageWidth = 0;
            imageHeight = 0;
            principalPoint = Vector2.zero;
            radialDistortion = Vector3.zero;
            tangentialDistortion = Vector2.zero;
            undistortedProjectionTransform = Matrix4x4.identity;
        }

        public CameraIntrinsic(Matrix4x4 undistortedProjectionTransform)
        {
            focalLength = Vector2.zero;
            imageWidth = 0;
            imageHeight = 0;
            principalPoint = Vector2.zero;
            radialDistortion = Vector3.zero;
            tangentialDistortion = Vector2.zero;
            this.undistortedProjectionTransform = undistortedProjectionTransform;
        }

        public CameraIntrinsic([NotNull] CameraIntrinsic intrinsic)
        {
            if (intrinsic == null) throw new ArgumentNullException(nameof(intrinsic));
#if ENABLE_WINMD_SUPPORT
            WindowsCameraIntrinsics = intrinsic.WindowsCameraIntrinsics;
            if (WindowsCameraIntrinsics == null) throw new NullReferenceException(nameof(WindowsCameraIntrinsics));
            focalLength = new Vector2(WindowsCameraIntrinsics.FocalLength.X, WindowsCameraIntrinsics.FocalLength.Y);
            imageWidth = WindowsCameraIntrinsics.ImageWidth;
            imageHeight = WindowsCameraIntrinsics.ImageHeight;
            principalPoint = new Vector2(WindowsCameraIntrinsics.PrincipalPoint.X, WindowsCameraIntrinsics.PrincipalPoint.Y);
            radialDistortion = new Vector3(WindowsCameraIntrinsics.RadialDistortion.X, WindowsCameraIntrinsics.RadialDistortion.Y, WindowsCameraIntrinsics.RadialDistortion.Z);
            tangentialDistortion = new Vector2(WindowsCameraIntrinsics.TangentialDistortion.X, WindowsCameraIntrinsics.TangentialDistortion.Y);
            undistortedProjectionTransform = WindowsCameraIntrinsics.UndistortedProjectionTransform.ToUnity();
#else
            focalLength = intrinsic.focalLength;
            imageWidth = intrinsic.imageWidth;
            imageHeight = intrinsic.imageHeight;
            principalPoint = intrinsic.principalPoint;
            radialDistortion = intrinsic.radialDistortion;
            tangentialDistortion = intrinsic.tangentialDistortion;
            undistortedProjectionTransform = intrinsic.undistortedProjectionTransform;
#endif
        }

#if ENABLE_WINMD_SUPPORT
        public CameraIntrinsic([NotNull] Windows.Media.Devices.Core.CameraIntrinsics cameraIntrinsics)
        {
            if (cameraIntrinsics == null) throw new ArgumentNullException(nameof(cameraIntrinsics));
            focalLength = new Vector2(cameraIntrinsics.FocalLength.X, cameraIntrinsics.FocalLength.Y);
            imageWidth = cameraIntrinsics.ImageWidth;
            imageHeight = cameraIntrinsics.ImageHeight;
            principalPoint = new Vector2(cameraIntrinsics.PrincipalPoint.X, cameraIntrinsics.PrincipalPoint.Y);
            radialDistortion = new Vector3(cameraIntrinsics.RadialDistortion.X, cameraIntrinsics.RadialDistortion.Y, cameraIntrinsics.RadialDistortion.Z);
            tangentialDistortion = new Vector2(cameraIntrinsics.TangentialDistortion.X, cameraIntrinsics.TangentialDistortion.Y);
            undistortedProjectionTransform = cameraIntrinsics.UndistortedProjectionTransform.ToUnity();
            WindowsCameraIntrinsics = cameraIntrinsics;
        }
        
        /// <summary>
        /// Unprojects pixel coordinates into a camera space ray from the camera origin, expressed as a X, Y coordinates on a plane one meter from the camera.
        /// Note: param needs to be point, windows has different coordinates
        /// </summary>
        public Vector2 UnprojectAtUnitDepth(Windows.Foundation.Point pixelCoordinate)
        {
            if (WindowsCameraIntrinsics == null) throw new NotImplementedException("Unprojection without UWP is not implemented yet.");
            System.Numerics.Vector2 unprojected = WindowsCameraIntrinsics.UnprojectAtUnitDepth(pixelCoordinate);
            return unprojected.ToUnity();
        }
#endif

        public override string ToString()
        {
            return $"Focal Length:{focalLength.ToString("G4")}, Principal Point:{principalPoint.ToString("G4")}, Image Width:{imageWidth:G4}, Image Height:{imageHeight:G4}, Radial Distortion:{radialDistortion.ToString("G4")}, Tangential Distortion:{tangentialDistortion.ToString("G4")}";
        }
    }
}
