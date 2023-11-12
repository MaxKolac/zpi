using ZPIServer.Commands;

namespace ZPIServer.EventArgs;

public class CommandEventArgs : System.EventArgs
{
    /// <summary>
    /// Instancja uruchomionej komendy.
    /// </summary>
    public Command Command { get; private set; }

    public CommandEventArgs(Command command)
    {
        Command = command;
    }
}
