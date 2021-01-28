using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;
using UnityEngine;
using UnityEngine.Assertions;

#if ENABLE_WINMD_SUPPORT
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Graphics.Imaging;
using Windows.Perception.Spatial;
using UnityEngine.XR.WSA;
using OpenCVForUnity.UtilsModule;
#endif

namespace LabAssistVision
{
    /// <summary>
    /// Provides access to the <see href="https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/locatable-camera">Locatable Camera</see> of the Microsoft HoloLens 2.
    /// </summary>
    public class LocatableCamera : ICamera
    {
        #region Member Variables
        private const string locatableCameraDisplayName = "QC Back Camera";
        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public event EventHandler<CameraInitializedEventArgs> CameraInitialized;
        public int FrameCount;
        public int FrameHeight { get; set; }
        public int FrameWidth { get; set; }
        [NotNull] private readonly Logger _logger;
        private readonly LocatableCameraProfile _cameraProfile;
        private readonly ColorFormat _format;
        private readonly Device _device = Device.HoloLens2;
        [CanBeNull] private Mat _bitmap;
        #endregion // Member Variables

        #region Constructor
        public LocatableCamera(LocatableCameraProfile cameraProfile, ColorFormat format)
        {
#if !ENABLE_WINMD_SUPPORT
            throw new InvalidOperationException("LocatableCamera is only supported on UWP. Use MonoCamera in Unity Editor.");
#endif
            _cameraProfile = cameraProfile;
            _format = format;
            _logger = new Logger(new LogHandler());
            Assert.IsNotNull(_logger, "_logger != null");
#if ENABLE_WINMD_SUPPORT
            Assert.IsNotNull(WorldOrigin, "worldOrigin != null");
#endif
        }
        #endregion // Constructor

        #region Internal Methods
        /// <summary>
        /// In order to do pixel manipulation on SoftwareBitmap images, the native memory buffer is accessed using <see cref="IMemoryBufferByteAccess"/> COM interface.
        /// The project needs to be configured to allow compilation of unsafe code. <see href="https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader"/>.
        /// </summary>
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private unsafe interface IMemoryBufferByteAccess
        {
            /// <summary>
            /// Gets a buffer as an array of bytes. <see href="https://docs.microsoft.com/de-de/windows/win32/winrt/imemorybufferbyteaccess-getbuffer"/>.
            /// </summary>
            /// <param name="value">A pointer to a byte array containing the buffer data</param>
            /// <param name="capacity">The number of bytes in the returned array</param>
            void GetBuffer(out byte* value, out uint capacity);
        }

#if ENABLE_WINMD_SUPPORT
        private MediaCapture _mediaCapture;
        private MediaFrameReader _frameReader;
        private SpatialCoordinateSystem _worldOrigin;
        private SpatialCoordinateSystem WorldOrigin
        {
            get
            {
                if (_worldOrigin == null)
                {
                    _worldOrigin = CreateWorldOrigin();
                }
                return _worldOrigin;
            }
        }

        private static SpatialCoordinateSystem CreateWorldOrigin()
        {
            //IntPtr worldOriginPtr = Microsoft.MixedReality.Toolkit.WindowsMixedReality.WindowsMixedRealityUtilities.UtilitiesProvider.ISpatialCoordinateSystemPtr;
            //WinRTExtensions.GetSpatialCoordinateSystem(coordinateSystemPtr); // https://github.com/microsoft/MixedReality-SpectatorView/blob/7796da6acb0ae41bed1b9e0e9d1c5c683b4b8374/src/SpectatorView.Unity/Assets/PhotoCapture/Scripts/WinRTExtensions.cs#L20
            IntPtr worldOriginPtr = WorldManager.GetNativeISpatialCoordinateSystemPtr();
            return RetrieveWorldOriginFromPointer(worldOriginPtr);
        }

        private static SpatialCoordinateSystem RetrieveWorldOriginFromPointer(IntPtr worldOriginPtr)
        {
            if (worldOriginPtr == IntPtr.Zero) throw new ArgumentException("World origin pointer is zero");
            SpatialCoordinateSystem spatialCoordinateSystem = Marshal.GetObjectForIUnknown(worldOriginPtr) as SpatialCoordinateSystem;
            if (spatialCoordinateSystem == null) throw new InvalidCastException("Failed to retrieve world origin from pointer");
            return spatialCoordinateSystem;
        }

        /// <summary>
        /// Retrieves the <see cref="MediaFrameSourceGroup">source group</see> using the display name of the camera.
        /// Defaults to the world-facing color camera of the HoloLens 2.
        /// </summary>
        private async Task<MediaFrameSourceGroup> SelectGroup(string displayName = locatableCameraDisplayName)
        {
            IReadOnlyList<MediaFrameSourceGroup> groups = await MediaFrameSourceGroup.FindAllAsync();
            foreach (MediaFrameSourceGroup group in groups)
            {
                if (group.DisplayName != displayName) continue;
                _logger.Log($"Selected group {group} on {_device}");
                return group;
            }
            throw new ArgumentException($"No source group for display name {displayName} found.");
        }

        /// <summary>
        /// Retrieve the device id from the display name. Defaults to the world-facing color camera of the HoloLens 2.
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        private async Task<string> GetDeviceId(string displayName = locatableCameraDisplayName)
        {
            MediaFrameSourceGroup group = await SelectGroup(displayName);
            return group.Id;
        }

        /// <summary>
        /// Initializes <see cref="MediaCapture"/> to use the world-facing locatable color camera.
        /// </summary>
        private async Task<bool> InitializeMediaCapture()
        {
            if (_mediaCapture != null)
            {
                _logger.LogWarning("Media capture already initialized");
                return false;
            }
            if (_device != Device.HoloLens2) throw new InvalidOperationException("Device not supported.");

            string deviceId = await GetDeviceId();
            IReadOnlyList<MediaCaptureVideoProfile> profiles = MediaCapture.FindKnownVideoProfiles(deviceId, KnownVideoProfile.VideoConferencing);
            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = deviceId,
                VideoProfile = profiles.First(),
                // Exclusive control is necessary to control frame-rate and resolution.
                // Note: The resolution and frame-rate of the built-in MRC camera UI might be reduced from its normal values when another app is using the photo/video camera.
                // See <see href="https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/mixed-reality-capture-for-developers"/>
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                // TODO: For RGB conversion, the MediaCaptureMemoryPreference.Auto could provide a Direct3DSurface instead of a SoftwareBitmap.
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };
            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync(settings);
            _logger.Log("Media capture successfully initialized.");
            return true;
        }

        /// <summary>
        /// Retrieves the target format specified in <see cref="CameraParameters"/> from the <see cref="MediaFrameSource">frame source</see>.
        /// </summary>
        /// <returns></returns>
        private MediaFrameFormat GetTargetFormat(MediaFrameSource frameSource, CameraParameters parameters)
        {
            MediaFrameFormat preferredFormat = frameSource.SupportedFormats.FirstOrDefault(format => CompareFormat(format, parameters));
            if (preferredFormat != null) return preferredFormat;
            _logger.LogWarning("Unable to choose the selected format, use fallback format.");
            preferredFormat = frameSource.SupportedFormats.OrderBy(x => x.VideoFormat.Width * x.VideoFormat.Height).FirstOrDefault();
            return preferredFormat;
        }

        /// <summary>
        /// Compares the target format specified in <see cref="CameraParameters"/> with a format.
        /// </summary>
        /// <returns>If the format is considered equal</returns>
        // adapted from https://github.com/microsoft/MixedReality-SpectatorView/blob/master/src/SpectatorView.Unity/Assets/PhotoCapture/Scripts/HoloLensCamera.cs
        private bool CompareFormat(MediaFrameFormat format, CameraParameters parameters)
        {
            const double epsilon = 0.00001;
            bool width = format.VideoFormat.Width == parameters.CameraResolutionWidth;
            bool height = format.VideoFormat.Height == parameters.CameraResolutionHeight;
            bool frameRate = Math.Abs((double)format.FrameRate.Numerator / (double)format.FrameRate.Denominator - parameters.FrameRate) < epsilon;
            return (width && height && frameRate);
        }

        /// <summary>
        /// Creates the frame reader using the target format and registers the <see cref="OnFrameArrived"/> event. The width is padded to be divisibly by 64.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CreateFrameReader()
        {
            const MediaStreamType mediaStreamType = MediaStreamType.VideoRecord;
            CameraParameters parameters = new CameraParameters(_cameraProfile);
            try
            {
                MediaFrameSource source = _mediaCapture.FrameSources.Values.Single(frameSource => frameSource.Info.MediaStreamType == mediaStreamType);
                MediaFrameFormat format = GetTargetFormat(source, parameters);
                await source.SetFormatAsync(format);

                _frameReader = await _mediaCapture.CreateFrameReaderAsync(source, format.Subtype);
                _frameReader.FrameArrived += OnFrameArrived;

                FrameWidth = Convert.ToInt32(format.VideoFormat.Width);
                FrameHeight = Convert.ToInt32(format.VideoFormat.Height);
                FrameWidth = PadTo64(FrameWidth);

                _logger.Log($"FrameReader initialized using {FrameWidth} x {FrameHeight}, frame rate: {format.FrameRate.Numerator} / {format.FrameRate.Denominator}, color format: {_format}");
            }
            catch (Exception exception)
            {
                _logger.LogError("Frame Reader could not be initialized");
                _logger.LogException(exception);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes the bitmap holding the camera image. Luminance (grayscale) of the NV12 format requires image height, chrominance is stored in half resolution. <see href="https://docs.microsoft.com/en-us/windows/win32/medfound/recommended-8-bit-yuv-formats-for-video-rendering#nv12"/>.
        /// </summary>
        private void InitializeBitmap()
        {
            if (_frameReader == null) throw new InvalidOperationException("Frame Reader must be initialized before creating bitmap.");
            int height = FrameHeight;
            if (_format != ColorFormat.Grayscale) height = FrameHeight * 3 / 2;
            _bitmap = new Mat(height, FrameWidth, CvType.CV_8UC1);
        }

        /// <summary>
        /// The camera of the device will be configured using the <see cref="LocatableCameraProfile"/> and <see cref="ColorFormat"/>, the pixel format is NV12.
        /// </summary>
        /// <returns>Whether video pipeline is successfully initialized</returns>
        private async Task<bool> InitializeMediaCaptureAsyncTask()
        {
            if (!await InitializeMediaCapture()) return false;
            if (!await CreateFrameReader()) return false;
            InitializeBitmap();

            _logger.Log("Media capture initialization successful");
            return true;
        }

        /// <summary>
        /// Starts the video pipeline and frame reading.
        /// </summary>
        /// <returns>Whether the frame reader is successfully started</returns>
        private async Task<bool> StartFrameReaderAsyncTask()
        {
            MediaFrameReaderStartStatus mediaFrameReaderStartStatus = await _frameReader.StartAsync();
            if (mediaFrameReaderStartStatus == MediaFrameReaderStartStatus.Success)
            {
                _logger.Log("Started Frame reader");
                return true;
            }

            _logger.LogError($"Could not start frame reader, status: {mediaFrameReaderStartStatus}");
            return false;
        }

        /// <summary>
        /// Stops the video pipeline and frame reading.
        /// </summary>
        /// <returns>Whether the video frame reader is successfully stopped</returns>
        private async Task<bool> StopFrameReaderAsyncTask()
        {
            await _frameReader.StopAsync();
            _logger.Log("Stopped frame reader");
            return true;
        }

        /// <summary>
        /// Invoked on each received video frame. Extracts the image according to the <see cref="ColorFormat"/> and invokes the <see cref="FrameArrived"/> event containing a <see cref="CameraFrame"/>.
        /// </summary>
        private unsafe void OnFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (args == null) throw new ArgumentNullException(nameof(args));
            using (MediaFrameReference frame = sender.TryAcquireLatestFrame())
            {
                if (frame == null) return;
                SoftwareBitmap originalSoftwareBitmap = frame.VideoMediaFrame?.SoftwareBitmap;
                if (originalSoftwareBitmap == null)
                {
                    _logger.LogWarning("Received frame without image.");
                    return;
                }

                CameraExtrinsic extrinsic = new CameraExtrinsic(frame.CoordinateSystem, WorldOrigin);
                CameraIntrinsic intrinsic = new CameraIntrinsic(frame.VideoMediaFrame.CameraIntrinsics);

                using (var input = originalSoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
                using (var inputReference = input.CreateReference())
                {
                    byte* inputBytes;
                    uint inputCapacity;
                    ((IMemoryBufferByteAccess)inputReference).GetBuffer(out inputBytes, out inputCapacity);
                    MatUtils.copyToMat((IntPtr)inputBytes, _bitmap);
                    int thisFrameCount = Interlocked.Increment(ref FrameCount);

                    // TODO: Check out of using block
                    CameraFrame cameraFrame = new CameraFrame(_bitmap, intrinsic, extrinsic, FrameWidth, FrameHeight, (uint)thisFrameCount, _format);
                    FrameArrivedEventArgs eventArgs = new FrameArrivedEventArgs(cameraFrame);
                    FrameArrived?.Invoke(this, eventArgs);
                }
                originalSoftwareBitmap?.Dispose();
            }
        }
#endif

        /// <summary>
        /// Pad the frame width to 64. Required for the grayscale and RGB shader.
        /// From https://github.com/qian256/HoloLensARToolKit/blob/master/HoloLensARToolKit/Assets/ARToolKitUWP/Scripts/ARUWPVideo.cs.
        /// </summary>
        /// <param name="frameWidth">The frame width to pad to 64.</param>
        private int PadTo64(int frameWidth)
        {
            if (frameWidth % 64 == 0) return frameWidth;
            int paddedFrameWidth = ((frameWidth >> 6) + 1) << 6;
            _logger.Log($"The width is padded to {paddedFrameWidth}");
            return paddedFrameWidth;
        }
        #endregion // Internal Methods

        #region Public Methods

        public async Task<bool> Initialize()
        {
#if ENABLE_WINMD_SUPPORT
            bool initialized = await InitializeMediaCaptureAsyncTask();
            CameraInitializedEventArgs args = new CameraInitializedEventArgs(FrameWidth, FrameHeight, _format);
            CameraInitialized?.Invoke(this, args);
            return initialized;
#else
            return await Task.FromResult(false);
#endif
        }
        public async Task<bool> StartCapture()
        {
#if ENABLE_WINMD_SUPPORT
            return await StartFrameReaderAsyncTask();
#else
            return await Task.FromResult(false);
#endif
        }

        public async Task<bool> StopCapture()
        {
#if ENABLE_WINMD_SUPPORT
            bool stopFrameReaderAsyncTask = await StopFrameReaderAsyncTask();
            if (!stopFrameReaderAsyncTask) return false;
            _mediaCapture.Dispose();
            _mediaCapture = null;
            return true;
#else
            return await Task.FromResult(false);
#endif
        }
        #endregion // Public Methods
    }
}
