// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

/*
 * Adapted from https://github.com/microsoft/MixedReality-SpectatorView/blob/7796da6acb0ae41bed1b9e0e9d1c5c683b4b8374/src/SpectatorView.Unity/Assets/PhotoCapture/Scripts/CameraExtrinsics.cs
 * Modified by cookieofcode (cookieofcode@gmail.com)
 */

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LabVision
{
    /// <summary>
    /// Provides the extrinsic, including the view from the world, of the camera.
    /// </summary>
    [Serializable]
    public class CameraExtrinsic
    {

        /// <summary>
        /// Camera's view from world matrix
        /// </summary>
        public Matrix4x4 viewFromWorld;

        public CameraExtrinsic(Matrix4x4 viewFromWorld)
        {
            this.viewFromWorld = viewFromWorld;
        }

        public CameraExtrinsic(CameraExtrinsic extrinsic)
        {
            viewFromWorld = extrinsic.viewFromWorld;
        }

#if ENABLE_WINMD_SUPPORT
        public CameraExtrinsic([NotNull] Windows.Perception.Spatial.SpatialCoordinateSystem cameraCoordinateSystem, [NotNull] Windows.Perception.Spatial.SpatialCoordinateSystem worldOrigin)
        {
            if (cameraCoordinateSystem == null) throw new ArgumentNullException(nameof(cameraCoordinateSystem));
            if (cameraCoordinateSystem == null) throw new ArgumentNullException(nameof(worldOrigin));
            System.Numerics.Matrix4x4? viewFromWorld = cameraCoordinateSystem.TryGetTransformTo(worldOrigin);
            //if (!viewFromWorld.HasValue) throw new InvalidOperationException("Could not retrieve extrinsics.");
            if (!viewFromWorld.HasValue)
            {
                Debug.LogWarning("Could no retrieve view from world. Fallback to identity");
                this.viewFromWorld = Matrix4x4.identity;
                return;
            }
            this.viewFromWorld = viewFromWorld.Value.ToUnity();
        }
#endif

        public override string ToString()
        {
            Vector3 position = viewFromWorld.GetColumn(3);
            Quaternion rotation = Quaternion.LookRotation(viewFromWorld.GetColumn(2), viewFromWorld.GetColumn(1));
            return $"Position: {position.ToString("G4")}, Rotation: {rotation.eulerAngles.ToString("G4")}";
        }
    }
}
