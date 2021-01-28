using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace LabAssistVision
{
    public class Label : MonoBehaviour
    {
        /// <summary>
        /// The MRTK ToolTip to provide a label.
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull] public ToolTip tooltip;

        /// <summary>
        /// The <see cref="Material"/> used for the <see cref="ToolTip"/> to change the color to blue.
        /// </summary>
        public Material blueMaterial;

        /// <summary>
        /// The <see cref="Material"/> used for the <see cref="ToolTip"/> to change the color to green.
        /// </summary>
        public Material greenMaterial;

        /// <summary>
        /// The <see cref="Renderer"/> of the <see cref="IToolTipBackground"/>.
        /// </summary>
        public MeshRenderer meshRenderer;

        /// <summary>
        /// Indicates the current color. <code>true</code> indicates green, <code>false</code> blue.
        /// </summary>
        private bool _color;

        private void Start()
        {
            Assert.IsNotNull(tooltip);
            Assert.IsNotNull(blueMaterial);
            Assert.IsNotNull(greenMaterial);
            Assert.IsNotNull(meshRenderer);
        }

        public void UpdatePosition(Vector3 position)
        {
            transform.position = position;
        }

        public void UpdateText(string text)
        {
            tooltip.ToolTipText = text;
        }

        /// <summary>
        /// Change the color of the label between green and blue.
        /// </summary>
        public void ChangeColor()
        {
            _color = !_color;
            meshRenderer.sharedMaterial = _color ? greenMaterial : blueMaterial;
        }
    }
}
