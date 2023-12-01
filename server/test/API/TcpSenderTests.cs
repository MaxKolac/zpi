using System.Net;
using ZPICommunicationModels;
using ZPIServer.API;
using ZPIServer.EventArgs;

namespace ZPIServerTests.API;

public class TcpSenderTests
{
    [Fact]
    public static async void CheckEnableMethod()
    {
        var sender = new TcpSender();
        TcpSender.TestEvents.InvokeEvent1(null, TcpSenderEventArgs.Empty);
        await Task.Delay(100);

        sender.Enable();
        TcpSender.TestEvents.InvokeEvent1(null, TcpSenderEventArgs.Empty);
        await Task.Delay(100);

        Assert.True(sender.CanSendMessages);
        Assert.Equal(1, sender.ConnectionsInitialized);
        Assert.Equal(0, sender.ConnectionsHandled);
    }

    [Fact]
    public static async void CheckFinalizer()
    {
        var sender = new TcpSender();
        sender.Enable();
        TcpSender.TestEvents.InvokeEvent1(null, TcpSenderEventArgs.Empty);

        await Task.Delay(100);

        Assert.True(sender.CanSendMessages);
        Assert.Equal(1, sender.ConnectionsInitialized);
        Assert.Equal(0, sender.ConnectionsHandled);

        //Check that GarbageCollection correctly calls Disable() in finalizer
        sender = new TcpSender();

        Assert.False(sender.CanSendMessages);
        Assert.Equal(0, sender.ConnectionsInitialized);
        Assert.Equal(0, sender.ConnectionsHandled);
    }

    [Fact]
    public static void CheckDisableMethod()
    {
        var sender = new TcpSender();
        sender.Enable();
        TcpSender.TestEvents.InvokeEvent1(null, TcpSenderEventArgs.Empty);
        sender.Disable();
        TcpSender.TestEvents.InvokeEvent1(null, TcpSenderEventArgs.Empty);

        Assert.False(sender.CanSendMessages);
        Assert.Equal(1, sender.ConnectionsInitialized);
        Assert.Equal(0, sender.ConnectionsHandled);
    }

    [Fact]
    public static async void CheckMessageSending()
    {
        //Setup a sender
        var sender = new TcpSender();
        sender.Enable();

        //Setup a receiver
        var receiver = new TcpReceiver(IPAddress.Loopback, 12345);
        receiver.Enable();

        //Setup an message sent invocation
        string sentMessage = "Hello world!";
        var args = new TcpSenderEventArgs(IPAddress.Loopback, 12345, ZPIEncoding.GetBytes(sentMessage));

        //Setup an event handler to handle the received message
        string receivedMessage = string.Empty;
        EventHandler<TcpReceiverEventArgs> handler = (sender, e) =>
        {
            receivedMessage = ZPIEncoding.GetString(e.Data);
        };
        TcpReceiver.OnSignalReceived += handler;

        //Act
        TcpSender.TestEvents.InvokeEvent1(null, args);

        //Give the system some time to rethink its life choices
        await Task.Delay(100);

        Assert.Equal(sentMessage, receivedMessage);

        TcpReceiver.OnSignalReceived -= handler;
    }

    [Theory]
    [InlineData(12345, 12345, 12345)] //3 messages on same port
    [InlineData(12345, 12346, 12347)] //3 messages on different ports
    public static async Task CheckMultipleMessagesSent(int port1, int port2, int port3)
    {
        //Setup a sender
        var sender = new TcpSender();
        sender.Enable();

        //Setup a receiver
        var receiver = new TcpReceiver(IPAddress.Loopback, new int[] { port1, port2, port3 }.Distinct().ToArray());
        receiver.Enable();

        //Setup up multiple messages to send
        string sentMessage1 = "Hello world! 1 " + port1;
        var args1 = new TcpSenderEventArgs(IPAddress.Loopback, port1, ZPIEncoding.GetBytes(sentMessage1));
        string sentMessage2 = "Hello world! 2 " + port2;
        var args2 = new TcpSenderEventArgs(IPAddress.Loopback, port2, ZPIEncoding.GetBytes(sentMessage2));
        string sentMessage3 = "Hello world! 3 " + port3;
        var args3 = new TcpSenderEventArgs(IPAddress.Loopback, port3, ZPIEncoding.GetBytes(sentMessage3));

        //Setup an event handler to handle the received message
        string receivedMessage = string.Empty;
        EventHandler<TcpReceiverEventArgs> handler = (sender, e) =>
        {
            receivedMessage = ZPIEncoding.GetString(e.Data);
        };
        TcpReceiver.OnSignalReceived += handler;

        //Act - first, send messages one by one
        TcpSender.TestEvents.InvokeEvent1(null, args1);
        await Task.Delay(100);
        Assert.Equal(1, sender.ConnectionsInitialized);
        Assert.Equal(1, sender.ConnectionsHandled);
        Assert.Equal(sentMessage1, receivedMessage);

        TcpSender.TestEvents.InvokeEvent1(null, args2);
        await Task.Delay(100);
        Assert.Equal(2, sender.ConnectionsInitialized);
        Assert.Equal(2, sender.ConnectionsHandled);
        Assert.Equal(sentMessage2, receivedMessage);

        TcpSender.TestEvents.InvokeEvent1(null, args3);
        await Task.Delay(100);
        Assert.Equal(3, sender.ConnectionsInitialized);
        Assert.Equal(3, sender.ConnectionsHandled);
        Assert.Equal(sentMessage3, receivedMessage);

        //Act - now try to send them all together
        //Before this however, a new handler is needed
        TcpReceiver.OnSignalReceived -= handler;
        handler = (sender, e) =>
        {
            receivedMessage += ZPIEncoding.GetString(e.Data);
        };
        TcpReceiver.OnSignalReceived += handler;

        TcpSender.TestEvents.InvokeEvent1(null, args1);
        TcpSender.TestEvents.InvokeEvent1(null, args2);
        TcpSender.TestEvents.InvokeEvent1(null, args3);
        await Task.Delay(500);
        Assert.Equal(6, sender.ConnectionsInitialized);
        Assert.Equal(6, sender.ConnectionsHandled);
        Assert.Contains(sentMessage1, receivedMessage);
        Assert.Contains(sentMessage2, receivedMessage);
        Assert.Contains(sentMessage3, receivedMessage);
    }

    [Theory]
    [InlineData(19000, 19877, 19876, 19877)] //1st message should fail, 2nd one should not
    public static async Task CheckBehaviourOnOneOrMoreSendFails(int targetPortToFail, int targetPortToSucceed, int receiverPort1, int receiverPort2)
    {
        //Setup a sender
        var sender = new TcpSender();
        sender.Enable();

        //Setup a receiver
        var receiver = new TcpReceiver(IPAddress.Loopback, new int[] { receiverPort1, receiverPort2 }.Distinct().ToArray());
        receiver.Enable();

        //Setup up multiple messages to send
        string messageToFail = "Hello world! 1 " + targetPortToFail;
        var argsToFail = new TcpSenderEventArgs(IPAddress.Loopback, targetPortToFail, ZPIEncoding.GetBytes(messageToFail));
        string messageToSucceed = "Hello world! 2 " + targetPortToSucceed;
        var argsToSucceed = new TcpSenderEventArgs(IPAddress.Loopback, targetPortToSucceed, ZPIEncoding.GetBytes(messageToSucceed));

        //Setup an event handler to handle the received message
        string receivedMessage = string.Empty;
        EventHandler<TcpReceiverEventArgs> handler = (sender, e) =>
        {
            receivedMessage = ZPIEncoding.GetString(e.Data);
        };
        TcpReceiver.OnSignalReceived += handler;

        //Act - first, send messages one by one
        TcpSender.TestEvents.InvokeEvent1(null, argsToFail);
        await Task.Delay(3100);
        Assert.Equal(1, sender.ConnectionsInitialized);
        Assert.Equal(0, sender.ConnectionsHandled);
        Assert.NotEqual(messageToFail, receivedMessage);

        TcpSender.TestEvents.InvokeEvent1(null, argsToSucceed);
        await Task.Delay(100);
        Assert.True(sender.ConnectionsInitialized >= 2);
        Assert.Equal(1, sender.ConnectionsHandled);
        Assert.Equal(messageToSucceed, receivedMessage);

        //Act - now try to send them all together
        //Before this however, a new handler is needed
        TcpReceiver.OnSignalReceived -= handler;
        handler = (sender, e) =>
        {
            receivedMessage += ZPIEncoding.GetString(e.Data);
        };
        TcpReceiver.OnSignalReceived += handler;

        TcpSender.TestEvents.InvokeEvent1(null, argsToFail);
        TcpSender.TestEvents.InvokeEvent1(null, argsToSucceed);
        await Task.Delay(3500);
        Assert.True(sender.ConnectionsInitialized >= 4);
        Assert.Equal(2, sender.ConnectionsHandled);
        Assert.DoesNotContain(messageToFail, receivedMessage);
        Assert.Contains(messageToSucceed, receivedMessage);

        //One last check since ConnectionsInitialized is not exactly deterministic while TcpSender is working
        sender.Disable();

        Assert.False(sender.CanSendMessages);
        //2 failed connections which were reattempted at most 3 times + 2 successful connections
        Assert.True(sender.ConnectionsInitialized <= (2 * 3) + 2);
        Assert.Equal(2, sender.ConnectionsHandled);
    }
}
