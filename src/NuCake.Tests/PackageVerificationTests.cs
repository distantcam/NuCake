using System;
using System.IO;
using System.Linq;
using NuGet;
using Xunit;

public class PackageVerificationTests
{
    private readonly string projectPath;
    private readonly string nupkgPath;

    public PackageVerificationTests()
    {
        projectPath = Path.GetFullPath(@"..\..\..\SampleProject\SampleProject.csproj");
        nupkgPath = Path.GetFullPath(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");
    }

    [Fact]
    public void VerifyPackage()
    {
        var log = Tools.RunMSBuild(projectPath);

        var package = new OptimizedZipPackage(nupkgPath);

        Assert.True(package.IsValid);
        Assert.Equal("SampleProject", package.Id);
        Assert.Equal("No Description", package.Description);
        Assert.Equal(new SemanticVersion(1, 0, 0, 0), package.Version);
        Assert.Equal(1, package.Authors.Count());
        Assert.Equal(Environment.UserName, package.Authors.First());
    }

    [Fact]
    public void TitleAttributeSetsTitle()
    {
        var log = Tools.RunMSBuild(projectPath, "SetTitle");

        var package = new OptimizedZipPackage(nupkgPath);

        Assert.Equal("TitleSet", package.Title);
    }

    [Fact]
    public void TitleAttributeSetsDescription()
    {
        var log = Tools.RunMSBuild(projectPath, "SetTitle");

        var package = new OptimizedZipPackage(nupkgPath);

        Assert.Equal("TitleSet", package.Description);
    }

    [Fact]
    public void DescriptionAttributeSetsDescription()
    {
        var log = Tools.RunMSBuild(projectPath, "SetDescription");

        var package = new OptimizedZipPackage(nupkgPath);

        Assert.Equal("Description Set", package.Description);
    }

    [Fact]
    public void DescriptionAttributeOverridesTitleAttribute()
    {
        var log = Tools.RunMSBuild(projectPath, "SetTitle", "SetDescription");

        var package = new OptimizedZipPackage(nupkgPath);

        Assert.Equal("Description Set", package.Description);
    }

    [Fact]
    public void CompanyAttributeSetsAuthor()
    {
        var log = Tools.RunMSBuild(projectPath, "SetCompany");

        var package = new OptimizedZipPackage(nupkgPath);

        Assert.Equal(1, package.Authors.Count());
        Assert.Equal("The Company Co", package.Authors.First());
    }

    [Fact]
    public void CopyrightAttributeSetsCopyright()
    {
        var log = Tools.RunMSBuild(projectPath, "SetCopyright");

        var package = new OptimizedZipPackage(nupkgPath);

        Assert.Equal("Copyright Blah", package.Copyright);
    }

    [Fact]
    public void InformationalVersionAttributeSetsVersion()
    {
        var log = Tools.RunMSBuild(projectPath, "SetInformationalVersion");

        var package = new OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0-info.nupkg");

        Assert.Equal(new SemanticVersion(1, 0, 0, "info"), package.Version);
    }

    [Fact]
    public void CultureAttributeSetsLanguage()
    {
        var log = Tools.RunMSBuild(projectPath, "SetCulture");

        var package = new OptimizedZipPackage(nupkgPath);

        Assert.Equal("en-AU", package.Language);
    }
}