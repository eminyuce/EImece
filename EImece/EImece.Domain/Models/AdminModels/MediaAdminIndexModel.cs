using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.AdminModels
{
    public class MediaAdminIndexModel
    {
        public int Id { get; set; }
        public EImeceImageType imageType { get; set; }
        public MediaModType mediaMod { get; set; }
        public BaseContent baseContent { get; set; }

    }
}
