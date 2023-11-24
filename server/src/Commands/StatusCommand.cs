using System.Text;

namespace ZPIServer.Commands;

public class StatusCommand : Command
{
    public const string SignalTranslatorArgument = "signaltranslator";
    public const string TcpReceiverArgument = "tcpreceiver";

    public string? ClassArgument { get; private set; }

    public StatusCommand(Logger? logger = null) : base(logger)
    {
    }

    public override void Execute()
    {
        switch (ClassArgument)
        {
            case null:
                _logger?.WriteLine($"{Status} requires 1 argument.");
                _logger?.WriteLine(GetHelp());
                break;
            case SignalTranslatorArgument:
            case TcpReceiverArgument:
                _logger?.WriteLine($"Status of {ClassArgument}:");
                break;
            default:
                _logger?.WriteLine("Unrecognized argument.");
                _logger?.WriteLine(GetHelp());
                break;
        }
        Invoke(this, new EventArgs.CommandEventArgs());
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Show the current status of the specified server component.");
        builder.AppendLine("Available components that can be checked are:");
        builder.AppendLine($"\t{SignalTranslatorArgument}");
        builder.AppendLine($"\t{TcpReceiverArgument}");
        builder.AppendLine("Examples:");
        builder.AppendLine($"\t{Status} {SignalTranslatorArgument}");
        builder.AppendLine($"\t{Status} {TcpHandlerArgument}");
        builder.AppendLine($"\t{Status} {TcpReceiverArgument}");
        return builder.ToString();
    }

    public override void SetArguments(params string[]? arguments)
    {
        if (arguments is null)
            return;

        if (arguments.Length == 1)
        {
            string arg = arguments[0];
            if (arg == SignalTranslatorArgument || arg == TcpHandlerArgument)
                ClassArgument = arg;
        }
        else if (arguments.Length > 1)
        {
            _logger?.WriteLine("Too many arguments.");
        }
    }
}
