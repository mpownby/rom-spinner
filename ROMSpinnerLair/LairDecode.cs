using System;
using System.Collections.Generic;
using System.Text;
using ROMSpinner.Common.Lair;

namespace ROMSpinner.Lair
{
    public class CLairDecode
    {
        CLair m_pLair = null;

        public CLairDecode(CLair pLair)
        {
            m_pLair = pLair;
        }

        public LairScenesData DecodeAllScenes()
        {
            List<LairSceneData> lstScenes = new List<LairSceneData>();
            List<LairSequenceData> lstSequences = new List<LairSequenceData>();

            // to make sure each sequence name is unique
            Dictionary<uint, string> dictSeqNames = new Dictionary<uint, string>();

            // do each scene
            for (byte u8 = 0x80; u8 <= 0xA7; u8++)
            {
                List<LairSequence> lst = m_pLair.GetSceneSequences(u8);

                char chLetter = 'A';    // for unique sequence naming

                List<string> lstSeqNames = new List<string>();

                // go through each sequence
                foreach (LairSequence seq in lst)
                {
                    LairSegment seg = seq.Segments[0];  // each sequence must have at least one segment
                    uint uAddr = seg.SegmentArray.Addr; // this is the address where the seqence begins

                    // set the sequence name

                    // if this sequence has not been used already, then decode it and create a name for it
                    if (!dictSeqNames.ContainsKey(uAddr))
                    {
                        LairSequenceData dat = DecodeSequence(seq);

                        dat.Name = string.Format("{0} [{1}]",
                            CLair.SceneIdxToName(u8), chLetter);

                        // add it to the global list
                        lstSequences.Add(dat);

                        dictSeqNames[uAddr] = dat.Name;

                        chLetter++; // move to the next letter
                    }
                    // else sequence has already been used elsewhere, so don't add it twice

                    lstSeqNames.Add(dictSeqNames[uAddr]);
                } // end foreach

                // add the scene data

                LairSceneData pScene = new LairSceneData();
                //pScene.SceneIdx = (uint) (u8 & 0x7F);    // strip off high bit since it would just confuse people
                pScene.SequenceNames = lstSeqNames;
                pScene.SceneName = CLair.SceneIdxToName(u8);
                lstScenes.Add(pScene);

            } // end for

            LairScenesData pRes = new LairScenesData();
            pRes.Scenes = lstScenes;
            pRes.Sequences = lstSequences;

            return pRes;
        }

        public LairSequenceData DecodeSequence(LairSequence seq)
        {
            LairSequenceData res = new LairSequenceData();

            List<LairSegment> lstSegs = seq.Segments;

            foreach (LairSegment seg in lstSegs)
            {
                if (seg.IsTrailer)
                {
                    res.ScoreIdx = (uint)seg.ScoreIdx.OurObj;
                    if (seg.HasNextSequence)
                    {
                        res.NextSequence = Convert.ToUInt32(seg.NextSequence.OurObj);
                    }
                    res.Ticks = Convert.ToUInt32(seg.TrailerTicks.OurObj);
                    res.CanEasterEggBeEnabled = Convert.ToBoolean(seg.CanEasterEggBeEnabled.OurObj);
                    res.IsStillFrame = Convert.ToBoolean(seg.IsStillFrame.OurObj);
                    res.PayAsYouGoAfterSequence = Convert.ToBoolean(seg.PayAsYouGoAfterSequence.OurObj);
                    res.IgnoreNextSeek = Convert.ToBoolean(seg.TrailerIgnoreNextSeek.OurObj);
                    res.FrameNum = Convert.ToUInt16(seg.FrameNum.OurObj);
                    res.Type = (SequenceType)seg.GetSequenceType().OurObj;

                    res.UseNextSelectionGroup = Convert.ToBoolean(seg.UseSegmentNextSelIdx.OurObj);
                    if (seg.HasNextSelGroup)
                    {
                        uint u = Convert.ToUInt32(seg.NextSelGroup.OurObj);
                        u &= 0x7F;  // strip off the high bit since it would just confuse the end user
                        res.NextSelectionGroup = u;
                    }
                }
                    // else it's a regular move
                else
                {
                    LairMoveData move = new LairMoveData();
                    move.Move = seg.GetMove();
                    move.TicksBeforeMoveCanBeAccepted = Convert.ToUInt32(seg.TicksBeforeInputAccepted.OurObj);

                    uint uTimeWindowCount = Convert.ToUInt32(seg.TimeWindowCount.OurObj);

                    for (uint u = 0; u < uTimeWindowCount; u++)
                    {
                        TimeWindow window = new TimeWindow();
                        if ((bool) seg.CanNextSeekBeIgnored.OurObj == true)
                        {
                            // exception can be thrown if seeks can't be skipped
                            window.IgnoreNextSeek = Convert.ToBoolean(seg.IsSeekSkippedOnNextSequence(u).OurObj);
                        }

                        window.NextSequence = Convert.ToUInt32(seg.GetNextSequence(u).OurObj);
                        window.Ticks = Convert.ToUInt32(seg.GetTicksForWindow(u).OurObj);

                        move.TimeWindows.Add(window);
                    }
                    res.Moves.Add(move);
                }
            }

            return res;
        }
    }
}
