using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using MidiWatcher.Contracts.Services;
using MidiWatcher.Models;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.Storage.Streams;

namespace MidiWatcher.ViewModels;

public class DevicesViewModel : ObservableRecipient
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private readonly IDeviceService _deviceService;

    private readonly ObservableCollection<DeviceInformation> _inPutDevices = new();
    public ObservableCollection<DeviceInformation> InputDevices => _inPutDevices;

    private readonly ObservableCollection<DeviceInformation> _selectedInputDevices = new();
    public ObservableCollection<DeviceInformation> SelectedInputDevices => _selectedInputDevices;

    private MidiInPort? _currentMidiInPort;
    internal MidiInPort? CurrentMidiInPort
    {
        get => _currentMidiInPort;
        set
        {
            _currentMidiInPort = value;
            if (_currentMidiInPort != null)
            {
                _currentMidiInPort.MessageReceived += MidiMessageReceived;
            }
        }
    }

    private DeviceInformation? _selectedInputDevice;
    public DeviceInformation? SelectedInputDevice
    {
        get => _selectedInputDevice;
        set
        {
            _currentMidiInPort?.Dispose();
            _selectedInputDevice = value;
            _selectedInputDevices.Clear();
            if (value is DeviceInformation device)
            {
                _selectedInputDevices.Add(device);
                ((Action)(async () => { CurrentMidiInPort = await MidiInPort.FromIdAsync(device.Id); })).Invoke();
            }
            OnPropertyChanged(nameof(PassThruEnabled));
        }

    }

    private readonly ObservableCollection<DeviceInformation> _outPutDevices = new();
    public ObservableCollection<DeviceInformation> OutputDevices => _outPutDevices;

    private readonly ObservableCollection<DeviceInformation> _selectedOutputDevices = new();
    public ObservableCollection<DeviceInformation> SelectedOutputDevices => _selectedOutputDevices;

    private IMidiOutPort? _currentMidiOutPort;

    private DeviceInformation? _selectedOutputDevice;
    public DeviceInformation? SelectedOutputDevice
    {
        get => _selectedOutputDevice;
        set
        {
            _currentMidiOutPort?.Dispose();
            _selectedOutputDevice = value;
            _selectedOutputDevices.Clear();
            if (value is DeviceInformation device)
            {
                _selectedOutputDevices.Add(device);
                ((Action)(async () => { _currentMidiOutPort = await MidiOutPort.FromIdAsync(device.Id); })).Invoke();
            }
            OnPropertyChanged(nameof(PassThruEnabled));
        }
    }

    public DevicesViewModel(IDeviceService deviceService)
    {
        _deviceService = deviceService;
        if (_deviceService.MidiInputDevices is DeviceInformationCollection inputDevices)
        {
            foreach (var inputDevice in inputDevices)
            {
                _inPutDevices.Add(inputDevice);
            }
        }
        _deviceService.InputsUpdated += UpdateInputDevices;

        if (_deviceService.MidiOutputDevices is DeviceInformationCollection outputDevices)
        {
            foreach (var outputDevice in outputDevices)
            {
                _outPutDevices.Add(outputDevice);
            }
        }
        _deviceService.OutputsUpdated += UpdateOutputDevices;

        ClearMessagesCommand = new RelayCommand(OnClearMessages);

    }

    private void OnClearMessages()
    {
        UiInvoke(() =>
        {
            _inputDeviceMessages.Clear();
            InputDeviceMessagesButtonVisible = false;
        });
    }

    public void UpdateInputDevices(object? sender, EventArgs e)
    {
        UiInvoke(() =>
        {
            _inPutDevices.Clear();
            if (_deviceService.MidiInputDevices is DeviceInformationCollection devices)
            {
                foreach (var device in devices)
                {
                    _inPutDevices.Add(device);
                }
            }
        });
    }

    public void UpdateOutputDevices(object? sender, EventArgs e)
    {
        UiInvoke(() =>
        {
            _outPutDevices.Clear();
            if (_deviceService.MidiOutputDevices is DeviceInformationCollection devices)
            {
                foreach (var device in devices)
                {
                    _outPutDevices.Add(device);
                }
            }
        });
    }

    // Toggle Switch:
    private bool _passThru = true;
    public bool PassThru
    {
        get => _passThru;
        set => _passThru = value;
    }

    public bool PassThruEnabled => _selectedInputDevice != null && _selectedOutputDevice != null;

    // Dispatcher:
    private void UiInvoke(System.Action code)
    {
        _dispatcherQueue.TryEnqueue(() => { code(); });
    }

    ~DevicesViewModel()
    {
        _deviceService.InputsUpdated -= UpdateInputDevices;
        _deviceService.OutputsUpdated += UpdateOutputDevices;
    }

    private void MidiMessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
    {
        var receivedMidiMessage = args.Message;
        if (_passThru) { SendMessage(receivedMidiMessage); }

        // Build the received MIDI message into a readable format
        StringBuilder outputMessage = new();
        outputMessage.Append(receivedMidiMessage.Timestamp.ToString()).Append(", Type: ").Append(receivedMidiMessage.Type);

        // Add MIDI message parameters to the output, depending on the type of message
        switch (receivedMidiMessage.Type)
        {
            case MidiMessageType.NoteOff:
                var noteOffMessage = (MidiNoteOffMessage)receivedMidiMessage;
                outputMessage.Append(", Channel: ").Append(noteOffMessage.Channel).Append(", Note: ").Append(noteOffMessage.Note).Append(", Velocity: ").Append(noteOffMessage.Velocity);
                break;
            case MidiMessageType.NoteOn:
                var noteOnMessage = (MidiNoteOnMessage)receivedMidiMessage;
                outputMessage.Append(", Channel: ").Append(noteOnMessage.Channel).Append(", Note: ").Append(noteOnMessage.Note).Append(", Velocity: ").Append(noteOnMessage.Velocity);
                break;
            case MidiMessageType.PolyphonicKeyPressure:
                var polyphonicKeyPressureMessage = (MidiPolyphonicKeyPressureMessage)receivedMidiMessage;
                outputMessage.Append(", Channel: ").Append(polyphonicKeyPressureMessage.Channel).Append(", Note: ").Append(polyphonicKeyPressureMessage.Note).Append(", Pressure: ").Append(polyphonicKeyPressureMessage.Pressure);
                break;
            case MidiMessageType.ControlChange:
                var controlChangeMessage = (MidiControlChangeMessage)receivedMidiMessage;
                outputMessage.Append(", Channel: ").Append(controlChangeMessage.Channel).Append(", Controller: ").Append(controlChangeMessage.Controller).Append(", Value: ").Append(controlChangeMessage.ControlValue);

                // Send CC msg to user control:
                UiInvoke(() =>
                {
                    ControlMsg = new() { Control = controlChangeMessage.Controller, Value = controlChangeMessage.ControlValue };
                });

                break;
            case MidiMessageType.ProgramChange:
                var programChangeMessage = (MidiProgramChangeMessage)receivedMidiMessage;
                outputMessage.Append(", Channel: ").Append(programChangeMessage.Channel).Append(", Program: ").Append(programChangeMessage.Program);
                break;
            case MidiMessageType.ChannelPressure:
                var channelPressureMessage = (MidiChannelPressureMessage)receivedMidiMessage;
                outputMessage.Append(", Channel: ").Append(channelPressureMessage.Channel).Append(", Pressure: ").Append(channelPressureMessage.Pressure);
                break;
            case MidiMessageType.PitchBendChange:
                var pitchBendChangeMessage = (MidiPitchBendChangeMessage)receivedMidiMessage;
                outputMessage.Append(", Channel: ").Append(pitchBendChangeMessage.Channel).Append(", Bend: ").Append(pitchBendChangeMessage.Bend);

                // Send PB msg to view:
                UiInvoke(() => { PBMsg = pitchBendChangeMessage.Bend; });

                break;
            case MidiMessageType.SystemExclusive:
                var systemExclusiveMessage = (MidiSystemExclusiveMessage)receivedMidiMessage;
                outputMessage.Append(", ");

                // Read the SysEx bufffer
                var sysExDataReader = DataReader.FromBuffer(systemExclusiveMessage.RawData);
                while (sysExDataReader.UnconsumedBufferLength > 0)
                {
                    var byteRead = sysExDataReader.ReadByte();
                    // Pad with leading zero if necessary
                    outputMessage.Append(byteRead.ToString("X2")).Append(' ');
                }
                break;
            case MidiMessageType.MidiTimeCode:
                var timeCodeMessage = (MidiTimeCodeMessage)receivedMidiMessage;
                outputMessage.Append(", FrameType: ").Append(timeCodeMessage.FrameType).Append(", Values: ").Append(timeCodeMessage.Values);
                break;
            case MidiMessageType.SongPositionPointer:
                var songPositionPointerMessage = (MidiSongPositionPointerMessage)receivedMidiMessage;
                outputMessage.Append(", Beats: ").Append(songPositionPointerMessage.Beats);
                break;
            case MidiMessageType.SongSelect:
                var songSelectMessage = (MidiSongSelectMessage)receivedMidiMessage;
                outputMessage.Append(", Song: ").Append(songSelectMessage.Song);
                break;
            case MidiMessageType.TuneRequest:
                var tuneRequestMessage = (MidiTuneRequestMessage)receivedMidiMessage;
                break;
            case MidiMessageType.TimingClock:
                var timingClockMessage = (MidiTimingClockMessage)receivedMidiMessage;
                break;
            case MidiMessageType.Start:
                var startMessage = (MidiStartMessage)receivedMidiMessage;
                break;
            case MidiMessageType.Continue:
                var continueMessage = (MidiContinueMessage)receivedMidiMessage;
                break;
            case MidiMessageType.Stop:
                var stopMessage = (MidiStopMessage)receivedMidiMessage;
                break;
            case MidiMessageType.ActiveSensing:
                var activeSensingMessage = (MidiActiveSensingMessage)receivedMidiMessage;
                break;
            case MidiMessageType.SystemReset:
                var systemResetMessage = (MidiSystemResetMessage)receivedMidiMessage;
                break;
            case MidiMessageType.None:
            // throw new InvalidOperationException();
            default:
                break;
        }

        // Skip TimingClock and ActiveSensing messages to avoid overcrowding the list. Commment this check out to see all messages
        if ((receivedMidiMessage.Type != MidiMessageType.TimingClock) && (receivedMidiMessage.Type != MidiMessageType.ActiveSensing))
        {
            UiInvoke(() =>
            {
                InputDeviceMessages.Add(outputMessage.ToString());
                InputDeviceMessagesButtonVisible = true;
            });
        }
    }

    private readonly ObservableCollection<string> _inputDeviceMessages = new();
    public ObservableCollection<string> InputDeviceMessages => _inputDeviceMessages;

    private bool _inputDeviceMessagesButtonVisible;

    public bool InputDeviceMessagesButtonVisible
    {
        get => _inputDeviceMessagesButtonVisible;
        set => SetProperty(ref _inputDeviceMessagesButtonVisible, value);
    }

    private void SendMessage(IMidiMessage midiMessage)
    {
        _currentMidiOutPort?.SendMessage(midiMessage);
    }

    // Passing data to User control (MidiControls):
    private ControlMessage? _controlMsg;
    public ControlMessage? ControlMsg
    {
        get => _controlMsg;
        set => SetProperty(ref _controlMsg, value);
    }

    // Passing data to Pitch Bend slider:
    private int _pbMsg = 8192;
    public int PBMsg
    {
        get => _pbMsg;
        set => SetProperty(ref _pbMsg, value);
    }

    public RelayCommand ClearMessagesCommand
    {
        get; init;
    }
}
