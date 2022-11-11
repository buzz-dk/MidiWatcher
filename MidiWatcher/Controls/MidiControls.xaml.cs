using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MidiWatcher.Models;

// User Control:
// Show received Midi CC numbers

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MidiWatcher.Controls;

public sealed partial class MidiControls : UserControl
{
    public MidiControls()
    {
        InitializeComponent();
    }

    private static DependencyProperty Register<T>(string propertyName)
    {
        return DependencyProperty.Register(propertyName, typeof(T), typeof(MidiControls), new PropertyMetadata(0));
    }

    //private static DependencyProperty Register<T>(string propertyName, object defaultValue)
    //{
    //    return DependencyProperty.Register(propertyName, typeof(T), typeof(MidiControls), new PropertyMetadata((T)defaultValue));
    //}

    // Repository for displayed sliders:
    private readonly SortedDictionary<int, Slider> Sliders = new();

    // Receive MidiCC:
    public ControlMessage ControlMsg
    {
        get => (ControlMessage)GetValue(ControlMsgProperty);
        set
        {
            //SetValue(ControlMsgProperty, value);
            if (value != null)
            {
                if (!Sliders.ContainsKey(value.Control))
                {
                    Sliders.Add(
                        value.Control,
                        new Slider()
                        {
                            Value = value.Value,
                            Header = GetCCString(value.Control),
                            Height = 100,
                            Margin = new Thickness(0, 0, 5, 0),
                            Orientation = Orientation.Vertical,
                            Maximum = Math.Max(value.Value, 127),
                            Minimum = 0
                        });
                    Panel.Children.Clear();
                    foreach (var slider in Sliders.Values)
                    {
                        Panel.Children.Add(slider);
                    }
                    SetVisibility(true);
                }
                else { Sliders[value.Control].Value = value.Value; }

            }
        }
    }

    public static readonly DependencyProperty ControlMsgProperty = Register<ControlMessage>(nameof(ControlMsg));

    // TODO: Move to settings (json) file...
    private enum MidiCC { Bank = 0, ModW = 1, Main = 7, Expr = 11, Sust = 64, Soft = 67 /* etc. */};

    private static string GetCCString(int cc)
    {
        if (Enum.IsDefined(typeof(MidiCC), cc))
        {
            return ((MidiCC)cc).ToString("F");
        }
        return "CC" + cc.ToString();
    }

    private void ButtonClear_Click(object sender, RoutedEventArgs e)
    {
        Panel.Children.Clear();
        Sliders.Clear();
        SetVisibility(false);
    }

    private void SetVisibility(bool visible)
    {
        var visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ButtonClear.Visibility = visibility;
    }
}
