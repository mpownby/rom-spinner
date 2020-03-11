using System;
using System.Collections.Generic;
using System.Text;
using ROMSpinner.Common;
using ROMSpinner.Common.Lair;

namespace ROMSpinner.Lair
{
    public class LairDetailedUI
    {
        CLair m_lair;
        byte m_u8SceneIdx;
        ITreeView m_view;
        List<LairSequence> m_lstSequences = null;

        public LairDetailedUI(CLair pLair, byte u8SceneIdx, ITreeView view)
        {
            m_lair = pLair;
            m_u8SceneIdx = u8SceneIdx;
            m_view = view;
            m_lstSequences = pLair.GetSceneSequences(u8SceneIdx);
        }

        public void UpdateTreeView()
        {
            ITreeNode nodeRoot = m_view.NewNode();
            nodeRoot.Text = "Scene Sequences - " + CLair.SceneIdxToName(m_u8SceneIdx);

            // go through every sequence
            for (int iSeqIdx = 0; iSeqIdx < m_lstSequences.Count; iSeqIdx++)
            {
                LairSequence seq = m_lstSequences[iSeqIdx];
                string strSeqTitle = "Sequence " + iSeqIdx.ToString("x");

                // sequence 1 always seems to be the resurrection one
                if (iSeqIdx == 1)
                {
                    strSeqTitle = "Resurrection " + strSeqTitle;
                }

                ITreeNode nodeSeq = m_view.NewNode();
                nodeSeq.Text = strSeqTitle;
                string nodeIcon = "blank16.png";	// icon will change depending on segment type

                List<LairSegment> lstSegs = seq.Segments;
                for (int iSegIdx = 0; iSegIdx < lstSegs.Count; iSegIdx++)
                {
                    LairSegment seg = lstSegs[iSegIdx];

                    string strTitle = "Segment " + iSegIdx + " - " +
                        seg.SegmentArray.ToString();
                    if (seg.IsTrailer)
                    {
                        ByteArrayAndObj bao = seg.GetSequenceType();
                        SequenceType type = (SequenceType)bao.OurObj;
                        strTitle = "Timeout " + strTitle;
                        if (type != SequenceType.Normal)
                        {
                            switch (type)
                            {
                                case SequenceType.EndSuccess:
                                    nodeIcon = "good.gif";
                                    break;
                                case SequenceType.AttractMode:
                                    // TODO
                                    break;
                                case SequenceType.EndDeath:
                                    nodeIcon = "bad.gif";
                                    break;
                                default:	// bones
                                    // TODO
                                    break;
                            }
                        }
                    }
                    else
                    {
                        strTitle = seg.GetMove() + " " + strTitle;
                    }

                    ITreeNode nodeSeg = m_view.NewNode();
                    nodeSeg.Text = strTitle;
                    DoSegment(seg, nodeSeg);
                    nodeSeq.AddChild(nodeSeg);
                } // end for

                nodeSeq.Icon = nodeIcon;
                nodeRoot.AddChild(nodeSeq);
            }

            m_view.AddChild(nodeRoot);  // add finished product
            m_view.CollapseAll();
        }

        private void DoSegment(LairSegment seg, ITreeNode nodeSeg)
        {
            ByteArrayAndObj bao = null;

            try
            {
                AddNode("Can Next Seek Be Ignored", seg.CanNextSeekBeIgnored, nodeSeg, true);
            }
            catch { }

            try
            {
                AddNode("Sequence Branches", seg.SequenceBranchCount, nodeSeg, true);
            }
            catch { }

            try
            {
                bao = seg.TimeWindowCount;
                ITreeNode nodeWindows = AddNode("Time Windows", bao, nodeSeg, true);

                byte uCount = (byte)bao.OurObj;

                // Iterate through timing windows to get details about each window
                for (uint u = 0; u < uCount; u++)
                {
                    string strPre = "Window " + u + ": ";
                    AddNode(strPre + "Sequence Branch", seg.GetNextSequence(u), nodeWindows, true);
                    AddNode(strPre + "Sequence Branch Offset", seg.GetBranchOffset(u), nodeWindows, true);
                    AddNode(strPre + "Ticks", seg.GetTicksForWindow(u), nodeWindows, false);

                    try
                    {
                        AddNode(strPre + "Ignore Next Seek",
                            seg.IsSeekSkippedOnNextSequence(u), nodeWindows, true);
                    }
                    catch
                    {
                    }
                }
            }
            catch { }

            try
            {
                AddNode("Ticks Before Move Can Be Accepted", seg.TicksBeforeInputAccepted, nodeSeg, false);
            }
            catch { }

            // if this is the trailer segment, then the stuff we get back is different
            if (seg.IsTrailer)
            {
                string s;
                bao = seg.ScoreIdx;
                uint uIdx = (uint)bao.OurObj;
                s = "(" + m_lair.ScoreIdxToScore(uIdx) + " points)";
                AddNode("Score Index", s, bao, nodeSeg, true);

                // only applies to trailer, where there is always only one next one
                try
                {
                    AddNode("When time expires, go to this Sequence", seg.NextSequence, nodeSeg, true);
                }
                catch { }

                try
                {
                    AddNode("Next Scene Selection Group", seg.NextSelGroup, nodeSeg, true);
                }
                catch { }

                bao = seg.TrailerTicks;
                AddNode("Time Ticks Before Next Sequence Begins", bao, nodeSeg, true);

                // easter egg
                AddNode("Easter Egg Window is Active", seg.CanEasterEggBeEnabled, nodeSeg, false);

                AddNode("Use Our Next Scene Selection Index", seg.UseSegmentNextSelIdx, nodeSeg, false);

                // scene repeats on death
                AddNode("Repeat Scene On Death", seg.RepeatSceneOnDeath, nodeSeg, false);

                // still frame
                AddNode("Is Still Frame", seg.IsStillFrame, nodeSeg, false);

                try
                {
                    // pay as you go
                    AddNode("Pay As You Go After Sequence", seg.PayAsYouGoAfterSequence, nodeSeg, false);
                }
                catch { }

                try
                {
                    AddNode("Ignore Next Seek", seg.TrailerIgnoreNextSeek, nodeSeg, false);
                }
                catch { }

                AddNode("Frame Number", seg.GetFrameNum(), nodeSeg, false);
            }
            else
            {
            }
        }

        private ITreeNode AddNode(string strText, ByteArrayAndObj bao, ITreeNode nodeParent, bool bHex)
        {
            return AddNode(strText, string.Empty, bao, nodeParent, bHex);
        }

        private ITreeNode AddNode(string strText, string strObjectSuffix, ByteArrayAndObj bao, ITreeNode nodeParent, bool bHex)
        {
            // add value
            strText += ": ";

            if (bHex &&
                ((bao.OurObj.GetType() == typeof(byte)) ||
                (bao.OurObj.GetType() == typeof(int)))
                )
            {
                uint u = Convert.ToUInt32(bao.OurObj);
                strText += u.ToString("x");
            }
            else
            {
                strText += bao.OurObj;
            }

            if (strObjectSuffix != string.Empty)
            {
                strText += " " + strObjectSuffix + " ";
            }

            // try adding 'which bit' to the end
            // add hex stuff
            strText += " - " + bao.OurByteArray.ToString();

            try
            {
                int iBit = 0;
                byte u8Mask = bao.ByteMask;
                uint uMaskBitCount = 0;

                // count how many bits are in the mask, if there are too many, we won't display each bit
                for (iBit = 0; iBit < 8; iBit++)
                {
                    if ((u8Mask & 1) == 1)
                    {
                        uMaskBitCount++;
                    }
                    u8Mask >>= 1;
                }

                u8Mask = bao.ByteMask;

                // if the whole byte isn't relevant
                if (u8Mask != 0xFF)
                {
                    if (uMaskBitCount <= 2)
                    {
                        byte u8BitVal = bao.OurByteArray.Array[0];
                        for (iBit = 0; iBit < 8; iBit++)
                        {
                            // if this bit is relevant
                            if ((u8Mask & 1) == 1)
                            {
                                strText += " - Bit " + iBit + " is " +
                                    (u8BitVal & 1);
                            }

                            u8BitVal >>= 1;
                            u8Mask >>= 1;
                        }
                    }
                    // too many bits, so just display the mask
                    else
                    {
                        strText += " - AND Bit Mask is " + u8Mask.ToString("x");
                    }
                }
            }
            catch { }

            // highlight entries that don't apply
            if (!bao.IsApplicable)
            {
                strText = "[N/A] " + strText;
            }

            ITreeNode nodeChild = m_view.NewNode();
            nodeChild.Text = strText;

            if (!bao.IsApplicable)
            {
                // TODO : some icon for N/A
            }

            nodeParent.AddChild(nodeChild);

            return nodeChild;
        }
    }
}
