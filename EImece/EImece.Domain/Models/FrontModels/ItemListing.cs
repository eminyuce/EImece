using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class ItemListing
    {
        public int RouteId { set; get; }
        public int Page { get; set; }
        public int RecordPerPage { get; set; }
        public string Filter { get; set; }
        public SortingType Sorting { get; set; }
    }
}
