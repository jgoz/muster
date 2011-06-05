namespace Muster
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal static class TypeExtensions
	{
		public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type typeWithAttributes)
				where TAttribute : Attribute
		{
			Object[] configAttributes = Attribute.GetCustomAttributes(typeWithAttributes, typeof(TAttribute), false);

			if (configAttributes != null)
			{
				foreach (TAttribute attribute in configAttributes)
					yield return attribute;
			}
		}

		public static T GetAttribute<T>(this Type typeWithAttributes)
				where T : Attribute
		{
			return GetAttributes<T>(typeWithAttributes).FirstOrDefault();
		}
	}
}