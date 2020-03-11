using System;

namespace ROMSpinner.Common
{
	/// <summary>
	/// Summary description for ByteChunk.
	/// </summary>
	public class ByteArray
	{
		private byte [] m_arrByteChunk;
		uint m_uAddr;

		public ByteArray(uint uAddr, byte [] arrByteChunk)
		{
			m_uAddr = uAddr;
			m_arrByteChunk = arrByteChunk;
		}

		public ByteArray(uint uAddr, byte u8Byte)
		{
			m_arrByteChunk = new byte[1];
			m_arrByteChunk[0] = u8Byte;
			m_uAddr = uAddr;
		}

		public uint Addr
		{
			get 
			{
				return m_uAddr;
			}
		}

		public byte [] Array
		{
			get
			{
				return m_arrByteChunk;
			}
		}

        public override string ToString()
        {
            string strRes = Addr.ToString("x") + ": " + Util.Buf2Str(Array);
            return strRes;
        }

	}

	public class ByteArrayAndUint
	{
		private ByteArray m_arr;
		private uint m_u;

		public ByteArrayAndUint(ByteArray arr, uint u)
		{
			m_arr = arr;
			m_u = u;
		}

		public ByteArray OurByteArray
		{
			get
			{
				return m_arr;
			}
		}

		public uint OurUint
		{
			get
			{
				return m_u;
			}
		}
	}

	public class ByteArrayAndObj
	{
		private ByteArray m_arr;
		private object m_o;

		// whether this information is applicable when put in context
		// (often, a segment will have a bit set that will be ignored because of its context)
		private bool m_bApplicable = true;
		
        // The relevant bits, if this byte array is just 1 byte in size.
        private byte m_u8ByteMask = 0xFF;   // default to all bits being relevant

		public ByteArrayAndObj(ByteArray arr, object o, bool bApplicable)
		{
			m_arr = arr;
			m_o = o;
			m_bApplicable = bApplicable;
		}

		public ByteArray OurByteArray
		{
			get
			{
				return m_arr;
			}
		}

		public object OurObj
		{
			get
			{
				return m_o;
			}
		}

		public bool IsApplicable
		{
			get
			{
				return m_bApplicable;
			}
		}

		/// <summary>
		/// If a specific bit from this byte array is relevant (some bytes have bit-level granularity)
		/// </summary>
		public byte WhichBit
		{
			set
			{
				// don't let them try any funny business
				if (m_arr.Array.Length > 1)
				{
					throw new Exception("WhichBit reserved for arrays of 1");
				}

                // range check
                if (value > 7)
                {
                    throw new Exception("WhichBit must be <= 7");
                }

                m_u8ByteMask = (byte) (1 << value);
			}
		}

        public byte ByteMask
        {
            set
            {
                // don't let them try any funny business
                if (m_arr.Array.Length > 1)
                {
                    throw new Exception("ByteMask reserved for arrays of 1");
                }

                m_u8ByteMask = value;
            }
            get
            {
                // don't let them try any funny business
                if (m_arr.Array.Length > 1)
                {
                    throw new Exception("ByteMask reserved for arrays of 1");
                }

                return m_u8ByteMask;
            }
        }
	}

}
