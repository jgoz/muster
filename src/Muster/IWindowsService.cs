﻿namespace Muster
{
	using System;

	/// <summary>
	/// Represents a logical windows service.
	/// </summary>
	public interface IWindowsService : IDisposable
	{
		/// <summary>
		/// Called when the service gets a request to start.
		/// </summary>
		/// <param name="args">Any startup arguments required by the service.</param>
		void OnStart(String[] args);

		/// <summary>
		/// Called when the service gets a request to stop.
		/// </summary>
		void OnStop();

		/// <summary>
		/// Called when a service gets a request to pause, but not stop completely.
		/// </summary>
		void OnPause();

		/// <summary>
		/// Called when a service gets a request to resume from a paused state.
		/// </summary>
		void OnContinue();

		/// <summary>
		/// Called when the machine the service is running on is shut down.
		/// </summary>
		void OnShutdown();
	}
}
