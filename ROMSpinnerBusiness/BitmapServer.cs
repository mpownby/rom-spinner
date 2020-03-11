using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace ROMSpinner.Business
{
    public class BitmapCacheThreadSafe
    {
        private static Dictionary<uint, Bitmap> m_dictBMP = new Dictionary<uint, Bitmap>();
        private static List<uint> m_lstQueuedFrames = new List<uint>();

        public static Bitmap Get(uint uFrameNumber)
        {
            lock (typeof(BitmapCacheThreadSafe))
            {
                bool bFrameSent = false;
                Bitmap bmp = null;

                // if bmp has been cached, simply return it
                if (m_dictBMP.TryGetValue(uFrameNumber, out bmp))
                {
                    bFrameSent = true;
                }

                // if frame was not sent, we need to grab it (which can take a while)
                if (!bFrameSent)
                {
                    // primitive safety check to make sure we don't cache too many frames
                    if (m_dictBMP.Count > 20)
                    {
                        // TODO : devise a way to remove the oldest entries
                        m_dictBMP.Clear();
                    }

                    bmp = DaphneIO.GetFrame(uFrameNumber);

                    // if we grabbed it successfully
                    if (bmp != null)
                    {
                        m_dictBMP[uFrameNumber] = bmp;
                    }
                    // else we failed, so no callback is called
                }

                return bmp;
            } // end lock
        }
    }
}
