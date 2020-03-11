using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using ROMSpinner.Common;

namespace ROMSpinner.Business
{
    public class DaphneIO
    {
        private static Networking m_pClient = null;

        // this class is meant to be static only
        private DaphneIO()
        {
        }

        private static bool Connect()
        {
            return Connect(2000);
        }

        private static bool Connect(uint uTimeoutMs)
        {
            bool bRes = false;
            m_pClient = new Networking();
            if (m_pClient.Connect(
                "127.0.0.1",
                0x1A14, uTimeoutMs))
            {
                bRes = true;
            }
            else
            {
                m_pClient = null;
            }
            return bRes;
        }

        private static bool IsConnected
        {
            get
            {
                if (m_pClient != null)
                {
                    return (m_pClient.IsConnected);
                }
                return false;
            }
        }

        public static Bitmap GetFrame(uint uFrame)
        {
            return GetFrame(uFrame, 3000);
        }

        public static Bitmap GetFrame(uint uFrame, uint uTimeoutMs)
        {
            // lock because we only allow 1 command sent to daphne at a time
            lock (typeof(DaphneIO))
            {
                Bitmap bmp = null;

                // this allows for one reconnect attempt (if we get disconnected) before giving up
                for (uint uAttempts = 0; (uAttempts < 2) && (bmp == null); uAttempts++)
                {
                    try
                    {
                        // if not connected, try to connect
                        if (!IsConnected)
                        {
                            if (!Connect(500))
                            {
                                return null;
                            }
                        }

                        bmp = GetFrameInternal(uFrame, uTimeoutMs);
                        break;  // if we get this far, we didn't get an exception, so if we are returning null, that is our final result

                    } // end try
                    catch
                    {
                        m_pClient.Disconnect();
                        m_pClient = null;
                    }
                }

                return bmp;
            } // end lock
        }   // function

        /// <summary>
        /// For internal use only, can throw exceptions and is not thread safe!
        /// </summary>
        /// <param name="uFrameNum"></param>
        /// <param name="uTimeoutMs"></param>
        /// <returns></returns>
        private static Bitmap GetFrameInternal(uint uFrame, uint TimeoutMs)
        {
            Bitmap bmp = null;
            byte[] buf = null;

            DaphneCommand cmd = new DaphneCommand();
            cmd.seek = uFrame;
            using (MemoryStream stream = new MemoryStream())
            {
                // send version
                buf = BitConverter.GetBytes(1);
                m_pClient.Send(buf);

                cmd.ToXML(stream);
                byte[] bufPayload = stream.ToArray();

                // send size
                buf = BitConverter.GetBytes(bufPayload.Length);
                m_pClient.Send(buf);

                // send payload
                m_pClient.Send(bufPayload);
            }

            // now receive the resulting frame back

            // get version
            buf = m_pClient.Receive(4, TimeoutMs);

            //byte[] bufTest = m_pClient.Receive(8, TimeoutMs);

            if (buf != null)
            {
                uint uVersion = BitConverter.ToUInt32(buf, 0);

                // if this is the right version
                if (uVersion == 1)
                {
                    // get size of XML
                    buf = m_pClient.Receive(4, TimeoutMs);
                    uint uXMLSize = BitConverter.ToUInt32(buf, 0);

                    // read the XML response
                    buf = m_pClient.Receive(uXMLSize, TimeoutMs);
                    response_v1 resp = response_v1.FromXML(buf);

                    // if we didn't get an error, then proceed
                    if (resp.error == string.Empty)
                    {
                        // get size of frame buffer
                        buf = m_pClient.Receive(4, TimeoutMs);
                        uint uFrameBufSize = BitConverter.ToUInt32(buf, 0);

                        // read the frame buffer
                        buf = m_pClient.Receive(uFrameBufSize, TimeoutMs);

                        bmp = new Bitmap((int)resp.frame.w,
                            (int)resp.frame.h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        Rectangle rect = new Rectangle(0, 0, (int)resp.frame.w, (int)resp.frame.h);
                        System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect,
                            System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

                        IntPtr ptr = bmpData.Scan0;

                        // copy array into bitmap
                        System.Runtime.InteropServices.Marshal.Copy(buf, 0, ptr, buf.Length);

                        // we're done
                        bmp.UnlockBits(bmpData);
                    }
                    // else we got an error, no more data is coming
                    else
                    {
                        string s = resp.error;
                    }
                }
                // version mismatch!
            } // end if we got something back from daphne

            // else buf is null

            return bmp;
        } // end function

    }   // class
} // namespace
