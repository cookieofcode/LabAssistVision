using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using LabAssistVision.ThreadUtils;
using UnityEngine;
using UnityEngine.Assertions;

namespace LabAssistVision
{
    /// <summary>
    /// Visualizes <see cref="TrackedObject">Tracked Objects</see> using a <see cref="Label"/>.
    /// Calculates the position in the world using the Spatial Mesh on the main thread.
    /// </summary>
    [RequireComponent(typeof(Controller))]
    [DisallowMultipleComponent]
    public class VisualizationManager : MonoBehaviour
    {
        [NotNull] private readonly Logger _logger = new Logger(new LogHandler());
        [NotNull] private List<SpatialTrackedObject> _spatialTrackedObjects = new List<SpatialTrackedObject>();
        private List<Label> _tooltips = new List<Label>();

        /// <summary>
        /// Indicates if tracked objects should be simulated. This option is for debug purposes, if no tracked objects are available.
        /// </summary>
        [Tooltip("Simulates a tracked object without the need for tracking.")]
        public bool simulateTrackedObjects;

        /// <summary>
        /// Holds simulated tracked objects configurable in the Unity Editor.
        /// </summary>
        [Tooltip("Configure the position of simulated tracked objects. Simulating tracked objects must be enabled.")]
        [NotNull] public List<Vector3> simulatedTrackedObjects = new List<Vector3>();

        /// <summary>
        /// The <see cref="Label"/> containing the tooltip to visualize a <see cref="TrackedObject"/>.
        /// </summary>
        public Label tooltipPrefab;

        // TODO: Check https://localjoost.github.io/migrating-to-mrtk2interacting-with/
        private const int SpatialAwarenessLayerMask = 1 << 31;

        public void Start()
        {
            Assert.IsNotNull(_logger, "_logger != null");
            Assert.IsNotNull(_spatialTrackedObjects, "_spatialTrackedObjects != null");
            Assert.IsNotNull(_tooltips, "_tooltips != null");
            Assert.IsNotNull(simulatedTrackedObjects);
        }

        public void Reset()
        {
            _spatialTrackedObjects = new List<SpatialTrackedObject>();
            List<Label> tmp = _tooltips;
            _tooltips = new List<Label>();
            foreach (Label tooltip in tmp)
            {
                Destroy(tooltip.gameObject);
            }
        }

        public void Update()
        {
            if (_spatialTrackedObjects.Count <= 0) return;
            if (_spatialTrackedObjects.Count != _tooltips.Count)
            {
                if (_spatialTrackedObjects.Count > _tooltips.Count)
                {
                    int tooltipsToInstantiate = _spatialTrackedObjects.Count - _tooltips.Count;
                    for (int i = 0; i < tooltipsToInstantiate; i++)
                    {
                        SpatialTrackedObject spatialTrackedObject = _spatialTrackedObjects[i];
                        Label tooltip = Instantiate(tooltipPrefab, spatialTrackedObject.Position, Quaternion.identity);
                        _tooltips.Add(tooltip);
                    }
                }
                else
                {
                    int tooltipsToDestroy = _tooltips.Count - _spatialTrackedObjects.Count;
                    for (int i = _spatialTrackedObjects.Count; i > _spatialTrackedObjects.Count - tooltipsToDestroy; i--)
                    {
                        Label tooltip = _tooltips[i];
                        _tooltips.Remove(tooltip);
                        Destroy(tooltip.gameObject);
                    }
                }
            }

            Assert.AreEqual(_spatialTrackedObjects.Count, _tooltips.Count);

            for (int i = 0; i < _spatialTrackedObjects.Count; i++)
            {
                Label tooltip = _tooltips[i];
                SpatialTrackedObject spatialTrackedObject = _spatialTrackedObjects[i];
                if (spatialTrackedObject.Position != Vector3.positiveInfinity)
                {
                    tooltip.UpdatePosition(spatialTrackedObject.Position); // TODO: Add Lerp
                }
                tooltip.UpdateText(spatialTrackedObject.Label);
            }
        }

        /// <summary>
        /// Updates <see cref="TrackedObject">Tracked Objects</see> by mapping them to <see cref="SpatialTrackedObject">Spatial Tracked Objects</see>.
        /// </summary>
        /// <param name="trackedObjects"></param>
        /// <param name="unprojectionOffset"></param>
        public void UpdateTrackedObjects([NotNull] List<TrackedObject> trackedObjects, Vector2 unprojectionOffset)
        {
            if (trackedObjects == null) throw new ArgumentNullException(nameof(trackedObjects));
            if (unprojectionOffset == null) throw new ArgumentNullException(nameof(unprojectionOffset));
            UnityDispatcher.InvokeOnAppThread(() =>
            {
                List<SpatialTrackedObject> spatialTrackedObjects = new List<SpatialTrackedObject>();

                if (simulateTrackedObjects && trackedObjects.Count == 0)
                {
                    spatialTrackedObjects.AddRange(simulatedTrackedObjects.Select(position => new SpatialTrackedObject("Simulated", position)));
                }
                else
                {
                    spatialTrackedObjects.AddRange(trackedObjects.Select(trackedObject => CreateSpatialTrackedObject(trackedObject, unprojectionOffset)));
                }
                _spatialTrackedObjects = spatialTrackedObjects;
            });
        }

        /// <summary>
        /// Converts a tracked object into a world tracked object.
        /// </summary>
        public SpatialTrackedObject CreateSpatialTrackedObject(TrackedObject trackedObject, Vector3 unprojectionOffset)
        {
            Vector3 cameraPosition = trackedObject.GetCameraPosition();
            Vector3 layForward = trackedObject.GetLayForward(unprojectionOffset);
            Vector3 position = GetPosition(cameraPosition, layForward);
            string label = trackedObject.Label;
            return new SpatialTrackedObject(label, position);
        }

        /// <summary>
        /// Retrieves the position using a ray with the camera position as origin in direction of the forward vector.
        /// The collision point with the Spatial Mesh determines the position.
        /// </summary>
        /// <param name="cameraPosition">The camera extrinsic</param>
        /// <param name="layForward">The forward vector of the camera</param>
        /// <returns>The position in the world</returns>
        public Vector3 GetPosition(Vector3 cameraPosition, Vector3 layForward)
        {
            if (!Microsoft.MixedReality.Toolkit.Utilities.SyncContextUtility.IsMainThread)
            {
                _logger.LogError("Position could not be determined. Ensure calling on main thread.");
                return Vector3.zero;
            }

            RaycastHit hit;
            if (!Physics.Raycast(cameraPosition, layForward * -1f, out hit, Mathf.Infinity, SpatialAwarenessLayerMask)) // TODO: Check -1
            {
#if ENABLE_WINMD_SUPPORT
                Debug.LogWarning("Raycast failed. Probably no spatial mesh provided.");
                return Vector3.positiveInfinity;
#else
                Debug.LogWarning("Raycast failed. Probably no spatial mesh provided. Use Holographic Remoting or HoloLens."); // TODO: Check mesh simulation
#endif
            }
            //frame.Dispose(); // TODO: Check disposal
            return hit.point;
        }
    }
}
