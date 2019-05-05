using System;


namespace OY.TotalCommander.TcPluginBase.Packer {
    // Enumerations below are managed wrappers for corresponding integer flags discribed in 
    // TC "WCX Writer's Reference" (see "Packer extensions" -> "Interface description" on www.ghisler.com/plugins.htm) 

    // Used inside the OpenArchiveData class
    public enum ArcOpenMode {
        List = 0, // Open file for reading of file names only
        Extract // Open file for processing (extract or test)
    }

    // Used as parameter type for ChangeVolProc callback method
    public enum ChangeValueProcMode {
        Ask = 0, // Ask user for location of next volume
        Notify // Notify app that next volume will be unpacked
    }

    // Used as parameter type for StartMemPack method
    [Flags]
    public enum MemPackOptions {
        None = 0,
        WantHeaders = 1 // Return archive headers with packed data
    }

    // Used as parameter type for ProcessFile method
    public enum ProcessFileOperation {
        Skip = 0, // Skip this file
        Test, // Test file integrity
        Extract // Extract to disk
    }

    // Type for property BackgroundFlags used to return value for GetBackgroundFlags WCX wrapper method
    [Flags]
    public enum PackBackgroundFlags {
        None = 0,
        Unpack = 1, // Plugin supports unpacking in background
        Pack = 2, // Plugin supports packing in background
        MemPack = 4 // Plugin supports packing into memory in background
    }

    // Used as parameter type for PackFiles method
    [Flags]
    public enum PackFilesFlags {
        None = 0,
        MoveFiles = 1, // Delete original after packing
        SavePaths = 2, // Save path names of files 
        Encrypt = 4 // Ask user for password, then encrypt
    }

    // Type for property Capabilities used to return value for GetPackerCaps WCX wrapper method
    [Flags]
    public enum PackerCapabilities {
        None = 0,
        New = 1, // Can create new archives
        Modify = 2, // Can modify exisiting archives
        Multiple = 4, // Archive can contain multiple files
        Delete = 8, // Can delete files
        Options = 16, // Has options dialog
        Mempack = 32, // Supports packing in memory
        ByContent = 64, // Detect archive type by content
        SearchText = 128, // Allow searching for text in archives created with this plugin
        Hide = 256, // Show as normal files (hide packer icon), open with Ctrl+PgDn, not Enter
        Encrypt = 512 // Plugin supports PK_PACK_ENCRYPT option
    }

    // Used as result type for most of Packer plugin methods
    public enum PackerResult {
        OK = 0, // Success (for PackToMem - there is more data)
        PackToMemDone, // For PackToMem only - success, there is no more data
        // Error codes
        EndArchive = 10, // No more files in archive
        NoMemory, // Not enough memory
        BadData, // CRC error in the data of the currently unpacked file
        BadArchive, // The archive as a whole is bad, e.g. damaged headers
        UnknownFormat, // Archive format unknown
        ErrorOpen, // Cannot open existing file
        ErrorCreate, // Cannot create file
        ErrorClose, // Error closing file
        ErrorRead, // Error reading from file
        ErrorWrite, // Error writing to file
        SmallBuf, // Buffer too small
        EAborted, // Function aborted by user
        NoFiles, // No files found
        TooManyFiles, // Too many files to pack
        NotSupported // Function not supported        
    }


    // This enum is not part of TC Packer plugin interface.
    // It defines what version of ReadHeader(Ex) method is called
    public enum HeaderDataMode {
        Ansi = 0, // ANSI version of TC function ReadHeader called
        ExAnsi, // ANSI version of TC function ReadHeaderEx called
        ExUnicode // Unicode version of TC function ReadHeaderEx called
    }
}
