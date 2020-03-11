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
    public partial class StartEndFrameViewer : UserControl
    {
        public StartEndFrameViewer()
        {
            InitializeComponent();
        }

        public void Init(
            uint uBeginFrame,
            uint uEndFrame,
            FrameChangedCallback callbackBeginChanged,
            FrameChangedCallback callbackEndChanged)
        {
            frameViewerBegin.Init(uBeginFrame, callbackBeginChanged);
            frameViewerEnd.Init(uEndFrame, callbackEndChanged);
        }
    }
}
