namespace Muster.Runner
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.ServiceProcess;

	public class ServiceRunner : MarshalByRefObject
	{
		public ServiceRunner()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
		}

		public void InstallServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			foreach (var type in CollectServiceTypes(assemblies, typeNames))
			{
				Console.WriteLine("Installing {0} from assembly {1}...", type.Name, type.Assembly.FullName);
				WindowsServiceInstaller.RuntimeInstall(type.Assembly, type);
			}
		}

		public void UninstallServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			foreach (var type in CollectServiceTypes(assemblies, typeNames))
			{
				Console.WriteLine("Uninstalling {0} from assembly {1}...", type.Name, type.Assembly.FullName);
				WindowsServiceInstaller.RuntimeUninstall(type.Assembly, type);
			}
		}

		public void RunServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			var serviceGroup = new WindowsServiceGroup();

			foreach (var type in CollectServiceTypes(assemblies, typeNames))
			{
				var service = Activator.CreateInstance(type) as IWindowsService;

				if (service == null)
					throw new InvalidOperationException(String.Format("Unable to instantiate {0} from assembly {1}", type.Name, type.Assembly.FullName));

				serviceGroup.Add(service);
			}

			// TODO: Service params?

			if (Environment.UserInteractive)
				ConsoleHarness.Run(new String[] { }, serviceGroup);
			else
				ServiceBase.Run(new WindowsServiceHarness(serviceGroup));
		}

		private static IEnumerable<Type> CollectServiceTypes(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			var foundTypes = new List<Type>();

			foreach (var assemblyPath in assemblies)
			{
				Assembly assembly = Assembly.LoadFile(Path.GetFullPath(assemblyPath));

				var serviceTypes = assembly.GetExportedTypes()
					.Where(t => t.GetInterfaces().Contains(typeof(IWindowsService)))
					.Where(t => t.IsClass && !t.IsAbstract);

				if (!serviceTypes.Any())
					throw new InvalidOperationException("Unable to find IWindowsService implementors in assembly " + assemblyPath);

				if (typeNames != null)
					serviceTypes = serviceTypes.Where(t => typeNames.Contains(t.Name) || (t.FullName != null && typeNames.Contains(t.FullName)));

				foundTypes.AddRange(serviceTypes);
			}

			if (typeNames != null && foundTypes.Count != typeNames.Count())
			{
				var names = foundTypes.Select(t => t.Name);
				var fullNames = foundTypes.Select(t => t.FullName);

				throw new InvalidOperationException("Unable to find service type(s): " + String.Join(", ", typeNames.Where(name => !names.Contains(name) && !fullNames.Contains(name))));
			}

			return foundTypes;
		}

		private static Assembly CurrentDomainAssemblyResolve(Object sender, ResolveEventArgs args)
		{
			return Assembly.Load(args.Name);
		}
	}
}
