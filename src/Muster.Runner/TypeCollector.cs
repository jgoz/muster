namespace Muster.Runner
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	public class TypeCollector
	{
		private readonly IEnumerable<String> _assemblies;
		private readonly IEnumerable<String> _typeNames;
		private readonly Boolean _filterTypes;

		public TypeCollector(IEnumerable<String> assemblies, IEnumerable<String> typeNames)
		{
			if (assemblies == null)
				throw new ArgumentNullException("assemblies");

			_assemblies = assemblies;
			_typeNames = typeNames;
			_filterTypes = typeNames != null;
		}

		public IEnumerable<Type> CollectConcreteTypes<T>()
		{
			var foundTypes = new List<Type>();

			foreach (var assemblyPath in _assemblies)
			{
				Assembly assembly = Assembly.Load(AssemblyName.GetAssemblyName(Path.GetFullPath(assemblyPath)));

				var serviceTypes = assembly.GetTypes()
					.Where(t => t.GetInterfaces().Contains(typeof(T)))
					.Where(t => t.IsClass && !t.IsAbstract);

				if (!serviceTypes.Any())
					throw new InvalidOperationException(String.Format("Unable to find {0} implementors in assembly {1}", typeof(T).Name, assemblyPath));

				if (_filterTypes)
					serviceTypes = serviceTypes.Where(t => _typeNames.Contains(t.Name) || (t.FullName != null && _typeNames.Contains(t.FullName)));

				foundTypes.AddRange(serviceTypes);
			}

			if (_filterTypes && foundTypes.Count != _typeNames.Count())
			{
				var names = foundTypes.Select(t => t.Name);
				var fullNames = foundTypes.Select(t => t.FullName);

				throw new InvalidOperationException("Unable to find service type(s): " + String.Join(", ", _typeNames.Where(name => !names.Contains(name) && !fullNames.Contains(name))));
			}

			return foundTypes;
		}
	}
}
