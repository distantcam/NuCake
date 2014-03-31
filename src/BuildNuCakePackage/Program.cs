using NuCake;
using NuGet;
using System;
using System.IO;
using System.Linq;

static class Program
{
    private static void Main(string[] args)
    {
        var outputPath = args.Length == 0 ? Path.GetFullPath(Path.Combine("..", "..", "..", "..", "output")) : args[0];

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var packageBuilder = new PackageBuilder();

        packageBuilder.Id = "NuCake";
        packageBuilder.Description = "NuCake - Fast simple nuget package creation.";
        packageBuilder.Authors.Add("Cameron MacFarland");
        packageBuilder.ProjectUrl = new Uri("https://github.com/distantcam/NuCake");
        packageBuilder.IconUrl = new Uri("https://raw.github.com/distantcam/NuCake/master/icons/icon_28050_100.png");
        packageBuilder.LicenseUrl = new Uri("http://opensource.org/licenses/MIT");
        packageBuilder.DevelopmentDependency = true;

        var attributes = typeof(CreatePackage).Assembly.GetCustomAttributesData();

        var nugetVersion = attributes.First(o => o.Constructor.DeclaringType.Name == "AssemblyInformationalVersionAttribute");
        var version = (string)nugetVersion.ConstructorArguments[0].Value;
        version = version.Substring(0, version.IndexOf('+'));
        packageBuilder.Version = SemanticVersion.Parse(version);

        packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = "NuCake.dll", Target = "build" } });
        packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = "NuCake.targets", Target = "build" } });

        var filename = Path.Combine(outputPath, packageBuilder.GetFullName() + ".nupkg");

        using (var file = new FileStream(filename, FileMode.Create))
        {
            packageBuilder.Save(file);
        }
    }
}