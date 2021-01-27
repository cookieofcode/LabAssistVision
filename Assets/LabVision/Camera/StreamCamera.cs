using System;
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
using UnityEngine.Video;

namespace LabVision
{
    /// <summary>
    /// Provides a local hosted video stream (http://127.0.0.1:8080) as input camera. As the <see cref="VideoPlayer"/> does not support authentication, the stream must be provided by a proxy.
    /// </summary>
    public class StreamCamera : MonoBehaviour, ICamera
    {
        private int _targetVideoWidth, _targetVideoHeight;
        public int paddedFrameWidth;
        public int frameHeight { get; set; }
        public int frameWidth { get; set; }
        private bool _available;

        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull] private Logger _logger;

        [NotNull] private VideoPlayer _videoPlayer;

        public event EventHandler<FrameArrivedEventArgs> FrameArrived;
        public event EventHandler<CameraInitializedEventArgs> CameraInitialized;

        public void Start()
        {
            _logger = new Logger(new LogHandler());
            Assert.IsNotNull(_logger, "_logger != null");

            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            Assert.IsNotNull(_videoPlayer, "_videoPlayer != null");
        }

        public async Task<bool> Initialize()
        {
            _videoPlayer.url = "http://127.0.0.1:8080/";
            _videoPlayer.frame = 100; // Skip the first 100 frames.
            _videoPlayer.isLooping = true; // Restart from beginning when done.
            _targetVideoWidth = 1280;
            _targetVideoHeight = 720;
            frameWidth = Convert.ToInt32(_targetVideoWidth);
            frameHeight = Convert.ToInt32(_targetVideoHeight);
            CameraInitialized?.Invoke(this, new CameraInitializedEventArgs(frameWidth, frameHeight, ColorFormat.Unknown));
            return await Task.FromResult(true);
        }

        public async Task<bool> StartCapture()
        {
            _videoPlayer.Play();
            _available = true;
            return await Task.FromResult(true);
        }

        public async Task<bool> StopCapture()
        {
            _videoPlayer.Stop();
            return await Task.FromResult(true);
        }

        public void Update()
        {
            if (!_available) return;
            if (_videoPlayer.texture == null) return;
            Texture2D texture = (Texture2D)_videoPlayer.texture;
            Color32[] pixels32 = texture.GetPixels32();
            Utils.setDebugMode(true);
            Mat argbMat = new Mat(_targetVideoHeight, _targetVideoWidth, CvType.CV_8UC4);
            MatUtils.copyToMat(pixels32, argbMat);
            Mat yuvMat = new Mat(_targetVideoHeight * 2 / 3, _targetVideoWidth, CvType.CV_8UC1);
            Imgproc.cvtColor(argbMat, yuvMat, Imgproc.COLOR_BGRA2YUV_I420);
            Mat submat = yuvMat.submat(0, _targetVideoHeight, 0, _targetVideoWidth);
            Core.flip(submat, submat, 0);
            Utils.setDebugMode(false);
            CameraIntrinsic intrinsic = new CameraIntrinsic();
            CameraExtrinsic extrinsic = new CameraExtrinsic(Matrix4x4.identity);
            CameraFrame frame = new CameraFrame(submat, intrinsic, extrinsic, _targetVideoWidth, _targetVideoHeight, 0, ColorFormat.Unknown); // TODO: frame count
            FrameArrivedEventArgs args = new FrameArrivedEventArgs(frame);
            FrameArrived?.Invoke(this, args);
        }
    }
}
