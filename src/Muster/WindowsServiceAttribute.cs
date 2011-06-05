namespace Muster
{
	using System;
	using System.ServiceProcess;

	/// <summary>
	/// Marks an IWindowsService with configuration and installation attributes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class WindowsServiceAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the service.
		/// </summary>
		public String Name { get; set; }

		/// <summary>
		/// Gets or sets the displayable name that shows in service manager (defaults to <see cref="Name"/>).
		/// </summary>
		public String DisplayName { get; set; }

		/// <summary>
		/// Gets or sets the long description of the service (defaults to <see cref="Name"/>).
		/// </summary>
		public String Description { get; set; }

		/// <summary>
		/// Gets or sets the username used to run the service (defaults to null).
		/// A null or empty UserName field causes the service to run as ServiceAccount.LocalService.
		/// </summary>
		public String UserName { get; set; }

		/// <summary>
		/// Gets or sets the password used to run the service (defaults to null).
		/// Ignored if <see cref="UserName"/> is empty or null.
		/// </summary>
		public String Password { get; set; }

		/// <summary>
		/// Gets or sets the service event log source (defaults to null). If empty or null,
		/// no event log source is set. If set, start and stop events will be logged at the given source.
		/// </summary>
		public String EventLogSource { get; set; }

		/// <summary>
		/// Gets or sets the service start method on system boot (defaults to <see cref="ServiceStartMode.Manual"/>). 
		/// </summary>
		public ServiceStartMode StartMode { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the service supports the Pause and Continue events (defaults to true).
		/// </summary>
		public Boolean CanPauseAndContinue { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the service supports the Shutdown event (defaults to true).
		/// </summary>
		public Boolean CanShutdown { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the service supports the Stop event (defaults to true).
		/// </summary>
		public Boolean CanStop { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsServiceAttribute"/> class.
		/// </summary>
		/// <param name="name">Name of the service.</param>
		public WindowsServiceAttribute(String name)
		{
			Name = name;
			Description = name;
			DisplayName = name;

			CanStop = true;
			CanShutdown = true;
			CanPauseAndContinue = true;
			StartMode = ServiceStartMode.Manual;
			EventLogSource = null;
			Password = null;
			UserName = null;
		}
	}
}
