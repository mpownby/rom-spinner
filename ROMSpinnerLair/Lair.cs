using System;
using System.Collections;
using System.Collections.Generic;
using ROMSpinner.Common;

namespace ROMSpinner.Lair
{
	/// <summary>
	/// Summary description for Lair.
	/// </summary>
	public class CLair
    {
        byte [] m_arrRom;

		public CLair(byte [] arrRom)
		{
			m_arrRom = arrRom;
		}

		private uint Load16(uint uOffset)
		{
			return Util.Load16(m_arrRom, uOffset);
		}

		/// <summary>
		/// Copies a small chunk out of our ROM data and returns it as a byte array
		/// </summary>
		/// <param name="iOffset"></param>
		/// <param name="iLength"></param>
		/// <returns></returns>
		private ByteArray SubArray(uint uOffset, uint uLength)
		{
			byte [] arrRes = new byte[uLength];

			// there's probably a more efficient way to do this
			for (uint i = 0; i < uLength; i++)
			{
				arrRes[i] = m_arrRom[uOffset + i];
			}

			return new ByteArray(uOffset, arrRes);
		}

		public List<SceneSelection> GetSceneSelections()
		{
			List<SceneSelection> table = new List<SceneSelection>();

			// go through each selection index
            for (uint i = 0x80; i <= 0x8D; i++)
            {
                uint uOffset = 0;
                
                uOffset = 0x2035;	// see 1CEB
				uOffset += ((i & 0x7F) << 2);	// offset in program ROM (see 1CEB)
                
                ByteArray arr = SubArray(uOffset, 4);	// each scene selection chunk is 4 bytes long (see 1D01)
                table.Add(new SceneSelection(arr, m_arrRom));
            }
			
			return table;
		}

		private void NextSeqHelper(ref uint iNextSeq, ref uint iMaxSeqIdx)
		{
			// if it isn't specifying the next scene selection index, then check to see if the next seq is our new max
			if ((iNextSeq & 0x80) == 0)
			{
				if (iNextSeq > iMaxSeqIdx)
				{
					iMaxSeqIdx = iNextSeq;
				}
			}
		}

        public List<LairSequence> GetSceneSequences(byte u8SceneId)
		{
			List<LairSequence> lstSequences = new List<LairSequence>();
            
			uint uSceneIdxOffset = (uint) u8SceneId & 0x7F;	// see 15C1
			uSceneIdxOffset <<= 1;	// see 15CC
			uSceneIdxOffset += 0x2108;	// see 15D4

            uint uSeq0AddressOffset = 0;

            uSeq0AddressOffset = Load16(uSceneIdxOffset);	// points to the address of sequence 0

			uint uSeqIdx = 0;
			uint uMaxSeqIdx = 0;	// total # of sequences
			
			// if this is the attract mode
			if (u8SceneId == 0xA5)
			{
				uMaxSeqIdx = 9;	// the max number of attract mode sequences is at least 9 (see 12A0 and 1FFF-2004)
			}

			// parse all sequences
			for (uSeqIdx = 0; uSeqIdx <= uMaxSeqIdx; uSeqIdx++)
			{
				uint uSeqOffset = Load16(uSeq0AddressOffset + (uSeqIdx << 1));	// where current sequence begins (see 15D9)
				uint uPos = uSeqOffset;

				bool bTrailerSeg = false;

				// this will hold byte arrays of all the segments of one sequence
				List<LairSegment> lstSegments = new List<LairSegment>();

				while (!bTrailerSeg)
				{
					byte u8Header = m_arrRom[uPos];
					ByteArray arrSeg;

					uint uLength = LairSegment.GetSegmentLength(m_arrRom[uPos], m_arrRom[uPos+1]);
					arrSeg = SubArray(uPos, uLength);	// get segment

					// make a new segment
					LairSegment seg = new LairSegment(arrSeg);
					seg.Parse();

					if (seg.HasNextSequence)
					{
						// this can throw an exception if there is no next sequence
						uint uNextSeq = Convert.ToUInt32(seg.NextSequence.OurObj);

						NextSeqHelper(ref uNextSeq, ref uMaxSeqIdx);
					}

					lstSegments.Add(seg);

					bTrailerSeg = seg.IsTrailer;
					uPos += uLength;
				}

				LairSequence seq = new LairSequence(lstSegments);
				lstSequences.Add(seq);
			} // end looping through all sequences

			return lstSequences;
		}

		/// <summary>
		/// Converts scene index byte into a human-readable name
		/// </summary>
		/// <param name="u8Idx"></param>
		/// <returns></returns>
		public static string SceneIdxToName(byte u8Idx)
		{
			string [] strBoards =
				{
					"Introduction",
					"Tentacles from the Ceiling",
					"Snake Room",
					"Flaming Ropes",
					"Pool of Water",
					"Bubbling Cauldron",
					"Giddy Goons",
					"Flattening Staircase",
					"Smithee",
					"Grim Reaper",
					"Wind Tunnel",
					"Closing Wall",
					"Room of Fire - Bench Covers Exit",
					"Flying Horse Barding",
					"Robot Knight on Checkered Floor",
					"Crypt Creeps",
					"Catwalk Bats",
					"Flaming Ropes (Mirrored)",
					"Pool of Water (Mirrored)",
					"Giant Bat",
					"Falling Platform - 9 Levels",
					"Smithee (Mirrored)",
					"Flying Horse Barding (Mirrored)",
					"Lizard King",
					"Drink Me",
					"Crypt Creeps (Mirrored)",
					"Grim Reaper (Mirrored)",
					"Tilting Floor",
					"Throne Room",
					"Robot Knight on Checkered Floor (Mirrored)",
					"Falling Platform - 9 Levels (Mirrored)",
					"Underground River",
					"Mudmen",
					"Black Knight on Horse",
					"Rolling Balls",
					"Electric Cage - Geyser",
					"The Dragon's Lair",
					"Attract Mode",
					"Falling Platform - 3 Levels",
					"Small Yellow Room"
				};

			return strBoards[u8Idx & 0x7F];
		}

		public uint ScoreIdxToScore(uint uIdx)
		{
			uint uOffset = 0x209A;	// see 1673
			uIdx <<= 1;	// see 1676
			uOffset += uIdx;	// see 167D
			return Load16(uOffset);	// see 167E
		}
    } // end class CLair

    #region SceneSelection
    public class SceneSelection
	{
		private ByteArray m_arr;
		private byte [] m_arrROM;

		public enum ChooseType
		{
			RandomScene,	// choose a random scene
			AlwaysChooseFirstScene,		// choose the first scene
			Choose_3rd_Then_2nd_Then_1st	// Always chooses 3rd branch, then 2nd branch, then 1st branch (see 1D8D-1DC0, only used for Dragon's Lair/FallingPlatform scene selection)
		}

		public SceneSelection(ByteArray arr, byte [] arrROM)
		{
			m_arr = arr;
			m_arrROM = arrROM;
		}

		public ByteArray Buf
		{
			get
			{
				return m_arr;
			}
		}

		public ByteArrayAndUint SceneCount
		{
			get
			{
				byte [] arr = m_arr.Array;
				byte u8BranchBits = arr[3];
				byte [] arrRes = { arr[3] };
				ByteArray ba = new ByteArray(m_arr.Addr + 3, arrRes);	// isolate the byte that has the relevant info for us
				uint uCount = 0;

				// branch bits of 1 means 1 branch, 7 means 3 branches
				while (u8BranchBits != 0)
				{
					u8BranchBits >>= 1;
					uCount++;
				}

				return new ByteArrayAndUint(ba, uCount);
			}
		}

		public ByteArrayAndObj Type
		{
			get
			{
				byte [] arr = m_arr.Array;
				byte u8 = arr[2];
				byte [] arrRes = { arr[2] };
				ByteArray ba = new ByteArray(m_arr.Addr + 2, arrRes);	// isolate the byte that has the relevant info for us
				ChooseType type;
				switch (u8)
				{
					case 8:	// used in attract mode, does not randomization (see 1D0D)
						type = ChooseType.AlwaysChooseFirstScene;
						break;
					case 2:	// most common, choose from 3 random boards
						type = ChooseType.RandomScene;
						break;
					case 0xA:	// used in last scene with falling platforms and dragon's lair
						type = ChooseType.Choose_3rd_Then_2nd_Then_1st;
						break;
					default:
						throw new Exception("unknown type");
				}

				return new ByteArrayAndObj(ba, type, true);
			}
		}

		public ArrayList GetBranches()
		{
			byte [] arr = m_arr.Array;
			uint uBranchOffset = Util.Load16(arr, 0);
			ArrayList lstRes = new ArrayList();

			byte u8BranchBits = arr[3];

			// branch bits of 1 means 1 branch, 7 means 3 branches
			while (u8BranchBits != 0)
			{
				byte [] arrSingle = { m_arrROM[uBranchOffset] };
				ByteArray ba = new ByteArray(uBranchOffset, arrSingle);
				lstRes.Add(ba);
				u8BranchBits >>= 1;
				uBranchOffset++;
			}

			return lstRes;
        }

    } // end class

    #endregion

}
