using ZPIServer.EventArgs;

namespace ZPIServer.Commands;

public class HelpCommand : Command
{
    public override void Execute()
    {
        Invoke(this, new CommandEventArgs(this));
    }

    public override string GetHelp()
    {
        throw new NotImplementedException();
    }

    public override void SetArguments(params string[]? arguments)
    {
        //TODO
    }
}
