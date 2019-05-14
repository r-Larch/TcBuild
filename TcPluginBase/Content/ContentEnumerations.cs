using System;


namespace TcPluginBase.Content {
    // Enumerations below are managed wrappers for corresponding integer flags discribed in 
    // TC "FS-Plugin writer's guide" and "Content-Plugin writer's guide" (www.ghisler.com/plugins.htm) 

    // Used as result type for CompareFiles method
    public enum ContentCompareResult {
        CannotCompare = -3, // The file cannot be compared with this function, continue with the next plugin.
        Aborted = -2, // Compare aborted.
        FileOpenError = -1, // Could not open at least one of the files.
        NotEqual = 0, // Two files are different.
        Equal, // Two files are equal, show equal sign in list.
        EqualWithTxt, // Two files are equal, show equal sign with 'TXT' below it in list.
        EqualWithIcon // Two files are equal, show icon from plugin resource.
    }

    // Used as result type for GetSupportedField method and parameter type for GetValue, SetValue and EditValue methods
    public enum ContentFieldType {
        NoMoreFields = 0, // The FieldIndex is beyond the last available field.
        Numeric32, // A 32-bit signed number.
        Numeric64, // A 64-bit signed number, e.g. for file sizes.
        NumericFloating, // A double precision floating point number.
        Date, // A date value (year, month, day).
        Time, // A time value (hour, minute, second). Date and time are in local time.
        Boolean, // A true/false value.
        MultipleChoice, // A value allowing a limited number of choices. 
        String, // A text string (ANSI).
        FullText, // A full text (multiple text strings), only used for searching, not supported in FS plugins.
        DateTime, // A timestamp of type FILETIME, as returned e.g. by FindFirstFile(). 
        WideString, // A text string (Unicode).
        CompareContent = 100 // Type is used in "Synchronize dirs" only. 
    }

    // Used as result type for GetDefaultSortOrder method
    public enum DefaultSortOrder {
        Desc = -1,
        Asc = 1
    }

    // Used as parameter type for EditValue method
    [Flags]
    public enum EditValueFlags {
        None = 0,
        Initialize // Use input field value to initialize the edit dialog.
    }

    // Used as parameter type for GetValue method
    [Flags]
    public enum GetValueFlags {
        None = 0,
        DelayIfSlow // Allows plugin to return ft_delayed for fields which take a long time to extract.
    }

    // Used as result type for GetValue method
    public enum GetValueResult {
        NotSupported = -5, // Function not supported.
        OnDemand = -4, // Field extraction takes a very long time, it will be retrieved when the user presses <SPACEBAR>.
        FieldEmpty = -3, // File does not contain the specified field,
        FileError = -2, // Error accessing the specified file.
        NoSuchField = -1, // Invalid field index.
        Delayed = 0, // Field extraction takes a long time, TC should request it again in a background thread.
        // !!! New in .NET FsPlugin interface !!!
        Success = 1 // GetValue returns this value in case of success, then wrapper returns field type to TC.
    }

    // Used as parameter type for SetValue method
    [Flags]
    public enum SetValueFlags {
        None = 0,
        FirstAttribute = 1, // First attribute of this file.
        LastAttribute = 2, // Last attribute of this file.
        OnlyDate = 4 // Only set the date of the datetime value.
    }

    // Used as result type for SetValue and EditValue methods
    public enum SetValueResult {
        SetCancel = -6, // User clicked "Cancel" in field editor.
        FileError = -2, // Error accessing the specified file.
        NoSuchField = -1, // Invalid field index.
        Success = 0 // OK
    }

    // Used as parameter type for SendStateInformation method
    [Flags]
    public enum StateChangeInfo {
        None = 0,
        ReadNewDir = 1, // It is called when TC reads one of the file lists.
        RefreshPressed = 2, // The user has pressed F2 or Ctrl+R to force a reload.
        ShowHint = 4 // A tooltip/hint window is shown for the current file.
    }

    // Used as parameter type for GetSupportedFieldFlags method
    public enum SupportedFieldOptions {
        None = 0,
        Edit = 1, // Allows to modify this field via "Files - Change attributes".
        // Combined with only ONE of the following flags
        SubstSize = 2, // Substitute file size, if plugin returns nothing
        SubstDateTime = 4, // Substitute file date+time (ft_datetime)
        SubstDate = 6, // Substitute file date (fd_date)
        SubstTime = 8, // Substitute file time (fd_time)
        SubstAttributes = 10, // Substitute file attributes (numeric)
        SubstAttributeString = 12, // Substitute file attribute string in form -a--
        PassThroughSizeFloat = 14, // Pass file size as ft_numeric_floating to ContentGetValue to format. 
        SubstMask = 14, // A combination of all above substitution flags. 
        FieldEdit = 16 // TC will show a button >> to call the plugin own field editor (via ContentEditValue).
    }
}
