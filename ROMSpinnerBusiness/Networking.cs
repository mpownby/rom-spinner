using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using ROMSpinner.Common;

namespace ROMSpinner.Business
{

    public class Networking
    {
        private Socket m_client;
        private byte[] m_buf = new byte[1024];
        private bool m_bConnected = false;
        private uint m_uBytesReceived = 0;
        private bool m_bReceiveReady = true;
        private SocketException m_sockEx = null;

        public Networking()
        {
        }

        /// <summary>
        /// in case we are inheriting an existing socket connection
        /// </summary>
        /// <param name="m_sck"></param>
        public Networking(Socket m_sck)
        {
            m_client = m_sck;
            m_bConnected = m_sck.Connected;
        }

        // For testing: ignores whether we have a valid cert
        // DON'T USE FOR PRODUCTION CODE!
        public static bool IgnoreServerCertificate(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public bool Connect(string strHostName, UInt16 u16Port, uint uTimeOutMs)
        {
            bool bRes = false;

            m_bConnected = false;
            m_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(strHostName), (int) u16Port);
            m_client.BeginConnect(iep, ConnectCallback, m_client);

            DateTime dt = DateTime.Now;

            for (; ; )
            {
                // if we connected
                if (m_bConnected)
                {
                    bRes = true;
                    break;
                }

                TimeSpan ts = DateTime.Now - dt;

                // if we've timed out without connecting, abort
                if (ts.TotalMilliseconds > uTimeOutMs)
                {
                    break;
                }

                System.Threading.Thread.Sleep(1);   // don't hog CPU
            }

            return bRes;
        }

        private void ConnectCallback(IAsyncResult iar)
        {
            try
            {
                Socket cli = (Socket)iar.AsyncState;
                cli.EndConnect(iar);
                m_bConnected = true;
            }
            catch (SocketException ex)
            {
                m_sockEx = ex;
            }
        }

        public bool Disconnect()
        {
            bool bRes = false;

            try
            {
                if (m_bConnected)
                {
                    m_client.Close();
                    m_client = null;
                    m_bConnected = false;
                    bRes = true;
                }
            }
            catch { }

            return bRes;
        }

        public bool IsConnected
        {
            get
            {
                if (m_client != null)
                {
                    return m_client.Connected;
                }
                return false;
            }
        }

        public int Send(byte[] buf)
        {
            int iRes = 0;

            iRes = m_client.Send(buf);

            return iRes;
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            SocketError errCode;
            int iBytesReceived = m_client.EndReceive(iar, out errCode);
            m_uBytesReceived = (uint) iBytesReceived;
            m_bReceiveReady = true;
        }

        public byte[] Receive(uint uExpectedBytes, uint uTimeOutMs)
        {
            byte[] bufTmp = new byte[uExpectedBytes];

            // debug: make sure buffer is being filled
            for (int i = 0; i < uExpectedBytes; i++)
            {
                bufTmp[i] = 0xFF;
            }

            object state = null;    // define if needed
            SocketError errCode;

            // if we are in the middle of receiving something, don't receive again
            if ((m_uBytesReceived != 0) || (!m_bReceiveReady))
            {
                return null;
            }

            DateTime dt = DateTime.Now;
            m_bReceiveReady = true;

            uint uTotalBytesReceived = 0;
            bool bTimedOut = false;

            using (MemoryStream stream = new MemoryStream())
            {
                while (uTotalBytesReceived < uExpectedBytes)
                {
                    // if we are ready to read another chunk of data
                    if (m_bReceiveReady)
                    {
                        // if we have previously received some data
                        if (m_uBytesReceived != 0)
                        {
                            // add to our overall stream
                            stream.Write(bufTmp, 0, (int) m_uBytesReceived);
                            uTotalBytesReceived += m_uBytesReceived;
                            m_uBytesReceived = 0;

                            // debug: make sure buffer is being filled
                            for (int i = 0; i < uExpectedBytes; i++)
                            {
                                bufTmp[i] = 0xFF;
                            }
                        }

                        int iBytesToRead = (int)(uExpectedBytes - uTotalBytesReceived);

                        if (iBytesToRead > 0)
                        {
                            m_bReceiveReady = false;
                            m_client.BeginReceive(bufTmp, 0, iBytesToRead, SocketFlags.None, out errCode, ReceiveCallback, state);
                        }
                    }

                    TimeSpan ts = DateTime.Now - dt;

                    // if we've timed out without connecting, abort
                    if (ts.TotalMilliseconds > uTimeOutMs)
                    {
                        bTimedOut = true;
                        break;
                    }

                    System.Threading.Thread.Sleep(1);   // don't hog CPU
                } // end while

                bufTmp = stream.ToArray();

                // if we failed ...
                if (bTimedOut)
                {
                    bufTmp = null;
                }
            } // end using

            return bufTmp;
        }

    } // end class
}
