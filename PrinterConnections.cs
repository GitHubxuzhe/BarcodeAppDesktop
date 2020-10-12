using System;
using System.IO.Ports;
using Exceptionless;

namespace BarcodeAppDesktop.Helper
{
    public enum ConnectionType
    {
        kUSBConnection,
        kSerialConnection,
        kUnknownConnection
    }

    public interface IPrinterConnection
    {
        bool IsConnected();
        bool Print(string strData);
        ConnectionType Type();
    }

    internal class USBConnection : IPrinterConnection
    {
        private readonly string m_strPrinterName;

        public USBConnection(string strPrinterName)
        {
            m_strPrinterName = strPrinterName;
        }

        #region implementation of IPrinterConnection

        public virtual bool IsConnected()
        {
            return RawPrinterHelper.IsConnect(m_strPrinterName);
        }

        public virtual bool Print(string strData)
        {
            return RawPrinterHelper.SendStringToPrinter(m_strPrinterName, strData);
        }

        public virtual ConnectionType Type()
        {
            return ConnectionType.kUSBConnection;
        }

        #endregion
    }

    internal class SerialConnection : IPrinterConnection
    {
        private readonly SerialPort m_serialPort;

        public SerialConnection(string strCOMPort)
        {
            try
            {
                m_serialPort = new SerialPort(strCOMPort);
                m_serialPort.DataReceived += OnDataReceived;
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().MarkAsCritical().Submit();
            }
        }

        ~SerialConnection()
        {
            if (m_serialPort != null && m_serialPort.IsOpen)
                m_serialPort.Close();
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
        }

        private bool Init()
        {
            if (m_serialPort != null)
            {
                try
                {
                    if (!m_serialPort.IsOpen)
                        m_serialPort.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    ex.ToExceptionless().MarkAsCritical().Submit();
                    return false;
                }
            }
            return false;
        }

        #region implementation of IPrinterConnection

        public virtual ConnectionType Type()
        {
            return ConnectionType.kUSBConnection;
        }

        public virtual bool IsConnected()
        {
            return true;
        }

        public virtual bool Print(string strData)
        {
            if (Init())
            {
                try
                {
                    m_serialPort.Write(strData);
                    //m_serialPort.Close();
                }
                catch (Exception ex)
                {
                    ex.ToExceptionless().MarkAsCritical().Submit();
                    return false;
                }
            }
            return false;
        }

        #endregion
    }
}