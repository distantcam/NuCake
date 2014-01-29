using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet;

namespace NuCake
{
    public class CreatePackage : Task
    {
        static CreatePackage()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => GetAssembly(args.Name);
        }

        static Assembly GetAssembly(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == name);
        }

        [Required]
        public ITaskItem SourceFile { get; set; }

        [Required]
        public string DestinationFolder { get; set; }

        public override bool Execute()
        {
            try
            {
                InnerExecute();
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException vex)
            {
                foreach (var line in Regex.Split(vex.Message, Environment.NewLine))
                    Log.LogError(line);
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true, true, null);
            }

            return !Log.HasLoggedErrors;
        }

        private void InnerExecute()
        {
            Directory.CreateDirectory(DestinationFolder);

            var packageBuilder = new PackageBuilder();

            packageBuilder.Id = Path.GetFileNameWithoutExtension(SourceFile.ItemSpec);
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(SourceFile.ItemSpec);
            packageBuilder.Authors.Add(fileVersionInfo.CompanyName.OrDefault(Environment.UserName));
            packageBuilder.Description = fileVersionInfo.FileDescription.OrDefault("No Description");

            packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = SourceFile.ItemSpec, Target = "lib" } });

            SemanticVersion version;
            using (var appDomainManager = new AppDomainManager(Path.GetDirectoryName(GetType().Assembly.Location)))
            {
                var metadata = appDomainManager.CreateInstanceAndUnwrap<AssemblyMetadata>();
                metadata.LoadMetadata(SourceFile.ItemSpec);

                if (String.IsNullOrEmpty(metadata.InformationalVersion) || !SemanticVersion.TryParse(metadata.InformationalVersion, out version))
                    version = SemanticVersion.Parse(fileVersionInfo.FileVersion);

                if (!String.IsNullOrEmpty(metadata.Description))
                    packageBuilder.Description = metadata.Description;
            }

            packageBuilder.Version = version;

            var packageFile = Path.Combine(DestinationFolder, packageBuilder.GetFullName()) + ".nupkg";
            using (var file = new FileStream(packageFile, FileMode.Create))
            {
                packageBuilder.Save(file);
            }

            Log.LogMessage("Package created -> {0}", packageFile);
        }
    }
}