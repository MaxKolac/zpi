namespace ZPIServer.Commands;

public abstract class Command
{
    public const string HelpCommand = "help";
    public const string ShutdownCommand = "shutdown";

    public abstract void Execute();

    public abstract string GetHelp();

    public abstract void SetArguments(params string[]? arguments);
}
