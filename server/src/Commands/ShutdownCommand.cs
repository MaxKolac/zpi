using System.Text;

namespace ZPIServer.Commands;

public class ShutdownCommand : Command
{
    public ShutdownCommand(Logger? logger = null) : base(logger)
    {
    }

    public override void Execute()
    {
        Invoke(this, System.EventArgs.Empty);
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Stops the server execution.");
        builder.AppendLine("Example:");
        builder.AppendLine("\tshutdown");
        return builder.ToString();
    }

    public override void SetArguments(params string[]? arguments)
    {
    }
}
