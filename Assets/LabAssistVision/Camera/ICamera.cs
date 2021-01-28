using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace LabAssistVision
{
    /// <summary>
    /// A camera providing video frames. The resolution (width and height) are not initialized until the camera is started.
    /// Listen to the <see cref="CameraInitialized"/> event for resolution and color format changes. Ensure to call <see cref="Initialize"/> before
    /// <see cref="StartCapture"/>. The event <see cref="FrameArrived"/> is invoked on each frame providing the <see cref="CameraFrame"/>.
    /// </summary>
    interface ICamera
    {
        /// <summary>
        /// The width of the initialized camera. Value is not set until <see cref="Initialize"/> is complete.
        /// </summary>
        int FrameWidth { get; }

        /// <summary>
        /// The height of the initialized camera. Value is not set until <see cref="Initialize"/> is complete.
        /// </summary>
        int FrameHeight { get; }

        /// <summary>
        /// Initializes the camera including <see cref="FrameWidth"/> and <see cref="FrameHeight"/>.
        /// </summary>
        Task<bool> Initialize();

        /// <summary>
        /// Starts to capture frames, the frames can be received listening to the <see cref="FrameArrived"/> event.
        /// </summary>
        Task<bool> StartCapture();

        /// <summary>
        /// Stops capturing frames.
        /// </summary>
        Task<bool> StopCapture();

        /// <summary>
        /// Invoked on each frame that is captured.
        /// </summary>
        event EventHandler<FrameArrivedEventArgs> FrameArrived;

        /// <summary>
        /// Invoked after the camera is initialized using <see cref="Initialize"/>.
        /// </summary>
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
