using System.IO;
using System.Runtime.CompilerServices;
using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Reporters;
using Xunit;

public class MyCustomReporter : IEnvironmentAwareReporter
{
    public bool IsWorkingInThisEnvironment(string forFile)
    {
        return GenericDiffReporter.IsTextFile(forFile);
    }

    public void Report(string approved, string received)
    {
        var a = File.Exists(approved) ? File.ReadAllText(approved) : "";
        var r = File.ReadAllText(received);
        QuietReporter.DisplayCommandLineApproval(approved, received);

        Assert.Equal(a, r);
    }
}

[UseReporter(typeof(MyCustomReporter))]
public class MSBuildTests
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    [Fact]
    public void NoAttributesGivesWarnings()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj"));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [Fact]
    public void SettingTitleRemovesWarning()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetTitle"));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [Fact]
    public void SettingDescriptionRemovesWarning()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetDescription"));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [Fact]
    public void SettingCompanyRemovesWarning()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetCompany"));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [Fact]
    public void SettingEverythingRemovesAllWarning()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetTitle", "SetDescription", "SetCompany", "SetInformationalVersion"));
    }
}