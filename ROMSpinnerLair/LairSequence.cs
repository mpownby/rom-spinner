using System;
using System.Collections;
using System.Collections.Generic;

namespace ROMSpinner.Lair
{
	/// <summary>
	/// Summary description for LairSequence.
	/// </summary>
	public class LairSequence
	{
        private List<LairSegment> m_lstSegments;

		public LairSequence(List<LairSegment> lstSegments)
		{
			m_lstSegments = lstSegments;
		}

        public List<LairSegment> Segments
		{
            get
            {
                return m_lstSegments;
            }
		}
	}
}
