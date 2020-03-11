using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ROMSpinner.Common;
using ROMSpinner.Lair;

namespace ROMSpinner.Business
{
    public class ROMManager
    {
        IROM m_romInstance = null;
        byte[] m_arrROMBuffer = null;

        public ROMManager()
        {
        }

        public class LairF2ROM : IROM
        {
            public string Name
            {
                get
                {
                    return "Dragon's Lair rev F2";
                }
            }

            // CRC values for Lair F2 ROMs
            public List<long> CRC
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
        }

        /// <summary>
        /// </summary>
        /// <param name="strZipFileName"></param>
        public bool LoadROM(string strZipFileName)
        {
            bool bRes = false;
            Zip zipInstance = new Zip();
            List<long> lstActualCRCs = zipInstance.GetZipCRCs(strZipFileName);
            List<long> lstDecryptedCRCs = null;
            
            // these are all of the roms to check
            IROM[] arrROMs = 
                {
                    new LairF2ROM()
                };

            // now try to auto-detect which rom image we've loaded
            foreach (IROM rom in arrROMs)
            {
                lstDecryptedCRCs = rom.CRC;

                bRes = CheckCRCs(lstActualCRCs, lstDecryptedCRCs);
                if (bRes)
                {
                    m_romInstance = rom;
                    break;
                }
            }

            // if we were successful, then load the rom
            if (bRes)
            {
                zipInstance.Read(strZipFileName, lstDecryptedCRCs);

                // now construct our final buffer
                using (MemoryStream memStream = new MemoryStream())
                {
                    // create the final buffer in the correct order
                    foreach (long uCRCDecrypted in lstDecryptedCRCs)
                    {
                        byte [] buf = zipInstance.GetReadBuf(uCRCDecrypted);
                        memStream.Write(buf, 0, buf.Length);
                    }

                    m_arrROMBuffer = memStream.ToArray();
                }
            }
            return bRes;
        }

        public IROM RomInstance
        {
            get
            {
                return m_romInstance;
            }
        }

        public byte[] RomBuffer
        {
            get
            {
                return m_arrROMBuffer;
            }
        }

        private bool CheckCRCs(List<long> lstActualCRCs, List<long> lstDecryptedCRCs)
        {
            bool bRes = true;
            foreach (long crcDecrypted in lstDecryptedCRCs)
            {
                // if the CRC doesn't match, we're done
                if (!lstActualCRCs.Contains(crcDecrypted))
                {
                    bRes = false;
                    break;
                }
            }

            return bRes;
        }
    }
}
