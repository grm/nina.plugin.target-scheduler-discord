using NINA.Core.Utility;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace NINA.Plugin.TargetScheduler.Discord.Services;

public interface IDiscordWebhookClient {
    Task SendContentAsync(string webhookUrl, string content, CancellationToken token);
}

public sealed class DiscordWebhookClient : IDiscordWebhookClient {
    private static readonly HttpClient Http = new() {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public async Task SendContentAsync(string webhookUrl, string content, CancellationToken token) {
        if (string.IsNullOrWhiteSpace(webhookUrl)) {
            throw new InvalidOperationException("Discord webhook URL is not configured.");
        }

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)) {
            throw new InvalidOperationException("Discord webhook URL is invalid.");
        }

        foreach (var chunk in TargetProgressFormatter.SplitForDiscord(content)) {
            token.ThrowIfCancellationRequested();
            var payload = JsonSerializer.Serialize(new { content = chunk });
            using var request = new HttpRequestMessage(HttpMethod.Post, uri) {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await Http.SendAsync(request, token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) {
                var body = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
                Logger.Error($"Discord webhook failed: {(int)response.StatusCode} {response.ReasonPhrase} {body}");
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
