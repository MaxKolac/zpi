using System.Diagnostics;
using ZPIServer.API;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer
{
    public static class Program
    {
        static readonly CancellationTokenSource serverLifetimeToken = new();
        static readonly Logger logger = new();

        static TcpHandler? tcpHandler;
        static SignalTranslator? signalTranslator;

        public static int Main(string[] args)
        {
            StartServer();
            while (!serverLifetimeToken.IsCancellationRequested)
            {
                Console.Write(">> ");
                string? input = Console.ReadLine(); //kurwa jego mać Microsoft
                logger.HandleCommand(input);
            }
            StopServer();
            return 0;
        }

        private static void OnCommandExecuted(object? sender, CommandEventArgs e)
        {
            if (sender is not null && sender is ShutdownCommand)
                serverLifetimeToken.Cancel();
        }

        private static void StartServer()
        {
            var stopwatch = Stopwatch.StartNew();
            logger.Start();
            tcpHandler = new TcpHandler(Settings.ServerAddress, Settings.TcpListeningPort, logger);
            tcpHandler.BeginListening();
            signalTranslator = new SignalTranslator(logger);
            signalTranslator.BeginTranslating();

            Command.OnExecuted += OnCommandExecuted;

            stopwatch.Stop();
            logger.WriteLine($"Done! {double.Round(stopwatch.Elapsed.TotalMilliseconds)} milliseconds elapsed.");
        }

        private static void StopServer()
        {
            logger?.WriteLine("Shutting the server down.");
            
            Command.OnExecuted -= OnCommandExecuted;

            signalTranslator?.StopTranslating();
            tcpHandler?.StopListening();
            logger?.Stop();
        }
    }
}