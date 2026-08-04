namespace EImece.Domain.Core.Entities;

public class Menu : BaseContent
{
    public int ParentId { get; set; }
    public bool MainPage { get; set; }
    public string? MenuLink { get; set; }
    public string? Link { get; set; }
    public string? PageTheme { get; set; }
    public bool LinkIsActive { get; set; }
}
