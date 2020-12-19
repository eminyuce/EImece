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
        private const string DefaultOfflineMessage =
             "The site is down for maintenance. Please check back later";

        /// <summary>
        /// This contains the datatime when the site should go offline should be offline
        /// </summary>
        public DateTime TimeWhenSiteWillGoOfflineUtc { get; private set; }

        /// <summary>
        /// This contains the IP address of the authprised person to let through
        /// </summary>
        public string IpAddressToLetThrough { get; private set; }

        /// <summary>
        /// A message to display in the Offline View
        /// </summary>
        public string Message { get; private set; }


        public OfflineFileData(string offlineFilePath)
        {
            var offlineContent = File.ReadAllText(offlineFilePath).Split(TextSeparator);

            DateTime parsedDateTime;
            TimeWhenSiteWillGoOfflineUtc = DateTime.TryParse(offlineContent[0],
                null, System.Globalization.DateTimeStyles.RoundtripKind,
                out parsedDateTime) ? parsedDateTime : DateTime.UtcNow;
            IpAddressToLetThrough = offlineContent[1];
            Message = offlineContent[2];
        }
        // more below
    }
}
