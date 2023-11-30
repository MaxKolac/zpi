using Newtonsoft.Json;
using ZPICommunicationModels;
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

    public static event EventHandler<TcpSenderEventArgs>? OnSendRequested;

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
            case HostType.User:
                _logger?.WriteLine(message + $"User recognized: {datasender.Name}. Decoding the user request.", nameof(SignalTranslator));

                //Attempt to deserialize the request itself first
                UserRequest? request;
                try
                {
                    request = ZPIEncoding.Decode<UserRequest>(e.Data);
                }
                catch (JsonSerializationException ex)
                {
                    _logger?.WriteLine($"API of {datasender.Type} failed to parse the received JSON string! {ex.Message}", nameof(SignalTranslator), Logger.MessageType.Error);
                    break;
                }

                //Something went terribly wrong if this is still a null
                if (request is null)
                {
                    _logger?.WriteLine($"Decoding the received user request resulted in a null! Discarding request.", nameof(SignalTranslator), Logger.MessageType.Error);
                    break;
                }

                //Recognize if this request is one of those that require ModelObjectId to be non-null
                if ((request.Request == UserRequest.RequestType.CameraDataAsJson ||
                    request.Request == UserRequest.RequestType.SingleHostDeviceAsJson) &&
                    request.ModelObjectId is null)
                {
                    //If its null, discard request
                    _logger?.WriteLine($"User's request did not contain {nameof(UserRequest.ModelObjectId)} which {request.Request} requires! Discarding request.", nameof(SignalTranslator), Logger.MessageType.Error);
                    break;
                }

                //Look for the object that user requested
                string requestedClassType = request.Request switch
                {
                    UserRequest.RequestType.CameraDataAsJson => nameof(CameraDataMessage),
                    UserRequest.RequestType.SingleHostDeviceAsJson => nameof(HostDevice),
                    UserRequest.RequestType.AllHostDevicesAsJson => $"List<{nameof(HostDevice)}>",
                    UserRequest.RequestType.SingleSectorAsJson => nameof(Sector),
                    UserRequest.RequestType.AllSectorsAsJson => $"List<{nameof(Sector)}>"
                };
                object? foundObject = null;
                if (request.Request == UserRequest.RequestType.CameraDataAsJson || request.Request == UserRequest.RequestType.SingleHostDeviceAsJson)
                    foundObject = context.HostDevices.Where((dev) => dev.Id == request.ModelObjectId).FirstOrDefault();
                else if (request.Request == UserRequest.RequestType.AllHostDevicesAsJson)
                    foundObject = context.HostDevices.ToList();
                else if (request.Request == UserRequest.RequestType.SingleSectorAsJson)
                    foundObject = context.Sectors.Where((sector) => sector.Id == request.ModelObjectId).FirstOrDefault();
                else if (request.Request == UserRequest.RequestType.AllSectorsAsJson)
                    foundObject = context.Sectors.ToList();

                //If the requested object doesn't exist, discard request
                //If after all that foundObject is null, nothing matching was found
                if (foundObject is null)
                {
                    _logger?.WriteLine($"User requested {request.Request} of a {requestedClassType} with ID = {request.ModelObjectId} which was not found in the database! Discarding request.", nameof(SignalTranslator), Logger.MessageType.Error);
                    return;
                }
                _logger?.WriteLine($"Found {requestedClassType} with ID = {request.ModelObjectId} requested by user. Building an invocation for {nameof(TcpSender)}.", nameof(SignalTranslator));

                //Depending on which exact request it is, create and encode the requested object
                byte[] messageToSend = request.Request switch
                {
                    UserRequest.RequestType.CameraDataAsJson => ZPIEncoding.Encode(new CameraDataMessage()
                    {
                        LargestTemperature = ((HostDevice)foundObject).LastKnownTemperature,
                        Status = ((HostDevice)foundObject).LastKnownStatus ?? DeviceStatus.Unknown,
                        Image = ((HostDevice)foundObject).LastImage ?? Array.Empty<byte>()
                    }),
                    UserRequest.RequestType.SingleHostDeviceAsJson => ZPIEncoding.Encode(foundObject as HostDevice),
                    UserRequest.RequestType.AllHostDevicesAsJson => ZPIEncoding.Encode(foundObject as List<HostDevice>),
                    UserRequest.RequestType.SingleSectorAsJson => ZPIEncoding.Encode(foundObject as Sector),
                    UserRequest.RequestType.AllSectorsAsJson => ZPIEncoding.Encode(foundObject as List<Sector>)
                };

                //Invoke the TcpSender
                var args = new TcpSenderEventArgs(datasender.Address, datasender.Port, messageToSend);
                OnSendRequested?.Invoke(this, args);
                break;
            case HostType.PuTTYClient:
                string rawData = string.Empty;
                foreach (var dataByte in e.Data)
                    rawData += dataByte;
                string decodedData = System.Text.Encoding.UTF8.GetString(e.Data);
                _logger?.WriteLine(message + $"Raw = '{rawData}', Decoded = '{decodedData}'.", nameof(SignalTranslator));
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
