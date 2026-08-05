namespace EImece.Domain.Core.Entities;

public class Brand : BaseContent
{
    public bool MainPage { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
