using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Xunit;

public static class Tools
{
    public static string RunMSBuild(string project, params string[] flags)
    {
        var exePath = GetPathToMSBuild();

        using (var process = Process.Start(new ProcessStartInfo(exePath, String.Format(@"""{0}"" /nologo /t:Rebuild /v:n /p:Configuration=Debug /p:DefineConstants=""{1}""", project, String.Join(";", flags)))
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }))
        {
            process.WaitForExit(10000);

            var lines = Regex.Split(process.StandardOutput.ReadToEnd(), Environment.NewLine)
                .Where(l => !String.IsNullOrWhiteSpace(l))
                .Select(l => l.Replace(Path.GetDirectoryName(Path.GetFullPath(project)), "[PROJECT_DIRECTORY]"))
                .Skip(1)
                .ToList();
            lines.RemoveAt(lines.Count - 1);

            return string.Join(Environment.NewLine, lines);
        }
    }

    private static string GetPathToMSBuild()
    {
        var path = Path.Combine(ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version40), @"MSBuild.exe");
        Assert.True(File.Exists(path), "MSBuild could not be found");
        return path;
    }
}