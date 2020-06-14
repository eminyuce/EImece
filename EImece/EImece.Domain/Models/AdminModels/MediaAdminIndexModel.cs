using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;

namespace EImece.Domain.Models.AdminModels
{
    public class MediaAdminIndexModel
    {
        public int Id { get; set; }
        public EImeceImageType ImageType { get; set; }
        public MediaModType MediaMod { get; set; }
        public BaseContent BaseContent { get; set; }
        public List<FileStorage> FileStorages { get; set; }
        public EImeceLanguage Lang { get; set; }
    }
}