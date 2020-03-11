using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using ROMSpinner.Common.Lair;

namespace ROMSpinner.LairUI
{
    [DefaultPropertyAttribute("FrameNumber")]
    public class GeneralSequenceProperties
    {
        protected LairSequenceData m_dat = null;
        private DurationOptions m_Duration = new DurationOptions();

        public GeneralSequenceProperties(LairSequenceData dat)
        {
            m_dat = dat;
        }

        [DescriptionAttribute("How many points are awarded to the player's score when this sequence runs")]
        [EditorAttribute(typeof(PointsTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Points
        {
            get
            {
                return "?";
            }
        }

        [DescriptionAttribute("How long this sequence lasts.")]
        [EditorAttribute(typeof(TickTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        //[TypeConverterAttribute(typeof(DurationOptionsConverter))]
        //public DurationOptions Duration
        public string Duration
        {
            get
            {
                string strRes = m_dat.Ticks + " ticks (" +
                    LairMath.TicksToFramesF(m_dat.Ticks).ToString("F1") + " frames)";
                return strRes;
            }
        }

        /// <summary>
        /// Used by TypeEditor
        /// </summary>
        /// <returns></returns>
        public uint GetTicks()
        {
            return m_dat.Ticks;
        }

        [DisplayName("Frame Number")]
        [DescriptionAttribute("Laserdisc frame that corresponds to this sequence.  " +
            "The previous sequence decides whether to seek to this frame.")]
        public UInt16 FrameNumber
        {
            get
            {
                return m_dat.FrameNum;
            }
            set
            {
                m_dat.FrameNum = value;
            }
        }

        [DisplayName("Seek If Timeout")]
        [DescriptionAttribute(
            "Whether to seek to the next sequence's frame number if this sequence times out.  " +
            "This should usually be true.  " +
            "One exception is when a move is optional, such as the first move of the flaming ropes scene.  "),
        DefaultValueAttribute(true)]
        public bool SeekIfTimeOut
        {
            get
            {
                return !m_dat.IgnoreNextSeek;
            }
            set
            {
                m_dat.IgnoreNextSeek = !value;
            }
        }

        [DisplayName("Easter Egg Allowed")]
        [DescriptionAttribute("Whether the unlimited lives easter egg can be enabled during this sequence"),
            DefaultValueAttribute(false),
            CategoryAttribute("Special")]
        public bool EasterEggAllowed
        {
            get
            {
                return m_dat.CanEasterEggBeEnabled;
            }
            set
            {
                m_dat.CanEasterEggBeEnabled = value;
            }
        }

        [DisplayName("Still Frame")]
        [DescriptionAttribute("Whether this sequence is a still frame such as a slide during the attract mode."),
            DefaultValueAttribute(false),
            CategoryAttribute("Special")]
        public bool StillFrame
        {
            get
            {
                return m_dat.IsStillFrame;
            }
            set
            {
                m_dat.IsStillFrame = value;
            }
        }
    }

    #region Normal
    public class NormalSequenceProperties : GeneralSequenceProperties
    {
        public NormalSequenceProperties(LairSequenceData dat)
            : base(dat)
        {
        }

        [DisplayName("Next Sequence If Timeout")]
        [CategoryAttribute("Next Sequence")]
        [DescriptionAttribute("The next sequence that runs if this sequence times out.")]
        public uint DefaultNextSequence
        {
            get
            {
                return m_dat.NextSequence;
            }
            set
            {
                m_dat.NextSequence = value;
            }
        }
    }
    #endregion

    #region Success
    public class SuccessSequenceProperties : GeneralSequenceProperties
    {
        public SuccessSequenceProperties(LairSequenceData dat)
            : base(dat)
        {
        }

        [DisplayName("Next Scene Group")]
        [DescriptionAttribute("If specified, this overrides which group of scenes comes next.  " +
            "This is only used by the falling platform scenes in order to make the next group of scenes be " +
            "either the flaming ropes or filling wall."),
            DefaultValueAttribute(false),
            CategoryAttribute("Next Scene Group")]
        public string NextSceneGroup
        {
            get
            {
                string strRes = "";
                if (m_dat.UseNextSelectionGroup == true)
                {
                    strRes = m_dat.NextSelectionGroup.ToString();
                }
                return strRes;
            }
            /*
            set
            {
                // TODO : show all scene groups here
            }
             */
        }

        // NOTE : Pay As You Go apparently can be enabled any time, but it only makes sense to
        //  enable it after a successful move.  Plus, no one uses it anyway.
        [DisplayName("Pay As You Go")]
        [DescriptionAttribute("Whether Pay-As-You-Go kicks in after sequence ends.  " +
            "This is used rarely, for example it is used at the successful conclusion of the falling platform stage."),
            DefaultValueAttribute(false),
            CategoryAttribute("Special")]
        public bool PayAsYouGo
        {
            get
            {
                return m_dat.PayAsYouGoAfterSequence;
            }
            set
            {
                m_dat.PayAsYouGoAfterSequence = value;
            }
        }

    }
    #endregion

    #region Death
    public class DeathSequenceProperties : GeneralSequenceProperties
    {
        public DeathSequenceProperties(LairSequenceData dat)
            : base(dat)
        {
        }

        [DisplayName("Repeat Current Scene")]
        [DescriptionAttribute("Whether the current scene will start over after this sequence ends.  " +
            "This is only used by the falling platform scenes."),
            DefaultValueAttribute(false),
            CategoryAttribute("Special")]
        public bool RepeatSceneOnDeath
        {
            get
            {
                return m_dat.RepeatSceneOnDeath;
            }
            set
            {
                m_dat.RepeatSceneOnDeath = value;
            }
        }
    }
    #endregion

    #region GameOver
    public class GameOverSequenceProperties : GeneralSequenceProperties
    {
        public GameOverSequenceProperties(LairSequenceData dat)
            : base(dat)
        {
        }

    }
    #endregion

    #region Attract
    public class AttractSequenceProperties : GeneralSequenceProperties
    {
        public AttractSequenceProperties(LairSequenceData dat)
            : base(dat)
        {
        }
    }
    #endregion


    #region Duration

    public class DurationOptions
    {
        private uint m_uTicks = 0;

        [DescriptionAttribute("Ticks are how the ROM tracks time internally.  One tick is equal to approximately 33 ms.")]
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

    public class DurationOptionsConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DurationOptions))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                value is DurationOptions)
            {
                DurationOptions dopt = (DurationOptions)value;

                double dSecs = LairMath.TicksToSeconds(dopt.Ticks);
                double dFrames = 23.976 * dSecs;    // lair disc frame numbers are arranged at 24FPS, not 30FPS
                string strRes = dFrames.ToString("F0") + " frames, ";

                strRes += dSecs.ToString("F2");
                strRes += " seconds";

                return strRes;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
    #endregion

}
