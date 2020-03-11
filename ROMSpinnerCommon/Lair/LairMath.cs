using System;
using System.Collections.Generic;
using System.Text;

namespace ROMSpinner.Common.Lair
{
    public class LairMath
    {
        public static double TicksToSeconds(uint uTicks)
        {
            return TicksToSeconds(uTicks, false);
        }

        public static double TicksToSeconds(uint uTicks, bool bIncludeLDV1000SearchDelay)
        {
            //uint uExtraTicks = uTicks / 31;	// see 1929
            uint uExtraTicks = uTicks / 30; // I was having accuracy problems, so I decided to try 30 instead of 31 and it was much better
            double dSeconds = (uTicks + uExtraTicks) * .032768;	// each tick is 32.768 ms
            if (bIncludeLDV1000SearchDelay)
            {
                dSeconds += LDV1000SearchDelaySeconds;
            }
            return dSeconds;
        }

        public static double TicksToFramesF(uint uTicks)
        {
            return TicksToFramesF(uTicks, false);
        }

        public static double TicksToFramesF(uint uTicks, bool bIncludeLDV1000SearchDelay)
        {
            double dRes = TicksToSeconds(uTicks, bIncludeLDV1000SearchDelay);
            dRes *= 23.976;
            return dRes;
        }

        public static uint TicksToFramesU(uint uTicks, bool bIncludeLDV1000SearchDelay)
        {
            double dRes = TicksToSeconds(uTicks, bIncludeLDV1000SearchDelay);
            dRes *= 23.976;
            uint uRes = (uint)dRes;
            if ((dRes - uRes) > 0)
            {
                uRes += 1;  // any remaining fraction means a new frame has started
            }
            return uRes;
        }

        /// <summary>
        /// How many seconds to add onto timing calculations when taking into account LDV1000 searching latency
        /// </summary>
        public static double LDV1000SearchDelaySeconds
        {
            get
            {
                return (6 *	// 6 commands (5 digits, 1 search byte)
                    (1 / 60.0) *	// each command strobe comes up every 60 Hz (16.6ms)
                    2);		// two bytes per command (0xFF and then the command byte itself)
            }
        }

    }
}
