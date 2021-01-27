using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LabVision;
using Microsoft.SqlServer.Server;
#if ENABLE_WINMD_SUPPORT
using Windows.Media.Devices.Core;
using Windows.Perception.Spatial;
#endif
using OpenCVForUnity.CoreModule;
using UnityEngine;

namespace LabVision
{
    interface ICamera
    {
        int frameWidth { get; }
        int frameHeight { get; }
        Task<bool> Initialize();
        Task<bool> StartCapture();
        Task<bool> StopCapture();
        event EventHandler<FrameArrivedEventArgs> FrameArrived;
        event EventHandler<CameraInitializedEventArgs> CameraInitialized;
    }

    public class FrameArrivedEventArgs
    {
        [NotNull] public CameraFrame Frame;

        public FrameArrivedEventArgs([NotNull] CameraFrame frame)
        {
            Frame = frame;
        }
    }
    public class CameraInitializedEventArgs
    {
        public int FrameWidth;
        public int FrameHeight;
        public ColorFormat Format;

        public CameraInitializedEventArgs(int width, int height, ColorFormat format)
        {
            FrameWidth = width;
            FrameHeight = height;
            Format = format;
        }
    }
}
