using System;
using System.Collections.Generic;
using System.Text;

namespace ROMSpinner.Common
{
    public interface IROM
    {
        // The formal name of the ROM image collection
        string Name
        {
            get;
        }

        /// <summary>
        /// List of CRC's of files that make up this ROM
        /// </summary>
        List<long> CRC
        {
            get;
        }
    }
}
