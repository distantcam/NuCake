using System.Runtime.CompilerServices;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

[UseReporter(typeof(DiffReporter))]
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