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

        public ITaskItem ReferenceFolder { get; set; }

        public ITaskItem[] SourceFiles { get; set; }

        public ITaskItem VersionFieldCount { get; set; }

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

            if (ReferenceFolder != null)
            {
                var nuSpec = Directory.GetFiles(ReferenceFolder.FullPath(), ReferenceLibrary.Filename() + ".nuspec", SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (nuSpec != null)
                    using (var stream = File.OpenRead(nuSpec))
                    {
                        var manifest = Manifest.ReadFrom(stream, NullPropertyProvider.Instance, false);
                        packageBuilder.Populate(manifest.Metadata);
                    }
            }

            if (string.IsNullOrWhiteSpace(packageBuilder.Id))
                packageBuilder.Id = Path.GetFileNameWithoutExtension(ReferenceLibrary.FullPath());

            if (string.IsNullOrWhiteSpace(packageBuilder.Description))
                packageBuilder.Description = fileVersionInfo.FileDescription;

            if (!string.IsNullOrWhiteSpace(fileVersionInfo.CompanyName))
                packageBuilder.Authors.Add(fileVersionInfo.CompanyName);

            if (ReferenceFolder == null)
            {
                packageBuilder.PopulateFiles("", new[] { new ManifestFile { Source = ReferenceLibrary.FullPath(), Target = "lib" } });

                var xmldoc = Path.ChangeExtension(ReferenceLibrary.FullPath(), ".xml");
                if (File.Exists(xmldoc))
                    packageBuilder.PopulateFiles("", new[] { new ManifestFile { Source = xmldoc, Target = "lib" } });
            }
            else
            {
                var files = Directory.GetFiles(ReferenceFolder.FullPath(), "*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(ReferenceLibrary.Filename() + ".nuspec"))
                    .Select(f => new ManifestFile { Source = f, Target = f.Replace(ReferenceFolder.FullPath(), "") })
                    .ToList();

                packageBuilder.PopulateFiles("", files);
            }

            using (var appDomainManager = new AppDomainManager(Path.GetDirectoryName(GetType().Assembly.Location)))
            {
                var metadata = appDomainManager.CreateInstanceAndUnwrap<AssemblyMetadata>();
                metadata.LoadMetadata(ReferenceLibrary.FullPath());
                ApplyToPackageBuilder(packageBuilder, metadata, fileVersionInfo.FileVersion);
            }

            if (string.IsNullOrWhiteSpace(packageBuilder.Description))
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
                if (ReferenceFolder == null)
                {
                    var pdb = Path.ChangeExtension(ReferenceLibrary.FullPath(), ".pdb");
                    if (File.Exists(pdb))
                        packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = pdb, Target = "lib" } });
                }

                foreach (var sourceFile in SourceFiles)
                {
                    var fullPath = Path.GetFullPath(sourceFile.FullPath());
                    var fileInfo = new FileInfo(fullPath);

                    if (fileInfo.Exists && fileInfo.Length > 0 && fullPath.StartsWith(Environment.CurrentDirectory))
                        packageBuilder.PopulateFiles("", new ManifestFile[] {
                                            new ManifestFile() { Source = fullPath, Target = Path.Combine("src", fullPath.Substring(Environment.CurrentDirectory.Length + 1)) }
                                        });
                }

                SavePackage(packageBuilder, ".symbols.nupkg", "Symbols created -> {0}");
            }
        }

        private void ApplyToPackageBuilder(PackageBuilder packageBuilder, AssemblyMetadata metadata, string fileVersion)
        {
            SemanticVersion version;
            if (!SemanticVersion.TryParse(metadata.InformationalVersion, out version))
            {
                if (string.IsNullOrEmpty(metadata.InformationalVersion))
                {
                    version = SemanticVersion.Parse(fileVersion);
                }
                else if (!SemanticVersion.TryParse(Regex.Replace(metadata.InformationalVersion, @"^(\d+\.\d+\.\d+)\+(\d+).*$", "$1.$2"), out version))
                {
                    if (!SemanticVersion.TryParse(Regex.Replace(metadata.InformationalVersion, @"^(\d+\.\d+\.\d+)\+.*$", "$1"), out version))
                    {
                        version = SemanticVersion.Parse(fileVersion);
                        Log.LogWarning("Version cannot be determined from AssemblyInformationalVersion '{0}'.", metadata.InformationalVersion);
                    }
                }
            }
            packageBuilder.Version = version;

            if (VersionFieldCount != null)
            {
                int count;
                if (int.TryParse(VersionFieldCount.ItemSpec, out count))
                    packageBuilder.Version = SemanticVersion.Parse(version.Version.ToString(count));
            }

            if (!string.IsNullOrEmpty(metadata.Description))
                packageBuilder.Description = metadata.Description;

            if (!string.IsNullOrEmpty(metadata.Title))
                packageBuilder.Title = metadata.Title;

            if (!string.IsNullOrEmpty(metadata.Copyright))
                packageBuilder.Copyright = metadata.Copyright;

            if (!string.IsNullOrEmpty(metadata.Culture))
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