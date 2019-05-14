using System;


namespace TcPluginBase.QuickSearch {
    // Enumerations below are managed wrappers for corresponding integer flags used in
    // QuickSearch "plugin" (tcmatch.dll)

    // Used as parameter type for MatchGetSetOptions method
    [Flags]
    public enum ExactNameMatch {
        None = 0,
        Beginning = 1,
        Ending = 2
    }

    // Used as result type for MatchGetSetOptions method
    [Flags]
    public enum MatchOptions {
        None = 0,
        OverrideInternalSearch = 1,
        NoLeadingTrailingAsterisk = 2,
        FileNameWithPath = 4,
        AllowEmptyResult = 8
    }
}
