using System;
using System.Drawing;


namespace TcPluginBase.Lister {
    public interface IListerPlugin {
        #region Mandatory Methods

        [TcMethod("ListLoad", "ListLoadW", Mandatory = true)]
        ILister? Load(ParentWindow parent, string fileToLoad, ShowFlags showFlags);

        #endregion Mandatory Methods

        #region Optional Methods

        [TcMethod("ListLoadNext", "ListLoadNextW")]
        ListerResult LoadNext(ILister lister, string fileToLoad, ShowFlags showFlags);

        [TcMethod("ListCloseWindow")]
        void CloseWindow(ILister lister);

        [TcMethod("ListSearchText", "ListSearchTextW")]
        ListerResult SearchText(ILister lister, string searchString, SearchParameter searchParameter);

        [TcMethod("ListSearchDialog")]
        ListerResult SearchDialog(ILister lister, bool findNext);

        [TcMethod("ListSendCommand")]
        ListerResult SendCommand(ILister lister, ListerCommand command, ShowFlags parameter);

        [TcMethod("ListPrint", "ListPrintW")]
        ListerResult Print(ILister lister, string fileToPrint, string defPrinter, PrintFlags printFlags, PrintMargins margins);

        [TcMethod("ListNotificationReceived")]
        int NotificationReceived(ILister lister, int message, int wParam, int lParam);

        [TcMethod("ListGetPreviewBitmap", "ListGetPreviewBitmapW")]
        Bitmap? GetPreviewBitmap(string fileToLoad, int width, int height, byte[] contentBuf);

        #endregion Optional Methods
    }
}
