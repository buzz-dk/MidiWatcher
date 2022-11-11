using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// https://stackoverflow.com/questions/9460034/custom-itemssource-property-for-a-usercontrol

namespace MidiWatcher.Controls;

public sealed partial class MidiMessages : UserControl
{
    public MidiMessages()
    {
        InitializeComponent();
    }

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable),
            typeof(MidiMessages), new PropertyMetadata(null));
}
