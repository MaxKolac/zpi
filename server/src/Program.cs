using System.Diagnostics;
using System.Net;
using ZPIServer.API;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer
{
    public static class Program
    {
        const string ServerConsolePrefix = "SERVER";

        static readonly CancellationTokenSource serverLifetimeToken = new();
        static readonly Logger logger = new();

        static TcpHandler? tcpHandler;
        static SignalTranslator? signalTranslator;

        public static int Main(string[] args)
        {
            StartServer(args);
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

        private static void StartServer(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            logger.Start();

            //Decyphering args
            if (args is not null && args.Length == 1)
            {
                try
                {
                    Settings.ServerAddress = IPAddress.Parse(args[0]);
                    logger.WriteLine($"Server will listen on IP address {args[0]}.", ServerConsolePrefix);
                }
                catch (FormatException)
                {
                    logger.WriteLine($"WARNING! Failed to format argument 0 to IPAddress. Server will launch on default value of 127.0.0.1.", ServerConsolePrefix);
                }
            }

            //Server initialization
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