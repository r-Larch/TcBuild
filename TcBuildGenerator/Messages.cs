using Microsoft.CodeAnalysis;


namespace TcBuildGenerator {
    public static class Messages {
        public static string[] PluginBaseClasses = {
            "TcPluginBase.FileSystem.FsPlugin",
            "TcPluginBase.Content.ContentPlugin",
            "TcPluginBase.Lister.ListerPlugin",
            "TcPluginBase.Packer.PackerPlugin",
            "TcPluginBase.QuickSearch.QuickSearchPlugin",
        };

        public static DiagnosticDescriptor NoPluginError => new(
            id: "TcBuild0001",
            title: "No Plugin Found",
            messageFormat: $"No plugin could be found. Please add a class that derives from a plugin base class like {string.Join(", ", PluginBaseClasses)}",
            category: "Category",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}
