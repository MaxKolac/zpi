using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Text;
using ZPICommunicationModels;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;
using ZPIServer.EventArgs;

namespace ZPIServer.Commands;

public class PingCommand : Command
{
    public const string IcmpArgument = "icmp";
    public const string CdmJsonArgument = "cdm";

    public string? FirstArg { get; private set; }
    public IPAddress? SecondArg { get; private set; }
    public int? ThirdArg { get; private set; }

    public PingCommand(Logger? logger) : base(logger)
    {
    }

    public override void Execute()
    {
        switch (FirstArg)
        {
            case null:
                _logger?.WriteLine($"{Ping} requires 2 or 3 arguments.");
                _logger?.WriteLine(GetHelp());
                break;
            case IcmpArgument:
                if (SecondArg is null)
                    break;

                var pingTargetArgs = new TcpSenderEventArgs(SecondArg, 0, Array.Empty<byte>());
                Invoke(this, pingTargetArgs);
                break;
            case CdmJsonArgument:
                if (SecondArg is null || ThirdArg is null)
                    break;

#pragma warning disable CA1416
                var testMessage = new CameraDataMessage()
                {
                    LargestTemperature = 123_456_789.123m,
                    Status = HostDevice.DeviceStatus.OK,
                    Image = HostDevice.ToByteArray(Image.FromFile("Commands\\PingCdmImage.png"), ImageFormat.Png) ?? Array.Empty<byte>()
                };
#pragma warning restore CA1416
                var json = JsonConvert.SerializeObject(testMessage);
                var payload = ZPIEncoding.GetBytes(json);
                var args = new TcpSenderEventArgs(SecondArg, (int)ThirdArg, payload);

                _logger?.WriteLine($"Attempting to send an example CameraDataMessage ({payload.Length} bytes) as Json to {SecondArg}:{ThirdArg}.");
                _logger?.WriteLine($"Message contents: {testMessage}.");
                Invoke(this, args);
                break;
            default:
                _logger?.WriteLine("Unrecognized argument.");
                _logger?.WriteLine(GetHelp());
                break;
        }
        Invoke(this, System.EventArgs.Empty);
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Command for testing communication between the server and specified host. Available arguments:");
        builder.AppendLine($"\t- {IcmpArgument} (IP Address)");
        builder.AppendLine("Sends an ICMP ping to the specified host.");
        builder.AppendLine($"\t- {CdmJsonArgument} (IP Address) (Target Port)");
        builder.AppendLine($"Sends an example {nameof(CameraDataMessage)} message in a Json format to the specified endpoint.");

        builder.AppendLine("Examples:");
        builder.AppendLine($"\t{Ping} {IcmpArgument} 192.168.1.1");
        builder.AppendLine($"\t{Ping} {CdmJsonArgument} 192.168.1.1 12000");
        return builder.ToString();
    }

    public override void SetArguments(params string[]? arguments)
    {
        if (arguments is null || arguments.Length == 0)
            return;

        if (arguments.Length < 2)
        {
            _logger?.WriteLine("Not enough arguments.");
        }
        else if (arguments.Length == 2)
        {
            FirstArg = arguments[0];

            if (FirstArg == IcmpArgument)
            {
                if (IPAddress.TryParse(arguments[1], out var ipResult))
                    SecondArg = ipResult;
                else
                    _logger?.WriteLine($"{arguments[1]} is not a valid IP address.");
            }
            else if (FirstArg == CdmJsonArgument)
            {
                _logger?.WriteLine($"Argument \'{CdmJsonArgument}\' requires both IP address and port.");
            }
        }
        else if (arguments.Length == 3)
        {
            FirstArg = arguments[0];

            if (FirstArg == CdmJsonArgument)
            {
                if (IPAddress.TryParse(arguments[1], out var ipResult))
                    SecondArg = ipResult;
                else
                    _logger?.WriteLine($"{arguments[1]} is not a valid IP address.");

                if (int.TryParse(arguments[2], out int intResult) && intResult >= 1024 && intResult <= 65535)
                    ThirdArg = intResult;
                else
                    _logger?.WriteLine($"{arguments[2]} is not a valid port number.");
            }
            else if (FirstArg == IcmpArgument)
            {
                _logger?.WriteLine($"Argument \'{IcmpArgument}\' does not require a port.");
            }
        }
        else
        {
            _logger?.WriteLine("Too many arguments.");
        }
    }
}
