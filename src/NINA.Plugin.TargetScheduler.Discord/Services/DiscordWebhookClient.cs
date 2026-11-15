using NINA.Core.Utility;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace NINA.Plugin.TargetScheduler.Discord.Services;

public interface IDiscordWebhookClient {
    Task SendContentAsync(string webhookUrl, string content, CancellationToken token, int embedColor = 0x5865F2);
}

public sealed class DiscordWebhookClient : IDiscordWebhookClient {
    private static readonly HttpClient Http = new() {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public async Task SendContentAsync(
        string webhookUrl,
        string content,
        CancellationToken token,
        int embedColor = 0x5865F2) {
        if (string.IsNullOrWhiteSpace(webhookUrl)) {
            throw new InvalidOperationException("Discord webhook URL is not configured.");
        }

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)) {
            throw new InvalidOperationException("Discord webhook URL is invalid.");
        }

        var chunks = TargetProgressFormatter.SplitForDiscord(content);
        for (var index = 0; index < chunks.Count; index++) {
            token.ThrowIfCancellationRequested();

            var title = index == 0
                ? TargetProgressFormatter.EmbedTitle
                : $"{TargetProgressFormatter.EmbedTitle} ({index + 1}/{chunks.Count})";

            var description = StripEmbeddedTitle(chunks[index]);

            var payload = JsonSerializer.Serialize(new {
                embeds = new[] {
                    new {
                        title,
                        description = string.IsNullOrWhiteSpace(description) ? "_No details._" : description,
                        color = embedColor,
                        timestamp = DateTime.UtcNow.ToString("o")
                    }
                }
            });

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

    private static string StripEmbeddedTitle(string description) {
        var prefixes = new[] {
            $"**{TargetProgressFormatter.EmbedTitle}**",
            TargetProgressFormatter.EmbedTitle,
            "**Target Scheduler progress**",
            "Target Scheduler progress"
        };

        foreach (var prefix in prefixes) {
            if (!description.StartsWith(prefix, StringComparison.Ordinal)) {
                continue;
            }

            var firstNewline = description.IndexOf('\n');
            return firstNewline >= 0 ? description[(firstNewline + 1)..].TrimStart() : string.Empty;
        }

        return description;
    }
}
