using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServerTests.Commands;

public class ShutdownCommandTests
{
    [Fact]
    static void CheckExecutionWithNoArguments()
    {
        object? sendingCommand = null;
        EventHandler<CommandEventArgs> handler = (sender, e) =>
        {
            sendingCommand = sender;
        };
        Command.OnExecuted += handler;

        var command = new ShutdownCommand();
        command.Execute();

        Assert.Equal(command, sendingCommand);
        
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
