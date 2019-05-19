
# TcBuild

TcBuild is a tool to build Total Commander plugins that are written in .NET

[![Build status](https://ci.appveyor.com/api/projects/status/a93fev9ym9leg16h/branch/master?svg=true)](https://ci.appveyor.com/project/r-Larch/tcbuild/branch/master)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/TcBuild.svg?style=flat)](https://www.nuget.org/packages/TcBuild/)


## Nuget

```powershell
Install-Package TcBuild
```

## Possible Plugins

|  Plugin           |  Total Commander name  | Example                                                             |
|-------------------|------------------------|---------------------------------------------------------------------|
| FileSystem Plugin |    .wfx - plugin       | [TcPlugin.AzureBlob](https://github.com/r-Larch/TcPlugin.AzureBlob) |
| Content Plugin    |    .wdx - plugin       | comming soon                                                        |
| Lister Plugin     |    .wlx - plugin       | comming soon                                                        |
| Packer Plugin     |    .wcx - plugin       | comming soon                                                        |

See [Total Commander plugin types](https://www.ghisler.ch/wiki/index.php/Plugin#Plugin_types).


# Create a plugin

Create a .NET Library Project targeting net472 or newer and install `TcBuild` nuget package.
The TcBuild nuget comes with some base classes and interfaces to get you started.
So you just have to create a class inheriting from 
one of the following plugin base classes:

* ContentPlugin
* FsPlugin
* ListerPlugin
* PackerPlugin
* QuickSearchPlugin

Then override all methods that you like to support in your plugin.


# Build a Release

The TcBuild nuget contains a MsBuild Task to perform all required transformations on your .dll
and it produces a `.zip` file, in build output folder, which can be installed with Total Commander auto installer.

More infos: [Installation using Total Commander's integrated plugin installer](https://www.ghisler.ch/wiki/index.php/Plugin#Installation_using_Total_Commander.27s_integrated_plugin_installer)


# Copyright

The idea for .NET Total Commander plugins came from **Oleg Yuvashev** 
which wrote the original **[TC .NET Interface](https://totalcmd.net/plugring/TCdotNetInterface.html)**
that runs on .NET 4.0 and is not that easy to get up and running.
 * [Oleg Yuvashev on sourceforge.net](https://sourceforge.net/p/tcdotnetinterface/code/HEAD/tree/trunk/)

So I (René Larch) used his code (it is Licesed as MIT) and wrote this Nuget with it.

### Pros and cons of TcBuild compared to TC .NET Interface

|						| TcBuild								| TC .NET Interface								|
|-----------------------|---------------------------------------|-----------------------------------------------|
| .NET Version			| ✔️ .NET 4.7.2							| ❌.NET 1.0 & .NET 4.0							|
| Getting started		| ✔️Install this Nuget					| ❌Lots of work									|
| Global Assembly Cache | ✔️--									| ❌It needs two dlls in GAC						|
| AppDomain Isolation   | ❌Not now, maybe I reimplement it later | ✔️Yes it load your Plugin in its own AppDomain	|


# Some docu
http://java.totalcmd.net/V1.7/javadoc/plugins/wfx/WFXPluginInterface.html