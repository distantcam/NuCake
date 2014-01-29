using System;
using System.Linq;
using System.Reflection;

namespace NuCake
{
    public class AssemblyMetadata : MarshalByRefObject
    {
        public string Description { get; private set; }
        public string InformationalVersion { get; private set; }

        public void LoadMetadata(string path)
        {
            var assembly = Assembly.ReflectionOnlyLoadFrom(path);

            var infoVersion = assembly.GetCustomAttributesData<AssemblyInformationalVersionAttribute>();
            if (infoVersion != null)
                InformationalVersion = (string)infoVersion.ConstructorArguments[0].Value;

            var description = assembly.GetCustomAttributesData<AssemblyDescriptionAttribute>();
            if (description != null)
                Description = (string)description.ConstructorArguments[0].Value;
        }
    }
}