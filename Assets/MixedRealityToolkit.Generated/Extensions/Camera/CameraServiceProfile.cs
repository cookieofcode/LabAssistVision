using System;
using LabAssistVision;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
    [MixedRealityServiceProfile(typeof(ICameraService))]
    [CreateAssetMenu(fileName = "CameraServiceProfile", menuName = "MixedRealityToolkit/CameraService Configuration Profile")]
    public class CameraServiceProfile : BaseMixedRealityProfile
    {
        /// <summary>
        /// VideoManager resolution and targeted frame rate (may not be achieved).
        /// </summary>
        [SerializeField]
        [Tooltip("The profile contains resolution and frame rate. This only affects the Locatable Camera of the HoloLens 2")]
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