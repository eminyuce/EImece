namespace EImece.Domain.Core.Payments;

public interface IIyzicoPaymentService
{
    bool IsConfigured { get; }
    string BaseUrl { get; }
    Task<CheckoutInitializeResult> InitializeCheckoutFormAsync(
        CheckoutInitializeRequest request,
        CancellationToken cancellationToken = default);
    Task<CheckoutRetrieveResult> RetrieveCheckoutFormAsync(
        string token,
        CancellationToken cancellationToken = default);
}
