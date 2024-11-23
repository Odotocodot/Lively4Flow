using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Lively
{
	public class ResultCreator
	{
		public static List<Result> SingleResult(string title, string iconPath, Action action, bool closeOnAction) =>
			new()
			{
				new Result
				{
					Title = title,
					IcoPath = iconPath,
					Action = _ =>
					{
						action();
						return closeOnAction;
					}
				}
			};
	}
}