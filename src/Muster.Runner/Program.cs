﻿namespace Muster.Runner
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Mono.Options;

	class Program
	{
		private static readonly String ProcessName = Assembly.GetExecutingAssembly().GetName().Name;
		private static readonly List<String> BinPath = new List<String> { "." };

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
				{ "p|bin-path=", "Additional path to search for dependent assemblies", v => BinPath.AddRange(v.Split(';')) },
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

			if (!File.Exists(configPath))
				Die("Config file not found: {0}", configPath);

			AppDomain serviceAppDomain = CreateServiceAppDomain(configPath);
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

			var serviceHandler = (ServiceHandler)serviceAppDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(ServiceHandler).FullName);

			try
			{
				if (install)
				{
					serviceHandler.InstallServices(assemblies, typeNames);
					return;
				}

				if (uninstall)
				{
					serviceHandler.UninstallServices(assemblies, typeNames);
					return;
				}

				serviceHandler.RunServices(assemblies, typeNames);

				AppDomain.Unload(serviceAppDomain);
			}
			catch (TargetInvocationException ex)
			{
				Console.WriteLine(ex);
				Die(ex.InnerException.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Die(ex.Message);
			}
		}

		static AppDomain CreateServiceAppDomain(String configPath)
		{
			var setup = AppDomain.CurrentDomain.SetupInformation;

			setup.ApplicationBase = Environment.CurrentDirectory;

			setup.CachePath = Path.Combine(Environment.CurrentDirectory, ".shadow");
			setup.ShadowCopyDirectories = null;
			setup.ShadowCopyFiles = "true";

			if (configPath != null)
				setup.ConfigurationFile = Path.GetFullPath(configPath);

			return AppDomain.CreateDomain("MusterServiceAppDomain", null, setup);
		}

		static Assembly ResolveAssembly(Object sender, ResolveEventArgs args)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.FullName == args.Name)
					return assembly;
			}

			foreach (String fullPath in BinPath.Select(Path.GetFullPath))
			{
				// TODO: Try other extensions?
				String assemblyFileName = new AssemblyName(args.Name).Name + ".dll";

				Assembly assembly = Assembly.LoadFrom(Path.Combine(fullPath, assemblyFileName));

				if (assembly != null)
					return assembly;
			}

			return null;
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
