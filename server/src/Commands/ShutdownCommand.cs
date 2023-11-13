﻿using System.Text;
using ZPIServer.EventArgs;

namespace ZPIServer.Commands;

public class ShutdownCommand : Command
{
    public ShutdownCommand(Logger logger) : base(logger) 
    {
    }

    public override void Execute()
    {
        Invoke(this, new CommandEventArgs());
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Stops the server execution.");
        builder.AppendLine("Example:");
        builder.Append("\tshutdown");
        return builder.ToString();
    }

    public override void SetArguments(params string[]? arguments)
    {
    }
}
