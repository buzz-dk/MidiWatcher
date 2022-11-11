using MidiWatcher.Contracts.Services;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace MidiWatcher.Services;

public class DeviceService : IDeviceService
{
    private readonly string _midiInputQueryString = MidiInPort.GetDeviceSelector();
    private bool inputEnumerationCompleted = false;

    private DeviceInformationCollection? _midiInputDevices;
    public DeviceInformationCollection? MidiInputDevices
    {
        get => _midiInputDevices;
        set => _midiInputDevices = value;
    }

    private readonly string _midiOutputQueryString = MidiOutPort.GetDeviceSelector();
    private bool outputEnumerationCompleted = false;

    private DeviceInformationCollection? _midiOutputDevices;
    public DeviceInformationCollection? MidiOutputDevices
    {
        get => _midiOutputDevices;
        set => _midiOutputDevices = value;
    }

    public async Task InitializeAsync()
    {
        _midiInDeviceWatcher = DeviceInformation.CreateWatcher(_midiInputQueryString);
        _midiInDeviceWatcher.Added += InputsChanged;
        _midiInDeviceWatcher.Removed += InputsChanged;
        _midiInDeviceWatcher.Updated += InputsChanged;
        _midiInDeviceWatcher.EnumerationCompleted += InputsEnumerationCompleted;

        _midiInDeviceWatcher.Start();

        _midiOutDeviceWatcher = DeviceInformation.CreateWatcher(_midiOutputQueryString);
        _midiOutDeviceWatcher.Added += OutputsChanged;
        _midiOutDeviceWatcher.Removed += OutputsChanged;
        _midiOutDeviceWatcher.Updated += OutputsChanged;
        _midiOutDeviceWatcher.EnumerationCompleted += OutputsEnumerationCompleted;

        _midiOutDeviceWatcher.Start();

        await Task.CompletedTask; // Not really doing anything... https://stackoverflow.com/questions/44096253/await-task-completedtask-for-what
    }

    internal DeviceWatcher? _midiInDeviceWatcher;
    internal DeviceWatcher? _midiOutDeviceWatcher;

    private async void InputsChanged(DeviceWatcher sender, object args)
    {
        // If all devices have been enumerated
        if (inputEnumerationCompleted)
        {
            UpdateInputDevices();
        }
        await Task.CompletedTask;
    }

    private async void InputsEnumerationCompleted(DeviceWatcher sender, object args)
    {
        inputEnumerationCompleted = true;
        UpdateInputDevices();
        await Task.CompletedTask;
    }

    private async void OutputsChanged(DeviceWatcher sender, object args)
    {
        // If all devices have been enumerated
        if (outputEnumerationCompleted)
        {
            UpdateOutputDevices();
        }
        await Task.CompletedTask;
    }

    private async void OutputsEnumerationCompleted(DeviceWatcher sender, object args)
    {
        outputEnumerationCompleted = true;
        UpdateOutputDevices();
        await Task.CompletedTask;
    }

    private async void UpdateInputDevices()
    {
        // Get a list of all MIDI devices
        _midiInputDevices = await DeviceInformation.FindAllAsync(_midiInputQueryString);
        InputsUpdated.Invoke(this, EventArgs.Empty);
    }

    private async void UpdateOutputDevices()
    {
        // Get a list of all MIDI devices
        _midiOutputDevices = await DeviceInformation.FindAllAsync(_midiOutputQueryString);
        OutputsUpdated.Invoke(this, EventArgs.Empty);
    }

    ~DeviceService()
    {
        if (_midiInDeviceWatcher != null)
        {
            if (_midiInDeviceWatcher.Status != DeviceWatcherStatus.Stopped)
            {
                _midiInDeviceWatcher.Stop();
            }
            _midiInDeviceWatcher.Added -= InputsChanged;
            _midiInDeviceWatcher.Removed -= InputsChanged;
            _midiInDeviceWatcher.Updated -= InputsChanged;
            _midiInDeviceWatcher.EnumerationCompleted -= InputsEnumerationCompleted;
        }
        if (_midiOutDeviceWatcher != null)
        {
            if (_midiOutDeviceWatcher.Status != DeviceWatcherStatus.Stopped)
            {
                _midiOutDeviceWatcher.Stop();
            }
            _midiOutDeviceWatcher.Added -= OutputsChanged;
            _midiOutDeviceWatcher.Removed -= OutputsChanged;
            _midiOutDeviceWatcher.Updated -= OutputsChanged;
            _midiOutDeviceWatcher.EnumerationCompleted -= OutputsEnumerationCompleted;
        }
    }

    public event EventHandler InputsUpdated = delegate { };
    public event EventHandler OutputsUpdated = delegate { };
}
