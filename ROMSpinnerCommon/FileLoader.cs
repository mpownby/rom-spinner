using System;
using System.IO;
using System.Collections.Generic;

namespace ROMSpinner.Common
{
	/// <summary>
	/// Summary description for FileLoader.
	/// </summary>
	public class FileLoader
	{
		public FileLoader()
		{
		}

		static public byte [] LoadFiles(List<string> lFileNames)
		{
            using (MemoryStream streamBuf = new MemoryStream())
            {
                foreach (string s in lFileNames)
                {
                    using (FileStream fs = File.OpenRead(s))
                    {
                        using (BinaryReader rdr = new BinaryReader(fs))
                        {
                            byte[] buf = rdr.ReadBytes((int)fs.Length);	// read stream into buffer
                            streamBuf.Write(buf, 0, buf.Length);	// write to memory stream
                        }
                    }
                }
                return streamBuf.ToArray();
            }
		}
	}
}
