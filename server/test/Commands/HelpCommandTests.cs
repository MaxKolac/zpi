using ZPIServer.Commands;

namespace ZPIServerTests.Commands;

public class HelpCommandTests
{
    [Fact]
    static void CheckExecutionWithNoArguments()
    {
        var commandToExecute = new HelpCommand();
        HelpCommand? receivedCommand = PerformExecution(commandToExecute, null);

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Null(receivedCommand?.CommandIdentifier);
    }

    [Theory]
    [InlineData(Command.Db)]
    [InlineData(Command.Help)]
    [InlineData(Command.Shutdown)]
    [InlineData(Command.Status)]
    static void CheckExecutionWithArguments(string argument)
    {
        var commandToExecute = new HelpCommand();
        HelpCommand? receivedCommand = PerformExecution(commandToExecute, new string[] { argument });

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Equal(argument, receivedCommand?.CommandIdentifier);
    }

    [Theory]
    [MemberData(nameof(GetInvalidArguments), MemberType = typeof(HelpCommandTests))]
    static void CheckExecutionWithInvalidArguments(string[]? arguments)
    {
        var commandToExecute = new HelpCommand();
        HelpCommand? receivedCommand = PerformExecution(commandToExecute, arguments);

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Null(receivedCommand?.CommandIdentifier);
    }

    private static HelpCommand? PerformExecution(HelpCommand commandToExecute, string[]? arguments)
    {
        HelpCommand? receivedCommand = null;
        EventHandler<EventArgs> handler = (sender, e) =>
        {
            receivedCommand = sender as HelpCommand;
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
