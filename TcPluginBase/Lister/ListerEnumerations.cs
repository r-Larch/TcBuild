using System;


namespace TcPluginBase.Lister {
    // Enumerations below are managed wrappers for corresponding integer flags discribed in
    // TC "LS-Plugin writer's guide" (www.ghisler.com/plugins.htm)

    /// <summary>
    /// Used as parameter type for SendCommand method
    /// </summary>
    public enum ListerCommand {
        Copy = 1,
        NewParams,
        SelectAll,
        SetPercent
    }

    /// <summary>
    /// Used as parameter type for ListerPluginEvent callback method
    /// </summary>
    public enum ListerMessage {
        Percent = 0xFFFE,
        FontStyle = 0xFFFD,
        WordWrap = 0xFFFC,
        ImageFit = 0xFFFB,
        NextFile = 0xFFFA,
        ImageCenter = 0xFFF9
    }

    /// <summary>
    /// Used as result type for most Lister plugin methods
    /// </summary>
    public enum ListerResult {
        OK = 0,
        Error
    }

    /// <summary>
    /// Used as parameter type for Print method
    /// </summary>
    [Flags]
    public enum PrintFlags {
        None = 0
    }

    /// <summary>
    /// Used as parameter type for SearchText method
    /// </summary>
    [Flags]
    public enum SearchParameter {
        None = 0,
        FindFirst = 1,
        MatchCase = 2,
        WholeWords = 4,
        Backwards = 8
    }

    /// <summary>
    /// Used as parameter type for Load, LoadNext and SendCommand methods
    /// </summary>
    [Flags]
    public enum ShowFlags {
        None = 0,
        WrapText = 1,
        FitToWindow = 2,
        Ansi = 4,
        Ascii = 8,
        Variable = 12,
        ForceShow = 16,
        FitLargerOnly = 32,
        Center = 64
    }
}
