using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace LabVision
{
    interface ICamera
    {
        int FrameWidth { get; }
        int FrameHeight { get; }
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
