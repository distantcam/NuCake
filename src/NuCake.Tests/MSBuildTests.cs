using System;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

[UseReporter(typeof(DiffReporter))]
public class MSBuildTests
{
    [Fact]
    public void NoAttributesGivesWarnings()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj"));
    }

    [Fact]
    public void SettingTitleRemovesWarning()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetTitle"));
    }

    [Fact]
    public void SettingDescriptionRemovesWarning()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetDescription"));
    }

    [Fact]
    public void SettingCompanyRemovesWarning()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetCompany"));
    }

    [Fact]
    public void SettingEverythingRemovesAllWarning()
    {
        Approvals.Verify(Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetTitle", "SetDescription", "SetCompany", "SetInformationalVersion"));
    }
}