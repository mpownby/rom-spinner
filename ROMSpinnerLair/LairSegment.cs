using System;
using System.Collections;
using ROMSpinner.Common;
using ROMSpinner.Common.Lair;

/*
 * SEGMENTS EXPLAINED:
 * Each segment can be at most 15 bytes long because one segment is loaded and expanded into A0x0-A0xF. (see 15b4 function)
 * 
 * Trailer Segment parsing is handled at 162F; trailer segments are always 7 bytes (see 1634-1655)
 * It gets loaded into memory differently than it is arranged in the ROM (see 162F)
 * 
 * Segment Offset:		Memory Offset (ie A0E0 + offset):
 * ---------------		---------------------------------
 * 0					0	bit 4 is reset on accepted move at 1819
 * 1					1
 * 2					E
 * 3					F
 * 4					5	bit 6 checked at 16cd (bit 6 always clear!), bit 3 never used
 * 5					3
 * 6					4
 * N/A					2 <-- gets scene index (see 165B)
 *						C <-- how many ticks before we'll accept input (see 178C)
 *
 * Non-Trailer Segment:
 * First three bytes are read into A0x0, no matter what (see 15F1-1600)
 * For A8: byte 3 is read into position 3
 * For 99 (correct move): byte 3 read into position 7
 * For 81 (incorrect move): nothing is read into position 7
 *	TmpIdx = (byte1 & 3) see 161F
 *  for (int i = 0; i < TmpIdx; i++)
 *  {
 *		A0xC+i = NextByte + i;	// see 1624, this will be byte 4 for correct move, byte 3 for incorrect move
 *  }
 */

namespace ROMSpinner.Lair
{
	/// <summary>
	/// Summary description for LairSegment.
	/// </summary>
	public class LairSegment
	{
		private ByteArray m_arr;
		private ByteArray m_baA0E5;	// this byte is used repeatedly

		// This is how the segment is parsed in the ROM code, it is always 16 bytes long, unused spaces are -1
		private int [] m_arrParsedSegment = null;

		// This contains the original offsets that the bytes in the parsed segment came from
		private int [] m_piParsedSegmentOrgOffsets = null;

		private bool m_bIsTrailer = false;
		private ByteArray m_baA0E1;	// byte 1 after parsing
		private ByteArrayAndObj m_baoFrameNum;	// frame number associated with trailer segment
		private Move m_Move = 0;	// move for non-trailer segment
		private SequenceType m_Type = 0;	// trailer segment type

		public ByteArray SegmentArray
		{
			get
			{
				return m_arr;
			}
		}

		public int [] ParsedSegmentArray
		{
			get
			{
				return m_arrParsedSegment;
			}
		}

		private int Load16(int iOffset)
		{
			byte [] buf = m_arr.Array;
			return buf[iOffset] | (buf[iOffset+1] << 8);
		}

		/// <summary>
		/// Helper function
		/// </summary>
		/// <param name="uWhichByte"></param>
		/// <returns></returns>
		private ByteArray GetParsedByte(uint uWhichByte)
		{
			if ((m_piParsedSegmentOrgOffsets[uWhichByte] == -1) ||
				(m_arrParsedSegment[uWhichByte] == -1))
			{
				throw new Exception("Byte " + uWhichByte + " wasn't parsed.");
			}

			ByteArray ba = new ByteArray((uint) m_piParsedSegmentOrgOffsets[uWhichByte],
				Convert.ToByte(m_arrParsedSegment[uWhichByte]));
			return ba;
		}

		/// <summary>
		/// For testing
		/// </summary>
		/// <param name="arr"></param>
		public LairSegment(byte [] arr)
		{
			m_arr = new ByteArray(0, arr);
		}

		public LairSegment(ByteArray arr)
		{
			m_arr = arr;
		}

		private void ParseSegmentHelper(uint uDstIdx, uint uSrcIdx)
		{
			m_arrParsedSegment[uDstIdx] = m_arr.Array[uSrcIdx];	// copy byte over
			m_piParsedSegmentOrgOffsets[uDstIdx] = Convert.ToInt32(m_arr.Addr + uSrcIdx);	// update address for this byte
		}

		public void ParseSegment()
		{
			byte [] bufSrc = m_arr.Array;
			m_arrParsedSegment = new int[16];
			m_piParsedSegmentOrgOffsets = new int[16];

			// initialize parsed segment with default values
			for (int i = 0; i < 16; i++)
			{
				m_arrParsedSegment[i] = -1;
				m_piParsedSegmentOrgOffsets[i] = -1;
			}

			uint uLength = GetSegmentLength(bufSrc[0], bufSrc[1]);
			uint idxSrc = 0, idxDst = 0;

			// safety check
			if (uLength != bufSrc.Length)
			{
				throw new Exception("Segment Length is Incorrect");
			}

			// if this is the trailer segment
			if ((bufSrc[0] & 0x80) == 0)
			{
				// see 162F
				ParseSegmentHelper(0, 0);
				ParseSegmentHelper(1, 1);
				ParseSegmentHelper(0xE, 2);
				ParseSegmentHelper(0xF, 3);
				ParseSegmentHelper(5, 4);
				ParseSegmentHelper(3, 5);
				ParseSegmentHelper(4, 6);
			}
				// not a trailer ...
			else
			{
				// see 15F1
				ParseSegmentHelper(idxDst++, idxSrc++);
				ParseSegmentHelper(idxDst++, idxSrc++);

				int iTmp = bufSrc[0] & 0x60;	// see 15FD
				
				// this loop from 1600-1607, it's a do/while
				do
				{
					// see 1600
					ParseSegmentHelper(idxDst++, idxSrc++);
					iTmp -= 0x20;	// see 1605
				} while (iTmp >= 0);

				// Having bit 4 set means that laserdisc searches can be suppressed (see 17cb),
                //   so there's an extra byte in the middle relating to this. (see 1609)
				if ((bufSrc[0] & 0x10) == 0x10)
				{
					ParseSegmentHelper(7, idxSrc++);	// see 1611
				}

				idxDst = 0xC;	// see 1622

				int iRemainingBytes = bufSrc[1] & 3;	// see 161F

				do
				{
					ParseSegmentHelper(idxDst++, idxSrc++);	// see 1624
					iRemainingBytes--;	// see 1629
				} while (iRemainingBytes >= 0);
			}
		}

		/// <summary>
		/// This function will throw an exception on failure
		/// </summary>
		public void Parse()
		{
			byte [] buf = m_arr.Array;

			ParseSegment();

            m_baA0E1 = GetParsedByte(1);

			// checks to see if _any_ segment has this characteristic described in 16CD
			if (m_arrParsedSegment[5] != -1)
			{
				// Verify that these bits are never used!
				if ((m_arrParsedSegment[5] & 0x40) == 0x40)
				{
					throw new Exception("A0E5 found with bit 6 set! 16CD!");
				}
				else if ((m_arrParsedSegment[5] & 0x8) == 0x8)
				{
					throw new Exception("A0E5 found with bit 3 set!");
				}
			}

			// if this is a trailer segment
			if ((buf[0] & 0x80) == 0)
			{
				m_bIsTrailer = true;
			}

			// compute next sequence
			if (m_bIsTrailer)
			{
				byte [] buf4 = { buf[4] };
				m_baA0E5 = new ByteArray(m_arr.Addr + 4, buf4);

				// do frame num
				int iFrameNum = Load16(5);
				byte [] arrFrameNum = { buf[5], buf[6] };
				m_baoFrameNum = new ByteArrayAndObj(new ByteArray(m_arr.Addr + 5, arrFrameNum), iFrameNum, true);

				// get segment type
				switch (buf[0] & 0xF0)
				{
					case 0x10:	// see 1493
						m_Type = SequenceType.EndDeath;
						break;
					case 0x20:	// see 153B
						m_Type = SequenceType.GameOver;
						break;
					case 0x40:	// see 174C (no input) and 16CB (checks for coin insertion)
						m_Type = SequenceType.AttractMode;
						break;
					default:
						// if the next sequence has the high bit set, then this segment is a successful end
						if ((m_baA0E1.Array[0] & 0x80) != 0)
						{
							m_Type = SequenceType.EndSuccess;
						}
						else
						{
							m_Type = SequenceType.Normal;
						}
						break;
				}
			}
			else
			{
				// get input type
				switch (buf[0] & 0xF)	// see 17A3
				{
					default:
						DoEx("Unknown Move");
						break;
					case 0:
						m_Move = Move.Sword;
						break;
					case 1:
						m_Move = Move.Up;
						break;
					case 2:
						m_Move = Move.Down;
						break;
					case 4:
						m_Move = Move.Left;
						break;
					case 5:
						m_Move = Move.UpLeft;
						break;
					case 6:
						m_Move = Move.DownLeft;
						break;
					case 8:
						m_Move = Move.Right;
						break;
					case 9:
						m_Move = Move.UpRight;
						break;
					case 0xA:
						m_Move = Move.DownRight;
						break;
				}
			}
		}
		
		/// <summary>
		/// Throws an exception.
		/// </summary>
		/// <param name="strMsg"></param>
		private void DoEx(string strMsg)
		{
			throw new Exception(strMsg);
		}

		private void RequireTrailer()
		{
			if (!m_bIsTrailer)
			{
				DoEx("Operation not allowed for Non-Trailer Segment");
			}
		}

		private void RequireNonTrailer()
		{
            if (m_bIsTrailer)
            {
                DoEx("Operation not allowed for Trailer Segment");
            }
        }

        #region Trailer
        public bool IsTrailer
		{
			get
			{
				return m_bIsTrailer;
			}
        }

        public ByteArrayAndObj GetFrameNum()
        {
            // make sure it's the trailer segment
            RequireTrailer();
            return m_baoFrameNum;
        }

        public ByteArrayAndObj FrameNum
        {
            get
            {
                return GetFrameNum();
            }
        }

        public ByteArrayAndObj ScoreIdx
        {
            get
            {
                RequireTrailer();

                ByteArray ba = GetParsedByte(0);
                uint uScoreIdx = (uint) (ba.Array[0] & 0x0F);	// high nibble is used for type, only low nibble applies to score
                ByteArrayAndObj bao = new ByteArrayAndObj(ba, uScoreIdx, true);
                bao.ByteMask = 0x0F;
                return bao;
            }
        }

        public ByteArrayAndObj GetSequenceType()
        {
            RequireTrailer();
            return new ByteArrayAndObj(new ByteArray(m_arr.Addr, m_arr.Array[0]), m_Type, true);
        }

        // bit 0
        public ByteArrayAndObj CanEasterEggBeEnabled
        {
            get
            {
                RequireTrailer();
                bool bRes = ((m_baA0E5.Array[0] & 0x1) != 0);	// see 16BE
                ByteArrayAndObj bao = new ByteArrayAndObj(m_baA0E5, bRes, true);
                bao.WhichBit = 0;
                return bao;
            }
        }

        // bit 1
        public ByteArrayAndObj UseSegmentNextSelIdx
        {
            get
            {
                RequireTrailer();
                bool bRes = ((m_baA0E5.Array[0] & 0x2) != 0);	// see 1EDE

                // only applies to non-death scenes and non-sequences
                bool bApplicable = (((m_arr.Array[0] & 0x10) == 0) &&	// see 1493
                    ((m_arr.Array[1] & 0x80) == 0x80));	// see 149C

                ByteArrayAndObj bao = new ByteArrayAndObj(m_baA0E5, bRes, bApplicable);
                bao.WhichBit = 1;
                return bao;
            }
        }

        // bit 2
        public ByteArrayAndObj RepeatSceneOnDeath
        {
            get
            {
                RequireTrailer();
                bool bRes = ((m_baA0E5.Array[0] & 0x4) != 0);	// see 14D7
                bool bApplicable = (m_Type == SequenceType.EndDeath);	// 14D7 is only run on death
                ByteArrayAndObj bao = new ByteArrayAndObj(m_baA0E5, bRes, bApplicable);
                bao.WhichBit = 2;
                return bao;
            }
        }

        // bit 4
        public ByteArrayAndObj IsStillFrame
        {
            get
            {
                RequireTrailer();
                bool bIsStillFrame = ((m_baA0E5.Array[0] & 0x10) != 0);	// see 168D
                ByteArrayAndObj bao = new ByteArrayAndObj(m_baA0E5, bIsStillFrame, true);
                bao.WhichBit = 4;
                return bao;
            }
        }

        // bit 5
        public ByteArrayAndObj PayAsYouGoAfterSequence
        {
            get
            {
                ByteArray ba = this.GetParsedByte(5);
                bool bEnabled = ((ba.Array[0] & 0x20) == 0x20);	// see 1485
                ByteArrayAndObj bao = new ByteArrayAndObj(ba, bEnabled, true);
                bao.WhichBit = 5;
                return bao;
            }
        }

        #endregion

        #region SequenceBranches

        /// <summary>
        /// Convenience function to avoid exceptions (since they are slow to debug)
        /// </summary>
        public bool HasNextSequence
        {
            get
            {
                bool bRes = false;
                if (m_bIsTrailer)
                {
                    byte u8 = m_baA0E1.Array[0];

                    // sequences don't have a high bit set
                    if ((u8 & 0x80) == 0)
                    {
                        bRes = true;
                    }
                }
                return bRes;
            }
        }

        public ByteArrayAndObj NextSequence
		{
			get
			{
                // if this isn't a trailer sequence, then the caller must go another function to get this info
                RequireTrailer();

				byte u8 = m_baA0E1.Array[0];

				// sequences don't have a high bit set
				if ((u8 & 0x80) != 0)
				{
					throw new Exception("No Sequence for this Segment");
				}
				return new ByteArrayAndObj(m_baA0E1, u8, true);
			}
		}

        /// <summary>
        /// Helper function used by GetNextSequence and others
        /// </summary>
        /// <param name="uTimingWindow"></param>
        /// <returns></returns>
        public ByteArrayAndObj GetBranchOffset(uint uTimingWindow)
        {
            RequireNonTrailer();    // this only used for non-trailers

            ByteArray ba = GetParsedByte(1);
            byte u8BranchOffsetVal = (byte)(ba.Array[0] >> 2);  // see 1782

            while (uTimingWindow > 0)   // see 179F
            {
                u8BranchOffsetVal >>= 2;    // see 179A
                uTimingWindow--;   // see 179E
            }

            uint uOffset = (uint)((3 & u8BranchOffsetVal) + 2); // see 17B1
            ByteArrayAndObj bao = new ByteArrayAndObj(ba, uOffset, true);
            bao.ByteMask = 0xFC;    // all but the lower 2 bits
            return bao;
        }

        public ByteArrayAndObj SequenceBranchCount
        {
            get
            {
                if (m_bIsTrailer)
                {
                    // this is implicit, so we don't return a byte array
                    throw new Exception("Trailer segment always has 1 branch");
                }

                ByteArray ba = GetParsedByte(0);    // see 1600
                byte u8 = ba.Array[0];
                u8 &= 0x60; // see 15FD
                u8 /= 0x20; // see 1605
                u8 += 1;    // see 1600, the loop is a do/while so the result will be +1 what it normally would've been.

                ByteArrayAndObj bao = new ByteArrayAndObj(ba, u8, true);
                bao.ByteMask = 0x60;   // see 15FD
                return bao;
            }
        }

        public ByteArrayAndObj GetNextSequence(uint uTimingWindow)
        {
            ByteArrayAndObj bao = GetBranchOffset(uTimingWindow);
            uint uOffset = (uint) bao.OurObj;
            ByteArray baBranch = GetParsedByte(uOffset);
            return new ByteArrayAndObj(baBranch, baBranch.Array[0], true);
        }

        #endregion

        public bool HasNextSelGroup
        {
            get
            {
                bool bRes = false;

                byte u8 = m_baA0E1.Array[0];

                // sel idx should have high bit set
                if ((u8 & 0x80) != 0)
                {
                    bRes = true;
                }

                return bRes;
            }
        }

        public ByteArrayAndObj NextSelGroup
		{
			get
			{
				byte u8 = m_baA0E1.Array[0];

				// sel idx should have high bit set
				if ((u8 & 0x80) == 0)
				{
					throw new Exception("No Sel Idx for this Segment");
				}

				// this is only applicable if we're supposed to use this for the next sel idx
				ByteArrayAndObj bao = UseSegmentNextSelIdx;
				bool bApplicable = (bool) bao.OurObj && bao.IsApplicable;

				return new ByteArrayAndObj(m_baA0E1, u8, bApplicable);
			}
        }

        #region Ticks

        public ByteArrayAndObj TrailerTicks
        {
            get
            {
                RequireTrailer();
                byte[] arr = { (byte) m_arrParsedSegment[0xE], (byte) m_arrParsedSegment[0xF] };
                ByteArray ba = new ByteArray((uint)m_piParsedSegmentOrgOffsets[0xE],
                    arr);
                uint uTrailerTicks = Convert.ToUInt32(Load16(2));	// always in same spot on trailer
                return new ByteArrayAndObj(ba, uTrailerTicks, true);
            }
        }

        public ByteArrayAndObj TicksBeforeInputAccepted
		{
            // NOTE : this appears to be moves only (non-trailer)
			get
			{
				ByteArray ba;
				try
				{
					ba = GetParsedByte(0xC);	// see 1787
				}
				catch
				{
					// better error message
					throw new Exception("Segment has no info about ticks before accepting input");
				}

				return new ByteArrayAndObj(ba, ba.Array[0], true);
			}
		}

        public ByteArrayAndObj TimeWindowCount
        {
            get
            {
                if (m_bIsTrailer)
                {
                    throw new Exception("Trailer segment has no move tick group");
                }

                ByteArray ba;
                try
                {
                    ba = GetParsedByte(0x1);    // see 177A
                    byte u8 = ba.Array[0];
                    u8 &= 3;    // see 1778
                    ByteArrayAndObj bao = new ByteArrayAndObj(ba, u8, true);
                    bao.ByteMask = 3;   // see 1778
                    return bao;
                }
                catch
                {
                    throw new Exception("Segment has no info about move tick groups");
                }
            }
        }

        public ByteArrayAndObj GetTicksForWindow(uint uTimeWindow)
        {
            RequireNonTrailer();

            ByteArray ba = GetParsedByte(0xD + uTimeWindow);    // see 1787 and 1796
            return new ByteArrayAndObj(ba, ba.Array[0], true);
        }

#endregion

		public Move GetMove()
		{
			if (m_bIsTrailer)
			{
				RequireNonTrailer();
			}
			return m_Move;
        }

        #region SeekSkipped

        /// <summary>
		/// When we transition to the next sequence, is a seek to the trailer segment's frame number required?
        /// For trailer segments only.
		/// </summary>
		public ByteArrayAndObj TrailerIgnoreNextSeek
		{
            get
            {
                bool bSeekSkipped = false;
                ByteArray ba;
                byte u8WhichBit = 0;

                // This is for trailer segments only
                RequireTrailer();

                ba = GetParsedByte(5);  // see 1729
                byte u8 = ba.Array[0];
                u8WhichBit = 7;
                if ((u8 & 0x80) == 0x80)
                {
                    bSeekSkipped = true;
                }

                // always applicable if we get this far without an exception because byte is defined
                ByteArrayAndObj bao = new ByteArrayAndObj(ba, bSeekSkipped, true);
                bao.WhichBit = u8WhichBit;

                return bao;
            }
		}

        public ByteArrayAndObj CanNextSeekBeIgnored
        {
            get
            {
                RequireNonTrailer();

                // If buf[0] & 0x10 is non-zero (ie bit 4 is set), it means that it can suppress laserdisc seeks,
                //		and so we may need to count down the timers for the move so that the disc stays in sync. (see 17CB)
                ByteArray ba = GetParsedByte(0);
                byte u8 = ba.Array[0];
                bool bSeekCanBeSkipped = ((u8 & 0x10) == 0x10);
                ByteArrayAndObj bao = new ByteArrayAndObj(ba, bSeekCanBeSkipped, true);
                bao.WhichBit = 4;
                return bao;
            }
        }

        /// <summary>
        /// When we transition to the next sequence, is a seek to the trailer segment's frame number required?
        /// For non-trailer segments only.
        /// </summary>
        /// <param name="uTimingWindow"></param>
        /// <returns></returns>
        public ByteArrayAndObj IsSeekSkippedOnNextSequence(uint uTimingWindow)
        {
            bool bSeekSkipped = false;
            ByteArray ba;
            byte u8WhichBit = 0;

            RequireNonTrailer();

            try
            {
                ba = GetParsedByte(7);
            }
            catch
            {
                // better message
                throw new Exception("Seek Implicitly Required for Next Sequence, No Data Available");
            }
            byte u8 = ba.Array[0];	// see 17E3, 17ED, 17F7

            ByteArrayAndObj bao = GetBranchOffset(uTimingWindow);
            uint uBranchOffset = (uint)bao.OurObj; // see 17D8

            // see 17DB
            switch (uBranchOffset)
            {
                case 2:
                    // if high bit is set, it means to NOT do a frame seek when going to next sequence
                    // (see 17ED)
                    if ((u8 & 0x80) == 0x80)
                    {
                        bSeekSkipped = true;
                    }
                    u8WhichBit = 7;
                    break;
                case 3:
                    // see 17f7
                    if ((u8 & 0x40) == 0x40)
                    {
                        bSeekSkipped = true;
                    }
                    u8WhichBit = 6;
                    break;
                default:
                    // see 17e3
                    if ((u8 & 0x20) == 0x20)
                    {
                        bSeekSkipped = true;
                    }
                    u8WhichBit = 5;
                    break;
            }

            // always applicable if we get this far without an exception because byte is defined
            bao = new ByteArrayAndObj(ba, bSeekSkipped, true);
            bao.WhichBit = u8WhichBit;

            return bao;
        }

        #endregion

		// calculates the length of the segment based upon the first two bytes in the segment
		static public uint GetSegmentLength(byte u8_0, byte u8_1)
		{
			uint uRes = 0;

			// if this is a trailer segment, it's always 7 bytes long (see 162F)
			if ((u8_0 & 0x80) == 0)
			{
				uRes = 7;
			}
				// else it's a move segment
			else
			{
				uRes = 2;	// all move segments are at least 2 bytes

				int iTmp = u8_0 & 0x60;	// see 15FD
				
				// this loop from 1600-1607, it's a do/while
				do
				{
					uRes++;	// see 1600
					iTmp -= 0x20;	// see 1605
				} while (iTmp >= 0);

				// having bit 4 set means there's an extra byte in the middle (see 1609)
				if ((u8_0 & 0x10) == 0x10)
				{
					uRes++;
				}

				uRes += (uint) (u8_1 & 3);	// see 161D
				uRes++;	// see 1624, loop is a do/while, so it runs one extra time
			}

			return uRes;
		}

	}
}
