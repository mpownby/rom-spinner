using System;
using System.IO;

namespace ROMSpinner.Common
{
	/// <summary>
	/// Summary description for Util.
	/// </summary>
	public class Util
	{
		public Util()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static uint Load16(byte [] arr, uint uOffset)
		{
			uint uRes = arr[uOffset + 1];
			uRes <<= 8;
			uRes |= arr[uOffset];
			return uRes;
		}

        public static string Buf2Str(byte[] buf)
        {
            string strRes = "";
            string strChars = "0123456789abcdef";

            for (int i = 0; i < buf.Length; i++)
            {
                byte u8 = buf[i];
                strRes += strChars[u8 >> 4];
                strRes += strChars[u8 & 0xF];
                strRes += " ";
            }

            return strRes;
        }

        public static byte[] HexStr2Buf(string strInput)
        {
            // FROM http://programmerramblings.blogspot.com/2008/03/convert-hex-string-to-byte-array-and.html
            // allocate byte array based on half of string length
            int numBytes = (strInput.Length) / 2;
            byte[] bytes = new byte[numBytes];

            // loop through the string - 2 bytes at a time converting it to decimal equivalent and store in byte array
            // x variable used to hold byte array element position
            for (int x = 0; x < numBytes; ++x)
            {
                bytes[x] = Convert.ToByte(strInput.Substring(x * 2, 2), 16);
            }

            // return the finished byte array of decimal values
            return bytes;
        }

        public static byte [] StreamToArray(Stream src)
        {
            byte[] bufTmp = new byte[2048];
            using (MemoryStream stream = new MemoryStream())
            {
                for (; ; )
                {
                    int iBytesRead = src.Read(bufTmp, 0, bufTmp.Length);
                    stream.Write(bufTmp, 0, iBytesRead);
                    if (iBytesRead != bufTmp.Length)
                    {
                        break;
                    }
                }
                return stream.ToArray();
            }
        }

        public static string ASCIIArrayToString(byte[] arr)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(arr);
        }

        public static byte[] StringToASCIIArray(string s)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(s);
        }

        public static string UTF8ArrayToString(byte[] arr)
        {
            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            return enc.GetString(arr);
        }

        public static byte[] StringToUTF8Array(string s)
        {
            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            return enc.GetBytes(s);
        }

        /// <summary>
        /// Returns true if two byte arrays are equal.
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <returns></returns>
        public static bool ArrayCompare(byte[] arr1, byte[] arr2)
        {
            bool bRes = false;

            if (arr1.Length == arr2.Length)
            {
                bRes = true;
                for (int i = 0; i < arr1.Length; i++)
                {
                    if (arr1[i] != arr2[i])
                    {
                        bRes = false;
                        break;
                    }
                }
            }

            return bRes;
        }

        public static string StripVersion(string strPathWithVersion)
        {
            int idx = strPathWithVersion.LastIndexOf('\\');
            string strRes = strPathWithVersion.Substring(0, idx);
            return strRes;
        }


    }
}
