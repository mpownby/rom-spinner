using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace ROMSpinner.Common
{
    public class UpdateTrees
    {
        private UpdateInfo m_DevUpdateInfo = new UpdateInfo();
        private UpdateInfo m_StableUpdateInfo = new UpdateInfo();

        public void ToXML(Stream stream)
        {
            XmlSerializer xs = new XmlSerializer(typeof(UpdateTrees));
            xs.Serialize(stream, this);
        }

        public static UpdateTrees FromXML(Stream stream)
        {
            stream.Position = 0;    // rewind for parsing
            XmlSerializer xs = new XmlSerializer(typeof(UpdateTrees));
            UpdateTrees d = (UpdateTrees)xs.Deserialize(stream);
            return d;
        }

        public UpdateInfo dev
        {
            get
            {
                return m_DevUpdateInfo;
            }
            set
            {
                m_DevUpdateInfo = value;
            }
        }

        public UpdateInfo stable
        {
            get
            {
                return m_StableUpdateInfo;
            }
            set
            {
                m_StableUpdateInfo = value;
            }
        }
    }

    public class UpdateInfo
    {
        private UpdateVer m_ver = new UpdateVer();

        public UpdateVer ver
        {
            get
            {
                return m_ver;
            }
            set
            {
                m_ver = value;
            }
        }

        // This is formatted as ASCII hex for compatibility reasons, and to make it
        //  easier for a human to immediately see the hex string (which isn't possible with base64).
        private string m_sha512 = "";

        public string sha512
        {
            get
            {
                return m_sha512;
            }
            set
            {
                m_sha512 = value;
            }
        }

        // the length of the update
        private int m_uLength = 0;
        public int length
        {
            get
            {
                return m_uLength;
            }
            set
            {
                m_uLength = value;
            }
        }

        private string m_strURL = "";
        public string url
        {
            get
            {
                return m_strURL;
            }
            set
            {
                m_strURL = value;
            }
        }

    }

    public class UpdateVer
    {
        private int m_iMajor = 0;
        private int m_iMinor = 0;
        private int m_iBuild = 0;

        public int Major
        {
            get
            {
                return m_iMajor;
            }
            set
            {
                m_iMajor = value;
            }
        }

        public int Minor
        {
            get
            {
                return m_iMinor;
            }
            set
            {
                m_iMinor = value;
            }
        }

        public int Build
        {
            get
            {
                return m_iBuild;
            }
            set
            {
                m_iBuild = value;
            }
        }
    }
}
