using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ROMSpinner.Lair;
using ROMSpinner.Common;
using ROMSpinner.Common.Lair;
using ROMSpinner.Business;

namespace ROMSpinner.Test
{
	[TestFixture]
	public class LairTestSegments
	{
		[Test]
		public void TestAttract()
		{
            ByteArrayAndObj bao;
			byte [] arr = { 0x40, 0, 0xF5, 0x04, 0x03, 0x43, 0x01 };
			LairSegment seg = new LairSegment(new ByteArray(0, arr));
			seg.Parse();
            bao = seg.ScoreIdx;
			Assert.AreEqual(0, bao.OurObj);
			bao = seg.GetSequenceType();
			Assert.AreEqual(SequenceType.AttractMode, bao.OurObj);
            bao = seg.TrailerTicks;
			Assert.AreEqual(0x4F5, bao.OurObj);

			bao = seg.GetFrameNum();
			Assert.AreEqual(323, bao.OurObj);

			Assert.AreEqual(0, seg.NextSequence.OurObj);

			// TODO : put in test for this unjnown 0x03 value
		}

		[Test]
		public void TestScoreIdx()
		{
			byte [] arr = { 0x74, 0, 0, 0, 0, 0, 0 };
			LairSegment seg = new LairSegment(new ByteArray(0, arr));
			seg.Parse();
            ByteArrayAndObj bao = seg.ScoreIdx;
			Assert.AreEqual(4, bao.OurObj);	// score idx should be low nibble of first byte
		}

		[Test]
		public void TestByte4()
		{
			byte [] arr = { 0x40, 0, 0xF5, 0x04, 0x03, 0x43, 0x01 };	// attract mode, non-still frame
			byte [] arr2 = { 0x40, 0x0A, 0xB4, 0, 0x10, 0xC5, 0x00 };	// attract mode, sequence 4
			byte [] arrPlat2 = { 1, 9, 0x54, 0x75, 0x82, 1, 9 };	// falling platform, sequence 2, trailer segment
			byte [] arrPlat = { 0x10, 0x0A, 0x19, 0x00, 0x04, 0xEA, 0x3B };	// falling platform death, sequence 9
			byte [] arrBarding = { 1, 9, 0xC, 0x1E, 7, 9, 0x2A };	// Flying Horse Barding, sequence 7, has some non-applicable bits
			
			ByteArrayAndObj bao;
			LairSegment seg = new LairSegment(new ByteArray(1, arr));
			seg.Parse();
			Assert.AreEqual(false, seg.IsStillFrame.OurObj);	// no still frame
			Assert.AreEqual(true, seg.CanEasterEggBeEnabled.OurObj);	// easter egg can be enabled
			Assert.AreEqual(5, seg.IsStillFrame.OurByteArray.Addr);
			Assert.AreEqual(1, seg.IsStillFrame.OurByteArray.Array.Length);	// make sure we're just getting 1 byte
			Assert.AreEqual(false, seg.RepeatSceneOnDeath.OurObj);	// scene does not repeat on death

			seg = new LairSegment(new ByteArray(0, arr2));
			seg.Parse();
			Assert.AreEqual(true, seg.IsStillFrame.OurObj);	// should be a still frame
			Assert.AreEqual(false, seg.CanEasterEggBeEnabled.OurObj);	// easter egg can't be enabled

			seg = new LairSegment(new ByteArray(0, arrPlat));
			seg.Parse();
			Assert.AreEqual(true, seg.RepeatSceneOnDeath.OurObj);	// this scene repeats on death
			Assert.AreEqual(true, seg.RepeatSceneOnDeath.IsApplicable);	// since this is a death scene, this is applicable
			Assert.AreEqual(false, seg.UseSegmentNextSelIdx.OurObj);	// since the bit isn't set, this should be false
			Assert.AreEqual(false, seg.UseSegmentNextSelIdx.IsApplicable);	// since this is a death scene, this should be false

			seg = new LairSegment(new ByteArray(0, arrBarding));
			seg.Parse();
			Assert.AreEqual(false, seg.RepeatSceneOnDeath.IsApplicable);	// since this isn't a death scene, this value isn't applicable
			Assert.AreEqual(true, seg.RepeatSceneOnDeath.OurObj);

			seg = new LairSegment(new ByteArray(0, arrPlat2));
			seg.Parse();
			bao = seg.UseSegmentNextSelIdx;
			Assert.AreEqual(true, bao.OurObj);	// this should be true, since the bit in question is set
			Assert.AreEqual(false, bao.IsApplicable);	// this should be false, since arrPlat2[1]'s hi bit is clear
			bao = seg.NextSequence;
			Assert.AreEqual(9, bao.OurObj);	// should be 9 with no exception
			bao = seg.PayAsYouGoAfterSequence;
			Assert.AreEqual(false, bao.OurObj);

			// this segment has a pay-as-you-go option in it
			seg = new LairSegment(Segments.SegFallingPlatSuccess);
			seg.Parse();
			bao = seg.PayAsYouGoAfterSequence;
			Assert.AreEqual(true, bao.OurObj);
		}

		[Test]
		public void TestPlatSeq7()
		{
			LairSegment seg = new LairSegment(new ByteArray(0, Segments.SegFallingPlatSuccess));
			seg.Parse();
			ByteArrayAndObj bao = seg.UseSegmentNextSelIdx;
			Assert.AreEqual(true, bao.OurObj);	// this should be true, since the falling platform scene is telling us to jump to flaming ropes/closing wall
			Assert.AreEqual(true, bao.IsApplicable);	// this should also be true, since this is not a death scene
			bool bException = false;
			try
			{
				bao = seg.NextSequence;
			}
			catch
			{
				bException = true;
			}
			Assert.AreEqual(true, bException);	// we should get an exception because there is no next sequence for this one

			bao = seg.NextSelGroup;	// no exception for this one
			Assert.AreEqual(0x81, bao.OurObj);	// this is our next selection index
			Assert.AreEqual(true, bao.IsApplicable);	// it is applicable
		}

		[Test]
		public void TestMirroredBarding()
		{
			byte [] arrMirroredBardingSuccess = { 5, 0x83, 0x87, 0, 0, 0xdc, 0x41 };	// mirrored flying barding, sequence 8, has a non-applicable next scene index branch

			LairSegment seg = new LairSegment(new ByteArray(0, arrMirroredBardingSuccess));
			seg.Parse();
			Assert.AreEqual(false, seg.UseSegmentNextSelIdx.OurObj);	// since the bit isn't set, this should be false
			Assert.AreEqual(true, seg.UseSegmentNextSelIdx.IsApplicable);	// since this isn't a death scene, this should be true

			ByteArrayAndObj bao = seg.NextSelGroup;
			Assert.AreEqual(arrMirroredBardingSuccess[1], bao.OurObj);
			Assert.AreEqual(false, bao.IsApplicable);	// not applicable since this isn't the end of the scene
		}

		[Test]
		public void TestSegLengths()
		{
			uint uLen = 0;

			byte [] arrTrailer = { 0x40, 0, 0xF5, 0x04, 0x03, 0x43, 0x01 };	// attract mode, trailer seg
			uLen = LairSegment.GetSegmentLength(arrTrailer[0], arrTrailer[1]);
			Assert.AreEqual(7, uLen);

			byte [] arrGoodMove = { 0x99, 1, 3, 0x80, 0x71, 0x16 };	// first correct move in flying barding
			uLen = LairSegment.GetSegmentLength(arrGoodMove[0], arrGoodMove[1]);
			Assert.AreEqual(6, uLen);

			byte [] arrBadMove = { 0x81, 1, 9, 0x71, 0x16 };	// first bad move on flying barding
			uLen = LairSegment.GetSegmentLength(arrBadMove[0], arrBadMove[1]);
			Assert.AreEqual(5, uLen);

			byte [] arrFlamimgRopes = { 0xA8, 0x13 };	// first bytes of flaming ropes scene
			uLen = LairSegment.GetSegmentLength(arrFlamimgRopes[0], arrFlamimgRopes[1]);
			Assert.AreEqual(8, uLen);
		}

		private bool CompareBuf(int [] buf1, byte [] buf2)
		{
			bool bRes = false;

			if (buf1.Length == buf2.Length)
			{
				bRes = true;

				for (int i = 0; i < buf1.Length; i++)
				{
					// if buf1 is defined at this position and it's not equal, then test fails
					if ((buf1[i] != -1) && (buf1[i] != buf2[i]))
					{
						bRes = false;
						break;
					}
				}
			}

			return bRes;
		}

		private void ParseHelper(byte [] before, byte [] after)
		{
			LairSegment seg = new LairSegment(before);
			seg.Parse();
			Assert.AreEqual(true, CompareBuf(seg.ParsedSegmentArray, after));
		}

		[Test]
		public void TestSegParse()
		{
			byte [] arrSegIntroBefore = { 0, 3, 0xAB, 0, 0, 0x90, 5 };	// intro, sequence 0, before parsing
			byte [] arrSegIntroAfter = { 0, 3, 0, 0x90, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xAB, 0 };

			ParseHelper(arrSegIntroBefore, arrSegIntroAfter);
			
			byte [] arrSegWallBefore = { 0x91, 0x11, 3, 0x80, 0, 0x2A };	// closing wall Sequence 0, UP move, before
			byte [] arrSegWallAfter = { 0x91, 0x11, 3, 0, 0, 0, 0, 0x80, 0, 0, 0, 0, 0, 0x2A, 0, 0 };	// closing wall after parse

			ParseHelper(arrSegWallBefore, arrSegWallAfter);

			byte [] arrSegFallingPlatBefore = { 0xA4, 0x13, 9, 7, 0x54, 0x41, 0xA, 0xA };
			byte [] arrSegFallingPlatAfter = { 0xA4, 0x13, 9, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0x54, 0x41, 0xA, 0xA };

			ParseHelper(arrSegFallingPlatBefore, arrSegFallingPlatAfter);

			byte [] arrSegFallingDeathBefore = { 0x88, 1, 9, 0x54, 0x82 };
			byte [] arrSegFallingDeathAfter = { 0x88, 1, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x54, 0x82, 0, 0 };
			ParseHelper(arrSegFallingDeathBefore, arrSegFallingDeathAfter);
		}

		[Test]
		public void TestSeekRequired()
		{
            bool bEx = false;
			LairSegment seg = new LairSegment(Segments.SegFillingWallMove);
			seg.Parse();
			ByteArrayAndObj bao;

            try
            {
                // this shouldn't work for a non-trailer segment
                bao = seg.TrailerIgnoreNextSeek;
            }
            catch
            {
                bEx = true;
            }
            Assert.AreEqual(true, bEx);

            bao = seg.IsSeekSkippedOnNextSequence(0);   // now use proper syntax
			Assert.AreEqual(true, bao.OurObj);	// seek not require for filling wall ...
			Assert.AreEqual(true, bao.IsApplicable);	// and it is applicable because this is a move
			Assert.AreEqual(3, bao.OurByteArray.Addr);	// the byte in question is at offset 3
            bao = seg.CanNextSeekBeIgnored;
            Assert.AreEqual(true, bao.OurObj);

			// now try attract mode
			seg = new LairSegment(Segments.SegAttractMode);
			seg.Parse();
            bao = seg.TrailerIgnoreNextSeek;
            Assert.AreEqual(false, bao.OurObj);  // seek is required
            Assert.AreEqual(true, bao.IsApplicable);    // always applicable for trailer segments
            Assert.AreEqual(4, bao.OurByteArray.Addr);  // the byte in question is at position 4
            Assert.AreEqual(0x80, bao.ByteMask);   // the bit in question is 7

            // try trailer of first move of flaming ropes
            seg = new LairSegment(Segments.SegFlamingRopes1);
            seg.Parse();
            bao = seg.TrailerIgnoreNextSeek;
            Assert.AreEqual(true, bao.OurObj);  // seek not required
            Assert.AreEqual(true, bao.IsApplicable);
            Assert.AreEqual(4, bao.OurByteArray.Addr);  // the byte in question is at position 4
            Assert.AreEqual(0x80, bao.ByteMask);   // the bit in question is 7

            // now try second move of flaming ropes
            seg = new LairSegment(Segments.SegFlamingRopes2);
            seg.Parse();

            // try move window 0
            bao = seg.IsSeekSkippedOnNextSequence(0);
            Assert.AreEqual(false, bao.OurObj);  // seek is required, because it's a death
            Assert.AreEqual(true, bao.IsApplicable);
            Assert.AreEqual(4, bao.OurByteArray.Addr);  // the byte in question is at position 4
            // try move window 1
            bao = seg.IsSeekSkippedOnNextSequence(1);
            Assert.AreEqual(true, bao.OurObj);  // seek not required because it's the correct move
            Assert.AreEqual(true, bao.IsApplicable);
            Assert.AreEqual(4, bao.OurByteArray.Addr);  // the byte in question is at position 4

            // now try room of fire / bench
            seg = new LairSegment(Segments.SegRoomOfFire3);
            seg.Parse();
            // seek will not be skipped on first two windows, but will be skipped on third
            bao = seg.IsSeekSkippedOnNextSequence(0);
            Assert.AreEqual(false, bao.OurObj);
            bao = seg.IsSeekSkippedOnNextSequence(1);
            Assert.AreEqual(false, bao.OurObj);
            bao = seg.IsSeekSkippedOnNextSequence(2);
            Assert.AreEqual(true, bao.OurObj);

            // now try a death scene
            seg = new LairSegment(Segments.SegDrinkMeUp);
            seg.Parse();
            bao = seg.CanNextSeekBeIgnored;
            Assert.AreEqual(false, bao.OurObj); // seek can't be ignored because this is a death segment
		}

        [Test]
        public void TestSeqBranchCounts()
        {
            bool bEx = false;
            LairSegment seg = new LairSegment(Segments.SegFillingWallMove);
            seg.Parse();
            ByteArrayAndObj bao;
            bao = seg.SequenceBranchCount;

            Assert.AreEqual(1, bao.OurObj);	// 1 branch
            Assert.AreEqual(true, bao.IsApplicable);	// always applicable
            Assert.AreEqual(0, bao.OurByteArray.Addr);	// the byte in question is at offset 0
            Assert.AreEqual(0x60, bao.ByteMask);

            // now try attract mode
            seg = new LairSegment(Segments.SegAttractMode);
            seg.Parse();
            try
            {
                bao = seg.SequenceBranchCount;
            }
            catch
            {
                bEx = true;
            }
            Assert.AreEqual(true, bEx);  // no move tick groups for trailer segment

            // now try second move of flaming ropes
            seg = new LairSegment(Segments.SegFlamingRopes2);
            seg.Parse();
            bao = seg.SequenceBranchCount;
            Assert.AreEqual(2, bao.OurObj);	// 2 branches for this segment
            Assert.AreEqual(true, bao.IsApplicable);	// always applicable

            // try room of fire weird segment
            seg = new LairSegment(Segments.SegRoomOfFire3);
            seg.Parse();
            bao = seg.SequenceBranchCount;
            Assert.AreEqual(3, bao.OurObj);	// 3 branches
            Assert.AreEqual(true, bao.IsApplicable);	// always applicable
        }

        [Test]
        public void TestBranches()
        {
            bool bEx = false;
            ByteArrayAndObj bao;
            LairSegment seg = new LairSegment(Segments.SegFillingWallMove);
            seg.Parse();

            // since this is a move, there can be more than one next sequence, so this should force us to go through another function
            try
            {
                bao = seg.NextSequence;
            }
            catch
            {
                bEx = true;
            }
            Assert.AreEqual(true, bEx);

            bao = seg.GetNextSequence(0);
            Assert.AreEqual(3, bao.OurObj); // go to sequence 3 if this move is made
            Assert.AreEqual(2, bao.OurByteArray.Addr);  // offset 2 contains this sequence
            Assert.AreEqual(true, bao.IsApplicable);

            // test first move of flaming ropes
            seg = new LairSegment(Segments.SegFlamingRopes2);
            seg.Parse();
            bao = seg.GetNextSequence(0);
            Assert.AreEqual(8, bao.OurObj); // first window, we go to sequence 8

            bao = seg.GetNextSequence(1);
            Assert.AreEqual(4, bao.OurObj); // second window, go to seq 4 (correct move)

            bao = seg.GetNextSequence(2);
            Assert.AreEqual(8, bao.OurObj); // third window, back to 8

        }
	}

	[TestFixture]
	public class LairTestSceneSelection
	{
		private SceneSelection MakeSceneSelection(uint uAddr, byte [] arr)
		{
			byte [] dummy = { 0 };
			ByteArray ba = new ByteArray(uAddr, arr);
			return new SceneSelection(ba, dummy);
		}

		[Test]
		public void TestSceneSelection()
		{
			byte [] arr1 = { 0, 0, 8, 1 };
			byte [] arr2 = { 0, 0, 2, 3 };
			byte [] arr3 = { 0, 0, 0xA, 7 };
			SceneSelection s;
			byte [] dummy = { 0 };

			// this just tests the last byte which holds the scene selection

			s = MakeSceneSelection(0, arr1);
			Assert.AreEqual(1, s.SceneCount.OurUint);
			Assert.AreEqual(3, s.SceneCount.OurByteArray.Addr);	// test address
			Assert.AreEqual(arr1[3], s.SceneCount.OurByteArray.Array[0]);	// make sure we're getting the correct byte back
			Assert.AreEqual(SceneSelection.ChooseType.AlwaysChooseFirstScene,
				s.Type.OurObj);

			s = MakeSceneSelection(1, arr2);
			Assert.AreEqual(2, s.SceneCount.OurUint);
			Assert.AreEqual(4, s.SceneCount.OurByteArray.Addr);
			Assert.AreEqual(arr2[3], s.SceneCount.OurByteArray.Array[0]);
			Assert.AreEqual(SceneSelection.ChooseType.RandomScene,
				s.Type.OurObj);

			s = MakeSceneSelection(2, arr3);
			Assert.AreEqual(3, s.SceneCount.OurUint);
			Assert.AreEqual(5, s.SceneCount.OurByteArray.Addr);
			Assert.AreEqual(arr3[3], s.SceneCount.OurByteArray.Array[0]);
			Assert.AreEqual(SceneSelection.ChooseType.Choose_3rd_Then_2nd_Then_1st,
				s.Type.OurObj);
		}
	}

	[TestFixture]
	public class LairTestTiming
	{
        [Test]
        public void TestTickCalc()
        {
            double d;
            double delta = 0.500;
            uint u;

            // closing wall sequence 0
            d = LairMath.TicksToFramesF(34, true);
            Assert.AreEqual(32, d, delta);
            d = LairMath.TicksToSeconds(34, true);
            Assert.AreEqual(1.34, d, 0.100);

            // closing wall death
            d = LairMath.TicksToFramesF(15, true);
            Assert.AreEqual(17, d, delta);
            d = LairMath.TicksToSeconds(15, true);
            Assert.AreEqual(.71, d, 0.100);

            // attract mode
            u = LairMath.TicksToFramesU(1269, false);
            Assert.AreEqual(1030, u);
            u = LairMath.TicksToFramesU(1269, true);
            Assert.AreEqual(1035, u);

            d = LairMath.TicksToFramesF(1269, true);
            Assert.AreEqual(1035, d, delta);
            d = LairMath.TicksToSeconds(1269, true);
            Assert.AreEqual(43.17, d, 0.100);
        }

		[Test]
		public void TestLDV1000Latency()
		{
			Assert.AreEqual(0.2, LairMath.LDV1000SearchDelaySeconds, 0.001);
		}

        [Test]
        public void TestTicksBeforeMoveAccepted()
        {
            LairSegment seg = new LairSegment(Segments.SegFillingWallMove);
            seg.Parse();
            ByteArrayAndObj bao;
            bao = seg.TicksBeforeInputAccepted;
            Assert.AreEqual(0, bao.OurObj);	// on this segment, we have 0 ticks before a move will be accepted

            seg = new LairSegment(Segments.SegAttractMode);
            seg.Parse();
            bool bEx = false;
            try
            {
                bao = seg.TicksBeforeInputAccepted;
            }
            catch
            {
                bEx = true;
            }
            Assert.AreEqual(true, bEx);
        }

        [Test]
        public void TestTimeWindowTicks()
        {
            LairSegment seg = new LairSegment(Segments.SegFillingWallMove);
            seg.Parse();
            ByteArrayAndObj bao;
            bao = seg.GetTicksForWindow(0);
            Assert.AreEqual(0x2A, bao.OurObj);

            seg = new LairSegment(Segments.SegFlamingRopes2);
            seg.Parse();
            bao = seg.GetTicksForWindow(0);
            Assert.AreEqual(0x20, bao.OurObj);
            bao = seg.GetTicksForWindow(1);
            Assert.AreEqual(0x17, bao.OurObj);

            // try an out of range one
            bool bEx = false;
            try
            {
                bao = seg.GetTicksForWindow(2); // out of range
            }
            catch
            {
                bEx = true;
            }
            Assert.AreEqual(true, bEx);
        }

        [Test]
        public void TestTimeWindowCounts()
        {
            bool bEx = false;
            LairSegment seg = new LairSegment(Segments.SegFillingWallMove);
            seg.Parse();
            ByteArrayAndObj bao;
            bao = seg.TimeWindowCount;

            Assert.AreEqual(1, bao.OurObj);	// 1 window
            Assert.AreEqual(true, bao.IsApplicable);	// always applicable
            Assert.AreEqual(1, bao.OurByteArray.Addr);	// the byte in question is at offset 1
            Assert.AreEqual(0x3, bao.ByteMask);   // bits 0 and 1 are relevant

            // now try attract mode
            seg = new LairSegment(Segments.SegAttractMode);
            seg.Parse();
            try
            {
                bao = seg.TimeWindowCount;
            }
            catch
            {
                bEx = true;
            }
            Assert.AreEqual(true, bEx);  // no move tick groups for trailer segment

            // now try second move of flaming ropes
            seg = new LairSegment(Segments.SegFlamingRopes2);
            seg.Parse();
            bao = seg.TimeWindowCount;
            Assert.AreEqual(2, bao.OurObj);	// 2 windows for this segment
            Assert.AreEqual(true, bao.IsApplicable);	// always applicable
            Assert.AreEqual(1, bao.OurByteArray.Addr);	// the byte in question is at offset 1

            // try room of fire weird segment
            seg = new LairSegment(Segments.SegRoomOfFire3);
            seg.Parse();
            bao = seg.TimeWindowCount;
            Assert.AreEqual(3, bao.OurObj);	// 3 windows for this segment
            Assert.AreEqual(true, bao.IsApplicable);	// always applicable
            Assert.AreEqual(1, bao.OurByteArray.Addr);	// the byte in question is at offset 1

        }

	}

    [TestFixture]
    public class LairClass
    {
        [Test]
        public void TestScore()
        {
            ROMManager m = new ROMManager();
            m.LoadROM("lair.zip");
            byte[] buf = m.RomBuffer;
            CLair l = new CLair(buf);

            uint uScore = l.ScoreIdxToScore(4);
            Assert.AreEqual(0x17B, uScore);
        }
    }

	[TestFixture]
	public class LairParseExerciser
	{
		[Test]
		public void ParseAllScenes()
		{
			//bool bEx = false;
            ROMManager m = new ROMManager();
            m.LoadROM("lair.zip");
            byte[] buf = m.RomBuffer;
			CLair l = new CLair(buf);

			for (byte u8 = 0x80; u8 <= 0xA7; u8++)
			{
				List<LairSequence> lst = l.GetSceneSequences(u8);
			}

			// if no exception is thrown, it means this test passed
		}
	}

    [TestFixture]
    public class LairROMs
    {
        [Test]
        public void TestF2()
        {
            ROMManager m = new ROMManager();
            bool bRes = m.LoadROM("lair.zip");
            Assert.AreEqual(true, bRes);

            byte[] buf = m.RomBuffer;
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte [] md5Hash = md5.ComputeHash(buf);
            byte [] md5CorrectHash =
                {
                    0xc1, 0x74, 0x26, 0xbb, 0x12, 0xab, 0x8d, 0x3b, 0xe7, 0x58, 0xd5, 0x27,
            0x6d, 0xbb, 0xaa, 0x9b
                };
            Assert.AreEqual(md5CorrectHash, md5Hash);
        }
    }

    [TestFixture]
    public class LairDecoder
    {
        private ROMManager m_manager = new ROMManager();
        private CLair m_lair = null;
        private CLairDecode m_dec = null;

        public LairDecoder()
        {
            m_manager.LoadROM("lair.zip");
            byte[] buf = m_manager.RomBuffer;
            m_lair = new CLair(buf);
            m_dec = new CLairDecode(m_lair);
        }

        [Test]
        public void SequenceClosingWall()
        {
            // closing wall scene
            List<LairSequence> lst = m_lair.GetSceneSequences(0x8b);
            LairSequenceData dat = m_dec.DecodeSequence(lst[0]); // decode sequence 0

            Assert.AreNotEqual(null, dat);

            Assert.AreEqual(1, dat.ScoreIdx);
            Assert.AreEqual(4, dat.NextSequence);
            Assert.AreEqual(34, dat.Ticks);
            Assert.AreEqual(false, dat.CanEasterEggBeEnabled);
            Assert.AreEqual(false, dat.IsStillFrame);
            Assert.AreEqual(false, dat.PayAsYouGoAfterSequence);
            Assert.AreEqual(false, dat.IgnoreNextSeek);
            Assert.AreEqual(9181, dat.FrameNum);
            Assert.AreEqual(SequenceType.Normal, dat.Type);
            Assert.AreEqual(0, dat.NextSelectionGroup); // not used
            Assert.AreEqual(false, dat.UseNextSelectionGroup);
            Assert.AreEqual(false, dat.RepeatSceneOnDeath);
            Assert.AreEqual("", dat.Name);  // since we didn't parse at a higher level, this should be blank

            // now do sequence 3 (success)
            dat = m_dec.DecodeSequence(lst[3]);   // sequence 3

            Assert.AreEqual(4, dat.ScoreIdx);
            Assert.AreEqual(0, dat.NextSequence);
            Assert.AreEqual(72, dat.Ticks);
            Assert.AreEqual(false, dat.CanEasterEggBeEnabled);
            Assert.AreEqual(false, dat.IsStillFrame);
            Assert.AreEqual(false, dat.PayAsYouGoAfterSequence);
            Assert.AreEqual(false, dat.IgnoreNextSeek);
            Assert.AreEqual(9215, dat.FrameNum);
            Assert.AreEqual(SequenceType.EndSuccess, dat.Type);
            Assert.AreEqual(2, dat.NextSelectionGroup); // not used
            Assert.AreEqual(false, dat.UseNextSelectionGroup);
            Assert.AreEqual(false, dat.RepeatSceneOnDeath);

            // now do sequence 4 (death)
            dat = m_dec.DecodeSequence(lst[4]);   // sequence 4

            Assert.AreEqual(0, dat.ScoreIdx);
            Assert.AreEqual(6, dat.NextSequence);
            Assert.AreEqual(15, dat.Ticks);
            Assert.AreEqual(false, dat.CanEasterEggBeEnabled);
            Assert.AreEqual(false, dat.IsStillFrame);
            Assert.AreEqual(false, dat.PayAsYouGoAfterSequence);
            Assert.AreEqual(false, dat.IgnoreNextSeek);
            Assert.AreEqual(9301, dat.FrameNum);
            Assert.AreEqual(SequenceType.EndDeath, dat.Type);
            Assert.AreEqual(0, dat.NextSelectionGroup); // not used
            Assert.AreEqual(false, dat.UseNextSelectionGroup);
            Assert.AreEqual(false, dat.RepeatSceneOnDeath);
            
        }

        [Test]
        public void AllSequences()
        {
            LairScenesData dat = m_dec.DecodeAllScenes();

            // verify scene names and that the scene list is in the right order
            for (int i = 0; i < dat.Scenes.Count; i++)
            {
                string strName = dat.Scenes[i].SceneName;
                Assert.AreEqual(CLair.SceneIdxToName((byte) (i | 0x80)), strName);
            }

            LairSequenceIndexer indexer = new LairSequenceIndexer(dat.Sequences);

            // try to find the filling wall board, sequence 0
            string strSeqName = dat.Scenes[0xb].SequenceNames[0];
            LairSequenceData datSeq = indexer.GetSequenceData(strSeqName);
            Assert.AreEqual(9181, datSeq.FrameNum); // make sure frame number matches up

            // test LairScenesIndexer
            LairScenesIndexer indexerScenes = new LairScenesIndexer(dat);
            datSeq = indexerScenes.GetSequenceData(0xb, 0);
            Assert.AreEqual(9181, datSeq.FrameNum); // make sure frame number matches up
        }

    }

}
