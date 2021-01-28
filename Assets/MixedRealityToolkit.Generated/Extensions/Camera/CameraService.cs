using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LabAssistVision;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.Unity;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
    [MixedRealityExtensionService(SupportedPlatforms.WindowsStandalone | SupportedPlatforms.WindowsUniversal)]
    public class CameraService : BaseExtensionService, ICameraService, IMixedRealityExtensionService
    {
        #region Member Variables
        public event EventHandler<CameraInitializedEventArgs> CameraInitialized;
        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public int FrameWidth => _camera.FrameWidth;
        public int FrameHeight => _camera.FrameHeight;
        public uint FrameCount = int.MaxValue;
        public bool Initialized { get; set; }
        public ColorFormat Format => _cameraServiceProfile.format;
        CameraFrame ICameraService.CameraFrame => _frame;

        private ICamera _camera;
        private CameraFrame _frame;

        private Shader Yuv2RGBNv12Shader => _cameraServiceProfile.rgbShader;

        /// <summary>
        /// Contains the YUV2RGB_NV12 Shader required for conversion on the GPU.
        /// </summary>
        private Material _mediaMaterialRGB;
        private Texture2D _luminance;
        private Texture2D _chrominance;
        private Mat _rgb;
        [NotNull] private readonly CameraServiceProfile _cameraServiceProfile;
        private LocatableCameraProfile LocatableCameraProfile => _cameraServiceProfile.locatableCameraProfile;

        private int _isProcessingFrame;
        public bool IsProcessingFrame
        {
            get { return Interlocked.CompareExchange(ref _isProcessingFrame, 1, 1) == 1; }
            set
            {
                if (value) Interlocked.CompareExchange(ref _isProcessingFrame, 1, 0);
                else Interlocked.CompareExchange(ref _isProcessingFrame, 0, 1);
            }
        }

        /// <summary>
        /// Introduced for async RGB conversion
        /// TODO: Optimize
        /// </summary>
        private readonly object _sync = new object();
        private CameraFrame _currentCameraFrame;
        private CameraFrame CurrentCameraFrame
        {
            get
            {
                lock (_sync)
                    return _currentCameraFrame;
            }
            set
            {
                lock (_sync)
                    _currentCameraFrame = value;
            }
        }

        private bool _newFrameAvailable;
        public bool NewFrameAvailable
        {
            get
            {
                if (!_newFrameAvailable) return false;
                _newFrameAvailable = false;
                return true;

            }
            set
            {
                _newFrameAvailable = value;
            }
        }
        #endregion Member Variables

        #region Contructors
        public CameraService(string name, uint priority, BaseMixedRealityProfile profile) : base(name, priority, profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            CameraServiceProfile serviceProfile = profile as CameraServiceProfile;
            if (serviceProfile == null) throw new ArgumentNullException(nameof(serviceProfile));
            _cameraServiceProfile = serviceProfile;
            if (Yuv2RGBNv12Shader == null) throw new ArgumentNullException(nameof(Yuv2RGBNv12Shader), "No conversion shader found. Check configuration. Ensure that YUV2RGB_NV12 Shader is always included (Project Settings > Graphics > Always Included Shaders)");
#if ENABLE_WINMD_SUPPORT
            _camera = new LocatableCamera(LocatableCameraProfile, Format);
            _camera.FrameArrived += OnFrameArrived;
            _camera.CameraInitialized += OnCameraInitialized;
#else
            _camera = new MonoCamera(Format);
            _camera.FrameArrived += OnFrameArrived;
            _camera.CameraInitialized += OnCameraInitialized;
#endif
        }
        #endregion // Constructors

        #region Internal Methods
        /// <summary>
        /// Instantiates required textures and registers RGB conversion if requested.
        /// </summary>
        private void OnCameraInitialized(object sender, CameraInitializedEventArgs e)
        {
            if (Format == ColorFormat.RGB)
            {
                _rgb = new Mat(FrameHeight, FrameWidth, CvType.CV_8UC3);
                _mediaMaterialRGB = new Material(Yuv2RGBNv12Shader);
                if (_mediaMaterialRGB == null) throw new InvalidOperationException("Media Material shader not found");
                // A single-component, 8-bit unsigned-normalized-integer format that supports 8 bits for the red channel.
                _luminance = new Texture2D(FrameWidth, FrameHeight, TextureFormat.R8, false);
                // A two-component, 16-bit unsigned-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
                _chrominance = new Texture2D(FrameWidth / 2, FrameHeight / 2, TextureFormat.RG16, false);
                _mediaMaterialRGB.SetTexture("luminanceChannel", _luminance);
                _mediaMaterialRGB.SetTexture("chrominanceChannel", _chrominance);
                Camera.onPreRender += OnPreRenderCallback;
            }
            CameraInitialized?.Invoke(this, e);
            Initialized = true;
        }

        /// <summary>
        /// Starts the NV12 to RGB conversion on the GPU.
        /// </summary>
        /// <param name="camera"></param>
        private void OnPreRenderCallback(Camera camera)
        {
            if (camera != Camera.main) return;
            if (CurrentCameraFrame == null) return;
            if (FrameCount == CurrentCameraFrame.FrameCount) return;
            FrameCount = CurrentCameraFrame.FrameCount;
            CoroutineRunner.StartCoroutine(Convert(CurrentCameraFrame));
        }

        /// <summary>
        /// Updates the textures required by the conversion shader and requests the conversion on the GPU.
        /// </summary>
        /// <param name="frame"></param>
        private IEnumerator Convert(CameraFrame frame)
        {
            Mat nv12 = frame.Mat;
            Mat luminance = nv12.submat(0, FrameHeight, 0, FrameWidth);
            Mat chrominance = nv12.submat(FrameHeight, nv12.height(), 0, nv12.width());
            Utils.fastMatToTexture2D(luminance, _luminance);
            Utils.fastMatToTexture2D(chrominance, _chrominance);
            var rt = RenderTexture.GetTemporary(FrameWidth, FrameHeight, 0, GraphicsFormat.R8G8B8A8_UNorm);
            Graphics.Blit(null, rt, _mediaMaterialRGB);
            yield return new WaitForEndOfFrame();
            AsyncGPUReadback.Request(rt, 0, TextureFormat.RGB24, OnCompleteReadback);
            RenderTexture.ReleaseTemporary(rt);
        }

        /// <summary>
        /// Invoked if the NV12 to RGB conversion is complete and the data is ready to be read to the CPU.
        /// </summary>
        private void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.LogError("GPU readback error");
                return;
            }

            MatUtils.copyToMat(request.GetData<uint>(), _rgb);
            Core.flip(_rgb, _rgb, 0); // image is flipped on x-axis
            CameraFrame newFrame = new CameraFrame(_rgb, CurrentCameraFrame.Intrinsic, CurrentCameraFrame.Extrinsic, CurrentCameraFrame.Width, CurrentCameraFrame.Height, CurrentCameraFrame.FrameCount, Format);
            FrameArrivedEventArgs args = new FrameArrivedEventArgs(newFrame);
            _frame = newFrame;
            FrameArrived?.Invoke(this, args);
            FPSUtils.VideoTick();
            NewFrameAvailable = true;
            IsProcessingFrame = false;
        }

        /// <summary>
        /// Depending on color format, the frame is passed on or buffered for conversion.
        /// </summary>
        private void OnFrameArrived(object sender, FrameArrivedEventArgs e)
        {
            if (Format == ColorFormat.Grayscale)
            {
                _frame = e.Frame;
                FPSUtils.VideoTick();
                FrameArrived?.Invoke(this, e);
            }
            else
            {
                if (IsProcessingFrame) return;
                IsProcessingFrame = true;
                CurrentCameraFrame = e.Frame;
            }
        }

        #endregion // Internal Methods

        #region Public Methods
        /// <summary>
        /// Stops the camera and creates a new <see cref="ICamera">camera</see> with a new profile and format.
        /// </summary>
        public async void ChangeVideoParameter(LocatableCameraProfile profile, ColorFormat format)
        {
            await _camera.StopCapture();
            _cameraServiceProfile.format = format;
            _camera.FrameArrived -= OnFrameArrived;
            _camera.CameraInitialized -= OnCameraInitialized;
#if ENABLE_WINMD_SUPPORT
            _camera = new LocatableCamera(profile, format);
#else
            _camera = new MonoCamera(format);
#endif
            _camera.FrameArrived += OnFrameArrived;
            _camera.CameraInitialized += OnCameraInitialized;
            await _camera.Initialize();
            await _camera.StartCapture();
        }

        public async Task<bool> StartCapture()
        {
            if (!Initialized) await _camera.Initialize();
            return await _camera.StartCapture();
        }

        public async Task<bool> StopCapture()
        {
            return await _camera.StopCapture();
        }
        #endregion // Public Methods
    }
}
