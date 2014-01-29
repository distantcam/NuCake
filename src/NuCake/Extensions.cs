using System;
using System.Linq;
using System.Reflection;

namespace NuCake
{
    public static class Extensions
    {
        public static string OrDefault(this string str, string @default)
        {
            if (String.IsNullOrWhiteSpace(str))
                return @default;

            return str;
        }

        public static CustomAttributeData GetCustomAttributesData<T>(this Assembly assembly)
        {
            return assembly.GetCustomAttributesData()
                .FirstOrDefault(d => d.Constructor.DeclaringType == typeof(T));
        }
    }
}