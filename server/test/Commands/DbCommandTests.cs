using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServerTests.Commands;

public class DbCommandTests
{
    [Fact]
    static void CheckExecutionWithNoArguments()
    {
        var commandToExecute = new DbCommand();
        DbCommand? receivedCommand = PerformExecution(commandToExecute, null);

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Null(receivedCommand?.FirstArg);
    }

    [Theory]
    [MemberData(nameof(GetValidArguments), MemberType = typeof(DbCommandTests))]
    static void CheckExecutionWithValidArguments(string[] arguments)
    {
        var commandToExecute = new DbCommand();
        DbCommand? receivedCommand = PerformExecution(commandToExecute, arguments);

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Contains(receivedCommand?.FirstArg, arguments);
    }

    [Theory]
    [MemberData(nameof(GetInvalidArguments), MemberType = typeof(DbCommandTests))]
    static void CheckExecutionWithInvalidArguments(string[]? arguments)
    {

        var commandToExecute = new DbCommand();
        DbCommand? receivedCommand = PerformExecution(commandToExecute, arguments);

        Assert.Equal(commandToExecute, receivedCommand);
        Assert.Null(receivedCommand?.FirstArg);
    }

    private static DbCommand? PerformExecution(DbCommand commandToExecute, string[]? arguments)
    {
        DbCommand? receivedCommand = null;
        EventHandler<CommandEventArgs> handler = (sender, e) =>
        {
            receivedCommand = sender as DbCommand;
        };

        Command.OnExecuted += handler;
        if (arguments is not null)
            commandToExecute.SetArguments(arguments);
        commandToExecute.Execute();
        Command.OnExecuted -= handler;

        return receivedCommand;
    }

    public static IEnumerable<object?[]> GetValidArguments()
    {
        yield return new object?[] { new string?[]{ DbCommand.ListAllArgument, null, null } };
        yield return new object?[] { new string?[]{ null, DbCommand.ListAllArgument, null } };
        yield return new object?[] { new string?[]{ null, null, DbCommand.ListAllArgument } };
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
