using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;

namespace ROMSpinner.Business
{
    public class Downloader
    {
        private bool m_bThreadRunning = false;
        private Thread m_thread = null;
        private Status m_status = Status.Stopped;
        private DownloaderIO m_DownloadIO = null;

        public bool StartDownload(Stream stream, string strURL)
        {
            // if thread is already running, we can't download another one
            if (m_bThreadRunning)
            {
                return false;
            }

            m_thread = new Thread(DownloaderThread);

            m_DownloadIO = new DownloaderIO();  // initialize variables
            m_DownloadIO.strURL = strURL;
            m_DownloadIO.streamWriter = stream;
            m_bThreadRunning = true;
            m_thread.Start(m_DownloadIO);
            m_status = Status.Running;

            return true;
        }

        public enum Status
        {
            Error,
            Running,
            Stopping,
            Stopped
        }

        public Status GetStatus()
        {
            UpdateStatus();
            return m_status;
        }

        /// <summary>
        /// This will tell the downloading thread to cancel.
        /// </summary>
        public void BeginCancel()
        {
            m_DownloadIO.bCanceled = true;
            m_status = Status.Stopping;
        }

        private void UpdateStatus()
        {
            if (m_thread != null)
            {
                if (m_thread.ThreadState == ThreadState.Stopped)
                {
                    m_thread = null;
                    m_status = Status.Stopped;
                }
            }

            // an error trumps all other statuses
            if (m_DownloadIO.bError)
            {
                m_status = Status.Error;
            }
        }

        private delegate int ReadDelegate(byte[] buf, int offset, int count);

        private void DownloaderThread(object o)
        {
            DownloaderIO io = (DownloaderIO)o;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(io.strURL);
            req.Credentials = CredentialCache.DefaultCredentials;

            if (io.strETag != null)
            {
                req.Headers["If-None-Match"] = io.strETag;
            }

            if (io.dtLastModified != DateTime.MinValue)
            {
                req.IfModifiedSince = io.dtLastModified;
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            io.i64TotalBytes = resp.ContentLength;
            io.i64CurBytes = 0;

            Stream streamReader = resp.GetResponseStream();

            byte[] buf = new byte[16384];
            bool bDone = false;

            // read until we finish or are canceled
            while ((!io.bCanceled) && (!bDone))
            {
                try
                {
                    ReadDelegate dlgt = new ReadDelegate(streamReader.Read);

                    IAsyncResult res = dlgt.BeginInvoke(buf, 0, buf.Length, null, null);

                    // sleep waiting for read to finish or for operation to be canceled
                    while ((!io.bCanceled) && (!res.IsCompleted))
                    {
                        Thread.Sleep(1);    // don't sleep too long or download will be slow
                    }

                    // WARNING : this will block if operation was canceled, and I don't
                    //  know how to get around this.
                    // Do I just not call EndInvoke upon cancelation? that seems messy
                    int iBytesRead = dlgt.EndInvoke(res);

                    if (!io.bCanceled)
                    {
                        // if we get this far, we've read a full buffer length
                        io.i64CurBytes += iBytesRead;

                        // save bytes read to the streamer
                        io.streamWriter.Write(buf, 0, iBytesRead);
                    }

                }
                catch (Exception ex)
                {
                    bDone = true;
                    io.bError = true;
                    io.strErrorMsg = ex.Message;
                }

                if (io.i64CurBytes == io.i64TotalBytes)
                {
                    bDone = true;
                }
            }
        }

    }

    public class DownloaderIO
    {
        public string strETag = null;
        public DateTime dtLastModified = DateTime.MinValue;
        public string strURL = null;

        public Int64 i64CurBytes = 0;
        public Int64 i64TotalBytes = 0;

        public bool bError = false;
        public string strErrorMsg = null;

        /////

        // whether the downloading operation has been canceled by parent thread
        public bool bCanceled = false;

        public Stream streamWriter = null; // where to save downloaded content
    }

}
