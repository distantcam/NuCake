﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NuCake;
using NuGet;

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

        // Change SemVer 2 format back to 1 for nuget
        version = Regex.Replace(version, @"^(\d+\.\d+\.\d+)\+(\d+).*$", "$1");

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