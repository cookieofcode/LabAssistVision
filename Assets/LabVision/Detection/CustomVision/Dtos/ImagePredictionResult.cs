using System;

namespace LabVision
{
    [Serializable]
    public class ImagePredictionResult
    {
        public string id;

        public string project;

        public string iteration;

        public DateTimeOffset created;

        public Prediction[] predictions;
    }
}