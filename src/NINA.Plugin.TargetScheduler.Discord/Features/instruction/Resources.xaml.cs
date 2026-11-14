using System.ComponentModel.Composition;
using System.Windows;

namespace NINA.Plugin.TargetScheduler.Discord.Features.instruction;

[Export(typeof(ResourceDictionary))]
public partial class Resources : ResourceDictionary {
    public Resources() {
        InitializeComponent();
    }
}
