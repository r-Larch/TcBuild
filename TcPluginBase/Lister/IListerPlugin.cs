using System.Drawing;


namespace TcPluginBase.Lister {
    public interface IListerPlugin {
        #region Mandatory Methods

        [TcMethod("ListLoad", "ListLoadW", Mandatory = true)]
        object Load(string fileToLoad, ShowFlags showFlags);

        #endregion Mandatory Methods

        #region Optional Methods

        [TcMethod("ListLoadNext", "ListLoadNextW")]
        ListerResult LoadNext(object control, string fileToLoad, ShowFlags showFlags);

        [TcMethod("ListCloseWindow")]
        void CloseWindow(object control);

        [TcMethod("ListSearchText", "ListSearchTextW")]
        ListerResult SearchText(object control, string searchString, SearchParameter searchParameter);

        [TcMethod("ListSendCommand")]
        ListerResult SendCommand(object control, ListerCommand command, ShowFlags parameter);

        [TcMethod("ListPrint", "ListPrintW")]
        ListerResult Print(object control, string fileToPrint, string defPrinter, PrintFlags printFlags, PrintMargins margins);

        [TcMethod("ListNotificationReceived")]
        int NotificationReceived(object control, int message, int wParam, int lParam);

        [TcMethod("ListGetPreviewBitmap", "ListGetPreviewBitmapW")]
        Bitmap? GetPreviewBitmap(string fileToLoad, int width, int height, byte[] contentBuf);

        [TcMethod("ListSearchDialog")]
        ListerResult SearchDialog(object control, bool findNext);

        #endregion Optional Methods
    }
}
