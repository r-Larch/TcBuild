using System;
using System.Drawing;
using System.IO;


namespace TcPluginBase.FileSystem {
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


    /// <summary>
    /// Used as result type for ExecuteOpen, ExecuteProperties, and ExecuteCommand methods
    /// </summary>
    public struct ExecResult {
        /// <summary>
        /// Command was executed successfully, no further action is needed.
        /// </summary>
        public static ExecResult Ok => new ExecResult(ExecEnum.OK);

        /// <summary>
        /// Execution failed.
        /// </summary>
        public static ExecResult Error => new ExecResult(ExecEnum.Error);

        /// <summary>
        /// Total Commander should download the file and execute it locally.
        /// </summary>
        public static ExecResult Yourself => new ExecResult(ExecEnum.Yourself);

        /// <summary>
        /// It was a (symbolic) link or .lnk file pointing to <param name="symlinkTarget"></param>.
        /// </summary>
        public static ExecResult SymLink(RemotePath symlinkTarget) => new ExecResult(ExecEnum.SymLink, symlinkTarget);


        internal ExecEnum Type;
        internal RemotePath SymlinkTarget;

        private ExecResult(ExecEnum type, RemotePath symlinkTarget = default)
        {
            Type = type;
            SymlinkTarget = symlinkTarget;
        }

        internal enum ExecEnum {
            OK = 0,
            Error = 1,
            Yourself = -1,
            SymLink = -2,
        }
    }


    // Used as parameter type for ExtractCustomIcon method
    [Flags]
    public enum ExtractIconFlags {
        None = 0,
        Small, // Requests the small 16x16 icon.
        Background // The function is called from the background thread.
    }


    /// <summary>
    /// Used as result type for ExtractCustomIcon method
    /// </summary>
    public struct ExtractIconResult {
        /// <summary>
        /// No icon is returned. Total Commander should show the default icon for this file type.
        /// </summary>
        public static ExtractIconResult UseDefault => new ExtractIconResult {Value = ExtractIconEnum.UseDefault};

        /// <summary>
        /// This return value is only valid if <see cref="ExtractIconFlags.Background"/> was NOT set. It tells the calling app to show a default icon, and request the true icon in a background thread. See remarks.
        /// </summary>
        /// <remarks>
        /// If you return <see cref="Delayed"/>, <see cref="FsPlugin.ExtractCustomIcon"/> will be called again from a background thread at a later time.
        /// A critical section is used by Total Commander to ensure that <see cref="FsPlugin.ExtractCustomIcon"/> is never entered twice at the same time.
        /// This return value should be used for icons which take a while to extract, e.g. EXE icons. In the FsPlugin sample plugin,
        /// the drive icons are returned immediately (because they are stored in the plugin itself), but the EXE icons are loaded with a delay.
        /// If the user turns off background loading of icons, the function will be called in the foreground with the <see cref="ExtractIconFlags.Background"/> flag.
        /// </remarks>
        public static ExtractIconResult Delayed => new ExtractIconResult {Value = ExtractIconEnum.Delayed};

        /// <summary>
        /// The icon must NOT be freed by Total Commander, e.g. because it was loaded with LoadIcon, or the DLL handles destruction of the icon.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="iconName">Name of the icon. Total Commander can use this to cache the icon</param>
        public static ExtractIconResult Extracted(Icon icon, string iconName = null) => new ExtractIconResult {Value = ExtractIconEnum.Extracted, Icon = icon, IconName = iconName};

        /// <summary>
        /// The icon MUST be destroyed by Total Commander, e.g. because it was created with CreateIcon(), or extracted with ExtractIconEx().
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="iconName">Name of the icon. Total Commander can use this to cache the icon</param>
        public static ExtractIconResult ExtractedDestroy(Icon icon, string iconName = null) => new ExtractIconResult {Value = ExtractIconEnum.ExtractedDestroy, Icon = icon, IconName = iconName};

        /// <summary>
        /// This attempts to load the Icon from the specified filePath.
        /// supply extractFlags to ensure the correct size gets loaded.
        /// </summary>
        /// <param name="filePath">a local file path (the file MUST exist)</param>
        /// <param name="extractFlags"></param>
        public static ExtractIconResult LoadFromFile(string filePath, ExtractIconFlags extractFlags)
        {
            if (string.IsNullOrEmpty(filePath)) {
                return ExtractIconResult.UseDefault;
            }

            const uint imageTypeIcon = 1; //  IMAGE_ICON
            const uint loadImageFlags = 0x10 + 0x8000; //  LR_LOADFROMFILE | LR_SHARED

            // use LoadImage, it produces better results than LoadIcon
            var extrIcon = (extractFlags & ExtractIconFlags.Small) == ExtractIconFlags.Small
                ? NativeMethods.LoadImage(IntPtr.Zero, filePath, imageTypeIcon, 16, 16, loadImageFlags)
                : NativeMethods.LoadImage(IntPtr.Zero, filePath, imageTypeIcon, 0, 0, loadImageFlags);

            if (extrIcon == IntPtr.Zero) {
                //var errorCode = NativeMethods.GetLastError();
                return ExtractIconResult.UseDefault;
            }

            return ExtractIconResult.Extracted(System.Drawing.Icon.FromHandle(extrIcon), filePath);
        }

        internal ExtractIconEnum Value { get; set; }
        internal Icon Icon { get; set; }
        internal string IconName { get; set; }

        internal enum ExtractIconEnum {
            UseDefault = 0, // No icon is returned. The calling app should show the default icon for this file type.
            Extracted, // An icon was returned in TheIcon. The icon must NOT be freed by the calling app.
            ExtractedDestroy, // An icon was returned in TheIcon. The icon MUST be destroyed by the calling app.
            Delayed // Tells the calling app to show a default icon, and request the true icon in a background thread.
        }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}, {nameof(Icon)}: {Icon?.Handle.ToString() ?? "null"}, {nameof(IconName)}: {IconName ?? "null"}";
        }
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
    public struct PreviewBitmapResult {
        /// <summary>
        /// There is no preview bitmap.
        /// </summary>
        public static PreviewBitmapResult None => new PreviewBitmapResult {Value = PreviewBitmapEnum.None};

        /// <summary>
        /// The image was extracted and is returned
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="bitmapName">Name of the bitmap. Total Commander can use this to cache the bitmap</param>
        /// <param name="cache">false to NOT cache the image</param>
        public static PreviewBitmapResult Extracted(Bitmap bitmap, string bitmapName = null, bool cache = true) => new PreviewBitmapResult {
            Value = PreviewBitmapEnum.Extracted,
            Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap)),
            BitmapName = bitmapName,
            Cache = cache
        };

        /// <summary>
        /// Tells the caller to extract the image by itself from bitmapPath.
        /// </summary>
        /// <param name="bitmapPath">The local path to the bitmap</param>
        /// <param name="cache">false to NOT cache the image</param>
        public static PreviewBitmapResult ExtractYourself(string bitmapPath, bool cache = true)
        {
            return new PreviewBitmapResult {Value = PreviewBitmapEnum.ExtractYourself, BitmapName = bitmapPath, Cache = cache};
        }

        /// <summary>
        /// Tells Total Commander to extract the image by itself, and then delete the temporary image file.
        /// The full local path to the temporary image file needs to be set in temporaryImageFile.
        /// The returned bitmap name must not be longer than MAX_PATH. In this case,
        /// the plugin downloads the file to TEMP and then asks TC to extract the image.
        /// </summary>
        /// <param name="temporaryImageFile"></param>
        /// <param name="cache">false to NOT cache the image</param>
        public static PreviewBitmapResult ExtractYourselfAndDelete(string temporaryImageFile, bool cache = true) => new PreviewBitmapResult {Value = PreviewBitmapEnum.ExtractYourselfAndDelete, BitmapName = temporaryImageFile, Cache = cache};


        internal string BitmapName { get; private set; }
        internal PreviewBitmapEnum Value { get; private set; }
        internal Bitmap Bitmap { get; private set; }
        internal bool Cache { get; private set; }


        [Flags]
        internal enum PreviewBitmapEnum {
            None = 0, // There is no preview bitmap.
            Extracted, // The image was extracted and is returned in ReturnedBitmap.
            ExtractYourself, // Tells the caller to extract the image by itself.
            ExtractYourselfAndDelete, // Tells the caller to extract the image by itself, and then delete the temporary image file.
            Cache = 256 // This value must be ADDED to one of the above values if the caller should cache the image.
        }
    }

    // Used as parameter type for RequestProc callback method
    public enum RequestType {
        //DomainInfo = -1, // !!! Used for .NET interface only !!!
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

        // no ReturnedText
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
