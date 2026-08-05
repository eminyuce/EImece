using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EImece.Domain.Core.Media;

namespace EImece.Web.Tests;

public sealed class SmokeTests : IClassFixture<EImeceWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SmokeTests(EImeceWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Health_ReturnsUpWithIntegrations()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;
        Assert.Equal("UP", root.GetProperty("status").GetString());
        Assert.True(root.TryGetProperty("integrations", out var integrations));
        Assert.Equal("SkiaSharp", integrations.GetProperty("images").GetProperty("engine").GetString());
        Assert.False(integrations.GetProperty("iyzico").GetProperty("configured").GetBoolean());
    }

    [Fact]
    public async Task DefaultImage_ReturnsJpeg()
    {
        var response = await _client.GetAsync("/images/defaultImage/w120h80/default.jpg");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/jpeg", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 100);
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xD8, bytes[1]);
    }

    [Fact]
    public async Task Captcha_ReturnsJpeg()
    {
        var response = await _client.GetAsync("/images/getcaptcha");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 50);
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xD8, bytes[1]);
    }

    [Fact]
    public async Task Home_ReturnsOk()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("EImece", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PaymentCheckout_ReturnsOk()
    {
        var response = await _client.GetAsync("/Payment/Checkout/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Checkout", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Iyzico", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmailTest_LogsOnlyInDevelopment()
    {
        var response = await _client.PostAsync("/api/integrations/email/test?to=phase9@eimece.local", content: null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(payload.GetProperty("sent").GetBoolean());
        Assert.True(payload.GetProperty("loggedOnly").GetBoolean());
    }

    [Fact]
    public async Task SecurityHeaders_ArePresent()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("SAMEORIGIN", response.Headers.GetValues("X-Frame-Options").Single());
    }

    [Theory]
    [InlineData(null, 150, 150)]
    [InlineData("w200h100", 200, 100)]
    [InlineData("w90", 90, 90)]
    public void ImageSizeParser_ParsesLegacyTokens(string? size, int width, int height)
    {
        var parsed = ImageSizeParser.Parse(size);
        Assert.Equal(width, parsed.Width);
        Assert.Equal(height, parsed.Height);
    }
}
