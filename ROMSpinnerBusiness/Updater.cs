using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ROMSpinner.Common;

namespace ROMSpinner.Business
{
    public class Updater
    {
        private UpdateTrees m_UpdateTrees = new UpdateTrees();

        public Updater()
        {
        }

        /// <summary>
        /// for testing purposes
        /// </summary>
        /// <param name="trees"></param>
        public Updater(UpdateTrees trees)
        {
            m_UpdateTrees = trees;
        }

        private UpdateInfo ActiveUpdateInfo
        {
            get
            {
                // TODO : update for development later
                return m_UpdateTrees.stable;
            }
        }

        public bool UpdateAvailable(int uOurMajor, int uOurMinor, int uOurBuild)
        {
            bool bRes = false;

            UpdateInfo info = ActiveUpdateInfo;

            int uMajor = info.ver.Major;
            int uMinor = info.ver.Minor;
            int uBuild = info.ver.Build;

            // now compare versions ...
            if (
                // if remote major version is greater than ours ...
                (uMajor > uOurMajor) ||
                // or if remote major version is the same as ours, 
                (
                    (uMajor == uOurMajor) &&
                // and remote minor version is greater
                    (
                        (uMinor > uOurMinor) ||
                // or minor versions are the same, and remote build is greater
                        ((uMinor == uOurMinor) && (uBuild > uOurBuild))
                    )
                )
            )
            {
                bRes = true;
            }

            return bRes;
        }

        public bool VerifyUpdate(Stream stream, byte[] expectedSHA512)
        {
            bool bRes = false;

            // TODO : write unit test

            return bRes;
        }
    }
}
