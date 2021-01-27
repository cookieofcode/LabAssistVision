using System;
using LabVision;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
	[MixedRealityServiceProfile(typeof(ICameraService))]
	[CreateAssetMenu(fileName = "CameraServiceProfile", menuName = "MixedRealityToolkit/CameraService Configuration Profile")]
	public class CameraServiceProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        /// <summary>
        /// VideoManager resolution and targeted frame rate (may not be achieved).
        /// </summary>
        [Tooltip("The video parameter needs to match the target device.\nFor HL2, it is recommended to use 1504x846x60")]
        public LocatableCameraProfile locatableCameraProfile = LocatableCameraProfile.HL2_1504x846_60;

        /// <summary>
        /// The YUV2RGB_NV12 Shader. Is assigned here to enforce the inclusion in the build.
        /// If using Shader.Find("Unlit/NV12"), add the shader to Graphics > Always Included Shaders
        /// </summary>
        public Shader rgbShader;

        /// <summary>
        /// The Grayscale_MRTK Shader. Is assigned here to enforce the inclusion in the build.
        /// If using Shader.Find("Unlit/GrayScale_MRTK"), add the shader to Graphics > Always Included Shaders
        /// </summary>
        public Shader luminanceShader;

        public ColorFormat format;
    }
}