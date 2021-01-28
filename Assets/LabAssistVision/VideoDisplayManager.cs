using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Extensions;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using UnityEngine.Assertions;

namespace LabAssistVision
{
    /// <summary>
    /// Provides a preview of the RGB or grayscale video stream for debugging purpose. Allows to visualize the results of <see cref="TrackedObject">Tracked Objects</see> in the preview.
    /// </summary>
    [DisallowMultipleComponent]
    public class VideoDisplayManager : MonoBehaviour
    {
        #region Member Variables
        [NotNull] private readonly Logger _logger = new Logger(new LogHandler());

        /// <summary>
        /// The <see cref="Material"/> used to render the <see cref="Texture"/>.
        /// </summary>
        [CanBeNull] private Material _videoDisplayMaterial;

        /// <summary>
        /// Holds the image of the current camera frame.
        /// </summary>
        [CanBeNull] private Texture2D _texture;

        /// <summary>
        /// The <see cref="Renderer"/> of the video display object.
        /// </summary>
        [CanBeNull] private MeshRenderer _meshRenderer;

        /// <summary>
        /// The last frame count to determine if the arrived frame is updated.
        /// </summary>
        private uint _lastFrameCount = uint.MaxValue;

        /// <summary>
        /// Indicates if the <see cref="_texture"/> is initialized.
        /// </summary>
        private bool _textureInitialized;

        /// <summary>
        /// The <see cref="Shader"/> used to display RGB images (e.g. "Unlit/Texture").
        /// </summary>
        [CanBeNull] public Shader rgbShader;

        /// <summary>
        /// The <see cref="Shader"/> used to display grayscale images using the luminance.
        /// </summary>
        [CanBeNull] public Shader luminanceShader;

        /// <summary>
        /// Enabling the video display has impact on performance. Can not be changed at runtime.
        /// </summary>
        [Tooltip("Video display is for debugging purpose. It has impact on performance.")]
        public bool displayVideo = true;

        /// <summary>
        /// Enabling the tracking preview has impact on performance. Video display must be enabled.
        /// </summary>
        [Tooltip("Tracking preview is for debugging purpose. It has impact on performance.")]
        public bool trackingPreview;

        /// <summary>
        /// Enables or disables the FPS display. Can not be changed at runtime.
        /// </summary>
        [Tooltip("FPS display is for debugging purpose.")]
        public bool displayFPS = true;

        /// <summary>
        /// The Video Display <see cref="GameObject"/>.
        /// </summary>
        [CanBeNull] public GameObject videoDisplay;

        /// <summary>
        /// The FPS Display <see cref="GameObject"/>.
        /// </summary>
        [CanBeNull] public GameObject fpsDisplay;

        /// <summary>
        /// Determines the scale factor of the video display, as the size depends on the camera resolution, which can be changed during runtime.
        /// </summary>
        public float scale = 0.0001f;

        private ICameraService _cameraService;
        private ICameraService CameraService
        {
            get
            {
                if (_cameraService != null) return _cameraService;
                _cameraService = MixedRealityToolkit.Instance.GetService<ICameraService>();
                return _cameraService;
            }
        }

        private IObjectTrackingService _objectTrackingService;
        private IObjectTrackingService ObjectTrackingService
        {
            get
            {
                if (_objectTrackingService != null) return _objectTrackingService;
                _objectTrackingService = MixedRealityToolkit.Instance.GetService<IObjectTrackingService>();
                return _objectTrackingService;
            }
        }
        #endregion // Member Variables

        #region Internal Methods
        private void OnCameraInitialized(object sender, CameraInitializedEventArgs args)
        {
            if (!displayVideo) return;
            int frameWidth = args.FrameWidth;
            int frameHeight = args.FrameHeight;
            ColorFormat format = args.Format;
            switch (format)
            {
                case ColorFormat.RGB:
                    _videoDisplayMaterial = new Material(rgbShader);
                    _texture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGB24, false);
                    break;
                case ColorFormat.Grayscale:
                    _videoDisplayMaterial = new Material(luminanceShader);
                    _texture = new Texture2D(frameWidth, frameHeight, TextureFormat.Alpha8, false);
                    break;
                default:
                    throw new InvalidOperationException($"Color format {format} not supported by Video Display Manager");
            }

            if (_meshRenderer == null)
            {
                Debug.Log("Could not initialize texture as mesh renderer of video display is not initialized");
                return;
            }

            _meshRenderer.material = _videoDisplayMaterial;
            _videoDisplayMaterial.mainTexture = _texture;

            if (videoDisplay != null)
            {
                videoDisplay.transform.localScale = new Vector3(frameWidth * -scale, frameHeight * -scale, 1); // -1 flips texture
            }

            _textureInitialized = true;
        }
        #endregion // Internal Methods

        #region Unity Overrides
        public void Start()
        {
            Assert.IsNotNull(_logger, "_logger != null");

            if (displayVideo)
            {
                if (videoDisplay != null)
                {
                    _meshRenderer = videoDisplay.GetComponent<MeshRenderer>();
                    Assert.IsNotNull(_meshRenderer, "_meshRenderer != null");
                    EnableVideoDisplay();
                }
                else
                {
                    displayVideo = false;
                }
            }

            if (displayFPS)
            {
                if (fpsDisplay != null)
                {
                    EnableFPSDisplay();
                }
                else
                {
                    displayFPS = false;
                }
            }

            if (CameraService.Initialized)
            {
                OnCameraInitialized(this, new CameraInitializedEventArgs(CameraService.FrameWidth, CameraService.FrameHeight, CameraService.Format));
                CameraService.CameraInitialized += OnCameraInitialized; // necessary for switching
            }
            else
            {
                CameraService.CameraInitialized += OnCameraInitialized;
            }
        }

        public void LateUpdate()
        {
            if (!displayVideo || !_textureInitialized) return;
            CameraFrame cameraFrame = CameraService.CameraFrame;
            if (cameraFrame == null) return;
            if (!FrameHasChanged(cameraFrame)) return;
            Mat image = cameraFrame.Mat;

            if (trackingPreview)
            {
                List<TrackedObject> trackedObjects = ObjectTrackingService.TrackedObjects;
                foreach (Rect2d rect in trackedObjects.Select(trackedObject => trackedObject.Rect))
                {
                    Imgproc.rectangle(image, rect.tl(), rect.br(), new Scalar(255, 255, 255), 10, 1, 0);
                }
                foreach (Point point in trackedObjects.Select(trackedObject => trackedObject.GetBoundingBoxTarget()))
                {
                    Imgproc.circle(image, point, 5, new Scalar(255, 255, 255), -3, 1, 0);
                }
            }

            Utils.fastMatToTexture2D(image, _texture, false);
        }

        /// <summary>
        /// Determines if the frame has changed since the last update. Not necessary but saves some resources.
        /// </summary>
        /// <param name="cameraFrame">The current camera frame.</param>
        /// <returns></returns>
        private bool FrameHasChanged(CameraFrame cameraFrame)
        {
            if (cameraFrame.FrameCount == _lastFrameCount) return false;
            _lastFrameCount = cameraFrame.FrameCount;
            return true;
        }

        #endregion // Unity Overrides

        #region Public Methods
        public void EnableVideoDisplay()
        {
            if (videoDisplay != null)
            {
                videoDisplay.SetActive(true);
                displayVideo = true;
                _logger.Log("Video display enabled");
            }
            else
            {
                _logger.Log("Could not enable video display");
            }
        }

        public void DisableVideoDisplay()
        {
            if (videoDisplay != null)
            {
                videoDisplay.SetActive(false);
                displayVideo = false;
                _logger.Log("Video display disabled");
            }
            else
            {
                _logger.Log("Could not disable video display");
            }
        }

        public void EnableFPSDisplay()
        {
            if (fpsDisplay != null)
            {
                fpsDisplay.SetActive(true);
                displayFPS = true;
                _logger.Log("FPS Display enabled");
            }
            else
            {
                _logger.Log("Could not enable FPS Display");
            }
        }

        public void DisableFPSDisplay()
        {
            if (fpsDisplay != null)
            {
                fpsDisplay.SetActive(false);
                displayFPS = false;
                _logger.Log("FPS display disabled");
            }
            else
            {
                _logger.Log("Could not disable FPS Display");
            }
        }
        #endregion // Public Methods
    }
}