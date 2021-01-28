using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using UnityEngine;
using UnityEngine.Assertions;

namespace LabAssistVision
{
    /// <summary>
    /// Experimental. Retrieves an ARGB image from the WebCam texture. Does not provide intrinsic or extrinsic.
    /// The YUV conversion via OpenCV is not in NV12 image format. But it works for luminancy only (grayscale).
    /// </summary>
    [Obsolete("Use MonoCamera")]
    [DisallowMultipleComponent]
    public class WebcamTextureCamera : MonoBehaviour, ICamera
    {

        private int _targetVideoWidth, _targetVideoHeight;
        public int paddedFrameWidth;
        public int FrameHeight { get; set; }
        public int FrameWidth { get; set; }


        private bool _available;

        [NotNull] private WebCamTexture _cameraTexture;
        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull] private Logger _logger;

        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public event EventHandler<CameraInitializedEventArgs> CameraInitialized;

        public void Start()
        {
            _logger = new Logger(new LogHandler());
            Assert.IsNotNull(_logger, "_logger != null");
        }

        public async Task<bool> Initialize()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            _logger.Log($"Found {devices.Length} devices.");
            if (devices.Length == 0) return false;

            WebCamDevice device = devices[0];
            foreach (WebCamDevice webcamDevice in devices)
            {
                //if (webcamDevice.name == "OBS Virtual Camera")
                //if (webcamDevice.name == "OBS-Camera")
                if (webcamDevice.name == "Integrated Camera")
                {
                    device = webcamDevice;
                }

            }

            // default
            _targetVideoWidth = 1280;
            _targetVideoHeight = 720;

            _cameraTexture = new WebCamTexture(device.name, _targetVideoWidth, _targetVideoHeight);
            if (_cameraTexture == null)
            {
                _logger.LogError("Could not create camera texture.");
            };
            _logger.Log($"Selected {device.name}");
            //Debug.LogFormat("Available resolutions: {0}", string.Join(", ", devices[0].availableResolutions));

            FrameWidth = Convert.ToInt32(_targetVideoWidth);
            FrameHeight = Convert.ToInt32(_targetVideoHeight);

            CameraInitializedEventArgs args = new CameraInitializedEventArgs(FrameWidth, FrameHeight, ColorFormat.Unknown);
            CameraInitialized?.Invoke(this, args);
            return await Task.FromResult(true);
        }

        public async Task<bool> StartCapture()
        {
            _cameraTexture.Play();
            paddedFrameWidth = _cameraTexture.width % 64 != 0 ? ((_cameraTexture.width >> 6) + 1) << 6 : _cameraTexture.width;
            //FrameHeight = _cameraTexture.height;
            //FrameWidth = _cameraTexture.width;
            _available = true;
            //Debug.Log(GraphicsFormatUtility.GetFormatString(_cameraTexture.graphicsFormat));
            return await Task.FromResult(true);
        }

        public async Task<bool> StopCapture()
        {
            _cameraTexture.Stop();
            return await Task.FromResult(true);
        }

        public void Update()
        {
            if (!_available) return;
            if (!_cameraTexture.didUpdateThisFrame) return;
            Color32[] pixels32 = _cameraTexture.GetPixels32();
            Utils.setDebugMode(true);
            Mat argbMat = new Mat(_targetVideoHeight, _targetVideoWidth, CvType.CV_8UC4);
            MatUtils.copyToMat(pixels32, argbMat);
            if (argbMat.empty()) return;
            // workaround obs cam: drop frame if grey / empty.
            double[] values = argbMat.get(0, 0);
            if (values[0] == 128 && values[1] == 129 && values[2] == 127 && values[3] == 255)
                return;
            Mat yuvMat = new Mat(_targetVideoHeight * 2 / 3, _targetVideoWidth, CvType.CV_8UC1);
            Imgproc.cvtColor(argbMat, yuvMat, Imgproc.COLOR_BGRA2YUV_I420);
            Mat submat = yuvMat.submat(0, _targetVideoHeight, 0, _targetVideoWidth);
            Core.flip(submat, submat, 0);
            Utils.setDebugMode(false);
            CameraIntrinsic intrinsic = new CameraIntrinsic();
            CameraExtrinsic extrinsic = new CameraExtrinsic(Matrix4x4.identity);
            CameraFrame frame = new CameraFrame(submat, intrinsic, extrinsic, _targetVideoWidth, _targetVideoHeight, frameCount++, ColorFormat.Unknown);
            FrameArrivedEventArgs args = new FrameArrivedEventArgs(frame);
            FrameArrived?.Invoke(this, args);
        }

        public uint frameCount { get; set; }
    }
}
