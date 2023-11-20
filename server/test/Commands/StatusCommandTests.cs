using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServerTests.Commands;

public class StatusCommandTests
{
    [Fact]
    static void CheckExecutionWithNoArguments()
    {
        var commandToExecute = new StatusCommand();
        StatusCommand? receivedCommand = PerformExecution(commandToExecute, null);

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Null(receivedCommand?.ClassArgument);
    }

    [Theory]
    [InlineData(StatusCommand.SignalTranslatorArgument)]
    [InlineData(StatusCommand.TcpHandlerArgument)]
    static void CheckExecutionWithArguments(string argument)
    {
        var commandToExecute = new StatusCommand();
        StatusCommand? receivedCommand = PerformExecution(commandToExecute, new string[] { argument });

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Equal(argument, receivedCommand?.ClassArgument);
    }

    [Theory]
    [MemberData(nameof(GetInvalidArguments), MemberType = typeof(StatusCommandTests))]
    static void CheckExecutionWithInvalidArguments(string[]? arguments)
    {

        var commandToExecute = new StatusCommand();
        StatusCommand? receivedCommand = PerformExecution(commandToExecute, arguments);

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Null(receivedCommand?.ClassArgument);
    }

    private static StatusCommand? PerformExecution(StatusCommand commandToExecute, string[]? arguments)
    {
        StatusCommand? receivedCommand = null;
        EventHandler<CommandEventArgs> handler = (sender, e) =>
        {
            receivedCommand = sender as StatusCommand;
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
        yield return new object?[] { new string?[] { "tcplistener", "tcplistener" } };
        yield return new object?[] { new string?[] { "signaltranslator", "signaltranslator" } };
        yield return new object?[] { new string?[] { "singaltransl" } };
        yield return new object?[] { new string?[] { "tpclistn" } };
        yield return new object?[] { new string?[] { "$" } };
        yield return new object?[] { new string?[] { null } };
        yield return new object?[] { new string?[] { null, null } };
    }
}
