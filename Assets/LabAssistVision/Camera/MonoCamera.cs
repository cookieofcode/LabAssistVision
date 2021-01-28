using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UtilsModule;
using UnityEngine;
using UnityEngine.Windows.WebCam;

namespace LabAssistVision
{
    /// <summary>
    /// Provides NV12 video input using the Unity Editor and the integrated camera.
    /// </summary>
    public class MonoCamera : ICamera
    {
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }

        [CanBeNull] public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        [CanBeNull] public event EventHandler<CameraInitializedEventArgs> CameraInitialized;

        private uint FrameCount { get; set; }
        [CanBeNull] private Mat _image;
        [CanBeNull] private PhotoCapture _photoCaptureObject;
        private Resolution _cameraResolution;
        private readonly ColorFormat _format;
        private TaskCompletionSource<bool> _stopped;

        public MonoCamera(ColorFormat format)
        {
            _format = format;
        }

        public Task<bool> Initialize()
        {
            _cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            FrameHeight = _cameraResolution.height;
            FrameWidth = _cameraResolution.width;
            CameraInitializedEventArgs a = new CameraInitializedEventArgs(FrameWidth, FrameHeight, _format);
            CameraInitialized?.Invoke(this, a);
            return Task.FromResult(true);
        }

        public Task<bool> StartCapture()
        {
            if (FrameHeight == 0 && FrameWidth == 0)
            {
                Debug.LogError("StartCapture() invoked before camera initialized.");
                return Task.FromResult(false);
            }
            PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
            {
                _photoCaptureObject = captureObject;
                UnityEngine.Windows.WebCam.CameraParameters cameraParameters = new UnityEngine.Windows.WebCam.CameraParameters
                {
                    hologramOpacity = 0.0f,
                    cameraResolutionWidth = FrameWidth,
                    cameraResolutionHeight = FrameHeight,
                    pixelFormat = CapturePixelFormat.NV12
                };

                _photoCaptureObject?.StartPhotoModeAsync(cameraParameters, delegate
                {
                    _photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                });
            });
            return Task.FromResult(true);
        }

        /// <summary>
        /// Processes the received frame, converts the image to grayscale if requested, and invokes the next photo request.
        /// </summary>
        private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            if (_stopped?.Task != null) return;
            if (result.resultType == PhotoCapture.CaptureResultType.UnknownError) return;
            if (photoCaptureFrame == null) return;
            Size size = new Size(FrameWidth, (double)FrameHeight * 3 / 2); // Luminance (grayscale) of the NV12 format requires image height, chrominance is stored in half resolution. <see href="https://docs.microsoft.com/en-us/windows/win32/medfound/recommended-8-bit-yuv-formats-for-video-rendering#nv12"/>.
            _image = new Mat(size, CvType.CV_8UC1);
            List<byte> imageBuffer = new List<byte>();
            photoCaptureFrame?.CopyRawImageDataIntoBuffer(imageBuffer);
            MatUtils.copyToMat(imageBuffer.ToArray(), _image);

            if (_format == ColorFormat.Grayscale)
            {
                Imgproc.cvtColor(_image, _image, Imgproc.COLOR_YUV2GRAY_NV12);
            }

            Matrix4x4 cameraToWorldMatrix = Matrix4x4.identity;
            photoCaptureFrame?.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
            CameraExtrinsic extrinsic = new CameraExtrinsic(cameraToWorldMatrix);

            Matrix4x4 projectionMatrix = Matrix4x4.identity;
            photoCaptureFrame?.TryGetProjectionMatrix(out projectionMatrix);
            CameraIntrinsic intrinsic = new CameraIntrinsic(projectionMatrix);

            CameraFrame cameraFrame = new CameraFrame(_image, intrinsic, extrinsic, FrameWidth, FrameHeight, FrameCount++, _format);
            FrameArrivedEventArgs args = new FrameArrivedEventArgs(cameraFrame);
            FrameArrived?.Invoke(this, args);

            _photoCaptureObject?.TakePhotoAsync(OnCapturedPhotoToMemory);
        }

        public async void Destroy()
        {
            await StopCapture();
        }

        private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            _photoCaptureObject?.Dispose();
            _photoCaptureObject = null;
            _stopped.SetResult(true);
            Debug.Log("Photo mode stopped.");
        }

        public async Task<bool> StopCapture()
        {
            _stopped = new TaskCompletionSource<bool>();
            _photoCaptureObject?.StopPhotoModeAsync(OnStoppedPhotoMode);
            bool stopped = await _stopped.Task;
            _stopped = null;
            return stopped;
        }
    }
}
