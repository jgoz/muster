namespace Muster.Runner
{
	using System;
	using System.Collections.Generic;

	public class WindowsServiceGroup : IWindowsService
	{
		private readonly IEnumerable<IWindowsService> _services;

		public WindowsServiceGroup(IEnumerable<IWindowsService> services)
		{
			if (services == null)
				throw new ArgumentNullException("services");

			_services = services;
		}

		public void Dispose()
		{
			DoToAll(s => s.Dispose());
		}

		public void OnStart(String[] args)
		{
			DoToAll(s => s.OnStart(args));
		}

		public void OnStop()
		{
			DoToAll(s => s.OnStop());
		}

		public void OnPause()
		{
			DoToAll(s => s.OnPause());
		}

		public void OnContinue()
		{
			DoToAll(s => s.OnContinue());
		}

		public void OnShutdown()
		{
			DoToAll(s => s.OnShutdown());
		}

		private void DoToAll(Action<IWindowsService> action)
		{
			foreach (var service in _services)
				action(service);
		}
	}
}
