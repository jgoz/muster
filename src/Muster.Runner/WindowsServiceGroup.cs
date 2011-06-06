namespace Muster.Runner
{
	using System;
	using System.Collections.Generic;

	public class WindowsServiceGroup : IWindowsService
	{
		private readonly List<IWindowsService> _services;

		public WindowsServiceGroup()
		{
			_services = new List<IWindowsService>();
		}

		public void Add(IWindowsService service)
		{
			_services.Add(service);
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
