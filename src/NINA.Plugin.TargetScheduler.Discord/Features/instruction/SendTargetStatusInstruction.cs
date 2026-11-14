using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Plugin.TargetScheduler.Discord.Features.settings;
using NINA.Plugin.TargetScheduler.Discord.Services;
using NINA.Sequencer.SequenceItem;
using System.ComponentModel.Composition;

namespace NINA.Plugin.TargetScheduler.Discord.Features.instruction;

[Export(typeof(ISequenceItem))]
[ExportMetadata("Name", "TS: Send target status to Discord")]
[ExportMetadata("Description", "Posts Target Scheduler progress for each active target to Discord (one line per target, sorted by completion %).")]
[ExportMetadata("Category", "Target Scheduler Discord")]
[ExportMetadata("Icon", "MessageSVG")]
[JsonObject(MemberSerialization.OptIn)]
public class SendTargetStatusInstruction : SequenceItem {
    private readonly PluginSettings settings;
    private readonly ITargetSchedulerClient targetSchedulerClient;
    private readonly IDiscordWebhookClient discordWebhookClient;

    [ImportingConstructor]
    public SendTargetStatusInstruction(PluginSettings settings)
        : this(settings, new TargetSchedulerClient(), new DiscordWebhookClient()) {
    }

    public SendTargetStatusInstruction(
        PluginSettings settings,
        ITargetSchedulerClient targetSchedulerClient,
        IDiscordWebhookClient discordWebhookClient) {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        this.targetSchedulerClient = targetSchedulerClient ?? throw new ArgumentNullException(nameof(targetSchedulerClient));
        this.discordWebhookClient = discordWebhookClient ?? throw new ArgumentNullException(nameof(discordWebhookClient));
    }

    private SendTargetStatusInstruction(SendTargetStatusInstruction source)
        : base(source) {
        settings = source.settings;
        targetSchedulerClient = source.targetSchedulerClient;
        discordWebhookClient = source.discordWebhookClient;
    }

    public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
        token.ThrowIfCancellationRequested();

        progress?.Report(new ApplicationStatus { Status = "Fetching Target Scheduler progress..." });
        var targets = await targetSchedulerClient.GetTargetProgressAsync(
            settings.TargetSchedulerApiBaseUrl,
            settings.IncludeInactiveTargets,
            token).ConfigureAwait(false);

        var message = TargetProgressFormatter.FormatMessage(targets, DateTimeOffset.UtcNow);
        Logger.Info($"Sending Target Scheduler Discord status for {targets.Count} target(s).");

        progress?.Report(new ApplicationStatus { Status = "Sending Discord webhook..." });
        await discordWebhookClient.SendContentAsync(settings.DiscordWebhookUrl, message, token).ConfigureAwait(false);

        Notification.ShowSuccess($"Discord: {targets.Count} target(s) posted");
        progress?.Report(new ApplicationStatus { Status = string.Empty });
    }

    public override object Clone() => new SendTargetStatusInstruction(this);

    public override string ToString() {
        return $"Category: {Category}, Item: {nameof(SendTargetStatusInstruction)}";
    }
}
