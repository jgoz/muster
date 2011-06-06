namespace Muster.Runner
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.ServiceProcess;

	public class ServiceRunner : MarshalByRefObject
	{
		public ServiceRunner()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => Assembly.Load(args.Name);
		}

		public void InstallServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			var typeCollector = new TypeCollector(assemblies, typeNames);

			foreach (var type in typeCollector.CollectConcreteTypes<IWindowsService>())
			{
				Console.WriteLine("Installing {0} from assembly {1}...", type.Name, type.Assembly.FullName);
				WindowsServiceInstaller.RuntimeInstall(type.Assembly, type);
			}
		}

		public void UninstallServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			var typeCollector = new TypeCollector(assemblies, typeNames);

			foreach (var type in typeCollector.CollectConcreteTypes<IWindowsService>())
			{
				Console.WriteLine("Uninstalling {0} from assembly {1}...", type.Name, type.Assembly.FullName);
				WindowsServiceInstaller.RuntimeUninstall(type.Assembly, type);
			}
		}

		public void RunServices(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			var typeCollector = new TypeCollector(assemblies, typeNames);
			var serviceGroup = new WindowsServiceGroup();

			foreach (var type in typeCollector.CollectConcreteTypes<IWindowsService>())
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
	}
}
