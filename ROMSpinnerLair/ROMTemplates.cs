using System;
using System.Collections.Generic;
using System.Text;
using ROMSpinner.Common;

namespace ROMSpinner.Lair
{
    public class LairF2 : IROM
    {
        private byte[] m_arrBuf = null;

        public string Name
        {
            get
            {
                return "Dragon's Lair rev F2";
            }
        }

        public List<long> CrcList
        {
            get
            {
                List<long> lstCRC = new List<long>();
                lstCRC.Add(0xF5EA3B9D);
                lstCRC.Add(0xDCC1DFF2);
                lstCRC.Add(0xAB514E5B);
                lstCRC.Add(0xF5EC23D2);
                return lstCRC;
            }
        }

        public byte[] Buf
        {
            get
            {
                return m_arrBuf;
            }
            set
            {
                m_arrBuf = value;
            }
        }
    }
}
