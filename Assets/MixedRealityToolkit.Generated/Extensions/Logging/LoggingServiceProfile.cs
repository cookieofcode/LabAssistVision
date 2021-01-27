using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
	[MixedRealityServiceProfile(typeof(ILoggingService))]
	[CreateAssetMenu(fileName = "LoggingServiceProfile", menuName = "MixedRealityToolkit/LoggingService Configuration Profile")]
	public class LoggingServiceProfile : BaseMixedRealityProfile
	{
		// Store config data in serialized fields
	}
}