using System;
using System.Drawing;
using Microsoft.Extensions.Configuration;


namespace TcPluginBase.Lister {
    public abstract class ListerPlugin<TLister> : TcPlugin, IListerPlugin<TLister> where TLister : ILister {
        public Color BitmapBackgroundColor { get; set; }

        /// <summary>
        /// CanHandle should return a expression which allows Lister to find out whether
        /// your plugin can probably handle the file or not. You can use this as a first
        /// test - more thorough tests may be performed in ListLoad().
        ///
        /// It's very important to define a good test string, especially when there are
        /// dozens of plugins loaded! The test string allows lister to load only those
        /// plugins relevant for that specific file type.
        /// </summary>
        public abstract SupportExpression CanHandle { get; }


        protected ListerPlugin(IConfiguration pluginSettings) : base(pluginSettings)
        {
            BitmapBackgroundColor = Color.White;
        }


        /// <summary>
        /// This gets called as a entry point
        /// </summary>
        /// <remarks>
        /// Load is called when a user opens lister with F3 or the Quick View Panel with Ctrl+Q, and when the DetectString string either doesn't exist, or its evaluation returns true.
        /// Please note that multiple Lister windows can be open at the same time! Therefore you cannot save settings in global variables.
        /// When lister is activated, it will set the focus to your window.If your window contains child windows, then make sure that you set the focus to the correct child when your main window receives the focus!
        /// If <see cref="ShowFlags.ForceShow"/> is defined, you may try to load the file even if the plugin wasn't made for it.
        /// Example: A plugin with line numbers may only show the file as such when the user explicitly chooses 'Image/Multimedia' from the menu.
        /// Lister plugins which only create thumbnail images do not need to implement this function.
        /// </remarks>
        public abstract TLister? Load(ParentWindow parent, string fileToLoad, ShowFlags showFlags);


        #region Optional Methods

        public virtual ListerResult LoadNext(TLister lister, string fileToLoad, ShowFlags showFlags)
        {
            return ListerResult.Error;
        }

        public virtual void CloseWindow(TLister lister)
        {
        }

        public virtual ListerResult SearchText(TLister lister, string searchString, SearchParameter searchParameter)
        {
            return ListerResult.Error;
        }

        public virtual ListerResult SearchDialog(TLister lister, bool findNext)
        {
            return ListerResult.Error;
        }

        public virtual ListerResult SendCommand(TLister lister, ListerCommand command, ShowFlags parameter)
        {
            return ListerResult.Error;
        }

        public virtual ListerResult Print(TLister lister, string fileToPrint, string defPrinter, PrintFlags printFlags, PrintMargins margins)
        {
            return ListerResult.Error;
        }

        public virtual int NotificationReceived(TLister lister, int message, int wParam, int lParam)
        {
            return 0;
        }

        public virtual Bitmap? GetPreviewBitmap(string fileToLoad, int width, int height, byte[] contentBuf)
        {
            return null;
        }

        #endregion Optional Methods
    }
}
