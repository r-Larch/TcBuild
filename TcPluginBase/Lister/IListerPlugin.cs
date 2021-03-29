using System;
using System.Drawing;


namespace TcPluginBase.Lister {
    public interface IListerPlugin<TLister> where TLister : ILister {
        #region Mandatory Methods

        [TcMethod("ListLoad", "ListLoadW", Mandatory = true)]
        TLister? Load(ParentWindow parent, string fileToLoad, ShowFlags showFlags);

        #endregion Mandatory Methods

        #region Optional Methods

        [TcMethod("ListLoadNext", "ListLoadNextW")]
        ListerResult LoadNext(TLister lister, string fileToLoad, ShowFlags showFlags);

        [TcMethod("ListCloseWindow")]
        void CloseWindow(TLister lister);

        [TcMethod("ListSearchText", "ListSearchTextW")]
        ListerResult SearchText(TLister lister, string searchString, SearchParameter searchParameter);

        [TcMethod("ListSearchDialog")]
        ListerResult SearchDialog(TLister lister, bool findNext);

        [TcMethod("ListSendCommand")]
        ListerResult SendCommand(TLister lister, ListerCommand command, ShowFlags parameter);

        [TcMethod("ListPrint", "ListPrintW")]
        ListerResult Print(TLister lister, string fileToPrint, string defPrinter, PrintFlags printFlags, PrintMargins margins);

        [TcMethod("ListNotificationReceived")]
        int NotificationReceived(TLister lister, int message, int wParam, int lParam);

        [TcMethod("ListGetPreviewBitmap", "ListGetPreviewBitmapW")]
        Bitmap? GetPreviewBitmap(string fileToLoad, int width, int height, byte[] contentBuf);

        #endregion Optional Methods
    }
}
