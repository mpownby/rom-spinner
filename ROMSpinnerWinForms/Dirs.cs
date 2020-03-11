using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ROMSpinner.Business;
using ROMSpinner.Common;

namespace ROMSpinner.Win
{
    public class Dirs
    {
        private static string VerStripped(string strSrc)
        {
            Directory.Delete(strSrc);   // delete the directory that was just created because it has unwanted version info attached
            strSrc = Util.StripVersion(strSrc); // strip off version info
            return strSrc;
        }

        public static string HomeDir
        {
            get
            {
                return VerStripped(Application.UserAppDataPath);
            }
        }

        public static string AddHomeDir(string strPath)
        {
            return HomeDir + "\\" + strPath;
        }

        public static string CommonDir
        {
            get
            {
                return VerStripped(Application.CommonAppDataPath);
            }
        }

        private static string PrefsFileName
        {
            get
            {
                const string m_strPrefsFileName = "RomSpinnerPrefs.xml";
                return AddHomeDir(m_strPrefsFileName);
            }
        }

        public static void SavePrefs(Prefs p)
        {
            using (FileStream fs = new FileStream(PrefsFileName, FileMode.Create))
            {
                p.ToXML(fs);
            }
        }

        public static Prefs LoadPrefs()
        {
            using (FileStream fs = new FileStream(PrefsFileName, FileMode.Open))
            {
                return Prefs.FromXML(fs);
            }
        }

    }
}
