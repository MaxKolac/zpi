using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServerTests.Commands;

public class HelpCommandTests
{
    [Fact]
    static void CheckExecutionWithNoArguments()
    {
        object? sendingCommand = null;
        Command? handledCommand = null;
        EventHandler<CommandEventArgs> handler = (sender, e) =>
        {
            sendingCommand = sender;
            handledCommand = e.Command;
        };
        Command.OnExecuted += handler;

        var command = new HelpCommand();
        command.Execute();

        Assert.Equal(command, sendingCommand);
        Assert.Equal(command, handledCommand);

        Command.OnExecuted -= handler;
    }

    [Theory]
    [InlineData(Command.HelpCommand)]
    [InlineData(Command.ShutdownCommand)]
    static void CheckExecutionWithArguments(string argument)
    {
        Command? handledCommand = null;
        object? sendingCommand = null;
        EventHandler<CommandEventArgs> handler = (sender, e) =>
        {
            handledCommand = e.Command;
            sendingCommand = sender;
        };

        Command.OnExecuted += handler;
        var command = new HelpCommand();
        command.SetArguments(argument);
        command.Execute();

        Assert.Equal(command, sendingCommand);
        Assert.Equal(command, handledCommand);
        Assert.Equal(argument, command.CommandIdentifier);

        Command.OnExecuted -= handler;
    }

    [Theory]
    [MemberData(nameof(GetInvalidArguments), MemberType = typeof(HelpCommandTests))]
    static void CheckExecutionWithInvalidArguments(string?[] arguments)
    {
        Command? handledCommand = null;
        object? sendingCommand = null;
        EventHandler<CommandEventArgs> handler = (sender, e) =>
        {
            handledCommand = e.Command;
            sendingCommand = sender;
        };

        Command.OnExecuted += handler;
        var command = new HelpCommand();
        command.SetArguments(arguments);
        command.Execute();

        Assert.Equal(command, sendingCommand);
        Assert.Equal(command, handledCommand);
        Assert.Null(command.CommandIdentifier);

        Command.OnExecuted -= handler;
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
