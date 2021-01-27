using System;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Unity.Barracuda;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
	[MixedRealityServiceProfile(typeof(IObjectDetectionService))]
	[CreateAssetMenu(fileName = "ObjectDetectionServiceProfile", menuName = "MixedRealityToolkit/ObjectDetectionService Configuration Profile")]
	public class ObjectDetectionServiceProfile : BaseMixedRealityProfile
	{
        /// <summary>
        /// The minimal probability on a prediction of an object to be tracked.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The minimal probability on a prediction of an object to be tracked.")]
        public double minimalPredictionProbability = 0.5;

        /// <summary>
        /// The maximal amount of concurrent requests.
        /// </summary>
        [SerializeField]
        [Range(0, 20)]
        [Tooltip("The maximal amount of concurrent requests.")]
        public int maxConcurrentRequestLimit = 1;

        /// <summary>
        /// If enabled, object detection is executed on repeat instead of tracking.
        /// </summary>
        [HideInInspector]
        [Tooltip("If enabled, object detection is executed on repeat instead of tracking.")]
        public bool detectOnRepeat;

        /// <summary>
        /// If enabled, object detection is executed on the device instead of using a cloud service.
        /// This experimental feature impacts performance.
        /// </summary>
        [Tooltip("Use local object detection instead of Cloud Detection. Experimental Feature.")]
        public bool useLocalModel;

        /// <summary>
        /// The exported model file as ONNX obtained from Custom Vision.
        /// </summary>
        [Tooltip("Model file of the exported model as ONNX from Custom Vision.")]
        public NNModel modelFile;

        /// <summary>
        /// The exported labels to the model file obtained from Custom Vision.
        /// </summary>
        [Tooltip("The labels of the exported model obtained from Custom Vision")]
        public TextAsset labels;

        /// <summary>
        /// The Prediction URL of the Custom Vision Prediction API.
        /// </summary>
        [Tooltip("Prediction URL of Custom Vision Prediction API. To be found under Performance > Prediction URL > If you have an image file")]
        public string predictionApi = "";

        /// <summary>
        /// The Prediction Key of the Custom Vision Prediction API.
        /// </summary>
        [Tooltip("Prediction key of Custom Vision Prediction API. To be found under Performance > Prediction URL > If you have an image file")]
        public string predictionKey = "";
    }
}