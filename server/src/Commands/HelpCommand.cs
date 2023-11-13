using System.Text;
using ZPIServer.EventArgs;

namespace ZPIServer.Commands;

public class HelpCommand : Command
{
    public string? CommandIdentifier { get; private set; }

    public HelpCommand(Logger logger) : base(logger) 
    { 
    }

    public override void Execute()
    {
        if (CommandIdentifier is not null)
        {
            switch (CommandIdentifier)
            {
                case Command.HelpCommand:
                    _logger.WriteLine(GetHelp(), null);
                    break;
                case Command.ShutdownCommand:
                    _logger.WriteLine(new ShutdownCommand(_logger).GetHelp(), null);
                    break;
                default:
                    CommandIdentifier = null;
                    _logger.WriteLine("Unrecognized command.", null);
                    _logger.WriteLine(GetAvailableCommands(), null);
                    break;
            }
        }
        else
        {
            _logger.WriteLine(GetAvailableCommands(), null);
        }
        Invoke(this, new CommandEventArgs());
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Shows all available commands.");
        builder.AppendLine("If entered with the name of a command as argument, it shows the syntax of that command.");
        builder.AppendLine("Example:");
        builder.Append("\thelp shutdown");
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
            _logger.WriteLine("Too many arguments.", null);
        }
    }

    private static string GetAvailableCommands()
    { 
        var builder = new StringBuilder();
        builder.AppendLine(Command.HelpCommand + " [command]");
        builder.Append(Command.ShutdownCommand);
        return builder.ToString();
    }
}
