﻿namespace ZPIServer.Commands;

public abstract class Command
{
    protected readonly Logger? _logger;
    public const string Db = "db";
    public const string Help = "help";
    public const string Ping = "ping";
    public const string Shutdown = "shutdown";
    public const string Status = "status";

    public static event EventHandler<System.EventArgs>? OnExecuted;

    protected Command(Logger? logger)
    {
        _logger = logger;
    }

    public abstract void Execute();

    public abstract string GetHelp();

    public abstract void SetArguments(params string[]? arguments);

    protected void Invoke(object? sender, System.EventArgs e) => OnExecuted?.Invoke(sender, e);
}
