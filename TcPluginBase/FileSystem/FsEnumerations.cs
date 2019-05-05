using System;


namespace OY.TotalCommander.TcPluginBase.FileSystem {
    // Enumerations below are managed wrappers for corresponding integer flags discribed in 
    // TC "FS-Plugin writer's guide" (www.ghisler.com/plugins.htm) 

    // Some enum members are marked "!!! Used for .NET interface only !!!").
    // They are processed in WfxWrapper and doesn't return to TC. 


    // Type for property BackgroundFlags used to return value for FsGetBackgroundFlags WFX wrapper method
    [Flags]
    public enum FsBackgroundFlags {
        None = 0,
        Download = 1, // Plugin supports downloads in background.
        Upload = 2, // Plugin supports uploads in background.
        AskUser = 4 // Plugin requires separate connection for background transfers -> ask user first.
    }

    // Used as parameter type for GetFile and PutFile methods
    [Flags]
    public enum CopyFlags {
        None = 0,
        Overwrite = 1, // If set, overwrite any existing file without asking. If not set, simply fail copying.
        Resume = 2, // Resume an aborted or failed transfer.
        Move = 4, // The plugin needs to delete the remote file after uploading
        ExistsSameCase = 8, // The remote file exists and has the same case (upper/lowercase) as the local file.
        ExistsDifferentCase = 0x10 // The remote file exists and has different case (upper/lowercase) than the local file.
    }

    // Used as result type for ExecuteOpen, ExecuteProperties, and ExecuteCommand methods
    public enum ExecResult {
        OK = 0, // Command was executed successfully, no further action is needed.
        Error = 1, // Execution failed.
        Yourself = -1, // Total Commander should download the file and execute it locally.
        SymLink = -2, // It was a (symbolic) link or .lnk file pointing to a different directory.

        OkReread = -10 // !!! Used for .NET interface only !!!
        // Command was executed successfully, reread current panel is required.
    }

    // Used as parameter type for ExtractCustomIcon method
    [Flags]
    public enum ExtractIconFlags {
        None = 0,
        Small, // Requests the small 16x16 icon.
        Background // The function is called from the background thread.
    }

    // Used as result type for ExtractCustomIcon method
    public enum ExtractIconResult {
        LoadFromFile = -1, // !!! Used for .NET interface only !!!
        // Plugin function ExtractCustomIcon returns path to icon file in "remoteName" parameter,
        // then WfxWrapper loads icon from this file and returns "Extracted" value.

        UseDefault = 0, // No icon is returned. The calling app should show the default icon for this file type.
        Extracted, // An icon was returned in TheIcon. The icon must NOT be freed by the calling app.
        ExtractedDestroy, // An icon was returned in TheIcon. The icon MUST be destroyed by the calling app.
        Delayed // Tells the calling app to show a default icon, and request the true icon in a background thread.
    }

    // Used as result type for GetFile, PutFile and RenMovFile methods
    public enum FileSystemExitCode {
        OK = 0, // The file was copied OK.
        FileExists, // The target file (local or remote) already exists, and resume isn't supported.
        FileNotFound, // The source file (local or remote) couldn't be found or opened.
        ReadError, // There was an error reading from the source file (local or remote).
        WriteError, // There was an error writing to the target file (local or remote), e.g. disk full.
        UserAbort, // Copying was aborted by the user (through ProgressProc).
        NotSupported, // The operation is not supported (e.g. resume).
        ExistsResumeAllowed // The target file (local or remote) already exists, and resume is supported. Not used for RenMovFile.
    }

    // Used as parameter type for LogProc callback method
    public enum LogMsgType {
        Connect = 1, // Connect to a file system requiring disconnect.
        Disconnect, // Disconnected successfully.
        Details, // Not so important messages like directory changing.
        TransferComplete, // A file transfer was completed successfully.
        ConnectComplete, // unused
        ImportantError, // An important error has occured.
        OperationComplete // An operation other than a file transfer has completed.
    }

    // Used as result type for GetPreviewBitmap method
    public enum PreviewBitmapResult {
        None = 0, // There is no preview bitmap.
        Extracted, // The image was extracted and is returned in ReturnedBitmap.
        ExtractYourself, // Tells the caller to extract the image by itself. 
        ExtractYourselfAndDelete, // Tells the caller to extract the image by itself, and then delete the temporary image file. 
        Cache = 256 // This value must be ADDED to one of the above values if the caller should cache the image. 
    }

    // Used as parameter type for RequestProc callback method
    public enum RequestType {
        DomainInfo = -1, // !!! Used for .NET interface only !!!
        // Asks information about .NET application domains in current TC process
        // and assemblies loaded to them. Can be used for debugging.

        Other = 0, // The requested string is none of the default types.
        UserName, // Asks for an User Name, e.g. for a connection.
        Password, // Asks for a Password, e.g. for a connection (shows ***).
        Account, // Asks for an Account (needed for some FTP servers).
        UserNameFirewall, // Asks for an User Name for a firewall.
        PasswordFirewall, // Asks for a Password for a firewall.
        TargetDir, // Asks for a Local Directory (with browse button).
        Url, // Asks for an URL.
        MsgOk, // Shows MessageBox with OK button.
        MsgYesNo, // Shows MessageBox with Yes/No buttons.
        MsgOkCancel // Shows MessageBox with OK/Cancel buttons.
    }

    // Used as parameter type for StatusInfo method
    public enum InfoOperation {
        None = 0, // !!! Used for .NET interface only !!!
        List = 1, // Retrieve a directory listing.
        GetSingle, // Get a single file from the plugin file system.
        GetMulti, // Get multiple files, may include subdirs.
        PutSingle, // Put a single file to the plugin file system.
        PutMulti, // Put multiple files, may include subdirs.
        RenMovSingle, // Rename/Move/Remote copy a single file.
        RenMovMulti, // RenMov multiple files, may include subdirs.
        Delete, // Delete multiple files, may include subdirs.
        Attrib, // Change attributes/times, may include subdirs.
        MkDir, // Create a single directory.
        Exec, // Start a single remote item, or a command line.
        CalcSize, // Calculating size of subdir (user pressed SPACE).        
        Search, // Searching for file names only (using FsFindFirst/NextFile/Close).        
        SearchText, // Searching for file contents (using also FsGetFile() calls).        
        SyncSearch, // Synchronize dirs searches subdirs for info.        
        SyncGet, // Synchronize: Downloading files from plugin.        
        SyncPut, // Synchronize: Uploading files to plugin.        
        SyncDelete, // Synchronize: Deleting files from plugin.        
        GetMultiThread, // Get multiple files, may include subdirs. Executes in background thread.            
        PutMultiThread // Put multiple files, may include subdirs. Executes in background thread.                        
    }

    // Used as parameter type for StatusInfo method
    public enum InfoStartEnd {
        Start, // Operation starts.
        End // Operation has ended.
    }
}
