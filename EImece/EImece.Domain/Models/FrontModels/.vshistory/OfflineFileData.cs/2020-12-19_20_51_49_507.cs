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
        public List<String> IpAddressToLetThrough { get; private set; }

        public OfflineFileData(string offlineFilePath)
        {
            string [] lines = File.ReadAllLines(offlineFilePath);
            if (lines != null)
            {
                IpAddressToLetThrough = lines.Select(r => r.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToList();
            }
        }
        // more below
    }
}
