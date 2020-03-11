using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace ROMSpinner.Business
{
    public class Prefs
    {
        private string m_strUserName;
        private string m_strPW;
        private bool m_bRememberUserName;
        private bool m_bRememberPW;

        public void ToXML(Stream stream)
        {
            // if they don't want to remember the password, then clear it now
            if (!m_bRememberPW)
            {
                m_strPW = string.Empty;
            }

            XmlSerializer xs = new XmlSerializer(typeof(Prefs));
            xs.Serialize(stream, this);
        }

        public static Prefs FromXML(Stream stream)
        {
            stream.Position = 0;    // rewind for parsing
            XmlSerializer xs = new XmlSerializer(typeof(Prefs));
            Prefs d = (Prefs)xs.Deserialize(stream);
            return d;
        }

        public string UserName
        {
            get
            {
                return m_strUserName;
            }
            set
            {
                m_strUserName = value;
            }
        }

        public string Password
        {
            get
            {
                return m_strPW;
            }
            set
            {
                m_strPW = value;
            }
        }

        public bool RememberUserName
        {
            get
            {
                return m_bRememberUserName;
            }
            set
            {
                m_bRememberUserName = value;
            }
        }

        public bool RememberPassword
        {
            get
            {
                return m_bRememberPW;
            }
            set
            {
                m_bRememberPW = value;
            }
        }
    }
}
