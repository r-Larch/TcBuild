
# TcBuild

TcBuild is a tool to build Total Commander plugins that are written in .NET


## Nuget

```powershell
Install-Package TcBuild
```

## Possible Plugins

|  Plugin           |  Total Commander name  |
|-------------------|------------------------|
| FileSystem Plugin |    .wfx - plugin       |
| Content Plugin    |    .wdx - plugin       |
| Lister Plugin     |    .wlx - plugin       |
| Packer Plugin     |    .wcx - plugin       |

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
