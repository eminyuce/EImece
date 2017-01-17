using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.HelperModels
{
    public class ErrorModel
    {
        public string RequestedUrl { get; set; }

        public string ReferrerUrl { get; set; }
    }
}
