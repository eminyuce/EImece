using EImece.Domain.Models.FrontModels;
using System;
using System.IO;

namespace EImece.Domain.Helpers
{
    public class OfflineHelper
    {
        public static OfflineFileData OfflineData { get; set; }

        /// <summary>
        /// This is true if we should redirect the user to the Offline View
        /// </summary>
        public bool ThisUserShouldBeOffline { get; private set; }

        public OfflineHelper(string currentIpAddress, Func<string, string> mapPath)
        {
            var offlineFilePath = mapPath(OfflineFileData.OfflineFilePath);
            if (File.Exists(offlineFilePath))
            {
                //The existance of the file says we want to go offline

                if (OfflineData == null)
                    //We need to read the data as new file was found
                    OfflineData = new OfflineFileData(offlineFilePath);

                ThisUserShouldBeOffline = !OfflineData.IpAddressToLetThrough.Contains(currentIpAddress);
            }
            else
            {
                //No file so not offline
                OfflineData = null;
            }
        }
    }
}