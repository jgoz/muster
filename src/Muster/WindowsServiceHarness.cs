namespace Muster
{
	using System;
	using System.ServiceProcess;

	/// <summary>
	/// A generic Windows Service wrapper for any class that implements <see cref="IWindowsService"/>.
	/// </summary>
	public partial class WindowsServiceHarness : ServiceBase
	{
		private readonly IWindowsService _service;

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsServiceHarness"/> class.
		/// </summary>
		/// <remarks>Default constructor to satisfy designer.</remarks>
		internal WindowsServiceHarness()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsServiceHarness"/> class.
		/// </summary>
		/// <param name="service">A logical Windows service.</param>
		public WindowsServiceHarness(IWindowsService service)
			: this()
		{
			if (service == null)
				throw new ArgumentNullException("service", "IWindowsService cannot be null.");

			_service = service;
			ConfigureServiceFromAttributes();
		}

		/// <summary>
		/// Gets the underlying service implementation.
		/// </summary>
		public IWindowsService ServiceImplementation { get { return _service; } }

		protected override void OnStart(String[] args)
		{
			_service.OnStart(args);
		}

		protected override void OnStop()
		{
			_service.OnStop();
		}

		protected override void OnPause()
		{
			_service.OnPause();
		}

		protected override void OnContinue()
		{
			_service.OnContinue();
		}

		protected override void OnShutdown()
		{
			_service.OnShutdown();
		}

		private void ConfigureServiceFromAttributes()
		{
			var attribute = _service.GetType().GetAttribute<WindowsServiceAttribute>();

			if (attribute == null)
				throw new InvalidOperationException(String.Format("IWindowsService implementer {0} must have a WindowsServiceAttribute.", _service.GetType().FullName));

			EventLog.Source = String.IsNullOrEmpty(attribute.EventLogSource) ? "WindowsServiceHarness" : attribute.EventLogSource;

			ServiceName = attribute.Name;
			CanStop = attribute.CanStop;
			CanPauseAndContinue = attribute.CanPauseAndContinue;
			CanShutdown = attribute.CanShutdown;

			// we don't handle: laptop power change event
			CanHandlePowerEvent = false;

			// we don't handle: Term Services session event
			CanHandleSessionChangeEvent = false;

			// always auto-event-log 
			AutoLog = true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_service.Dispose();

				if (_components != null)
					_components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
