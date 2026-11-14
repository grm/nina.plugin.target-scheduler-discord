using System.ComponentModel.Composition;
using System.Windows;

namespace NINA.Plugin.TargetScheduler.Discord.Features.settings;

[Export(typeof(ResourceDictionary))]
public partial class Resources : ResourceDictionary {
    public Resources() {
        InitializeComponent();
        this[new Plugin().Name + "_Options"] = this["OptionsTemplate"];
        Remove("OptionsTemplate");
    }
}
