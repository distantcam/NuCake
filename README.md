![Icon](https://raw.github.com/distantcam/NuCake/master/icons/icon_28050_100.png)

# NuCake [![Build status](https://ci.appveyor.com/api/projects/status/m05c0q48ej5ssedm)](https://ci.appveyor.com/project/distantcam/nucake)

**NuCake** is a package for quickly creating NuGet packages from your project.

All you need to do is install **NuCake** and it does the rest.

Available here http://nuget.org/packages/NuCake

To Install from the NuGet Package Manager Console 
    
    PM> Install-Package NuCake

## Attributes

**NuCake** uses the assembly's attributes to determine the NuGet package information.

- The project name is used as the ID.
- `AssemblyInformationalVersionAttribute` sets the version. If it's not availabe the file version is used instead.
- `AssemblyTitleAttribute` sets the title.
- `AssemblyDescriptionAttribute` sets the description.
- `AssemblyCopyrightAttribute` sets the copyright.
- `AssemblyCultureAttribute` sets the language.
- `AssemblyCompanyAttribute` sets the author.

The author and description are required. If there is no attribute for them, **NuCake** uses default values and returns a warning during the build.

## MSBuild

**NuCake** adds an msbuild task that will build your NuGet package from the various attributes.

You can also provide a reference folder to **NuCake**. Any files in the reference folder will be copied into the NuGet package instead, so make sure the output of your build is also in this folder if you want to include it.

If there is a nuget spec file in the reference folder then the metadata from that will be used where possible.

To specify the reference folder you'll need to add some properties to your project.

    <PropertyGroup>
      <NuCakeReferenceFolder>$(MSBuildProjectDirectory)\$(OutputPath)nuget</NuCakeReferenceFolder>
      <NuCakeVersionFieldCount>3</NuCakeVersionFieldCount>
    </PropertyGroup>

`NuCakeVersionFieldCount` allows you to say how many parts of the version to use as your NuGet version (1-4).

## Icon

Cake by Kon Issara from The Noun Project