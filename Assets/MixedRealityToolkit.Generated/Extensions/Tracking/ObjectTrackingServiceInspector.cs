#if UNITY_EDITOR
using System;
using Microsoft.MixedReality.Toolkit.Editor;

namespace Microsoft.MixedReality.Toolkit.Extensions.Editor
{	
	[MixedRealityServiceInspector(typeof(IObjectTrackingService))]
	public class ObjectTrackingServiceInspector : BaseMixedRealityServiceInspector
	{
		public override void DrawInspectorGUI(object target)
		{
			ObjectTrackingService service = (ObjectTrackingService)target;
			
			// Draw inspector here
		}
	}
}

#endif