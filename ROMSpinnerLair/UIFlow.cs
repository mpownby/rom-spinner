using System;
using System.Collections.Generic;
using System.Text;
using ROMSpinner.Common;
using ROMSpinner.Common.Lair;

namespace ROMSpinner.Lair
{
    public class LairFlowUI
    {
        CLair m_lair;
        byte m_u8SceneIdx;
        ITreeView m_view;
        List<LairSequence> m_lstSequences = null;
        List<ITreeNode> m_lstSuccessNodes = new List<ITreeNode>();
        List<ITreeNode> m_lstDeathNodes = new List<ITreeNode>();

        public LairFlowUI(CLair pLair, byte u8SceneIdx, ITreeView view)
        {
            m_lair = pLair;
            m_u8SceneIdx = u8SceneIdx;
            m_view = view;
            m_lstSequences = pLair.GetSceneSequences(u8SceneIdx);
        }

        public void UpdateTreeView()
        {
            ITreeNode nodeNew = m_view.NewNode();
            GetSegmentsNode(nodeNew, 0, false, 0);   // non-death sequence
            m_view.AddChild(nodeNew);

            nodeNew = m_view.NewNode();
            GetSegmentsNode(nodeNew, 1, false, 0);   // resurrection sequence
            m_view.AddChild(nodeNew);

            m_view.CollapseAll();

            DecorateDeath();
            DecorateSuccess();
        }

        private void GetSegmentsNode(ITreeNode nodeRoot,
            uint uSequenceIdx, bool bSeekIgnored, uint uPointSum)
        {
            ByteArrayAndObj bao = null;
            ITreeNode nodeChild = null;

            if (uSequenceIdx <= 1)
            {
                if (uSequenceIdx == 0)
                {
                    nodeRoot.Text = "If Dirk Completed Previous Scene";
                }
                else if (uSequenceIdx == 1)
                {
                    nodeRoot.Text = "If Dirk Died on Previous Scene";
                }
                // add an extra node so things don't get too crowded
                nodeChild = nodeRoot.NewNode();
                nodeRoot.AddChild(nodeChild);
                nodeRoot = nodeChild;
            }

            nodeRoot.Text = "Sequence " + uSequenceIdx;

            List<LairSegment> lstSegments = m_lstSequences[(int)uSequenceIdx].Segments;

            // get new points for getting this far
            uint uScoreIdx = (uint)lstSegments[lstSegments.Count - 1].ScoreIdx.OurObj;
            uint uPoints = m_lair.ScoreIdxToScore(uScoreIdx);
            uPointSum += uPoints;

            // go through each segment
            foreach (LairSegment segment in lstSegments)
            {
                ITreeNode node = nodeRoot.NewNode();

                // if this is a move segment
                if (!segment.IsTrailer)
                {
                    double dTimeSum = 0.0;
                    byte u8 = 0;

                    bao = segment.TicksBeforeInputAccepted;
                    u8 = (byte)bao.OurObj;
                    dTimeSum = LairMath.TicksToSeconds(u8); // start with ticks before input is accepted

                    Move move = segment.GetMove();

                    byte uTimeWindowCount = (byte)segment.TimeWindowCount.OurObj;

                    for (uint u = 0; u < uTimeWindowCount; u++)
                    {
                        node = nodeRoot.NewNode();
                        node.Text = move.ToString() + " (" + dTimeSum + " to ";

                        byte uTicks = (byte)segment.GetTicksForWindow(u).OurObj;
                        dTimeSum += LairMath.TicksToSeconds(uTicks);    // we add in order to be relative to the sequence beginning time

                        node.Text += dTimeSum + " elapsed seconds)";

                        bao = segment.GetNextSequence(u);
                        byte u8NextSeq = (byte)bao.OurObj;
                        bool bNextSeekIgnored = false;
                        try
                        {
                            bNextSeekIgnored = (bool)segment.IsSeekSkippedOnNextSequence(u).OurObj;
                        }
                        catch { }

                        // On the final scenes (dragon's lair, falling platform), the next seqence can be 0xff
                        // I haven't disassembled the ROM to see what it means so for now I just ignore it.
                        if (u8NextSeq != 0xff)
                        {
                            nodeChild = node.NewNode();
                            GetSegmentsNode(nodeChild, u8NextSeq, bNextSeekIgnored, uPointSum);
                            node.AddChild(nodeChild);
                        }
                        nodeRoot.AddChild(node);
                    }
                }
                // else it's the trailer
                else
                {
                    if (!bSeekIgnored)
                    {
                        if ((bool)segment.IsStillFrame.OurObj == false)
                        {
                            nodeRoot.Text += " - Seek & Play Frame ";
                        }
                        else
                        {
                            nodeRoot.Text += " - Still Frame ";
                        }
                        nodeRoot.Text += segment.GetFrameNum().OurObj;
                    }
                    else
                    {
                        nodeRoot.Text += " - Seek Ignored";
                    }

                    if (uPoints > 0)
                    {
                        nodeRoot.Text += " - +" + uPoints + " points";
                    }

                    bao = segment.GetSequenceType();
                    SequenceType type = (SequenceType)bao.OurObj;
                    switch (type)
                    {
                        case SequenceType.EndDeath:
                            node.Text = "Death - " + uPointSum + " Total Points";
                            m_lstDeathNodes.Add(node);
                            nodeRoot.AddChild(node);
                            node = AddTimeoutNode(nodeRoot, segment);
                            nodeChild = node.NewNode();
                            nodeChild.Text = "Go to Sequence 1 of Next Scene";
                            node.AddChild(nodeChild);
                            break;
                        case SequenceType.Normal:
                            node = AddTimeoutNode(nodeRoot, segment);

                            // since this isn't the end, add another node to the timeout
                            byte u8NextSeq = (byte)segment.NextSequence.OurObj;
                            bool bNextSeekIgnored = (bool)segment.TrailerIgnoreNextSeek.OurObj;
                            nodeChild = node.NewNode();
                            GetSegmentsNode(nodeChild, u8NextSeq, bNextSeekIgnored, uPointSum);
                            node.AddChild(nodeChild);
                            break;
                        case SequenceType.EndSuccess:
                            node.Text = "Success - " + uPointSum + " Total Points";
                            m_lstSuccessNodes.Add(node);
                            nodeRoot.AddChild(node);
                            node = AddTimeoutNode(nodeRoot, segment);
                            nodeChild = node.NewNode();
                            nodeChild.Text = "Go to Sequence 0 of Next Scene";
                            node.AddChild(nodeChild);
                            break;

                        // if it's something like game over, then ignore
                        default:
                            break;
                    }
                }
            } // end foreach
        }

        private ITreeNode AddTimeoutNode(ITreeNode nodeRoot, LairSegment segment)
        {
            ITreeNode node = nodeRoot.NewNode();
            node.Text = "Timeout after " + LairMath.TicksToSeconds((uint)segment.TrailerTicks.OurObj) + " seconds";
            nodeRoot.AddChild(node);
            return node;
        }

        private void DecorateDeath()
        {
            // put icons next to all success paths
            foreach (ITreeNode nodeDeath in m_lstDeathNodes)
            {
                ITreeNode node = nodeDeath;

                // go all the way up
                while (node.Parent != null)
                {
                    node.Icon = "bad.gif";
                    node = node.Parent;
                }

                node.Icon = "bad.gif";
            }
        }

        private void DecorateSuccess()
        {
            // put icons next to all success paths
            foreach (ITreeNode nodeSuccess in m_lstSuccessNodes)
            {
                ITreeNode node = nodeSuccess;

                // go all the way up
                while (node.Parent != null)
                {
                    node.Icon = "good.gif";
                    node = node.Parent;
                }

                node.Icon = "good.gif"; // so expand button will work
            }
        }

    }
}
