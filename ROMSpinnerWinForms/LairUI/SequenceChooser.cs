using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ROMSpinner.Common.Lair;

namespace ROMSpinner.LairUI
{
    public partial class SequenceChooser : UserControl
    {
        public delegate void IndexChangedCallback(int idx);

        private LairScenesData m_datScenes = null;
        private LairScenesIndexer m_indexerScenes = null;
        private IndexChangedCallback m_pCallback = null;
        
        public SequenceChooser()
        {
            InitializeComponent();
        }

        public LairScenesData Scenes
        {
            set
            {
                m_datScenes = value;

                // make it easy to locate sequences
                m_indexerScenes = new LairScenesIndexer(value);
            }
        }

        public void BindScene(int iSceneIdx, IndexChangedCallback pCallback)
        {
            m_pCallback = pCallback;
            LairSceneData scene = m_datScenes.Scenes[iSceneIdx];

            for (int idx = 0; idx < scene.SequenceNames.Count; idx++)
            {
                LairSequenceData seq = m_indexerScenes.GetSequenceData(iSceneIdx, idx);
                string s = scene.SequenceNames[idx];
                string strType = seq.Type.ToString();
                gridPointers.Rows.Add(new object[] { idx, strType, s });
            }
        }

        private void gridPointers_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            m_pCallback(e.RowIndex);
        }

    }
}
