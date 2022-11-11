using Windows.Devices.Enumeration;

namespace MidiWatcher.Contracts.Services;

public interface IDeviceService
{
    DeviceInformationCollection? MidiInputDevices
    {
        get;
        set;
    }

    DeviceInformationCollection? MidiOutputDevices
    {
        get;
        set;
    }

    Task InitializeAsync();

    public event EventHandler InputsUpdated;
    public event EventHandler OutputsUpdated;
}