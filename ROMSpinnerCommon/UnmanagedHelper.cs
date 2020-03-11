using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ROMSpinner.Common
{
    /*
    public class UnmanagedHelper
    {
#if DEBUG
        [DllImport("ROMSpinnerUnmanaged_dbg")]
#else
        [DllImport("ROMSpinnerUnmanaged")]
#endif
        static extern uint GetCRC(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pUserKey,
            int uKeyLength,
            long uScrambledCRC);

#if DEBUG
        [DllImport("ROMSpinnerUnmanaged_dbg")]
#else
        [DllImport("ROMSpinnerUnmanaged")]
#endif
        static extern uint GetSceneSelectionOffset(
            uint uSceneSelectionIdx,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pUserKey,
            int uKeyLength,
            uint uScrambledVal);

#if DEBUG
        [DllImport("ROMSpinnerUnmanaged_dbg")]
#else
        [DllImport("ROMSpinnerUnmanaged")]
#endif
        static extern uint GetSceneIdxOffset(
            uint uSceneIdx,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pUserKey,
            int uKeyLength,
            uint uScrambledVal);

#if DEBUG
        [DllImport("ROMSpinnerUnmanaged_dbg")]
#else
        [DllImport("ROMSpinnerUnmanaged")]
#endif
        static extern uint GetSeq0AddressOffset(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pUserKey,
            int uKeyLength,
            uint uScrambledVal);

        public static uint DecryptCRC(byte [] arrUserKey, long uScrambledCRC)
        {
            return GetCRC(arrUserKey, arrUserKey.Length, uScrambledCRC);
        }

        public static uint DecryptSceneSelectionOffset(uint uSceneSelIdx, byte [] arrUserKey, uint uScrambledVal)
        {
            return GetSceneSelectionOffset(uSceneSelIdx, arrUserKey, arrUserKey.Length, uScrambledVal);
        }

        public static uint DecryptSceneIdxOffset(uint uSceneIdx, byte [] arrUserKey, uint uScrambledVal)
        {
            return GetSceneIdxOffset(uSceneIdx, arrUserKey, arrUserKey.Length, uScrambledVal);
        }

        public static uint DecryptSeq0AddressOffset(byte [] arrUserKey, uint uScrambledVal)
        {
            return GetSeq0AddressOffset(arrUserKey, arrUserKey.Length, uScrambledVal);
        }

        // RSA Verify
#if DEBUG
        [DllImport("ROMSpinnerUnmanaged_dbg")]
#else
        [DllImport("ROMSpinnerUnmanaged")]
#endif
        static extern bool VerifyBuf(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int uBufLength,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pSig,
            int uSigLength);

        /// <summary>
        /// This will take pBuf, compute the SHA512 hash of it, and then
        ///  verify that the signature matches the hash using a hard-coded public key.
        /// </summary>
        /// <param name="pBuf">Buffer to verify.</param>
        /// <param name="pSig">The RSA signature.</param>
        /// <returns></returns>
        public static bool VerifyBufWithRSASig(byte[] pBuf, byte[] pSig)
        {
            return VerifyBuf(pBuf, pBuf.Length, pSig, pSig.Length);
        }
    }
     */
}
