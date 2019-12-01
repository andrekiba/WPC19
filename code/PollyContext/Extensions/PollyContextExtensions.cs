using Microsoft.Extensions.Logging;
using Polly;

namespace PollyContext.Extensions
{
	public static class PollyContextExtensions
	{
		public static Context WithLogger(this Context context, ILogger logger)
		{
			context[PolicyContextKeys.Logger] = logger;
			return context;
		}

		public static ILogger GetLogger(this Context context)
		{
			if (context.TryGetValue(PolicyContextKeys.Logger, out var logger))
			{
				return logger as ILogger;
			}
			return null;
		}
	}
}
