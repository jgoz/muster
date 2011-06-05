namespace Muster.Runner
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.ServiceProcess;
	using Mono.Options;

	class Program
	{
		private static readonly String ProcessName = Assembly.GetExecutingAssembly().GetName().Name;

		static void Main(String[] args)
		{
			Boolean install = false;
			Boolean uninstall = false;
			String[] typeNames = null;
			Boolean showHelp = false;

			var options = new OptionSet
			{
				{ "i|install", "Install the specified service(s)", v => install = v != null },
				{ "u|uninstall", "Uninstall the specified service(s)", v => uninstall = v != null },
				{ "t|types=", "Comma-separated list of individual service types", v => typeNames = v.Split(',') },
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

			if (install)
			{
				InstallServices(assemblies, typeNames);
				return;
			}

			if (uninstall)
			{
				UninstallServices(assemblies, typeNames);
				return;
			}

			RunServices(assemblies, typeNames);
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

		private static void InstallServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			foreach (var tuple in CollectServiceTypes(assemblies, typeNames))
			{
				Console.WriteLine("Installing {0} from assembly {1}...", tuple.Item2.Name, tuple.Item1.FullName);
				WindowsServiceInstaller.RuntimeInstall(tuple.Item1, tuple.Item2);
			}
		}

		private static void UninstallServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			foreach (var tuple in CollectServiceTypes(assemblies, typeNames))
			{
				Console.WriteLine("Uninstalling {0} from assembly {1}...", tuple.Item2.Name, tuple.Item1.FullName);
				WindowsServiceInstaller.RuntimeUninstall(tuple.Item1, tuple.Item2);
			}
		}

		private static void RunServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			var services = new List<IWindowsService>();

			foreach (var tuple in CollectServiceTypes(assemblies, typeNames))
			{
				var service = Activator.CreateInstance(tuple.Item2) as IWindowsService;

				if (service == null)
					Die("Unable to instantiate {0} from assembly {1}", tuple.Item2.Name, tuple.Item1.FullName);

				services.Add(service);
			}

			RunService(new WindowsServiceGroup(services));
		}

		static void RunService(IWindowsService implementation)
		{
			// TODO: Service params?

			if (Environment.UserInteractive)
				ConsoleHarness.Run(new String[] { }, implementation);
			else
				ServiceBase.Run(new WindowsServiceHarness(implementation));
		}

		static IEnumerable<Tuple<Assembly, Type>> CollectServiceTypes(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			var foundTypes = new List<Tuple<Assembly, Type>>();

			foreach (var assemblyPath in assemblies)
			{
				try
				{
					Assembly assembly = Assembly.LoadFile(Path.GetFullPath(assemblyPath));

					var serviceTypes = assembly.GetExportedTypes()
						.Where(t => t.GetInterfaces().Contains(typeof(IWindowsService)))
						.Where(t => t.IsClass && !t.IsAbstract);

					if (!serviceTypes.Any())
						Die("Unable to find IWindowsService implementors in assembly " + assemblyPath);

					if (typeNames != null)
						serviceTypes = serviceTypes.Where(t => typeNames.Contains(t.Name) || (t.FullName != null && typeNames.Contains(t.FullName)));

					foundTypes.AddRange(serviceTypes.Select(t => new Tuple<Assembly, Type>(assembly, t)));
				}
				catch (Exception ex)
				{
					Die(ex.Message);
				}
			}

			if (typeNames != null && foundTypes.Count != typeNames.Count())
			{
				var names = foundTypes.Select(t => t.Item2.Name);
				var fullNames = foundTypes.Select(t => t.Item2.FullName);

				Die("Unable to find service type(s): " + String.Join(", ", typeNames.Where(name => !names.Contains(name) && !fullNames.Contains(name))));
			}

			return foundTypes;
		}

		static void Die(String message, params Object[] args)
		{
			Console.WriteLine("{0}: {1}", ProcessName, String.Format(message, args));
			Console.WriteLine("Try `{0} --help' for more information.", ProcessName);
			Environment.Exit(1);
		}
	}
}
