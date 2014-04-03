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

        private static Assembly GetAssembly(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == name);
        }

        [Required]
        public ITaskItem ReferenceLibrary { get; set; }

        [Required]
        public ITaskItem DestinationFolder { get; set; }

        public ITaskItem ReferenceDirectory { get; set; }

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
            Directory.CreateDirectory(DestinationFolder.FullPath());

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(ReferenceLibrary.FullPath());

            var packageBuilder = new PackageBuilder();

            if (ReferenceDirectory != null)
            {
                var nuSpec = Directory.GetFiles(ReferenceDirectory.FullPath(), ReferenceLibrary.Filename() + ".nuspec", SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (nuSpec != null)
                    using (var stream = File.OpenRead(nuSpec))
                    {
                        var manifest = Manifest.ReadFrom(stream, NullPropertyProvider.Instance, false);
                        packageBuilder.Populate(manifest.Metadata);
                    }
            }

            packageBuilder.Id = Path.GetFileNameWithoutExtension(ReferenceLibrary.FullPath());
            packageBuilder.Description = fileVersionInfo.FileDescription;
            if (!String.IsNullOrWhiteSpace(fileVersionInfo.CompanyName))
            {
                packageBuilder.Authors.Add(fileVersionInfo.CompanyName);
            }

            if (ReferenceDirectory == null)
            {
                packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = ReferenceLibrary.FullPath(), Target = "lib" } });

                var xmldoc = Path.ChangeExtension(ReferenceLibrary.FullPath(), ".xml");
                if (File.Exists(xmldoc))
                    packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = xmldoc, Target = "lib" } });
            }
            else
            {
                var files = Directory.GetFiles(ReferenceDirectory.FullPath(), "*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".pdb") && !f.EndsWith(".nuspec"))
                    .Select(f => new ManifestFile() { Source = f, Target = f.Replace(ReferenceDirectory.FullPath(), "") })
                    .ToList();

                packageBuilder.PopulateFiles("", files);
            }

            using (var appDomainManager = new AppDomainManager(Path.GetDirectoryName(GetType().Assembly.Location)))
            {
                var metadata = appDomainManager.CreateInstanceAndUnwrap<AssemblyMetadata>();
                metadata.LoadMetadata(ReferenceLibrary.FullPath());
                ApplyToPackageBuilder(packageBuilder, metadata, fileVersionInfo.FileVersion);
            }

            if (String.IsNullOrWhiteSpace(packageBuilder.Description))
            {
                packageBuilder.Description = "No Description";
                Log.LogWarning("No description found. Add either a AssemblyTitleAttribute or AssemblyDescriptionAttribute to your project.");
            }

            if (!packageBuilder.Authors.Any())
            {
                packageBuilder.Authors.Add(Environment.UserName);
                Log.LogWarning("No company name found. Add a AssemblyCompanyAttribute to your project.");
            }

            SavePackage(packageBuilder, ".nupkg", "Package created -> {0}");

            CreateSourcePackage(packageBuilder);
        }

        private void CreateSourcePackage(PackageBuilder packageBuilder)
        {
            if (SourceFiles != null && SourceFiles.Any())
            {
                if (ReferenceDirectory != null)
                {
                    var files = Directory.GetFiles(ReferenceDirectory.FullPath(), "*", SearchOption.AllDirectories)
                        .Where(f => f.EndsWith(".pdb"))
                        .Select(f => new ManifestFile() { Source = f, Target = f.Replace(ReferenceDirectory.FullPath(), "") })
                        .ToList();

                    packageBuilder.PopulateFiles("", files);
                }

                var pdb = Path.ChangeExtension(ReferenceLibrary.FullPath(), ".pdb");
                if (File.Exists(pdb))
                    packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = pdb, Target = "lib" } });

                foreach (var sourceFile in SourceFiles)
                {
                    var fullPath = Path.GetFullPath(sourceFile.FullPath());

                    if (File.Exists(sourceFile.FullPath()) && fullPath.StartsWith(Environment.CurrentDirectory))
                        packageBuilder.PopulateFiles("", new ManifestFile[] {
                                            new ManifestFile() { Source = fullPath, Target = Path.Combine("src", fullPath.Substring(Environment.CurrentDirectory.Length + 1)) }
                                        });
                }

                SavePackage(packageBuilder, ".symbols.nupkg", "Symbols created -> {0}");
            }
        }

        public void ApplyToPackageBuilder(PackageBuilder packageBuilder, AssemblyMetadata metadata, string fileVersion)
        {
            SemanticVersion version;
            if (!SemanticVersion.TryParse(metadata.InformationalVersion, out version))
            {
                if (String.IsNullOrEmpty(metadata.InformationalVersion))
                    version = SemanticVersion.Parse(fileVersion);
                else
                    version = SemanticVersion.Parse(Regex.Replace(metadata.InformationalVersion, @"^(\d+\.\d+\.\d+)\+(\d+).*$", "$1.$2"));
            }
            packageBuilder.Version = version;

            if (!String.IsNullOrEmpty(metadata.Description))
                packageBuilder.Description = metadata.Description;

            if (!String.IsNullOrEmpty(metadata.Title))
                packageBuilder.Title = metadata.Title;

            if (!String.IsNullOrEmpty(metadata.Copyright))
                packageBuilder.Copyright = metadata.Copyright;

            if (!String.IsNullOrEmpty(metadata.Culture))
                packageBuilder.Language = metadata.Culture;
        }

        private void SavePackage(PackageBuilder packageBuilder, string filenameSuffix, string logMessage)
        {
            var filename = Path.Combine(DestinationFolder.FullPath(), packageBuilder.GetFullName()) + filenameSuffix;
            using (var file = new FileStream(filename, FileMode.Create))
            {
                packageBuilder.Save(file);
            }

            Log.LogMessage(logMessage, filename);
        }
    }
}