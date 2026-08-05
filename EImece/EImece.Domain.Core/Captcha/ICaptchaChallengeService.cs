namespace EImece.Domain.Core.Captcha;

public sealed class CaptchaChallenge
{
    public required string Question { get; init; }
    public required int Answer { get; init; }
    public required byte[] ImageBytes { get; init; }
    public string ContentType { get; init; } = "image/jpeg";
}

public interface ICaptchaChallengeService
{
    CaptchaChallenge CreateArithmeticChallenge(bool includeNoise = true);
}
