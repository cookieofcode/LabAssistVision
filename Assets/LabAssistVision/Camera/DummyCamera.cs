using System;
using System.Threading.Tasks;

namespace LabAssistVision
{
    /// <summary>
    /// Camera not providing any information. All tasks will always succeed. The <see cref="FrameArrived"/> and <see cref="CameraInitialized"/> event are never invoked.
    /// </summary>
    public class DummyCamera : ICamera
    {
        public DummyCamera(int frameWidth, int frameHeight)
        {
            FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;
        }

        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public event EventHandler<CameraInitializedEventArgs> CameraInitialized;

        public int FrameWidth { get; }
        public int FrameHeight { get; }

        public async Task<bool> Initialize()
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> StartCapture()
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> StopCapture()
        {
            return await Task.FromResult(true);
        }
    }
}
