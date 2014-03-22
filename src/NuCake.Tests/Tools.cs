using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

public static class Tools
{
    public class StringLogger : ConsoleLogger
    {
        private readonly List<string> logs;

        public StringLogger()
            : base(LoggerVerbosity.Normal, s => { }, c => { }, () => { })
        {
            logs = new List<string>();
            this.WriteHandler = this.AddLine;
        }

        private void AddLine(string line)
        {
            logs.Add(line);
        }

        public override string ToString()
        {
            return string.Join("", logs);
        }
    }

    public static string RunMSBuild(string projectFileName, params string[] flags)
    {
        var pc = new ProjectCollection();
        var globalProperty = new Dictionary<string, string>();
        globalProperty.Add("Configuration", "Debug");
        globalProperty.Add("Platform", "AnyCPU");
        globalProperty.Add("DefineConstants", String.Join(";", flags));

        var buildRequest = new BuildRequestData(projectFileName, globalProperty, null, new string[] { "Rebuild" }, null);

        var logger = new StringLogger();

        var buildParameters = new BuildParameters(pc) { Loggers = new ILogger[] { logger } };

        var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);

        var lines = Regex.Split(logger.ToString(), Environment.NewLine)
            .Where(l => !String.IsNullOrWhiteSpace(l))
            .Select(l => l.Replace(Path.GetDirectoryName(Path.GetFullPath(projectFileName)), "[PROJECT_DIRECTORY]"))
            .Select(l => l.Replace(Path.GetTempPath(), "[TEMP_DIRECTORY]"))
            .Skip(1) // Build started at
            .ToList();
        lines.RemoveAt(lines.Count - 1); // Time elapsed

        return string.Join(Environment.NewLine, lines);
    }
}