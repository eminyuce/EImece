using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class OfflineFileData
    {
        internal const string OfflineFilePath = "~/App_Data/offlinefile.txt";
        //The offline file contains three fields separated by the 'TextSeparator' char
        //a) datetimeUtc to go offline
        //b) the ip address to allow through
        //c) Message to show the user

        private const char TextSeparator = '|';

        /// <summary>
        /// This contains the IP address of the authprised person to let through
        /// </summary>
        public string [] IpAddressToLetThrough { get; private set; }

      


        public OfflineFileData(string offlineFilePath)
        {
            var offlineContent = File.ReadAllText(offlineFilePath).Split(TextSeparator);
            IpAddressToLetThrough = offlineContent[0];
        }
        // more below
    }
}
