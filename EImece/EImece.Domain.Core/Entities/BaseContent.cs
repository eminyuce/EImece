using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Core.Entities;

public abstract class BaseContent : BaseEntity
{
    public string? Description { get; set; }
    public bool ImageState { get; set; }
    public string? MetaKeywords { get; set; }

    [ForeignKey(nameof(MainImage))]
    public int? MainImageId { get; set; }

    public FileStorage? MainImage { get; set; }

    public string? UpdateUserId { get; set; }
    public string? AddUserId { get; set; }
}
