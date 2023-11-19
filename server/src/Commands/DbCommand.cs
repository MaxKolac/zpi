using System.Text;
using ZPIServer.Models;

namespace ZPIServer.Commands;

public class DbCommand : Command
{
    public const string ListAllArgument = "listall";

    public string? FirstArg { get; private set; }

    public DbCommand(Logger? logger = null) : base(logger)
    {
    }

    public override void Execute()
    {
        if (FirstArg is not null)
        {
            switch (FirstArg)
            {
                case ListAllArgument:
                    using (var context = new DatabaseContext())
                    {
                        int recordCount = 0;
                        _logger?.WriteLine($"Records in {nameof(DatabaseContext.HostDevices)}:");
                        foreach (var record in context.HostDevices.ToList())
                        {
                            _logger?.WriteLine(record.ToString());
                            recordCount++;
                        }
                        _logger?.WriteLine($"\tTotal amount: {recordCount}");

                        recordCount = 0;
                        _logger?.WriteLine($"Records in {nameof(DatabaseContext.Sectors)}:");
                        foreach (var record in context.Sectors.ToList())
                        {
                            _logger?.WriteLine(record.ToString());
                            recordCount++;
                        }
                        _logger?.WriteLine($"\tTotal amount: {recordCount}");
                    }
                    break;
                default:
                    _logger?.WriteLine("Unrecognized argument.");
                    _logger?.WriteLine(GetHelp());
                    break;
            }
        }
        else
        {
            _logger?.WriteLine($"{Command.Db} requires 1 or more arguments.");
            _logger?.WriteLine(GetHelp());
        }
        Invoke(this, new EventArgs.CommandEventArgs());
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Command for interacting with the server's database. Available arguments:");
        builder.AppendLine($"\t- {ListAllArgument}");
        builder.AppendLine("\tShows all records from all tables.");

        builder.AppendLine("Examples:");
        builder.AppendLine($"\t{Command.Db} {ListAllArgument}");
        return builder.ToString();
    }

    public override void SetArguments(params string[]? arguments)
    {
        if (arguments is null)
            return;

        if (arguments.Length == 1)
        {
            FirstArg = arguments[0];
        }
        else if (arguments.Length > 1)
        {
            _logger?.WriteLine("Too many arguments.");
        }
    }
}
