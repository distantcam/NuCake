﻿using System;
using System.Reflection;

#if SetTitle
[assembly: AssemblyTitle("TitleSet")]
#endif

#if SetDescription
[assembly: AssemblyDescription("Description Set")]
#endif

#if SetCompany
[assembly:AssemblyCompany("The Company Co")]
#endif

#if SetInformationalVersion
[assembly: AssemblyInformationalVersion("1.0.0-info")]
#endif

#if SetCopyright
[assembly: AssemblyCopyright("Copyright Blah")]
#endif

#if SetCulture
[assembly: AssemblyCulture("en-AU")]
#endif

[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]