using NINA.Core.Utility;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.Json;

namespace NINA.Plugin.TargetScheduler.Discord.Features.settings;

[Export(typeof(PluginSettings))]
[PartCreationPolicy(CreationPolicy.Shared)]
public sealed class PluginSettings : BaseINPC, IDisposable {
    private const string DefaultApiBaseUrl = "http://localhost:8188/ts/v0";

    private readonly string settingsDirectory;
    private readonly string settingsPath;
    private StoredSettings stored = new();
    private bool initialized;

    [ImportingConstructor]
    public PluginSettings() {
        var id = new Plugin().Identifier;
        settingsDirectory = Path.Combine(CoreUtil.APPLICATIONTEMPPATH, "PluginSettings", id);
        settingsPath = Path.Combine(settingsDirectory, "settings.json");
    }

    public void Initialize() {
        if (initialized) {
            return;
        }

        if (File.Exists(settingsPath)) {
            try {
                stored = JsonSerializer.Deserialize<StoredSettings>(File.ReadAllText(settingsPath)) ?? new StoredSettings();
            } catch (Exception ex) {
                Logger.Error("Failed to load Target Scheduler Discord settings", ex);
                stored = new StoredSettings();
            }
        }

        initialized = true;
        RaisePropertyChanged(nameof(DiscordWebhookUrl));
        RaisePropertyChanged(nameof(TargetSchedulerApiBaseUrl));
        RaisePropertyChanged(nameof(IncludeInactiveTargets));
    }

    public string DiscordWebhookUrl {
        get => stored.DiscordWebhookUrl ?? string.Empty;
        set {
            if (stored.DiscordWebhookUrl == value) {
                return;
            }

            stored.DiscordWebhookUrl = value?.Trim() ?? string.Empty;
            Persist();
            RaisePropertyChanged();
        }
    }

    public string TargetSchedulerApiBaseUrl {
        get => string.IsNullOrWhiteSpace(stored.TargetSchedulerApiBaseUrl)
            ? DefaultApiBaseUrl
            : stored.TargetSchedulerApiBaseUrl;
        set {
            var normalized = string.IsNullOrWhiteSpace(value) ? DefaultApiBaseUrl : value.Trim().TrimEnd('/');
            if (stored.TargetSchedulerApiBaseUrl == normalized) {
                return;
            }

            stored.TargetSchedulerApiBaseUrl = normalized;
            Persist();
            RaisePropertyChanged();
        }
    }

    public bool IncludeInactiveTargets {
        get => stored.IncludeInactiveTargets;
        set {
            if (stored.IncludeInactiveTargets == value) {
                return;
            }

            stored.IncludeInactiveTargets = value;
            Persist();
            RaisePropertyChanged();
        }
    }

    public void Dispose() {
        initialized = false;
    }

    private void Persist() {
        Directory.CreateDirectory(settingsDirectory);
        var temporaryPath = settingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(stored, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporaryPath, settingsPath, true);
    }

    private sealed class StoredSettings {
        public string DiscordWebhookUrl { get; set; } = string.Empty;
        public string TargetSchedulerApiBaseUrl { get; set; } = DefaultApiBaseUrl;
        public bool IncludeInactiveTargets { get; set; }
    }
}
