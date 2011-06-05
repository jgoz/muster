namespace Muster
{
	using System;

	/// <summary>
	/// Run a service from the console given a service implementation
	/// </summary>
	public static class ConsoleHarness
	{
		private enum ServiceState
		{
			Running,
			Paused,
			Stopped
		}

		public static void Run(String[] args, IWindowsService service)
		{
			ServiceState state = ServiceState.Running;

			service.OnStart(args);

			while (state != ServiceState.Stopped)
			{
				Console.WriteLine("Currently {0}. [Q]uit, [P]ause, [R]esume: ", state);
				TryHandleConsoleInput(service, Console.ReadKey(true).Key, ref state);
			}

			service.OnStop();
		}

		private static void TryHandleConsoleInput(IWindowsService service, ConsoleKey key, ref ServiceState state)
		{
			switch (key)
			{
				case ConsoleKey.Q:
					state = ServiceState.Stopped;
					break;

				case ConsoleKey.P:
					service.OnPause();
					state = ServiceState.Paused;
					break;

				case ConsoleKey.R:
					service.OnContinue();
					state = ServiceState.Running;
					break;
			}
		}
	}
}
