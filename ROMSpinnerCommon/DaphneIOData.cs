using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace ROMSpinner.Common
{
    /// <summary>
    /// to send commands to DaphneLoader
    /// </summary>
    public class DaphneLoaderCommand
    {
        public void ToXML(Stream stream)
        {
            XmlSerializer xs = new XmlSerializer(typeof(DaphneLoaderCommand));
            xs.Serialize(stream, this);
        }

        private string m_strLaunch = "";
        public string launch
        {
            get
            {
                return m_strLaunch;
            }
            set
            {
                m_strLaunch = value;
            }
        }
    }

    public class DaphneCommand
    {
        public void ToXML(Stream stream)
        {
            XmlSerializer xs = new XmlSerializer(typeof(DaphneCommand));
            xs.Serialize(stream, this);
        }

        public static DaphneCommand FromXML(byte[] array)
        {
            using (MemoryStream stream = new MemoryStream(array))
            {
                return FromXML(stream);
            }
        }

        public static DaphneCommand FromXML(Stream stream)
        {
            stream.Position = 0;    // rewind for parsing
            XmlSerializer xs = new XmlSerializer(typeof(DaphneCommand));
            DaphneCommand d = (DaphneCommand)xs.Deserialize(stream);
            return d;
        }

        private uint m_uSeek = 0;
        public uint seek
        {
            get
            {
                return m_uSeek;
            }
            set
            {
                m_uSeek = value;
            }
        }
    }

    /// <summary>
    /// Daphne response
    /// </summary>
    public class response_v1
    {
        private frame m_frame = null;
        public frame frame
        {
            get
            {
                return m_frame;
            }
            set
            {
                m_frame = value;
            }
        }

        public void ToXML(Stream stream)
        {
            XmlSerializer xs = new XmlSerializer(typeof(response_v1));
            xs.Serialize(stream, this);
        }

        public static response_v1 FromXML(Stream stream)
        {
            stream.Position = 0;    // rewind for parsing
            XmlSerializer xs = new XmlSerializer(typeof(response_v1));
            response_v1 d = (response_v1)xs.Deserialize(stream);
            return d;
        }

        public static response_v1 FromXML(byte[] arr)
        {
            using (MemoryStream stream = new MemoryStream(arr))
            {
                return FromXML(stream);
            }
        }

        private string m_error = "";
        public string error
        {
            get
            {
                return m_error;
            }
            set
            {
                m_error = value;
            }
        }

    }

    public class frame
    {
        private uint m_w = 0;
        public uint w
        {
            get
            {
                return m_w;
            }
            set
            {
                m_w = value;
            }
        }

        private uint m_h = 0;
        public uint h
        {
            get
            {
                return m_h;
            }
            set
            {
                m_h = value;
            }
        }

        public uint m_pitch = 0;
        public uint pitch
        {
            get
            {
                return m_pitch;
            }
            set
            {
                m_pitch = value;
            }
        }
    }
}
