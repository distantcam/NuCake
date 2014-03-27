using System;
using System.IO;
using System.Linq;
using NuCake;
using NuGet;

static class Program
{
    private static void Main()
    {
        var packageBuilder = new PackageBuilder();

        packageBuilder.Id = "NuCake";
        packageBuilder.Description = "NuCake - Fast simple nuget package creation.";
        packageBuilder.Authors.Add("Cameron MacFarland");
        packageBuilder.ProjectUrl = new Uri("https://github.com/distantcam/NuCake");
        packageBuilder.IconUrl = new Uri("https://raw.github.com/distantcam/NuCake/master/icons/icon_28050_100.png");
        packageBuilder.LicenseUrl = new Uri("http://opensource.org/licenses/MIT");
        packageBuilder.DevelopmentDependency = true;

        var nugetVersion = typeof(CreatePackage).Assembly.GetCustomAttributesData().First(o => o.Constructor.DeclaringType.Name == "NugetVersionAttribute");
        packageBuilder.Version = SemanticVersion.Parse((string)nugetVersion.ConstructorArguments[0].Value);

        packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = "NuCake.dll", Target = "build" } });
        packageBuilder.PopulateFiles("", new ManifestFile[] { new ManifestFile() { Source = "NuCake.targets", Target = "build" } });

        var filename = packageBuilder.GetFullName() + ".nupkg";
        using (var file = new FileStream(filename, FileMode.Create))
        {
            packageBuilder.Save(file);
        }
    }
}