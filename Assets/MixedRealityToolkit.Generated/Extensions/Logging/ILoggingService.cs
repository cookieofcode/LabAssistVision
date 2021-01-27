using System;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
	public interface ILoggingService : IMixedRealityExtensionService
    {
        Logger GetLogger();
    }
}