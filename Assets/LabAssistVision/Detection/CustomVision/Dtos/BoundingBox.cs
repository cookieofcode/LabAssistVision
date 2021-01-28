using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabAssistVision
{
    [Serializable]
    public class BoundingBox
    {
        public BoundingBox(float left, float top, float width, float height)
        {
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
        }

        public double left;

        public double top;

        public double width;

        public double height;
    }
}
