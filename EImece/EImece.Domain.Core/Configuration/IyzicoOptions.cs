namespace EImece.Domain.Core.Configuration;

public sealed class IyzicoOptions
{
    public const string SectionName = "Iyzico";

    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string EnabledInstallments { get; set; } = "1,2,4,6,9";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(SecretKey);
}
