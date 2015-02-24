using System;
using System.IO;
using System.Reflection;

namespace NuCake
{
    public class AppDomainManager : IDisposable
    {
        private readonly AppDomain appDomain;

        public AppDomainManager(string basePath)
        {
            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = basePath;

            appDomain = AppDomain.CreateDomain(Path.GetFileName(Path.GetDirectoryName(basePath)) + "_AppDomain", null, domainSetup);
            appDomain.ReflectionOnlyAssemblyResolve += (sender, args) => Assembly.ReflectionOnlyLoad(args.Name);
        }

        public T CreateInstanceAndUnwrap<T>()
        {
            return (T)appDomain.CreateInstanceAndUnwrap(
                typeof(T).Assembly.FullName,
                typeof(T).FullName);
        }

        private void DisposeManaged()
        {
            if (appDomain != null)
                AppDomain.Unload(appDomain);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}