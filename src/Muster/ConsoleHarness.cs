namespace Muster
{
	using System;
	using System.Threading;

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

		/// <summary>
		/// Run a given <see cref="IWindowsService"/> using the console harness.
		/// </summary>
		/// <param name="args">Arguments to pass to the service <see cref="IWindowsService.OnStart"/> method.</param>
		/// <param name="service">The service to run.</param>
		public static void Run(String[] args, IWindowsService service)
		{
			ServiceState state = ServiceState.Running;

			using (service)
			{
				service.OnStart(args);

				while (state != ServiceState.Stopped)
				{
					Console.WriteLine("[muster] Currently {0}: [Q]uit [P]ause [R]esume", state);

					while (!Console.KeyAvailable)
						Thread.Sleep(250);

					TryHandleConsoleInput(service, Console.ReadKey(true).Key, ref state);
				}

				service.OnStop();
			}
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
