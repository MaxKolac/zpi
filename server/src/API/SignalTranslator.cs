using Newtonsoft.Json;
using System.Text;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;
using ZPIServer.API.CameraLibraries;
using ZPIServer.Commands;
using ZPIServer.EventArgs;
using static ZPICommunicationModels.Models.HostDevice;

namespace ZPIServer.API;

/// <summary>
/// Klasa, która subskrybuje wydarzenie <see cref="TcpReceiver.OnSignalReceived"/> i odpowiednio reaguje na jej inwokacje.
/// </summary>
public class SignalTranslator
{
    private readonly Logger? _logger;
    private readonly Dictionary<HostType, int> _invocationDictionary;
    private int _invocations = 0;

    /// <summary>
    /// Wskazuje czy <see cref="SignalTranslator"/> został uruchomiony i obsługuje inwokacje wydarzenia <see cref="TcpReceiver.OnSignalReceived"/>.
    /// </summary>
    public bool IsTranslating { get; private set; } = false;

    public SignalTranslator(Logger? logger = null)
    {
        _logger = logger;
        _invocationDictionary = new Dictionary<HostType, int>();
        foreach (var hostType in Enum.GetValues(typeof(HostType)))
            _invocationDictionary.Add((HostType)hostType, 0);

        Command.OnExecuted += ShowStatus;
    }

    ~SignalTranslator()
    {
        Command.OnExecuted -= ShowStatus;
    }

    /// <summary>
    /// Rozpoczyna pracę <see cref="SignalTranslator"/>.
    /// </summary>
    public void BeginTranslating()
    {
        if (IsTranslating)
            return;

        IsTranslating = true;
        TcpReceiver.OnSignalReceived += HandleReceivedSignal;
        _logger?.WriteLine("Starting up.", nameof(SignalTranslator));
    }

    /// <summary>
    /// Kończy pracę <see cref="SignalTranslator"/>.
    /// </summary>
    public void StopTranslating()
    {
        if (!IsTranslating)
            return;

        _logger?.WriteLine("Shutting down.", nameof(SignalTranslator));
        TcpReceiver.OnSignalReceived -= HandleReceivedSignal;
        IsTranslating = false;
    }

    void HandleReceivedSignal(object? sender, TcpReceiverEventArgs e)
    {
        _invocations++;
        HostDevice? datasender = null;
        using var context = new DatabaseContext();
        datasender = context.HostDevices.Where((host) => host.Address.Equals(e.SenderIp)).FirstOrDefault();
        //If device with that IP is not found, assume device is unknown
        datasender ??= new HostDevice()
        {
            Name = "Unknown",
            Address = e.SenderIp,
            Port = e.SenderPort,
            Type = HostType.Unknown,
            LastKnownStatus = DeviceStatus.Unknown
        };

        string message = $"Received {e.Data.Length} bytes of data from {datasender.Type} device. Address = {e.SenderIp}:{e.SenderPort}. ";
        _invocationDictionary[datasender.Type]++;
        switch (datasender.Type)
        {
            default:
            case HostType.Unknown:
                _logger?.WriteLine(message + "Ignoring...", nameof(SignalTranslator));
                break;
            case HostType.CameraSimulator:
            case HostType.PythonCameraSimulator:
                _logger?.WriteLine(message + $"Sender is '{datasender.Name}'. Forwarding to their respective API library.", nameof(SignalTranslator));
                ICamera? api = datasender.Type switch
                {
                    HostType.CameraSimulator => new CameraSimulatorAPI(),
                    HostType.PythonCameraSimulator => new PythonCameraSimulatorAPI(_logger),
                    _ => null
                };

                //This is where individual APIs perform their magic and catch exceptions if their magic trick blew up
                try
                {
                    api?.DecodeReceivedBytes(e.Data);
                }
                catch (ArgumentException ex)
                {
                    _logger?.WriteLine($"API of {datasender.Type} received invalid argument! {ex.Message}", nameof(SignalTranslator), Logger.MessageType.Error);
                }
                catch (JsonException ex)
                {
                    _logger?.WriteLine($"API of {datasender.Type} failed to parse the received JSON string! {ex.Message}", nameof(SignalTranslator), Logger.MessageType.Error);
                }

                var decodedMessage = api?.GetDecodedMessage();
                if (decodedMessage is not null)
                {
                    //Apply received changes if they were succesfully decoded
                    datasender.LastKnownTemperature = decodedMessage.LargestTemperature;
                    datasender.LastImage = decodedMessage.Image;
                    datasender.LastKnownStatus = decodedMessage.Status;
                    context.SaveChanges();
                    _logger?.WriteLine($"{datasender.Name} ({nameof(HostDevice.Id)}: {datasender.Id}) updated with new temperature and image. Status set to {datasender.LastKnownStatus}.", nameof(SignalTranslator));
                }
                else
                {
                    datasender.LastKnownStatus = DeviceStatus.DataCorrupted;
                    context.SaveChanges();
                    _logger?.WriteLine($"API of {datasender.Type} returned a null {nameof(CameraDataMessage)} object! No changes in the database were made. Marking device's record with {nameof(DeviceStatus.DataCorrupted)}.", nameof(SignalTranslator), Logger.MessageType.Warning);
                }
                break;
            case HostType.PuTTYClient:
                string rawData = string.Empty;
                foreach (var dataByte in e.Data)
                    rawData += dataByte;
                string decodedData = Encoding.UTF8.GetString(e.Data);
                _logger?.WriteLine(message + $"Raw = '{rawData}', Decoded = '{decodedData}'.", nameof(SignalTranslator));
                break;
            case HostType.User:
                _logger?.WriteLine(message + $"User recognized: {datasender.Name}.", nameof(SignalTranslator));
                break;
        }
    }

    private void ShowStatus(object? sender, System.EventArgs e)
    {
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.SignalTranslatorArgument)
        {
            _logger?.WriteLine($"Running: {IsTranslating}");
            _logger?.WriteLine($"Logging: {_logger is not null}");
            _logger?.WriteLine($"Signals translated: {_invocations}");
            _logger?.WriteLine($"Signals per {nameof(HostType)} devices: ");
            foreach (KeyValuePair<HostType, int> kvp in _invocationDictionary)
                _logger?.WriteLine($"\t{kvp.Key}: {kvp.Value}");
        }
    }
}
