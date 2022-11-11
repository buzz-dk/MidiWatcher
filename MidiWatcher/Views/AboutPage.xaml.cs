using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using MidiWatcher.ViewModels;
using Windows.System;

namespace MidiWatcher.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel
    {
        get;
    }

    public AboutPage()
    {
        ViewModel = App.GetService<AboutViewModel>();
        InitializeComponent();
    }

    private async void MarkdownTextLinkClicked(object sender, LinkClickedEventArgs e)
    {
        if (Uri.TryCreate(e.Link, UriKind.Absolute, out var link))
        {
            await Launcher.LaunchUriAsync(link);
        }
    }
}
