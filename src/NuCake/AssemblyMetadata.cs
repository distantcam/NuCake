using System;
using System.Linq;
using System.Reflection;

namespace NuCake
{
    public class AssemblyMetadata : MarshalByRefObject
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string InformationalVersion { get; private set; }
        public string Copyright { get; private set; }
        public string Culture { get; private set; }

        public void LoadMetadata(string path)
        {
            var assembly = Assembly.ReflectionOnlyLoadFrom(path);

            SetIfAvailable<AssemblyInformationalVersionAttribute>(assembly, s => InformationalVersion = s);
            SetIfAvailable<AssemblyDescriptionAttribute>(assembly, s => Description = s);
            SetIfAvailable<AssemblyTitleAttribute>(assembly, s => Title = s);
            SetIfAvailable<AssemblyCopyrightAttribute>(assembly, s => Copyright = s);
            SetIfAvailable("NugetVersionAttribute", assembly, s => InformationalVersion = s);

            Culture = assembly.GetName().CultureInfo.ToString();
        }

        private void SetIfAvailable<TAttribute>(Assembly assembly, Action<string> setter)
        {
            var attribute = assembly.GetCustomAttributesData<TAttribute>();
            if (attribute != null)
                setter((string)attribute.ConstructorArguments[0].Value);
        }

        private void SetIfAvailable(string typeName, Assembly assembly, Action<string> setter)
        {
            var attribute = assembly.GetCustomAttributesData(typeName);
            if (attribute != null)
                setter((string)attribute.ConstructorArguments[0].Value);
        }
    }
}