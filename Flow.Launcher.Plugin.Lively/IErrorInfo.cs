using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively
{
	public interface IErrorInfo
	{
		List<string> Errors { get; }
	}
}