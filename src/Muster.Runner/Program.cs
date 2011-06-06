namespace Muster.Runner
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Mono.Options;

	class Program
	{
		private static readonly String ProcessName = Assembly.GetExecutingAssembly().GetName().Name;

		static void Main(String[] args)
		{
			Boolean install = false;
			Boolean uninstall = false;
			String[] typeNames = null;
			String configPath = null;
			Boolean showHelp = false;

			var options = new OptionSet
			{
				{ "i|install", "Install the specified service(s)", v => install = v != null },
				{ "u|uninstall", "Uninstall the specified service(s)", v => uninstall = v != null },
				{ "t|types=", "Comma-separated list of individual service types", v => typeNames = v.Split(',') },
				{ "c|config=", "Path to service configuration file", v => configPath = v },
				{ "h|help", "Show this message and exit", v => showHelp = v != null },
			};

			var assemblies = new List<String>();
			try
			{
				assemblies = options.Parse(args);
			}
			catch (OptionException e)
			{
				Die(e.Message);
			}

			if (showHelp)
			{
				ShowHelp(options);
				return;
			}

			if (assemblies.Count == 0)
				Die("At least one assembly is required.");

			if (install && uninstall)
				Die("Cannot install and uninstall at the same time.");

			var serviceAppDomain = CreateServiceAppDomain(configPath);

			Type serviceRunnerType = typeof(ServiceRunner);
			var serviceRunner = (ServiceRunner)serviceAppDomain.CreateInstanceAndUnwrap(serviceRunnerType.Assembly.FullName, serviceRunnerType.FullName);

			try
			{
				if (install)
				{
					serviceRunner.InstallServices(assemblies, typeNames);
					return;
				}

				if (uninstall)
				{
					serviceRunner.UninstallServices(assemblies, typeNames);
					return;
				}

				serviceRunner.RunServices(assemblies, typeNames);
			}
			catch (Exception ex)
			{
				Die(ex.Message);
			}
		}

		static AppDomain CreateServiceAppDomain(String configPath)
		{
			var setup = new AppDomainSetup { ApplicationBase = Environment.CurrentDirectory };

			if (configPath != null)
				setup.ConfigurationFile = Path.GetFullPath(configPath);

			return AppDomain.CreateDomain("MusterServiceAppDomain", null, setup);
		}

		static void ShowHelp(OptionSet options)
		{
			Console.WriteLine("Usage: {0} [OPTIONS]+ Assembly[,Assembly...]", ProcessName);
			Console.WriteLine("Run/install Windows services from a list of assemblies.");
			Console.WriteLine("Services must implement Muster.IWindowsService.");
			Console.WriteLine("If no service types are specified, every eligible service will be used.");
			Console.WriteLine();
			Console.WriteLine("Options:");
			options.WriteOptionDescriptions(Console.Out);
		}

		static void Die(String message, params Object[] args)
		{
			Console.WriteLine("{0}: {1}", ProcessName, String.Format(message, args));
			Console.WriteLine("Try `{0} --help' for more information.", ProcessName);
			Environment.Exit(1);
		}
	}
}
