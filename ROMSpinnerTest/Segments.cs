using System;
using System.Collections.Generic;
using System.Text;

namespace ROMSpinner.Test
{
    class Segments
    {
        static public byte[] SegFillingWallMove
        {
            get
            {
                byte[] arr = { 0x91, 0x11, 3, 0x80, 0, 0x2A };
                return arr;
            }
        }

        static public byte[] SegFlamingRopes1
        {
            get
            {
                byte[] arr = { 1, 0xA, 0x42, 0, 0x80, 0xe9, 0x0d };
                return arr;
            }
        }

        static public byte[] SegFlamingRopes2
        {
            get
            {
                byte[] arr = { 0xb8, 0x12, 0x8, 4, 0x40, 0, 0x20, 0x17 };
                return arr;
            }
        }

        static public byte[] SegRoomOfFire3
        {
            get
            {
                byte[] arr = { 0xd1, 0x93, 7, 0xff, 4, 0x20, 0, 0xa, 0x19, 0x1d };
                return arr;
            }
        }

        static public byte[] SegAttractMode
        {
            get
            {
                byte[] arr = { 0x40, 0, 0xF5, 0x04, 0x03, 0x43, 0x01 };
                return arr;
            }
        }

        static public byte[] SegFallingPlatSuccess
        {
            get
            {
                byte[] arrPlat7 = { 0xb, 0x81, 0x8a, 0x0, 0x22, 0x06, 0x3c };	// falling platform sequence 7 (success)
                return arrPlat7;
            }
        }

        static public byte[] SegDrinkMeUp
        {
            get
            {
                byte[] arr = { 0x81, 1, 4, 0, 0x3d };
                return arr;
            }
        }
    }
}
