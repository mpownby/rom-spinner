using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ROMSpinner.Common
{
    public class Zip
    {
        Hashtable m_hashBuf = null;

        public Zip()
        {
        }

        /// <summary>
        /// This function returns the CRC's of the files inside the zip file.
        /// The CRCs can be used to auto-detect which type of ROM it is.
        /// This function will throw an exception on error.
        /// </summary>
        /// <param name="lZipFileName">Name of ZIP file</param>
        /// <returns></returns>
        public List<long> GetZipCRCs(string strZipFileName)
        {
            List<long> lstResult = new List<long>();
            using (FileStream readStream = File.OpenRead(strZipFileName))
            {
                using (ZipInputStream zipStream = new ZipInputStream(readStream))
                {
                    ZipEntry theEntry;
                    while ((theEntry = zipStream.GetNextEntry()) != null)
                    {
                        lstResult.Add(theEntry.Crc);
                    }
                }
            }

            return lstResult;
        }

        public void Read(string strZipFileName, List<long> lstCRCs)
        {
            m_hashBuf = new Hashtable();

            using (FileStream readStream = File.OpenRead(strZipFileName))
            {
                using (ZipInputStream zipStream = new ZipInputStream(readStream))
                {
                    ZipEntry theEntry;
                    while ((theEntry = zipStream.GetNextEntry()) != null)
                    {
                        // if this file is relevant
                        if (lstCRCs.Contains(theEntry.Crc))
                        {
                            // add this new buf to hash table, so that the final buf can be created in the right order
                            m_hashBuf[theEntry.Crc] = ReadZipFile(zipStream);
                        }
                        // else the file isn't a ROM file so we don't need to load it
                    }
                }
            }
        }

        /// <summary>
        /// Reads an entire individual file from a zip archive
        /// </summary>
        /// <param name="zipStream"></param>
        /// <returns></returns>
        static public byte[] ReadZipFile(ZipInputStream zipStream)
        {
            byte[] bufResult = null;

            using (MemoryStream memStream = new MemoryStream())
            {
                byte[] buf = new byte[2048];
                for (; ; )
                {
                    int iBytesRead = zipStream.Read(buf, 0, buf.Length);
                    if (iBytesRead > 0)
                    {
                        memStream.Write(buf, 0, iBytesRead);
                    }

                    // if we read less than we expected, we're done
                    if (iBytesRead != buf.Length)
                    {
                        break;
                    }
                }

                bufResult = memStream.ToArray();
            }

            return bufResult;
        }

        public byte[] GetReadBuf(long uCRC)
        {
            return (byte []) m_hashBuf[uCRC];
        }
    }
}
