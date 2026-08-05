using EImece.Domain.Core.Media;

namespace EImece.Domain.Core.Captcha;

public sealed class CaptchaChallengeService : ICaptchaChallengeService
{
    private readonly IImageProcessingService _images;

    public CaptchaChallengeService(IImageProcessingService images)
    {
        _images = images;
    }

    public CaptchaChallenge CreateArithmeticChallenge(bool includeNoise = true)
    {
        var a = Random.Shared.Next(1, 5);
        var b = Random.Shared.Next(1, 5);
        var question = $"{a} + {b} = ?";
        var image = _images.CreateCaptchaImage(question, includeNoise);
        return new CaptchaChallenge
        {
            Question = question,
            Answer = a + b,
            ImageBytes = image.Bytes,
            ContentType = image.ContentType
        };
    }
}
