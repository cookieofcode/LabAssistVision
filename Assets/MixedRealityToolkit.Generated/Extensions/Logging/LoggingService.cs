using System;
using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
	[MixedRealityExtensionService(SupportedPlatforms.WindowsStandalone|SupportedPlatforms.WindowsUniversal)]
    [Obsolete]
	public class LoggingService : BaseExtensionService, ILoggingService, IMixedRealityExtensionService
	{
		private LoggingServiceProfile loggingServiceProfile;
        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull] private readonly Logger _logger;

		public LoggingService(string name,  uint priority,  BaseMixedRealityProfile profile) : base(name, priority, profile) 
		{
			loggingServiceProfile = profile as LoggingServiceProfile;
            _logger = new Logger(new LogHandler());
        }

        public Logger GetLogger()
        {
            return _logger;
        }
    }
}
