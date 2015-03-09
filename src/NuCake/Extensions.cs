using System;
using System.Linq;
using System.Reflection;

namespace NuCake
{
    public static class Extensions
    {
        public static CustomAttributeData GetCustomAttributesData<T>(this Assembly assembly)
        {
            return assembly.GetCustomAttributesData()
                .FirstOrDefault(d => d.Constructor.DeclaringType == typeof(T));
        }

        public static CustomAttributeData GetCustomAttributesData(this Assembly assembly, string typeName)
        {
            return assembly.GetCustomAttributesData()
                .FirstOrDefault(d => string.Equals(d.Constructor.DeclaringType.Name, typeName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}