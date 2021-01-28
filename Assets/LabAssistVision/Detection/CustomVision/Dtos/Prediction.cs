using System;

namespace LabAssistVision
{
    [Serializable]
    public class Prediction
    {
        public double probability;

        public string tagId;

        public string tagName;

        public BoundingBox boundingBox;

        public string tagType;

        public Prediction(float max, string label, BoundingBox extractedBoxesBox)
        {
            probability = max;
            tagName = label;
            boundingBox = extractedBoxesBox;
        }
    }
}