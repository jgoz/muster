namespace Muster.Test
{
	using System;

	public class TestService : IWindowsService
	{
		public void OnStart(string[] args)
		{
			Console.WriteLine("We are starting.");
		}

		public void OnStop()
		{
			Console.WriteLine("We are stopping.");
		}

		public void OnPause()
		{
			Console.WriteLine("We are pausing.");
		}

		public void OnContinue()
		{
			Console.WriteLine("We are resuming.");
		}

		public void OnShutdown()
		{
			Console.WriteLine("We are shutting down.");
		}

		public void Dispose()
		{
		}
	}
}
