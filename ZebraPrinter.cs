namespace BarcodeAppDesktop.Helper
{
    //singleton class to represent the Zebra printer that connected
    //set the Connection property before use it to print
    public class ZebraPrinter
    {
        private static readonly ZebraPrinter zebra = new ZebraPrinter();
        private IPrinterConnection mIConnection;

        private ZebraPrinter()
        {
        }

        public IPrinterConnection Connection
        {
            get { return mIConnection; }
            set { mIConnection = value; }
        }

        public static ZebraPrinter Get()
        {
            return zebra;
        }

        public bool Print(string strData)
        {
            return mIConnection != null && mIConnection.Print(strData);
        }

        public bool IsConnected()
        {
            return mIConnection != null && mIConnection.IsConnected();
        }
    }
}