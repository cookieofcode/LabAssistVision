using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LabAssistVision;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
	public interface ICameraService : IMixedRealityExtensionService
	{
        event EventHandler<FrameArrivedEventArgs> FrameArrived;
        event EventHandler<CameraInitializedEventArgs> CameraInitialized;
        int FrameWidth { get; }
        int FrameHeight { get; }
        [CanBeNull] CameraFrame CameraFrame { get; }
        Task<bool> StartCapture();
        Task<bool> StopCapture();
        bool Initialized { get; }
        ColorFormat Format { get; }
        void ChangeVideoParameter(LocatableCameraProfile profile, ColorFormat format);
    }
}