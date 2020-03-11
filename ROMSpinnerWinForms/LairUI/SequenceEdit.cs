using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ROMSpinner.Common.Lair;
using ROMSpinner.Business;

namespace ROMSpinner.LairUI
{
    public partial class SequenceEdit : UserControl
    {
        private LairSequenceData m_dat = null;
        private LairSequenceData m_datOld = null;

        public SequenceEdit()
        {
            InitializeComponent();

            lstSeqType.DataSource = Enum.GetNames(typeof(SequenceType));
        }

        public LairSequenceData SequenceData
        {
            set
            {
                m_dat = value;

                // if a new index has been selected
                // (we don't want to grab frames if we don't have to since it is potentially slow)
                if (m_datOld != m_dat)
                {
                    uint uEndFrame = m_dat.FrameNum + LairMath.TicksToFramesU(m_dat.Ticks, true);

                    startEndFrameViewer1.Init(
                        m_dat.FrameNum,
                        uEndFrame,
                        OnBeginFrameChanged,
                        OnEndFrameChanged);

                    txtSeqName.Text = m_dat.Name;

                    lstSeqType.SelectedIndex = (int)m_dat.Type;
                    RefreshProp(m_dat.Type);
                    m_datOld = m_dat;
                }
            }
        }

        private void RefreshProp(SequenceType type)
        {
            if (m_dat != null)
            {
                switch (type)
                {
                    case SequenceType.Normal:
                        propertySegment.SelectedObject = new LairUI.NormalSequenceProperties(m_dat);
                        break;
                    case SequenceType.EndSuccess:
                        propertySegment.SelectedObject = new LairUI.SuccessSequenceProperties(m_dat);
                        break;
                    case SequenceType.EndDeath:
                        propertySegment.SelectedObject = new LairUI.DeathSequenceProperties(m_dat);
                        break;
                    case SequenceType.GameOver:
                        propertySegment.SelectedObject = new LairUI.GameOverSequenceProperties(m_dat);
                        break;
                    case SequenceType.AttractMode:
                        propertySegment.SelectedObject = new LairUI.AttractSequenceProperties(m_dat);
                        break;
                }
            }
        }

        private void lstSeqType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int idx = cb.SelectedIndex;
            RefreshProp((SequenceType)idx);
        }

        private void OnBeginFrameChanged(uint uNewFrameNum)
        {
        }

        private void OnEndFrameChanged(uint uNewFrameNum)
        {
        }

    }
}
