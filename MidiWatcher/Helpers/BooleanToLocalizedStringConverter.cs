using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Windows.ApplicationModel.Resources;

namespace MidiWatcher.Helpers;

public class BooleanToLocalizedStringConverter : IValueConverter
{
    public BooleanToLocalizedStringConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (((bool)value ? "Xaml_Yes/Text".GetLocalized() : "Xaml_No/Text".GetLocalized()));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new ArgumentException("Not Implemented");
    }
}
