using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using MidiWatcher.Helpers;

namespace MidiWatcher.ViewModels;

public sealed class AboutViewModel : ObservableRecipient
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public AboutViewModel()
    {
    }

    private string? _markdownText;

    // An asynchronous property!
    // https://techhelpnotes.com/c-how-to-call-an-async-method-from-a-getter-or-setter/

    public string? MarkdownText
    {
        get
        {
            if (_markdownText == null)
            {
                var directory = AppDomain.CurrentDomain.BaseDirectory;
                var file = @"Assets\" + "markdown_file".GetLocalized();
                UiInvoke(async () => { MarkdownText = await GetTextFile(Path.Combine(directory, file)); });
                // Program continues - the setter is called when the file is loaded - setter notifies the UI
            }
            return _markdownText;
        }
        set => SetProperty(ref _markdownText, value);
    }

    private static async Task<string> GetTextFile(string path)
    {
        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch (Exception)
        {
            return "# " + "Error_File_Not_Found".GetLocalized();
        }
    }

    // Dispatcher:
    private void UiInvoke(Action code) => _dispatcherQueue.TryEnqueue(() => { code(); });
}
