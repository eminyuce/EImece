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
        /// This contains the IP address of the authprised person to let through
        /// </summary>
        public string [] IpAddressToLetThrough { get; private set; }

        public OfflineFileData(string offlineFilePath)
        {
            IpAddressToLetThrough = File.ReadAllLines(offlineFilePath).Select(r => r.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToList(); ;
        }
        // more below
    }
}
