using System.Text;

namespace ZPIServer.Commands;

public class HelpCommand : Command
{
    public string? CommandIdentifier { get; private set; }

    public HelpCommand(Logger? logger = null) : base(logger)
    {
    }

    public override void Execute()
    {
        switch (CommandIdentifier)
        {
            case null:
                _logger?.WriteLine(GetAvailableCommands());
                break;
            case Db:
                _logger?.WriteLine(new DbCommand(_logger).GetHelp());
                break;
            case Help:
                _logger?.WriteLine(GetHelp());
                break;
            case Ping:
                _logger?.WriteLine(new PingCommand(_logger).GetHelp());
                break;
            case Shutdown:
                _logger?.WriteLine(new ShutdownCommand(_logger).GetHelp());
                break;
            case Status:
                _logger?.WriteLine(new StatusCommand(_logger).GetHelp());
                break;
            default:
                CommandIdentifier = null;
                _logger?.WriteLine("Unrecognized command.");
                _logger?.WriteLine(GetAvailableCommands());
                break;
        }
        Invoke(this, System.EventArgs.Empty);
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Shows all available commands.");
        builder.AppendLine("If entered with the name of a command as argument, it shows the syntax of that command.");
        builder.AppendLine("Example:");
        builder.AppendLine($"\t{Help} {Db}");
        builder.AppendLine($"\t{Help} {Shutdown}");
        return builder.ToString();
    }

    public override void SetArguments(params string?[]? arguments)
    {
        if (arguments is null)
            return;

        if (arguments.Length == 1)
        {
            CommandIdentifier = arguments[0];
        }
        else if (arguments.Length > 1)
        {
            _logger?.WriteLine("Too many arguments.");
        }
    }

    private static string GetAvailableCommands()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{Db} [{DbCommand.ListAllArgument}/{DbCommand.GetImageArgument}]");
        builder.AppendLine($"{Help} [command]");
        builder.AppendLine($"{Ping} [{PingCommand.IcmpArgument}/{PingCommand.CdmJsonArgument}] [address] [port]");
        builder.AppendLine($"{Shutdown}");
        builder.AppendLine($"{Status} [{StatusCommand.SignalTranslatorArgument}/{StatusCommand.TcpReceiverArgument}/{StatusCommand.TcpSenderArgument}]");
        return builder.ToString();
    }
}
