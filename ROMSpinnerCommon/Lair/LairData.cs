using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace ROMSpinner.Common.Lair
{
    public enum SequenceType
    {
        Normal,
        EndSuccess,
        EndDeath,
        GameOver,
        AttractMode
    }

    public enum Move
    {
        Sword,
        Up,
        Down,
        Left,
        UpLeft,
        DownLeft,
        Right,
        UpRight,
        DownRight
    }

    /// <summary>
    /// all scenes, designed to be serialized via XML
    /// </summary>
    public class LairScenesData
    {
        private List<LairSceneData> m_lstScenes = null;
        
        public List<LairSceneData> Scenes
        {
            get
            {
                return m_lstScenes;
            }
            set
            {
                m_lstScenes = value;
            }
        }

        private List<LairSequenceData> m_lstSequences = null;

        public List<LairSequenceData> Sequences
        {
            get
            {
                return m_lstSequences;
            }
            set
            {
                m_lstSequences = value;
            }
        }
    }

    public class LairSceneData
    {
        /*
        private uint m_uSceneIdx = 0;  // ID of the scene

        public uint SceneIdx
        {
            get
            {
                return m_uSceneIdx;
            }
            set
            {
                m_uSceneIdx = value;
            }
        }
         */

        private string m_strSceneName = "";

        public string SceneName
        {
            get
            {
                return m_strSceneName;
            }
            set
            {
                m_strSceneName = value;
            }
        }

        private List<string> m_lstSeqNames; // names of each sequence in this scene

        public List<string> SequenceNames
        {
            get
            {
                return m_lstSeqNames;
            }
            set
            {
                m_lstSeqNames = value;
            }
        }
    }

    /// <summary>
    /// this class is designed to be serialized to/from XML
    /// </summary>
    public class LairSequenceData
    {
        private string m_strName = "";

        public string Name
        {
            get
            {
                return m_strName;
            }
            set
            {
                m_strName = value;
            }
        }

        private SequenceType m_type = SequenceType.Normal;
        public SequenceType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }

        private bool m_bEasterEggPossible = false;
        public bool CanEasterEggBeEnabled
        {
            get
            {
                return m_bEasterEggPossible;
            }
            set
            {
                m_bEasterEggPossible = value;
            }
        }

        private bool m_bIsStillFrame = false;
        public bool IsStillFrame
        {
            get
            {
                return m_bIsStillFrame;
            }
            set
            {
                m_bIsStillFrame = value;
            }
        }

        private uint m_uNextSequence = 0;
        public uint NextSequence
        {
            get
            {
                return m_uNextSequence;
            }
            set
            {
                m_uNextSequence = value;
            }
        }

        private uint m_uNextSelectionGroup = 0;
        public uint NextSelectionGroup
        {
            get
            {
                return m_uNextSelectionGroup;
            }
            set
            {
                m_uNextSelectionGroup = value;
            }
        }

        private bool m_bUseNextSelectionGroup = false;
        public bool UseNextSelectionGroup
        {
            get
            {
                return m_bUseNextSelectionGroup;
            }
            set
            {
                m_bUseNextSelectionGroup = value;
            }
        }

        private bool m_bPayAsYouGoAfterSequence = false;
        public bool PayAsYouGoAfterSequence
        {
            get
            {
                return m_bPayAsYouGoAfterSequence;
            }
            set
            {
                m_bPayAsYouGoAfterSequence = value;
            }
        }

        private bool m_bRepeatSceneOnDeath = false;
        public bool RepeatSceneOnDeath
        {
            get
            {
                return m_bRepeatSceneOnDeath;
            }
            set
            {
                m_bRepeatSceneOnDeath = value;
            }
        }

        private uint m_uScoreIdx = 0;
        public uint ScoreIdx
        {
            get
            {
                return m_uScoreIdx;
            }
            set
            {
                m_uScoreIdx = value;
            }
        }

        private bool m_bTrailerIgnoreNextSeek = false;
        public bool IgnoreNextSeek
        {
            get
            {
                return m_bTrailerIgnoreNextSeek;
            }
            set
            {
                m_bTrailerIgnoreNextSeek = value;
            }
        }

        private uint m_uTrailerTicks = 0;
        public uint Ticks
        {
            get
            {
                return m_uTrailerTicks;
            }
            set
            {
                m_uTrailerTicks = value;
            }
        }

        private UInt16 m_u16FrameNum = 0;
        public UInt16 FrameNum
        {
            get
            {
                return m_u16FrameNum;
            }
            set
            {
                m_u16FrameNum = value;
            }
        }

        private List<LairMoveData> m_lstMoves = new List<LairMoveData>();
        public List<LairMoveData> Moves
        {
            get
            {
                return m_lstMoves;
            }
            set
            {
                m_lstMoves = value;
            }
        }

    } // end class

    public class LairMoveData
    {
        private Move m_move = 0;   // arbitrary default
        public Move Move
        {
            get
            {
                return m_move;
            }
            set
            {
                m_move = value;
            }
        }

        private List<TimeWindow> m_lstTimeWindows = new List<TimeWindow>();
        public List<TimeWindow> TimeWindows
        {
            get
            {
                return m_lstTimeWindows;
            }
            set
            {
                m_lstTimeWindows = value;

                // range check: this is a limitation in the dragon's lair program
                if (m_lstTimeWindows.Count > 3)
                {
                    throw new Exception("TimeWindows can not contain more than 3 entries");
                }
            }
        }

        private uint m_uTicksBeforeMoveCanBeAccepted = 0;
        public uint TicksBeforeMoveCanBeAccepted
        {
            get
            {
                return m_uTicksBeforeMoveCanBeAccepted;
            }
            set
            {
                m_uTicksBeforeMoveCanBeAccepted = value;
            }
        }

    }

    public class TimeWindow
    {
        private uint m_uNextSequenceIdx = 0;
        public uint NextSequence
        {
            get
            {
                return m_uNextSequenceIdx;
            }
            set
            {
                m_uNextSequenceIdx = value;
            }
        }

        private bool m_bIgnoreNextSeek = false;
        public bool IgnoreNextSeek
        {
            get
            {
                return m_bIgnoreNextSeek;
            }
            set
            {
                m_bIgnoreNextSeek = value;
            }
        }

        private uint m_uTicks = 0;
        public uint Ticks
        {
            get
            {
                return m_uTicks;
            }
            set
            {
                m_uTicks = value;
            }
        }
    }

    /// <summary>
    /// Class to make it easy to lookup a sequence using the sequence name.
    /// </summary>
    public class LairSequenceIndexer
    {
        private Dictionary<string, LairSequenceData> m_dict = null;

        public LairSequenceIndexer(List<LairSequenceData> lstSequenceData)
        {
            DoIndexing(lstSequenceData);
        }

        private void DoIndexing(List<LairSequenceData> lstSequenceData)
        {
            m_dict = new Dictionary<string, LairSequenceData>();

            foreach (LairSequenceData dat in lstSequenceData)
            {
                m_dict[dat.Name] = dat;
            }
        }

        public LairSequenceData GetSequenceData(string strName)
        {
            LairSequenceData res = null;

            if (m_dict.ContainsKey(strName))
            {
                res = m_dict[strName];
            }

            return res;
        }
    }

    /// <summary>
    /// Class to make it easy to lookup a sequence using the scene index
    /// </summary>
    public class LairScenesIndexer
    {
        private LairScenesData m_scenes = null;
        private LairSequenceIndexer m_indexer = null;

        public LairScenesIndexer(LairScenesData scenes)
        {
            m_scenes = scenes;
            m_indexer = new LairSequenceIndexer(scenes.Sequences);
        }

        /// <summary>
        /// Retrieves sequence by scene index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public LairSequenceData GetSequenceData(int idxScene, int idxSequence)
        {
            LairSequenceData res = null;

            // range check
            if (m_scenes.Scenes.Count > idxScene)
            {
                LairSceneData pScene = m_scenes.Scenes[idxScene];

                // range check
                if (pScene.SequenceNames.Count > idxSequence)
                {
                    string strSeqName = pScene.SequenceNames[idxSequence];
                    res = m_indexer.GetSequenceData(strSeqName);
                }
            }
            // else out of range

            return res;
        }
    }

}
