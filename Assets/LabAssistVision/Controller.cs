using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Extensions;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using OpenCVForUnity.UnityUtils;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace LabAssistVision
{
    /// <summary>
    /// Handles input of the camera stream, handles object detection, object tracking, visualization and user input.
    /// </summary>
    [RequireComponent(typeof(VideoDisplayManager))]
    [RequireComponent(typeof(VisualizationManager))]
    [RequireComponent(typeof(FPSDisplayManager))]
    [DisallowMultipleComponent]
    public class Controller : MonoBehaviour
    {
        #region Member Variables
        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull] private VisualizationManager _visualizationManager;
        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull] private VideoDisplayManager _videoDisplayManager;
        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull] private Logger _logger;

        /// <summary>
        /// Label showing the current minimal prediction probability.
        /// </summary>
        public TextMeshPro minimalPredictionProbabilityLabel;

        /// <summary>
        /// Label showing the current fixed tracker count.
        /// </summary>
        public TextMeshPro fixedTrackerCountLabel;

        /// <summary>
        /// Label showing the max concurrent request limit.
        /// </summary>
        public TextMeshPro maxConcurrentRequestLabel;

        /// <summary>
        /// Enables the debug option for OpenCV.
        /// </summary>
        public bool debug;

        /// <summary>
        /// Indicates if a frame is processed synchronous or asynchronous.
        /// </summary>
        public bool sync;

        /// <summary>
        /// Reference to the slider to adjust the max concurrent request limit.
        /// </summary>
        public PinchSlider maxConcurrentRequestsSlider;

        /// <summary>
        /// Reference to the slider to adjust the fixed tracker count.
        /// </summary>
        public PinchSlider fixedTrackerCountSlider;

        /// <summary>
        /// Reference to the slider to adjust the minimal prediction probability.
        /// </summary>
        public PinchSlider minimalPredictionProbabilitySlider;

        /// <summary>
        /// Reference to the switch to set if the fixed tracker count is forced.
        /// This option is handled by <see cref="Microsoft.MixedReality.Toolkit.Extensions.ObjectTrackingService"/>.
        /// </summary>
        public Interactable forceFixedTrackerCountSwitch;

        /// <summary>
        /// Reference to the switch to invoke the detection on repeat. The amount of concurrent requests is limited by the value of <see cref="maxConcurrentRequestsSlider"/>.
        /// </summary>
        public Interactable detectOnRepeatSwitch;

        /// <summary>
        /// Reference to the switch to toggle between synchronous and asynchronous handling of the frame.
        /// If the frame processing takes longer than the target application frame rate, asynchronous processing is recommended.
        /// Both modes only process one frame at the same time.
        /// </summary>
        public Interactable processFrameSync;

        /// <summary>
        /// Indicates the status of the Application. The value of the status can be:
        /// <para/>Idle: Nothing has been initialized. Video capture should be initialized at this status.
        /// <para/>Running: Video pipeline is running, each frame is processed.
        /// </summary>
        private Status _status = Status.Idle;

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

        private IObjectDetectionService _objectDetectionService;

        private IObjectDetectionService ObjectDetectionService
        {
            get
            {
                if (_objectDetectionService != null) return _objectDetectionService;
                _objectDetectionService = MixedRealityToolkit.Instance.GetService<IObjectDetectionService>();
                return _objectDetectionService;
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

        /// <summary>
        /// Signal to indicate that an object detection has been requested. From https://stackoverflow.com/a/49233660
        /// </summary>
        private bool IsObjectDetectionRequested
        {
            get { return Interlocked.CompareExchange(ref _objectDetectionRequested, 1, 1) == 1; }
            set
            {
                if (value) Interlocked.CompareExchange(ref _objectDetectionRequested, 1, 0);
                else Interlocked.CompareExchange(ref _objectDetectionRequested, 0, 1);
            }
        }
        private int _objectDetectionRequested;

        /// <summary>
        /// Signal to indicate that an object detection has been requested. From https://stackoverflow.com/a/49233660
        /// </summary>
        private bool IsProcessingFrame
        {
            get { return Interlocked.CompareExchange(ref _isProcessingFrame, 1, 1) == 1; }
            set
            {
                if (value) Interlocked.CompareExchange(ref _isProcessingFrame, 1, 0);
                else Interlocked.CompareExchange(ref _isProcessingFrame, 0, 1);
            }
        }
        private int _isProcessingFrame;

        #endregion // Member Variables

        #region Internal Methods
        /// <summary>
        /// Receives the current <see cref="CameraFrame"/> to process.
        /// </summary>
        /// <param name="frame">The current frame</param>
        private void ProcessFrameSync([NotNull] CameraFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (_status != Status.Running) return;
            if (IsObjectDetectionRequested)
            {
                if (!_objectDetectionService.detectOnRepeat)
                {
                    IsObjectDetectionRequested = false;
                }
                ObjectDetectionService.DetectAsync(frame).ContinueWith(OnObjectsDetected);
            }

            if (_objectDetectionService.detectOnRepeat) return;
            List<TrackedObject> trackedObjects = ObjectTrackingService.TrackSync(frame);
            IsProcessingFrame = false;
            Vector2 unprojectionOffset = ObjectTrackingService.unprojectionOffset;
            _visualizationManager.UpdateTrackedObjects(trackedObjects, unprojectionOffset);
        }

        private async void OnObjectsDetected([NotNull] Task<List<DetectedObject>> detectedObjectsTask)
        {
            if (detectedObjectsTask == null) throw new ArgumentNullException(nameof(detectedObjectsTask));
            if (detectedObjectsTask.Result == null) return;
            List<DetectedObject> detectedObjects = await detectedObjectsTask;
            if (detectedObjects.Count == 0) Debug.Log("No objects detected");
            if (_objectDetectionService.detectOnRepeat)
            {
                List<TrackedObject> trackedObjects = detectedObjectsTask.Result.Select(detectedObject => new TrackedObject(detectedObject)).ToList();
                FPSUtils.TrackTick();
                Vector2 unprojectionOffset = ObjectTrackingService.unprojectionOffset;
                _visualizationManager.UpdateTrackedObjects(trackedObjects, unprojectionOffset);
            }
            else
            {
                ObjectTrackingService.Reset();
                List<TrackedObject> trackedObjects = ObjectTrackingService.InitializeTrackers(detectedObjects);
                if (detectedObjects.Count != trackedObjects.Count) Debug.LogWarning("Could not initialize tracker for all detected objects");
                Vector2 unprojectionOffset = ObjectTrackingService.unprojectionOffset;
                _visualizationManager.UpdateTrackedObjects(trackedObjects, unprojectionOffset);
            }
        }

        private async Task<bool> StartFrameReaderAsync()
        {
            if (sync)
            {
                CameraService.FrameArrived += CameraServiceOnFrameArrivedSync;
            }
            else
            {
                CameraService.FrameArrived += CameraServiceOnFrameArrivedAsync;
            }

            return await CameraService.StartCapture();
        }

        private void CameraServiceOnFrameArrivedSync(object sender, FrameArrivedEventArgs e)
        {
            if (IsProcessingFrame) return;
            IsProcessingFrame = true;
            CameraFrame frame = e.Frame;
            ProcessFrameSync(frame);
            IsProcessingFrame = false;
        }
        private void CameraServiceOnFrameArrivedAsync(object sender, FrameArrivedEventArgs e)
        {
            if (IsProcessingFrame) return;
            IsProcessingFrame = true;
            CameraFrame frame = e.Frame;
            IsProcessingFrame = true;
            Task.Run(() => ProcessFrameSync(frame)).ContinueWith(_ => IsProcessingFrame = false);
        }
        #endregion // Internal Methods

        #region Unity Overrides
        private async void Start()
        {
            if (debug) Utils.setDebugMode(true, true);

            _logger = MixedRealityToolkit.Instance.GetService<ILoggingService>().GetLogger();
            Assert.IsNotNull(_logger, "_logger != null");

            _videoDisplayManager = GetComponent<VideoDisplayManager>();
            Assert.IsNotNull(_videoDisplayManager, "VideoDisplayManager != null");

            _visualizationManager = GetComponent<VisualizationManager>();
            Assert.IsNotNull(_visualizationManager, "_visualizationManager != null");

            _cameraService = MixedRealityToolkit.Instance.GetService<ICameraService>();
            _objectTrackingService = MixedRealityToolkit.Instance.GetService<IObjectTrackingService>();
            _objectDetectionService = MixedRealityToolkit.Instance.GetService<IObjectDetectionService>();

            maxConcurrentRequestsSlider.OnValueUpdated.AddListener(OnMaxConcurrentRequestUpdated);
            maxConcurrentRequestsSlider.SliderValue = _objectDetectionService.maxConcurrentRequests / 20.0f;

            fixedTrackerCountSlider.OnValueUpdated.AddListener(OnFixedTrackerCountUpdated);
            fixedTrackerCountSlider.SliderValue = _objectTrackingService.FixedTrackerCount / 20.0f;

            minimalPredictionProbabilitySlider.OnValueUpdated.AddListener(OnMinimalPredictionProbabilityUpdated);
            fixedTrackerCountSlider.SliderValue = (float)_objectDetectionService.minimalPredictionProbability;

            forceFixedTrackerCountSwitch.OnClick.AddListener(OnFixedTrackerCountToggled);
            forceFixedTrackerCountSwitch.IsToggled = _objectTrackingService.ForceFixedTrackerCount;

            detectOnRepeatSwitch.OnClick.AddListener(OnDetectOnRepeatToggled);
            detectOnRepeatSwitch.IsToggled = _objectDetectionService.detectOnRepeat;

            processFrameSync.OnClick.AddListener(OnProcessFrameSyncToggled);
            processFrameSync.IsToggled = sync;

            PointerUtils.SetGazePointerBehavior(PointerBehavior.AlwaysOn);

            await StartFrameReaderAsync();
            _status = Status.Running;
        }

        private void OnFixedTrackerCountToggled()
        {
            _objectTrackingService.ToggleFixedTrackerCount();
            forceFixedTrackerCountSwitch.IsToggled = _objectTrackingService.ForceFixedTrackerCount;
        }

        private void OnDetectOnRepeatToggled()
        {
            _objectDetectionService.ToggleDetectOnRepeat();
            detectOnRepeatSwitch.IsToggled = _objectDetectionService.detectOnRepeat;
        }

        private void OnProcessFrameSyncToggled()
        {
            sync = processFrameSync.IsToggled;
            Debug.Log($"Changed process frame sync to {sync}");
        }

        private void OnMaxConcurrentRequestUpdated(SliderEventData eventData)
        {
            if (eventData == null) return;
            int value = (int)(eventData.NewValue * 20f);

            maxConcurrentRequestLabel.text = $"{value}";
            ObjectDetectionService.ChangeMaxConcurrentRequestLimit(value);
            Debug.Log($"Max object detection request limit changed to {value}");
        }

        private void OnFixedTrackerCountUpdated(SliderEventData eventData)
        {
            if (eventData == null) return;
            int value = (int)(eventData.NewValue * 20f);

            fixedTrackerCountLabel.text = $"{value}";
            ObjectTrackingService.ChangeFixedTrackerCount(value);
            Debug.Log($"Changed tracker count value to {value}");
        }

        private void OnMinimalPredictionProbabilityUpdated(SliderEventData eventData)
        {
            if (eventData == null) return;
            double value = eventData.NewValue;
            minimalPredictionProbabilityLabel.text = $"{value:0.0}";
            ObjectDetectionService.ChangeMinimalPredictionProbability(value);
            Debug.Log($"Changed minimal prediction probability to {value:0.0}");
        }

        private void LateUpdate()
        {
            HandleKeyboardInput();
            FPSUtils.RenderTick();
        }

        /// <summary>
        /// Handle keyboard commands. Adapted from https://github.com/qian256/HoloLensARToolKit/blob/master/HoloLensARToolKit/Assets/ARToolKitUWP/Scripts/ARUWPController.cs.
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (_videoDisplayManager.displayVideo)
                {
                    _videoDisplayManager.DisableVideoDisplay();
                }
                else
                {
                    _videoDisplayManager.EnableVideoDisplay();
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (_videoDisplayManager.displayFPS)
                {
                    _videoDisplayManager.DisableFPSDisplay();
                }
                else
                {
                    _videoDisplayManager.EnableFPSDisplay();
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (_status == Status.Running)
                {
                    IsObjectDetectionRequested = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_status == Status.Running)
                {
                    Reset();
                }
            }
        }

        #endregion // Unity Overrides

        #region Public Methods

        public void Reset()
        {
            IsObjectDetectionRequested = false;
            ObjectDetectionService.Reset();
            ObjectTrackingService.Reset();
            _visualizationManager.Reset();
            IsProcessingFrame = false;
        }

        public void ChangeTracker(Tracker tracker)
        {
            ObjectTrackingService.SwitchTracker(tracker);
            Debug.Log($"Changed Tracker to {tracker}");
        }

        public void SetMOSSETracker()
        {
            ChangeTracker(Tracker.MosseTracker);
        }

        public void SetKCFTracker()
        {
            ChangeTracker(Tracker.KCFTracker);
        }

        public void SetBoostingTracker()
        {
            ChangeTracker(Tracker.BoostingTracker);
        }

        public void SetCSRTTracker()
        {
            ChangeTracker(Tracker.CSRTTracker);
        }

        public void SetMedianFlowTracker()
        {
            ChangeTracker(Tracker.MedianFlowTracker);
        }

        public void SetMILTracker()
        {
            ChangeTracker(Tracker.MILTracker);
        }

        public void SetTLDTracker()
        {
            ChangeTracker(Tracker.TLDTracker);
        }

        private bool videoParameterSelectionInit = false;

        private void ChangeVideoParameter(LocatableCameraProfile parameter, ColorFormat format)
        {
            // skip first change
            if (videoParameterSelectionInit)
            {
                if (_status != Status.Running) return;
                CameraService.ChangeVideoParameter(parameter, format);
                Debug.Log($"Changed video parameter to {parameter} and format {format}");
            }
            else
            {
                videoParameterSelectionInit = true;
            }
        }

        public void SetVideoParameter424x240x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_424x240_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter424x240x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_424x240_15, ColorFormat.RGB);
        }
        public void SetVideoParameter424x240x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_424x240_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter424x240x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_424x240_30, ColorFormat.RGB);
        }

        public void SetVideoParameter500x280x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_500x282_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter500x280x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_500x282_15, ColorFormat.RGB);
        }
        public void SetVideoParameter500x280x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_500x282_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter500x280x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_500x282_30, ColorFormat.RGB);
        }

        public void SetVideoParameter640x360x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_640x360_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter640x360x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_640x360_15, ColorFormat.RGB);
        }
        public void SetVideoParameter640x360x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_640x360_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter640x360x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_640x360_30, ColorFormat.RGB);
        }

        public void SetVideoParameter760x428x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_760x428_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter760x428x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_760x428_15, ColorFormat.RGB);
        }
        public void SetVideoParameter760x428x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_760x428_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter760x428x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_760x428_30, ColorFormat.RGB);
        }

        public void SetVideoParameter960x540x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_960x540_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter960x540x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_960x540_15, ColorFormat.RGB);
        }
        public void SetVideoParameter960x540x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_960x540_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter960x540x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_960x540_30, ColorFormat.RGB);
        }

        public void SetVideoParameter1128x636x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1128x636_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1128x636x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1128x636_15, ColorFormat.RGB);
        }
        public void SetVideoParameter1128x636x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1128x636_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1128x636x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1128x636_30, ColorFormat.RGB);
        }

        public void SetVideoParameter1280x720x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1280x720_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1280x720x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1280x720_15, ColorFormat.RGB);
        }
        public void SetVideoParameter1280x720x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1280x720_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1280x720x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1280x720_30, ColorFormat.RGB);
        }

        public void SetVideoParameter1920x1080x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1920x1080_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1920x1080x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1920x1080_15, ColorFormat.RGB);
        }
        public void SetVideoParameter1920x1080x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1920x1080_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1920x1080x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1920x1080_30, ColorFormat.RGB);
        }

        public void SetVideoParameter1504x846x5Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_5, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1504x846x5RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_5, ColorFormat.RGB);
        }
        public void SetVideoParameter1504x846x10RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_10, ColorFormat.RGB);
        }
        public void SetVideoParameter1504x846x10Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_10, ColorFormat.Grayscale);
        }

        public void SetVideoParameter1504x846x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1504x846x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_15, ColorFormat.RGB);
        }
        public void SetVideoParameter1504x846x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1504x846x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_30, ColorFormat.RGB);
        }

        public void SetVideoParameter1504x846x60Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_60, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1504x846x60RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1504x846_60, ColorFormat.RGB);
        }

        public void SetVideoParameter1952x1100x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1952x1100_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1952x1100x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1952x1100_15, ColorFormat.RGB);
        }
        public void SetVideoParameter1952x1100x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1952x1100_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1952x1100x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1952x1100_30, ColorFormat.RGB);
        }
        public void SetVideoParameter1952x1100x60Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1952x1100_60, ColorFormat.Grayscale);
        }
        public void SetVideoParameter1952x1100x60RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_1952x1100_60, ColorFormat.RGB);
        }

        public void SetVideoParameter896x504x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_896x504_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter896x504x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_896x504_15, ColorFormat.RGB);
        }
        public void SetVideoParameter896x504x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_896x504_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter896x504x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_896x504_30, ColorFormat.RGB);
        }

        public void SetVideoParameter2272x1278x15Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_2272x1278_15, ColorFormat.Grayscale);
        }
        public void SetVideoParameter2272x1278x15RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_2272x1278_15, ColorFormat.RGB);
        }
        public void SetVideoParameter2272x1278x30Grayscale()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_2272x1278_30, ColorFormat.Grayscale);
        }
        public void SetVideoParameter2272x1278x30RGB()
        {
            ChangeVideoParameter(LocatableCameraProfile.HL2_2272x1278_30, ColorFormat.RGB);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void RequestObjectDetection()
        {
            IsObjectDetectionRequested = true;
        }

        #endregion // Public Methods
    }

    public enum Status
    {
        Idle,
        Running,
    }
}