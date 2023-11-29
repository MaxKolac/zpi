using ZPIServer.Commands;

namespace ZPIServerTests.Commands;

public class ShutdownCommandTests
{
    [Fact]
    static void CheckExecutionWithNoArguments()
    {
        var commandToExecute = new ShutdownCommand();
        ShutdownCommand? receivedCommand = PerformExecution(commandToExecute, null);

        Assert.Equal(commandToExecute, receivedCommand);
    }

    [Theory]
    [InlineData(Command.Help)]
    [InlineData(Command.Shutdown)]
    static void CheckExecutionWithArguments(string argument)
    {
        var commandToExecute = new ShutdownCommand();
        ShutdownCommand? receivedCommand = PerformExecution(commandToExecute, new string[] { argument });

        Assert.Equal(commandToExecute, receivedCommand);
    }

    [Theory]
    [MemberData(nameof(GetInvalidArguments), MemberType = typeof(ShutdownCommandTests))]
    static void CheckExecutionWithInvalidArguments(string[]? arguments)
    {
        var commandToExecute = new ShutdownCommand();
        ShutdownCommand? receivedCommand = PerformExecution(commandToExecute, arguments);

        Assert.Equal(commandToExecute, receivedCommand);
    }

    private static ShutdownCommand? PerformExecution(ShutdownCommand commandToExecute, string[]? arguments)
    {
        ShutdownCommand? receivedCommand = null;
        EventHandler<CommandEventArgs> handler = (sender, e) =>
        {
            receivedCommand = sender as ShutdownCommand;
        };

        Command.OnExecuted += handler;
        if (arguments is not null)
            commandToExecute.SetArguments(arguments);
        commandToExecute.Execute();
        Command.OnExecuted -= handler;

        return receivedCommand;
    }

    public static IEnumerable<object?[]> GetInvalidArguments()
    {
        yield return new object?[] { new string?[] { "", "", "", "   " } };
        yield return new object?[] { new string?[] { "help", "help" } };
        yield return new object?[] { new string?[] { "shutdown", "shutdown" } };
        yield return new object?[] { new string?[] { "suhtdown" } };
        yield return new object?[] { new string?[] { "hlep" } };
        yield return new object?[] { new string?[] { "$" } };
        yield return new object?[] { new string?[] { null } };
        yield return new object?[] { new string?[] { null, null } };
    }
}
