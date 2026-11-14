using NINA.Core.Utility;
using NINA.Plugin.TargetScheduler.Discord.Features.settings;

namespace NINA.Plugin.TargetScheduler.Discord.Services;

public static class TargetStatusPublisher {
    public static async Task<int> PublishAsync(
        PluginSettings settings,
        CancellationToken token,
        ITargetSchedulerClient targetSchedulerClient = null,
        IDiscordWebhookClient discordWebhookClient = null) {
        if (settings == null) {
            throw new ArgumentNullException(nameof(settings));
        }

        targetSchedulerClient ??= new TargetSchedulerClient();
        discordWebhookClient ??= new DiscordWebhookClient();

        var targets = await targetSchedulerClient.GetTargetProgressAsync(
            settings.TargetSchedulerApiBaseUrl,
            settings.IncludeInactiveTargets,
            token).ConfigureAwait(false);

        var message = TargetProgressFormatter.FormatMessage(targets, DateTimeOffset.UtcNow);
        Logger.Info($"Sending Target Scheduler Discord status for {targets.Count} target(s).");
        await discordWebhookClient.SendContentAsync(settings.DiscordWebhookUrl, message, token).ConfigureAwait(false);
        return targets.Count;
    }
}
