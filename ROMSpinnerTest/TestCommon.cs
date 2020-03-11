using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ROMSpinner.Common;

namespace ROMSpinner.Test
{
    [TestFixture]
    public class TestCommon
    {
        [Test]
        public void Hex2Bin()
        {
            string s = "aabbccddeeff0011";
            byte[] arr = { 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0, 0x11 };
            byte[] arr2 = Util.HexStr2Buf(s);
            Assert.AreEqual(arr, arr2);
        }

        [Test]
        public void ArrayCompare()
        {
            byte[] arr1 = { 0, 1, 2, 3 };
            byte[] arr2 = { 0, 1, 2, 3 };
            byte[] arr3 = { 0, 1, 2, 4 };
            bool bRes = Util.ArrayCompare(arr1, arr2);
            Assert.AreEqual(true, bRes);

            bRes = Util.ArrayCompare(arr1, arr3);
            Assert.AreEqual(false, bRes);
        }

        [Test]
        public void TestVersionStrip()
        {
            string s = "abc\\1.2.3.4";
            string sStripped = Util.StripVersion(s);
            Assert.AreEqual("abc", sStripped);
        }
    }
}
