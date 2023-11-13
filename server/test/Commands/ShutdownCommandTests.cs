using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServerTests.Commands;

public class ShutdownCommandTests
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

        var command = new ShutdownCommand();
        command.Execute();

        Assert.Equal(command, sendingCommand);
        Assert.Equal(command, handledCommand);
        
        Command.OnExecuted -= handler;
    }

    //[Theory]
    //static void CheckExecutionWithArguments()
    //{

    //}

    //[Theory]
    //static void CheckExecutionWithInvalidArguments()
    //{

    //}
}
