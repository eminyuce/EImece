namespace EImece.Domain.Models.FrontModels
{
    /// <summary>
    /// Raw values for the Organization JSON-LD script; URL assembly stays in the partial.
    /// </summary>
    public class JsonLdOrganizationModel
    {
        public string CompanyName { get; set; }
        public string LogoSetting { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
