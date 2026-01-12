using System;


namespace TcPluginBase.QuickSearch {
    // Enumerations below are managed wrappers for corresponding integer flags used in
    // QuickSearch "plugin" (tcmatch.dll)

    /// <summary>
    /// Used as parameter type for MatchGetSetOptions method
    /// </summary>
    [Flags]
    public enum ExactNameMatch {
        None = 0,
        Beginning = 1,
        Ending = 2
    }

    /// <summary>
    /// Used as result type for MatchGetSetOptions method
    /// </summary>
    [Flags]
    public enum MatchOptions {
        None = 0,
        OverrideInternalSearch = 1,
        NoLeadingTrailingAsterisk = 2,
        FileNameWithPath = 4,
        AllowEmptyResult = 8
    }
}
