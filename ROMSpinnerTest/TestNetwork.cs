using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using NUnit.Framework;
using ROMSpinner.Business;
using ROMSpinner.Common;

namespace ROMSpinner.Test
{
    [TestFixture]
    public class TestNetwork
    {
        [Test]
        public void TestDownload()
        {
            string strURL = "http://www.rulecity.com";
            byte[] bufDownloaded = null;

            // first, get correct buffer using built-in download routines
            using (WebClient wc = new WebClient())
            {
                bufDownloaded = wc.DownloadData(strURL);
            }

            Downloader d = new Downloader();

            using (MemoryStream strWriter = new MemoryStream())
            {
                d.StartDownload(strWriter, strURL);

                bool bTimeout = false;
                bool bDone = false;
                DateTime dt = DateTime.Now;
                while (!bDone)
                {
                    Downloader.Status stat = d.GetStatus();
                    if (stat == Downloader.Status.Stopped)
                    {
                        bDone = true;
                    }

                    TimeSpan ts = DateTime.Now - dt;
                    if (ts.Seconds > 5)
                    {
                        bTimeout = true;
                        bDone = true;
                    }
                }

                // we shouldn't have timed out
                Assert.AreEqual(false, bTimeout);

                byte [] bufNew = strWriter.ToArray();
                Assert.AreEqual(bufDownloaded, bufNew);
            }
        }

        [Test]
        public void TestUpdateAvailable()
        {
            UpdateInfo info = new UpdateInfo();
            info.ver.Major = 1;
            info.ver.Minor = 0;
            info.ver.Build = 1;

            // set all active updates to our contrived info
            UpdateTrees trees = new UpdateTrees();
            trees.stable = info;
            trees.dev = info;

            Updater u = new Updater(trees);

            // this should indicate that an update is available.
            bool bRes = u.UpdateAvailable(1, 0, 0);
            Assert.AreEqual(true, bRes);

            // but if versions match, no update should be available
            bRes = u.UpdateAvailable(1, 0, 1);
            Assert.AreEqual(false, bRes);

            // what if minor and build are greater but major is less?
            bRes = u.UpdateAvailable(0, 5, 5);
            Assert.AreEqual(true, bRes);

            // major is the same but minor is greater?
            bRes = u.UpdateAvailable(1, 1, 0);
            Assert.AreEqual(false, bRes);
        }

        private Socket m_sck = null;
        private Socket m_sckServer = null;

        [Test]
        public void TestSockets()
        {
            m_sck = null;
            m_sckServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp);
            m_sckServer.NoDelay = true;

            UInt16 u16Port = 6676;

            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), (int) u16Port);
            m_sckServer.Bind(iep);
            m_sckServer.Listen(5);
            IAsyncResult iar = m_sckServer.BeginAccept(AcceptCallback, null);

            // try to connect to our listening socket
            Networking n = new Networking();
            bool bRes = n.Connect("127.0.0.1", u16Port, 5000);
            Assert.AreEqual(true, bRes);
            
            // wait until we get a connection (dangerous but oh well, it's just a unit test and shouldn't fail at this spot anyway)
            while (m_sck == null) ;

            // send 8 bytes through to the client
            byte[] buf = new byte[4];
            buf[0] = 1;
            m_sck.Send(buf);
            buf[0] = 83;
            m_sck.Send(buf);

            // now see if we can retrieve those 8 bytes
            byte[] buf2 = n.Receive(4, 2000);
            Assert.AreEqual(1, buf2[0]);

            // get 4 more
            buf2 = n.Receive(4, 2000);
            Assert.AreEqual(83, buf2[0]);

            // clean-up
            m_sck.Close();
            n.Disconnect();
            m_sckServer.Close();
        }

        private void AcceptCallback(IAsyncResult iar)
        {
            try
            {
                m_sck = m_sckServer.EndAccept(iar);
            }
                // if m_sckServer is closed before this callback has been called, then the callback gets called for some reason
            catch (ObjectDisposedException)
            {
            }
        }

        private class ThreadComm
        {
            public Bitmap m_bmp = null;
            public uint m_uFrameNum = 0;
            public bool m_bThreadDone = false;
        }

        private delegate void GetFrameCallback(ThreadComm comm);

        [Test]
        public void TestGetFrame()
        {
            uint uTimeoutMs = 5000;

            Bitmap bmpSrc = new Bitmap("test.bmp");
            Assert.AreNotEqual(bmpSrc, null);

            m_sck = null;
            m_sckServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp);
            m_sckServer.NoDelay = true;
            m_sckServer.LingerState.Enabled = true;

            UInt16 u16Port = 6676;

            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), (int)u16Port);
            m_sckServer.Bind(iep);
            m_sckServer.Listen(5);
            IAsyncResult iar = m_sckServer.BeginAccept(AcceptCallback, null);

            // this try is here to ensure that m_sckServer gets closed
            try
            {
                uint uFrame = 567;

                // initiate the process
                ThreadComm threadComm = new ThreadComm();
                threadComm.m_uFrameNum = uFrame;

                GetFrameCallback callback = GetFrameThread;
                IAsyncResult iarCallback = callback.BeginInvoke(threadComm, null, null);

                // wait until we get a connection, or the other thread finishes prematurely (times out)
                while (m_sck == null)
                {
                    // if the other thread is done already, that is an error too
                    if (threadComm.m_bThreadDone)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1);   // don't hog CPU
                }

                Assert.AreNotEqual(true, threadComm.m_bThreadDone);
                Assert.AreNotEqual(null, m_sck);

                Networking n = new Networking(m_sck);
                try
                {

                    // get version from client
                    byte[] buf = n.Receive(4, uTimeoutMs);
                    Assert.AreEqual(1, buf[0]);

                    // get XML length
                    byte[] buf2 = n.Receive(4, uTimeoutMs);
                    uint uLength = BitConverter.ToUInt32(buf2, 0);

                    // get the XML data
                    buf = n.Receive(uLength, uTimeoutMs);
                    DaphneCommand cmd = DaphneCommand.FromXML(buf);

                    // make sure the frame is correct
                    Assert.AreEqual(cmd.seek, uFrame);

                    response_v1 resp = new response_v1();
                    frame fr = new frame();
                    resp.frame = fr;
                    fr.w = (uint)bmpSrc.Width;
                    fr.h = (uint)bmpSrc.Height;
                    fr.pitch = (uint)bmpSrc.Width * 3;    // 3 bytes per pixel
                    using (MemoryStream stream = new MemoryStream())
                    {
                        resp.ToXML(stream); // convert it to XML
                        byte[] bufXML = stream.ToArray();

                        int iVersion = 1;
                        int iSize = bufXML.Length;

                        // send version
                        buf = BitConverter.GetBytes(iVersion);
                        n.Send(buf);

                        // send XML size
                        buf = BitConverter.GetBytes(iSize);
                        n.Send(buf);

                        // send XML
                        n.Send(bufXML);
                    }

                    // send the raw bitmap bits
                    Rectangle rect = new Rectangle(0, 0, bmpSrc.Width, bmpSrc.Height);
                    System.Drawing.Imaging.BitmapData pData = bmpSrc.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmpSrc.PixelFormat);
                    IntPtr pPixels = pData.Scan0;

                    // copy bitmap into byte array
                    int iLength = pData.Height * pData.Stride;
                    buf2 = new byte[iLength];
                    System.Runtime.InteropServices.Marshal.Copy(pPixels, buf2, 0, iLength);
                    bmpSrc.UnlockBits(pData);   // we're done with the bitmap being locked

                    // send bitmap length
                    buf = BitConverter.GetBytes(buf2.Length);
                    n.Send(buf);
                    // send bitmap
                    n.Send(buf2);

                    // wait for other thread to exit
                    callback.EndInvoke(iarCallback);

                    // check pixel values on resulting bmp
                    Color clrDst, clrSrc;
                    Bitmap bmp = threadComm.m_bmp;

                    for (int col = 0; col < 5; col++)
                    {
                        clrDst = bmp.GetPixel(col, 0);
                        clrSrc = bmpSrc.GetPixel(col, 0);
                        Assert.AreEqual(clrSrc.ToArgb(), clrDst.ToArgb());
                    }
                }
                finally
                {
                    // clean-up
                    n.Disconnect();
                }
            }

                // make sure this clean-up code is called regardless of any tests that fail
            finally
            {
                m_sckServer.Close();
            }
        }

        private void GetFrameThread(ThreadComm comm)
        {
            try
            {
                comm.m_bmp = DaphneIO.GetFrame(comm.m_uFrameNum);
            }
            catch
            {
            }
            comm.m_bThreadDone = true;
        }


    }
}
