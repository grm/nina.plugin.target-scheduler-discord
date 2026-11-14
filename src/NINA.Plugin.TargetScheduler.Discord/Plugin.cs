using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Plugin.TargetScheduler.Discord.Features.settings;
using System.ComponentModel.Composition;

namespace NINA.Plugin.TargetScheduler.Discord;

[Export(typeof(IPluginManifest))]
public class Plugin : PluginBase {
    [Import]
    public PluginSettings Settings { get; set; }

    public override Task Initialize() {
        Settings.Initialize();
        return Task.CompletedTask;
    }

    public override Task Teardown() {
        Settings.Dispose();
        return Task.CompletedTask;
    }
}
