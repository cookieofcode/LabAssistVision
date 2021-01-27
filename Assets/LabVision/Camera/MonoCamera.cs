using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UtilsModule;
using UnityEngine;
using UnityEngine.Windows.WebCam;

namespace LabVision
{
    /// <summary>
    /// Provides NV12 video input using the Unity Editor and the integrated camera.
    /// </summary>
    public class MonoCamera : ICamera
    {
        public int frameWidth { get; set; }
        public int frameHeight { get; set; }

        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public event EventHandler<CameraInitializedEventArgs> CameraInitialized;

        private PhotoCapture _photoCaptureObject;
        private Mat _image;
        private Resolution _cameraResolution;
        private uint frameCount { get; set; }
        private readonly ColorFormat _format;
        private TaskCompletionSource<bool> _stopped;


        public MonoCamera(ColorFormat format)
        {
            _format = format;
        }

        public Task<bool> Initialize()
        {
            _cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            frameHeight = _cameraResolution.height;
            frameWidth = _cameraResolution.width;
            CameraInitializedEventArgs a = new CameraInitializedEventArgs(frameWidth, frameHeight, _format);
            CameraInitialized?.Invoke(this, a);
            return Task.FromResult(true);
        }

        public Task<bool> StartCapture()
        {
            if (frameHeight == 0 && frameWidth == 0)
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
                    cameraResolutionWidth = frameWidth,
                    cameraResolutionHeight = frameHeight,
                    pixelFormat = CapturePixelFormat.NV12
                };

                _photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate
                {
                    _photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                });
            });
            return Task.FromResult(true);
        }

        private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            if (_stopped?.Task != null) return;
            if (result.resultType == PhotoCapture.CaptureResultType.UnknownError) return;
            if (photoCaptureFrame == null) return;
            Size size = new Size(frameWidth, (double)frameHeight * 3 / 2);
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

            CameraFrame cameraFrame = new CameraFrame(_image, intrinsic, extrinsic, frameWidth, frameHeight, frameCount++, _format);
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
            _photoCaptureObject.Dispose();
            _photoCaptureObject = null;
            _stopped.SetResult(true);
            Debug.Log("Photo mode stopped.");
        }

        public async Task<bool> StopCapture()
        {
            _stopped = new TaskCompletionSource<bool>();
            _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
            bool stopped = await _stopped.Task;
            _stopped = null;
            return stopped;
        }
    }
}
