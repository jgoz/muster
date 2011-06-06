namespace Muster
{
	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Configuration.Install;
	using System.Linq;
	using System.Reflection;
	using System.ServiceProcess;

	/// <summary>
	/// A generic windows service installer.
	/// </summary>
	[RunInstaller(true)]
	public partial class WindowsServiceInstaller : Installer
	{
		/// <summary>
		/// Gets or sets the Windows service configuration to use.
		/// </summary>
		public WindowsServiceAttribute Configuration { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsServiceInstaller"/> class with
		/// the Type of the Windows service to install.
		/// </summary>
		/// <param name="windowsServiceType">
		/// Type of the Windows service to install, which must implement <see cref="IWindowsService"/>
		/// and must have a <see cref="WindowsServiceAttribute"/> class attribute.
		/// </param>
		public WindowsServiceInstaller(Type windowsServiceType)
		{
			if (!windowsServiceType.GetInterfaces().Contains(typeof(IWindowsService)))
				throw new ArgumentException("Type to install must implement IWindowsService.", "windowsServiceType");

			var attribute = windowsServiceType.GetAttribute<WindowsServiceAttribute>();

			if (attribute == null)
				throw new ArgumentException("Type to install must be marked with a WindowsServiceAttribute.", "windowsServiceType");

			Configuration = attribute;
		}

		/// <summary>
		/// Performs a transacted installation at run-time of the <see cref="WindowsServiceInstaller"/>
		/// using the assembly path from <see cref="Assembly.GetEntryAssembly"/>.
		/// </summary>
		/// <typeparam name="TWindowsService">Type of the Windows service to install.</typeparam>
		public static void RuntimeInstall<TWindowsService>()
				where TWindowsService : IWindowsService
		{
			RuntimeInstall(Assembly.GetEntryAssembly(), typeof(TWindowsService));
		}

		/// <summary>
		/// Performs a transacted installation at run-time of the <see cref="WindowsServiceInstaller"/>
		/// for a given assembly and service type.
		/// </summary>
		/// <param name="serviceAssembly">Assembly containing the Windows service to install.</param>
		/// <param name="serviceType">Type of the Windows service to install.</param>
		public static void RuntimeInstall(Assembly serviceAssembly, Type serviceType)
		{
			String path = "/assemblypath=" + serviceAssembly.Location;

			using (var ti = new TransactedInstaller())
			{
				ti.Installers.Add(new WindowsServiceInstaller(serviceType));
				ti.Context = new InstallContext(null, new[] { path });
				ti.Install(new Hashtable());
			}
		}

		/// <summary>
		/// Performs a transacted uninstallation at run-time of the <see cref="WindowsServiceInstaller"/>
		/// using the assembly path from <see cref="Assembly.GetEntryAssembly"/>.
		/// </summary>
		/// <typeparam name="TWindowsService">Type of the Windows service to uninstall.</typeparam>
		public static void RuntimeUninstall<TWindowsService>()
				where TWindowsService : IWindowsService
		{
			RuntimeUninstall(Assembly.GetEntryAssembly(), typeof(TWindowsService));
		}

		/// <summary>
		/// Performs a transacted uninstallation at run-time of the <see cref="WindowsServiceInstaller"/>
		/// for a given assembly and service type.
		/// </summary>
		/// <param name="serviceAssembly">Assembly containing the Windows service to uninstall.</param>
		/// <param name="serviceType">Type of the Windows service to uninstall.</param>
		public static void RuntimeUninstall(Assembly serviceAssembly, Type serviceType)
		{
			String path = "/assemblypath=" + serviceAssembly.Location;

			using (var ti = new TransactedInstaller())
			{
				ti.Installers.Add(new WindowsServiceInstaller(serviceType));
				ti.Context = new InstallContext(null, new[] { path });
				ti.Uninstall(null);
			}
		}


		/// <summary>
		/// Install the service.
		/// </summary>
		/// <param name="stateSaver">An <see cref="IDictionary"/> used to save information needed to perform a commit, rollback, or uninstall operation.</param>
		public override void Install(IDictionary stateSaver)
		{
			Console.WriteLine("Installing service {0}.", Configuration.Name);

			ConfigureInstallers();
			base.Install(stateSaver);
		}

		/// <summary>
		/// Uninstall the service.
		/// </summary>
		/// <param name="stateSaver">An <see cref="IDictionary"/> used to save information needed to perform a commit, rollback, or uninstall operation.</param>
		public override void Uninstall(IDictionary stateSaver)
		{
			Console.WriteLine("Un-Installing service {0}.", Configuration.Name);

			ConfigureInstallers();
			base.Uninstall(stateSaver);
		}

		private void ConfigureInstallers()
		{
			Installers.Add(ConfigureProcessInstaller());
			Installers.Add(ConfigureServiceInstaller());
		}

		private ServiceProcessInstaller ConfigureProcessInstaller()
		{
			return String.IsNullOrWhiteSpace(Configuration.UserName)
				? new ServiceProcessInstaller { Account = ServiceAccount.LocalService, Username = null, Password = null }
				: new ServiceProcessInstaller { Account = ServiceAccount.User, Username = Configuration.UserName, Password = Configuration.Password };
		}

		private ServiceInstaller ConfigureServiceInstaller()
		{
			return new ServiceInstaller
			{
				ServiceName = Configuration.Name,
				DisplayName = Configuration.DisplayName,
				Description = Configuration.Description,
				StartType = Configuration.StartMode,
			};
		}
	}
}
