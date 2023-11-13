using ZPIServer.EventArgs;

namespace ZPIServer.Commands;

public abstract class Command
{
    public const string HelpCommand = "help";
    public const string ShutdownCommand = "shutdown";

    public static event EventHandler<CommandEventArgs>? OnExecuted;

    public abstract void Execute();

    public abstract string GetHelp();

    public abstract void SetArguments(params string[]? arguments);

    protected void Invoke(object? sender, CommandEventArgs e) => OnExecuted?.Invoke(sender, e);
}
