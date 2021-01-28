using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabAssistVision
{
    /// <summary>
    /// Provides the camera parameters including height, width and frame rate according to the selected profile.
    /// </summary>
    public class CameraParameters
    {
        /// <summary>
        /// A valid height resolution for use with the camera.
        /// </summary>
        public int CameraResolutionHeight;

        /// <summary>
        /// A valid width resolution for use with the camera.
        /// </summary>
        public int CameraResolutionWidth;

        /// <summary>
        /// The frame rate at which to capture video.
        /// </summary>
        public float FrameRate;

        public CameraParameters(LocatableCameraProfile profile)
        {
            switch (profile)
            {
                case LocatableCameraProfile.HL2_424x240_15:
                    CameraResolutionWidth = 424;
                    CameraResolutionHeight = 240;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_424x240_30:
                    CameraResolutionWidth = 424;
                    CameraResolutionHeight = 240;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_500x282_15:
                    CameraResolutionWidth = 500;
                    CameraResolutionHeight = 282;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_500x282_30:
                    CameraResolutionWidth = 500;
                    CameraResolutionHeight = 282;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_640x360_15:
                    CameraResolutionWidth = 640;
                    CameraResolutionHeight = 360;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_640x360_30:
                    CameraResolutionWidth = 640;
                    CameraResolutionHeight = 360;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_760x428_15:
                    CameraResolutionWidth = 760;
                    CameraResolutionHeight = 428;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_760x428_30:
                    CameraResolutionWidth = 760;
                    CameraResolutionHeight = 428;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_960x540_15:
                    CameraResolutionWidth = 960;
                    CameraResolutionHeight = 540;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_960x540_30:
                    CameraResolutionWidth = 960;
                    CameraResolutionHeight = 540;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_1128x636_15:
                    CameraResolutionWidth = 1128;
                    CameraResolutionHeight = 636;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_1128x636_30:
                    CameraResolutionWidth = 1128;
                    CameraResolutionHeight = 636;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_1280x720_15:
                    CameraResolutionWidth = 1280;
                    CameraResolutionHeight = 720;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_1280x720_30:
                    CameraResolutionWidth = 1280;
                    CameraResolutionHeight = 720;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_1504x846_5:
                    CameraResolutionWidth = 1504;
                    CameraResolutionHeight = 846;
                    FrameRate = 5.0f;
                    break;
                case LocatableCameraProfile.HL2_1504x846_10:
                    CameraResolutionWidth = 1504;
                    CameraResolutionHeight = 846;
                    FrameRate = 5.0f;
                    break;
                case LocatableCameraProfile.HL2_1504x846_15:
                    CameraResolutionWidth = 1504;
                    CameraResolutionHeight = 846;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_1504x846_30:
                    CameraResolutionWidth = 1504;
                    CameraResolutionHeight = 846;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_1504x846_60:
                    CameraResolutionWidth = 1504;
                    CameraResolutionHeight = 846;
                    FrameRate = 60.0f;
                    break;
                case LocatableCameraProfile.HL2_1920x1080_15:
                    CameraResolutionWidth = 1920;
                    CameraResolutionHeight = 1080;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_1920x1080_30:
                    CameraResolutionWidth = 1920;
                    CameraResolutionHeight = 1080;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_1952x1100_15:
                    CameraResolutionWidth = 1952;
                    CameraResolutionHeight = 1100;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_1952x1100_30:
                    CameraResolutionWidth = 1952;
                    CameraResolutionHeight = 1100;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_1952x1100_60:
                    CameraResolutionWidth = 1952;
                    CameraResolutionHeight = 1100;
                    FrameRate = 60.0f;
                    break;
                case LocatableCameraProfile.HL2_896x504_15:
                    CameraResolutionWidth = 896;
                    CameraResolutionHeight = 504;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_896x504_30:
                    CameraResolutionWidth = 896;
                    CameraResolutionHeight = 504;
                    FrameRate = 30.0f;
                    break;
                case LocatableCameraProfile.HL2_2272x1278_15:
                    CameraResolutionWidth = 2272;
                    CameraResolutionHeight = 1278;
                    FrameRate = 15.0f;
                    break;
                case LocatableCameraProfile.HL2_2272x1278_30:
                    CameraResolutionWidth = 2272;
                    CameraResolutionHeight = 1278;
                    FrameRate = 30.0f;
                    break;
                default:
                    throw new ArgumentException("Parameter not supported");
            }
        }
    }

    /// <summary>
    /// Supported camera profiles for the <see href="https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/locatable-camera">Locatable Camera of the Microsoft HoloLens 2</see>.
    /// </summary>
    public enum LocatableCameraProfile
    {
        HL2_424x240_15,
        HL2_424x240_30,
        HL2_500x282_15,
        HL2_500x282_30,
        HL2_640x360_15,
        HL2_640x360_30,
        HL2_760x428_15,
        HL2_760x428_30,
        HL2_896x504_15,
        HL2_896x504_30,
        HL2_960x540_15,
        HL2_960x540_30,
        HL2_1128x636_15,
        HL2_1128x636_30,
        HL2_1280x720_15,
        HL2_1280x720_30,
        HL2_1504x846_5,
        HL2_1504x846_10,
        HL2_1504x846_15,
        HL2_1504x846_30,
        HL2_1504x846_60,
        HL2_1920x1080_15,
        HL2_1920x1080_30,
        HL2_1952x1100_15,
        HL2_1952x1100_30,
        HL2_1952x1100_60,
        HL2_2272x1278_15,
        HL2_2272x1278_30
    }
}
