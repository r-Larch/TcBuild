using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;


namespace TcPluginBase.FileSystem {
    // Enumerations below are managed wrappers for corresponding integer flags discribed in
    // TC "FS-Plugin writer's guide" (www.ghisler.com/plugins.htm)

    // Some enum members are marked "!!! Used for .NET interface only !!!").
    // They are processed in WfxWrapper and doesn't return to TC.

    /// <summary> Type for property BackgroundFlags used to return value for FsGetBackgroundFlags WFX wrapper method </summary>
    [Flags]
    public enum FsBackgroundFlags {
        None = 0,
        /// <summary> Plugin supports downloads in background. </summary>
        Download = 1,
        /// <summary> Plugin supports uploads in background. </summary>
        Upload = 2,
        /// <summary> Plugin requires separate connection for background transfers -> ask user first. </summary>
        AskUser = 4
    }

    /// <summary> Used as parameter type for GetFile and PutFile methods </summary>
    [Flags]
    public enum CopyFlags {
        None = 0,
        /// <summary> If set, overwrite any existing file without asking. If not set, simply fail copying.</summary>
        Overwrite = 1,
        /// <summary> Resume an aborted or failed transfer.</summary>
        Resume = 2,
        /// <summary> The plugin needs to delete the remote file after uploading </summary>
        Move = 4,
        /// <summary> The remote file exists and has the same case (upper/lowercase) as the local file. </summary>
        ExistsSameCase = 8,
        /// <summary> The remote file exists and has different case (upper/lowercase) than the local file. </summary>
        ExistsDifferentCase = 0x10
    }


    /// <summary> Used as result type for ExecuteOpen, ExecuteProperties, and ExecuteCommand methods </summary>
    public struct ExecResult {
        /// <summary> Command was executed successfully, no further action is needed. </summary>
        public static ExecResult Ok => new ExecResult(ExecEnum.OK);

        /// <summary> Execution failed. </summary>
        public static ExecResult Error => new ExecResult(ExecEnum.Error);

        /// <summary> Total Commander should download the file and execute it locally. </summary>
        public static ExecResult Yourself => new ExecResult(ExecEnum.Yourself);

        /// <summary>
        /// It was a (symbolic) link or .lnk file pointing to another file or directory.
        /// You can also switch to a directory on the local harddisk! To do this,
        /// return a path starting either with a drive letter, or an UNC location (\\server\share).
        /// The maximum allowed length of such a path is MAX_PATH-1 = 259 characters!
        /// </summary>
        /// <param name="symlinkTarget">The file or directory where the symlink points to.</param>
        public static ExecResult SymLink(string symlinkTarget) => new ExecResult(ExecEnum.SymLink, symlinkTarget);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ExecEnum Type;
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string SymlinkTarget;

        private ExecResult(ExecEnum type, string symlinkTarget = default)
        {
            Type = type;
            SymlinkTarget = symlinkTarget;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public enum ExecEnum {
            OK = 0,
            Error = 1,
            Yourself = -1,
            SymLink = -2,
        }
    }


    /// <summary> Used as parameter type for ExtractCustomIcon method </summary>
    [Flags]
    public enum ExtractIconFlags {
        None = 0,
        /// <summary> Requests the small 16x16 icon. </summary>
        Small,
        /// <summary> The function is called from the background thread. </summary>
        Background
    }


    /// <summary> Used as result type for ExtractCustomIcon method </summary>
    public struct ExtractIconResult {
        /// <summary> No icon is returned. Total Commander should show the default icon for this file type. </summary>
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ExtractIconEnum Value { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Icon Icon { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string IconName { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public enum ExtractIconEnum {
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

    public readonly struct GetFileResult {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public FileSystemExitCode Code { get; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string FileName { get; }

        private GetFileResult(FileSystemExitCode code, string fileName = null)
        {
            Code = code;
            FileName = fileName;
        }

        /// <summary> The file was copied OK, but name or extension has changed. </summary>
        public static GetFileResult OkNameChanged(string newFileName) => new GetFileResult(FileSystemExitCode.OK, newFileName);

        /// <summary> The file was copied OK. </summary>
        public static GetFileResult Ok => new GetFileResult(FileSystemExitCode.OK);
        /// <summary> The target file already exists, and resume is not supported. </summary>
        public static GetFileResult FileExists => new GetFileResult(FileSystemExitCode.FileExists);
        /// <summary> The source file couldn't be found or opened. </summary>
        public static GetFileResult FileNotFound => new GetFileResult(FileSystemExitCode.FileNotFound);
        /// <summary> There was an error reading from the source file. </summary>
        public static GetFileResult ReadError => new GetFileResult(FileSystemExitCode.ReadError);
        /// <summary> There was an error writing to the target file, e.g. disk full. </summary>
        public static GetFileResult WriteError => new GetFileResult(FileSystemExitCode.WriteError);
        /// <summary> Copying was aborted by the user (through ProgressProc). </summary>
        public static GetFileResult UserAbort => new GetFileResult(FileSystemExitCode.UserAbort);
        /// <summary> The operation is not supported (e.g. resume). </summary>
        public static GetFileResult NotSupported => new GetFileResult(FileSystemExitCode.NotSupported);
        /// <summary> The target file already exists, and resume is supported. </summary>
        public static GetFileResult ExistsResumeAllowed => new GetFileResult(FileSystemExitCode.ExistsResumeAllowed);
    }

    public readonly struct PutFileResult {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public FileSystemExitCode Code { get; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string FileName { get; }

        private PutFileResult(FileSystemExitCode code, string fileName = null)
        {
            Code = code;
            FileName = fileName;
        }

        /// <summary> The file was copied OK, but name or extension has changed. </summary>
        public static PutFileResult OkNameChanged(string newFileName) => new PutFileResult(FileSystemExitCode.OK, newFileName);

        /// <summary> The file was copied OK. </summary>
        public static PutFileResult Ok => new PutFileResult(FileSystemExitCode.OK);
        /// <summary> The target file already exists, and resume is not supported. </summary>
        public static PutFileResult FileExists => new PutFileResult(FileSystemExitCode.FileExists);
        /// <summary> The source file couldn't be found or opened. </summary>
        public static PutFileResult FileNotFound => new PutFileResult(FileSystemExitCode.FileNotFound);
        /// <summary> There was an error reading from the source file. </summary>
        public static PutFileResult ReadError => new PutFileResult(FileSystemExitCode.ReadError);
        /// <summary> There was an error writing to the target file, e.g. disk full. </summary>
        public static PutFileResult WriteError => new PutFileResult(FileSystemExitCode.WriteError);
        /// <summary> Copying was aborted by the user (through ProgressProc). </summary>
        public static PutFileResult UserAbort => new PutFileResult(FileSystemExitCode.UserAbort);
        /// <summary> The operation is not supported (e.g. resume). </summary>
        public static PutFileResult NotSupported => new PutFileResult(FileSystemExitCode.NotSupported);
        /// <summary> The target file already exists, and resume is supported. </summary>
        public static PutFileResult ExistsResumeAllowed => new PutFileResult(FileSystemExitCode.ExistsResumeAllowed);
    }

    public readonly struct RenMovFileResult {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public FileSystemExitCode Code { get; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RenMovFileResult(FileSystemExitCode code) => Code = code;

        /// <summary> The file was copied/moved OK. </summary>
        public static RenMovFileResult Ok => new RenMovFileResult(FileSystemExitCode.OK);
        /// <summary> The target file already exists, and resume is not supported. </summary>
        public static RenMovFileResult FileExists => new RenMovFileResult(FileSystemExitCode.FileExists);
        /// <summary> The source file couldn't be found or opened. </summary>
        public static RenMovFileResult FileNotFound => new RenMovFileResult(FileSystemExitCode.FileNotFound);
        /// <summary> There was an error reading from the source file. </summary>
        public static RenMovFileResult ReadError => new RenMovFileResult(FileSystemExitCode.ReadError);
        /// <summary> There was an error writing to the target file, e.g. disk full. </summary>
        public static RenMovFileResult WriteError => new RenMovFileResult(FileSystemExitCode.WriteError);
        /// <summary> Copying was aborted by the user (through ProgressProc). </summary>
        public static RenMovFileResult UserAbort => new RenMovFileResult(FileSystemExitCode.UserAbort);
        /// <summary> The operation is not supported (e.g. resume). </summary>
        public static RenMovFileResult NotSupported => new RenMovFileResult(FileSystemExitCode.NotSupported);
    }

    /// <summary> Used as result type for <see cref="FsPlugin.GetFile"/>, <see cref="FsPlugin.PutFile"/> and <see cref="FsPlugin.RenMovFile"/> methods </summary>
    public enum FileSystemExitCode {
        /// <summary> The file was copied OK. </summary>
        OK = 0,
        /// <summary> The target file (local or remote) already exists, and resume isn't supported. </summary>
        FileExists,
        /// <summary> The source file (local or remote) couldn't be found or opened. </summary>
        FileNotFound,
        /// <summary> There was an error reading from the source file (local or remote). </summary>
        ReadError,
        /// <summary> There was an error writing to the target file (local or remote), e.g. disk full. </summary>
        WriteError,
        /// <summary> Copying was aborted by the user (through ProgressProc). </summary>
        UserAbort,
        /// <summary> The operation is not supported (e.g. resume). </summary>
        NotSupported,
        /// <summary> The target file (local or remote) already exists, and resume is supported. Not used for <see cref="FsPlugin.RenMovFile"/>. </summary>
        ExistsResumeAllowed
    }

    /// <summary> Used as parameter type for <see cref="FsPlugin.LogProc"/> callback method </summary>
    public enum LogMsgType {
        /// <summary> Connect to a file system requiring disconnect. </summary>
        Connect = 1,
        /// <summary> Disconnected successfully. </summary>
        Disconnect,
        /// <summary> Not so important messages like directory changing. </summary>
        Details,
        /// <summary> A file transfer was completed successfully. </summary>
        TransferComplete,
        /// <summary> unused </summary>
        ConnectComplete,
        /// <summary> An important error has occured. </summary>
        ImportantError,
        /// <summary> An operation other than a file transfer has completed. </summary>
        OperationComplete
    }


    /// <summary> Used as result type for <see cref="FsPlugin.GetPreviewBitmap"/> method </summary>
    public struct PreviewBitmapResult {
        /// <summary> There is no preview bitmap. </summary>
        public static PreviewBitmapResult None => new PreviewBitmapResult {Value = PreviewBitmapEnum.None};

        /// <summary> The image was extracted and is returned </summary>
        /// <param name="bitmap"></param>
        /// <param name="bitmapName">Name of the bitmap. Total Commander can use this to cache the bitmap</param>
        /// <param name="cache">false to NOT cache the image</param>
        public static PreviewBitmapResult Extracted(Bitmap bitmap, string bitmapName = null, bool cache = true) => new PreviewBitmapResult {
            Value = PreviewBitmapEnum.Extracted,
            Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap)),
            BitmapName = bitmapName,
            Cache = cache
        };

        /// <summary> Tells the caller to extract the image by itself from bitmapPath. </summary>
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


        [EditorBrowsable(EditorBrowsableState.Never)]
        public string BitmapName { get; private set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PreviewBitmapEnum Value { get; private set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Bitmap Bitmap { get; private set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Cache { get; private set; }


        [Flags, EditorBrowsable(EditorBrowsableState.Never)]
        public enum PreviewBitmapEnum {
            None = 0, // There is no preview bitmap.
            Extracted, // The image was extracted and is returned in ReturnedBitmap.
            ExtractYourself, // Tells the caller to extract the image by itself.
            ExtractYourselfAndDelete, // Tells the caller to extract the image by itself, and then delete the temporary image file.
            Cache = 256 // This value must be ADDED to one of the above values if the caller should cache the image.
        }
    }

    /// <summary> Used as parameter type for <see cref="FsPlugin.RequestProc"/> callback method </summary>
    internal enum RequestType {
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

    /// <summary> Used as parameter type for <see cref="FsPlugin.StatusInfo"/> method </summary>
    public enum InfoOperation {
        // !!! Used for .NET interface only !!!
        None = 0,
        /// <summary> Retrieve a directory listing. </summary>
        List = 1,
        /// <summary> Get a single file from the plugin file system. </summary>
        GetSingle,
        /// <summary> Get multiple files, may include subdirs. </summary>
        GetMulti,
        /// <summary> Put a single file to the plugin file system. </summary>
        PutSingle,
        /// <summary> Put multiple files, may include subdirs. </summary>
        PutMulti,
        /// <summary> Rename/Move/Remote copy a single file. </summary>
        RenMovSingle,
        /// <summary> RenMov multiple files, may include subdirs. </summary>
        RenMovMulti,
        /// <summary> Delete multiple files, may include subdirs. </summary>
        Delete,
        /// <summary> Change attributes/times, may include subdirs. </summary>
        Attributes,
        /// <summary> Create a single directory. </summary>
        MkDir,
        /// <summary> Start a single remote item, or a command line. </summary>
        Exec,
        /// <summary> Calculating size of subdir (user pressed SPACE). </summary>
        CalcSize,
        /// <summary> Searching for file names only (using FsFindFirst/NextFile/Close). </summary>
        Search,
        /// <summary> Searching for file contents (using also FsGetFile() calls). </summary>
        SearchText,
        /// <summary> Synchronize dirs searches subdirs for info. </summary>
        SyncSearch,
        /// <summary> Synchronize: Downloading files from plugin. </summary>
        SyncGet,
        /// <summary> Synchronize: Uploading files to plugin. </summary>
        SyncPut,
        /// <summary> Synchronize: Deleting files from plugin. </summary>
        SyncDelete,
        /// <summary> Get multiple files, may include subdirs. Executes in background thread. </summary>
        GetMultiThread,
        /// <summary> Put multiple files, may include subdirs. Executes in background thread. </summary>
        PutMultiThread
    }

    /// <summary> Used as parameter type for <see cref="FsPlugin.StatusInfo"/> method </summary>
    public enum InfoStartEnd {
        /// <summary> Operation starts. </summary>
        Start,
        /// <summary> Operation has ended. </summary>
        End
    }
}
