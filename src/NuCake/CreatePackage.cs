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
        public ITaskItem ReferenceLibrary { get; set; }

        [Required]
        public string DestinationFolder { get; set; }

        public ITaskItem[] SourceFiles { get; set; }

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

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(ReferenceLibrary.ItemSpec);

            var packageBuilder = new PackageBuilder();

            packageBuilder.Id = Path.GetFileNameWithoutExtension(ReferenceLibrary.ItemSpec);
            packageBuilder.Authors.Add(fileVersionInfo.CompanyName.OrDefault(Environment.UserName));
            packageBuilder.Description = fileVersionInfo.FileDescription;

            packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = ReferenceLibrary.ItemSpec, Target = "lib" } });

            var xmldoc = Path.ChangeExtension(ReferenceLibrary.ItemSpec, ".xml");
            if (File.Exists(xmldoc))
                packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = xmldoc, Target = "lib" } });

            SemanticVersion version;
            using (var appDomainManager = new AppDomainManager(Path.GetDirectoryName(GetType().Assembly.Location)))
            {
                var metadata = appDomainManager.CreateInstanceAndUnwrap<AssemblyMetadata>();
                metadata.LoadMetadata(ReferenceLibrary.ItemSpec);

                if (String.IsNullOrEmpty(metadata.InformationalVersion) || !SemanticVersion.TryParse(metadata.InformationalVersion, out version))
                    version = SemanticVersion.Parse(fileVersionInfo.FileVersion);

                if (!String.IsNullOrEmpty(metadata.Description))
                    packageBuilder.Description = metadata.Description;
            }
            packageBuilder.Version = version;

            if (String.IsNullOrWhiteSpace(packageBuilder.Description))
            {
                packageBuilder.Description = "No Description";
                Log.LogWarning("No description found. Add either a AssemblyTitleAttribute or AssemblyDescriptionAttribute to your project.");
            }

            if (String.IsNullOrWhiteSpace(fileVersionInfo.CompanyName))
            {
                packageBuilder.Authors.Add(Environment.UserName);
                Log.LogWarning("No company name found. Add a AssemblyCompanyAttribute to your project.");
            }
            else
            {
                packageBuilder.Authors.Add(fileVersionInfo.CompanyName);
            }

            SavePackage(packageBuilder, ".nupkg", "Package created -> {0}");

            // Now create the symbol package

            if (SourceFiles.Any())
            {
                var pdb = Path.ChangeExtension(ReferenceLibrary.ItemSpec, ".pdb");
                if (File.Exists(pdb))
                    packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = pdb, Target = "lib" } });

                foreach (var sourceFile in SourceFiles)
                {
                    var fullPath = Path.GetFullPath(sourceFile.ItemSpec);

                    if (File.Exists(sourceFile.ItemSpec) && fullPath.StartsWith(Environment.CurrentDirectory))
                        packageBuilder.PopulateFiles("", new ManifestFile[] {
                            new ManifestFile() { Source = fullPath, Target = Path.Combine("src", fullPath.Substring(Environment.CurrentDirectory.Length + 1)) }
                        });
                }

                SavePackage(packageBuilder, ".symbols.nupkg", "Symbols created -> {0}");
            }
        }

        private void SavePackage(PackageBuilder packageBuilder, string filenameSuffix, string logMessage)
        {
            var filename = Path.Combine(DestinationFolder, packageBuilder.GetFullName()) + filenameSuffix;
            using (var file = new FileStream(filename, FileMode.Create))
            {
                packageBuilder.Save(file);
            }

            Log.LogMessage(logMessage, filename);
        }
    }
}