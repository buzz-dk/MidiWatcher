using Microsoft.UI.Xaml.Controls;

using MidiWatcher.ViewModels;

namespace MidiWatcher.Views;

public sealed partial class DevicesPage : Page
{
    public DevicesViewModel ViewModel
    {
        get;
    }

    public DevicesPage()
    {
        ViewModel = App.GetService<DevicesViewModel>();
        InitializeComponent();
    }
}
