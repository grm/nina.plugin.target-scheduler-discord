using FluentAssertions;
using NINA.Plugin.TargetScheduler.Discord.Features.instruction;
using NINA.Plugin.TargetScheduler.Discord.Features.settings;
using NINA.Plugin.TargetScheduler.Discord.Models;
using NINA.Plugin.TargetScheduler.Discord.Services;
using NUnit.Framework;
using System.IO;

namespace NINA.Plugin.TargetScheduler.Discord.Tests;

public class TargetStatusPublisherTests {
    private string settingsDirectory;
    private RecordingTargetSchedulerClient targetScheduler;
    private RecordingDiscordClient discord;

    [SetUp]
    public void SetUp() {
        settingsDirectory = Path.Combine(Path.GetTempPath(), "TargetSchedulerDiscordTests", Guid.NewGuid().ToString("N"));
        targetScheduler = new RecordingTargetSchedulerClient();
        discord = new RecordingDiscordClient();
    }

    [TearDown]
    public void TearDown() {
        if (Directory.Exists(settingsDirectory)) {
            Directory.Delete(settingsDirectory, true);
        }
    }

    [Test]
    public async Task Sequencer_instance_loads_saved_options_without_manifest_initialization() {
        using var options = new PluginSettings(settingsDirectory);
        options.Initialize();
        options.DiscordWebhookUrl = "https://example.invalid/webhooks/first";
        options.TargetSchedulerApiBaseUrl = "http://localhost:9191/ts/v0";
        options.IncludeInactiveTargets = true;

        // A different MEF container gets a different PluginSettings instance.
        using var sequencerSettings = new PluginSettings(settingsDirectory);
        var count = await TargetStatusPublisher.PublishAsync(
            sequencerSettings, CancellationToken.None, targetScheduler, discord);

        count.Should().Be(1);
        discord.WebhookUrls.Should().Equal(options.DiscordWebhookUrl);
        targetScheduler.Requests.Should().Equal((options.TargetSchedulerApiBaseUrl, true));
    }

    [Test]
    public async Task Every_send_picks_up_options_edits_after_a_previous_send() {
        using var options = new PluginSettings(settingsDirectory);
        options.Initialize();
        options.DiscordWebhookUrl = "https://example.invalid/webhooks/first";
        using var sequencerSettings = new PluginSettings(settingsDirectory);
        sequencerSettings.Initialize();

        await TargetStatusPublisher.PublishAsync(sequencerSettings, CancellationToken.None, targetScheduler, discord);
        options.DiscordWebhookUrl = "https://example.invalid/webhooks/second";
        options.TargetSchedulerApiBaseUrl = "http://localhost:9292/ts/v0";
        options.IncludeInactiveTargets = true;
        await TargetStatusPublisher.PublishAsync(sequencerSettings, CancellationToken.None, targetScheduler, discord);

        discord.WebhookUrls.Should().Equal(
            "https://example.invalid/webhooks/first", "https://example.invalid/webhooks/second");
        targetScheduler.Requests.Should().Equal(
            ("http://localhost:8188/ts/v0", false), ("http://localhost:9292/ts/v0", true));
    }

    [Test]
    public async Task Cloned_instruction_uses_latest_saved_options_and_propagates_send_failure() {
        using var options = new PluginSettings(settingsDirectory);
        options.Initialize();
        options.DiscordWebhookUrl = "https://example.invalid/webhooks/first";
        using var sequencerSettings = new PluginSettings(settingsDirectory);
        sequencerSettings.Initialize();
        var instruction = new SendTargetStatusInstruction(sequencerSettings, targetScheduler, discord) {
            Name = "TS: Send target status to Discord", Category = "Target Scheduler Discord", Attempts = 2
        };
        var clone = (SendTargetStatusInstruction)instruction.Clone();
        options.DiscordWebhookUrl = "https://example.invalid/webhooks/second";
        options.TargetSchedulerApiBaseUrl = "http://localhost:9393/ts/v0";
        options.IncludeInactiveTargets = true;
        var failure = new InvalidOperationException("Simulated Discord failure");
        discord.Failure = failure;

        Func<Task> execute = () => clone.Execute(null, CancellationToken.None);
        (await execute.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(failure);

        discord.WebhookUrls.Should().Equal(options.DiscordWebhookUrl);
        targetScheduler.Requests.Should().Equal((options.TargetSchedulerApiBaseUrl, true));
        clone.Name.Should().Be(instruction.Name);
        clone.Category.Should().Be(instruction.Category);
        clone.Attempts.Should().Be(2);
    }

    [Test]
    public async Task Cancelled_send_does_not_call_either_client() {
        using var settings = new PluginSettings(settingsDirectory);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Func<Task> send = () => TargetStatusPublisher.PublishAsync(settings, cancellation.Token, targetScheduler, discord);
        await send.Should().ThrowAsync<OperationCanceledException>();

        targetScheduler.Requests.Should().BeEmpty();
        discord.WebhookUrls.Should().BeEmpty();
    }

    private sealed class RecordingTargetSchedulerClient : ITargetSchedulerClient {
        public List<(string ApiBaseUrl, bool IncludeInactive)> Requests { get; } = new();

        public Task<IReadOnlyList<TargetProgress>> GetTargetProgressAsync(
            string apiBaseUrl, bool includeInactiveTargets, CancellationToken token) {
            Requests.Add((apiBaseUrl, includeInactiveTargets));
            IReadOnlyList<TargetProgress> targets = new[] {
                new TargetProgress { ProjectName = "Test project", TargetName = "Test target", Active = true }
            };
            return Task.FromResult(targets);
        }
    }

    private sealed class RecordingDiscordClient : IDiscordWebhookClient {
        public List<string> WebhookUrls { get; } = new();
        public Exception Failure { get; set; }

        public Task SendContentAsync(string webhookUrl, string content, CancellationToken token, int embedColor = 0x5865F2) {
            WebhookUrls.Add(webhookUrl);
            if (string.IsNullOrWhiteSpace(webhookUrl)) {
                throw new InvalidOperationException("Discord webhook URL is not configured.");
            }

            return Failure == null ? Task.CompletedTask : Task.FromException(Failure);
        }
    }
}
