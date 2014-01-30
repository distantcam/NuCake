using System;
using System.Linq;
using NuGet;
using Xunit;

public class PackageVerificationTests
{
    [Fact]
    public void VerifyPackage()
    {
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");

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
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetTitle");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");

        Assert.Equal("TitleSet", package.Title);
    }

    [Fact]
    public void TitleAttributeSetsDescription()
    {
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetTitle");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");

        Assert.Equal("TitleSet", package.Description);
    }

    [Fact]
    public void DescriptionAttributeSetsDescription()
    {
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetDescription");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");

        Assert.Equal("Description Set", package.Description);
    }

    [Fact]
    public void DescriptionAttributeOverridesTitleAttribute()
    {
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetTitle", "SetDescription");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");

        Assert.Equal("Description Set", package.Description);
    }

    [Fact]
    public void CompanyAttributeSetsAuthor()
    {
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetCompany");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");

        Assert.Equal(1, package.Authors.Count());
        Assert.Equal("The Company Co", package.Authors.First());
    }

    [Fact]
    public void CopyrightAttributeSetsCopyright()
    {
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetCopyright");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");

        Assert.Equal("Copyright Blah", package.Copyright);
    }

    [Fact]
    public void InformationalVersionAttributeSetsVersion()
    {
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetInformationalVersion");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0-info.nupkg");

        Assert.Equal(new SemanticVersion(1, 0, 0, "info"), package.Version);
    }

    [Fact]
    public void CultureAttributeSetsLanguage()
    {
        var log = Tools.RunMSBuild(@"..\..\..\SampleProject\SampleProject.csproj", "SetCulture");

        var package = new NuGet.OptimizedZipPackage(@"..\..\..\SampleProject\NuGetBuild\SampleProject 1.0.0.nupkg");

        Assert.Equal("en-AU", package.Language);
    }
}