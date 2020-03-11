using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ROMSpinner.Business;

namespace ROMSpinner.Win.CommonUI
{
    public delegate void FrameChangedCallback(uint uNewFrame);

    public partial class FrameViewer : UserControl
    {
        private FrameChangedCallback m_callback = null;

        // to queue up frames if we get another request before the background worker is finished
        List<uint> lstQueuedFrames = new List<uint>();

        public FrameViewer()
        {
            InitializeComponent();
        }

        public void Init(uint uFrameNum, FrameChangedCallback callback)
        {
            m_callback = callback;
            numericUpDown1.Value = uFrameNum;

            // setting the numeric up/down value will automatically call UpdatePicture
            UpdatePicture(uFrameNum);
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            FixAspectRatio();
        }

        /// <summary>
        /// Make sure the image isn't stretched
        /// </summary>
        private void FixAspectRatio()
        {
            double dRatio = ((double)pictureBox1.Size.Width) / pictureBox1.Size.Height;

            // if width is too wide
            if (dRatio > 1.333)
            {
                int iWidth = (pictureBox1.Height * 4) / 3;
                pictureBox1.Width = iWidth;
            }
                // else height is too tall
            else
            {
                pictureBox1.Height = (pictureBox1.Width * 3) / 4;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            uint uNewFrame = Convert.ToUInt32(numericUpDown1.Value);
            UpdatePicture(uNewFrame);
            m_callback(uNewFrame);
        }

        private void UpdatePicture(uint uFrameNum)
        {
            try
            {
                backgroundWorker1.RunWorkerAsync(uFrameNum);
            }
                // if backgroundworker is busy, this exception will be thrown
            catch (InvalidOperationException)
            {
                // queue up this frame for when the background worker isn't busy
                lstQueuedFrames.Add(uFrameNum);
            }
        }

        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap bmp = BitmapCacheThreadSafe.Get((uint) e.Argument);
            e.Result = bmp;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox1.Image = (Bitmap) e.Result;

            // check to see if we have any queued frames
            if (lstQueuedFrames.Count > 0)
            {
                // sanity check: if too many frames are queued, drop the excess
                // (extra queued frames can result if the user spams the up or down arrow)
                if (lstQueuedFrames.Count > 4)
                {
                    int iExcess = lstQueuedFrames.Count - 4;

                    // get rid of the oldest requests
                    lstQueuedFrames.RemoveRange(0, iExcess);
                }

                // NOTE : the sequence of operations here is important, I think
                // (list must be handled before UpdatePicture can be called to avoid thread conflicts)
                uint uFrame = lstQueuedFrames[0];
                lstQueuedFrames.RemoveAt(0);
                UpdatePicture(uFrame);
            }
        }

    }
}
