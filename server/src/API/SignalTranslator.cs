﻿using System.Net;
using System.Text;
using ZPIServer.API.CameraLibraries;
using ZPIServer.Commands;
using ZPIServer.EventArgs;
using ZPIServer.Models;

namespace ZPIServer.API;

/// <summary>
/// Klasa, która subskrybuje wydarzenie <see cref="TcpHandler.OnSignalReceived"/> i odpowiednio reaguje na jej inwokacje.
/// </summary>
public class SignalTranslator
{
    private readonly Logger? _logger;
    private int _invocations = 0;

    /// <summary>
    /// Wskazuje czy <see cref="SignalTranslator"/> został uruchomiony i obsługuje inwokacje wydarzenia <see cref="TcpHandler.OnSignalReceived"/>.
    /// </summary>
    public bool IsTranslating { get; private set; } = false;

    public SignalTranslator(Logger? logger = null)
    {
        _logger = logger;
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
        TcpHandler.OnSignalReceived += HandleReceivedSignal;
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
        TcpHandler.OnSignalReceived -= HandleReceivedSignal;
        IsTranslating = false;
    }

    void HandleReceivedSignal(object? sender, TcpHandlerEventArgs e)
    {
        _invocations++;
        //TODO: Query the DB for the proper record based on received IP
        var datasender = new HostDevice() 
        { 
            Address = e.SenderIp,
            Type = e.SenderIp.Equals(IPAddress.Parse("127.0.0.1")) ? HostType.PuTTYClient : HostType.CameraSimulator,
            LastKnownStatus = HostDevice.DeviceStatus.OK
        };
        datasender ??= new HostDevice()
        {
            Address = e.SenderIp,
            Type = HostType.Unknown,
            LastKnownStatus = HostDevice.DeviceStatus.Unknown
        };

        string message = $"Received {e.Data.Length} bytes of data from {datasender.Type} device. Address = {e.SenderIp}:{e.SenderPort}. ";
        switch (datasender.Type)
        {
            case HostType.Unknown:
                _logger?.WriteLine(message + "Ignoring...", nameof(SignalTranslator));
                break;
            case HostType.CameraSimulator:
                _logger?.WriteLine(message + $"Forwarding to {nameof(CameraSimulatorAPI)}.", nameof(SignalTranslator));
                break;
            case HostType.PuTTYClient:
                string rawData = string.Empty;
                foreach (var dataByte in e.Data)
                    rawData += dataByte;
                string decodedData = Encoding.UTF8.GetString(e.Data);

                _logger?.WriteLine(message + $"Recognized PuTTY client. Raw = '{rawData}', Decoded = '{decodedData}'.", nameof(SignalTranslator));
                break;
            case HostType.User:
                _logger?.WriteLine(message + $"User recognized: {datasender.Name}.", nameof(SignalTranslator));
                break;
        }
    }

    private void ShowStatus(object? sender, CommandEventArgs e)
    {
        if (sender is StatusCommand command && command.ClassArgument == StatusCommand.SignalTranslatorArgument)
        {
            _logger?.WriteLine($"Running: {IsTranslating}", null);
            _logger?.WriteLine($"Logging: {_logger is not null}", null);
            _logger?.WriteLine($"Signals translated: {_invocations}", null);
        }
    }
}
