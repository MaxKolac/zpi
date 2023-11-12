using System.Diagnostics;
using ZPIServer.API;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer
{
    public static class Program
    {
        static TcpHandler? tcpHandler;
        static SignalTranslator? signalTranslator;


        public static int Main(string[] args)
        {
            StartServer();
            while (true)
            {
                //server lifetime loop
                Console.Write(">> ");
                string? consoleInput = Console.ReadLine();
                HandleCommand(consoleInput);
            }
            StopServer();
            return 0;
        }

        private static void StartServer()
        {
            var stopwatch = Stopwatch.StartNew();
            tcpHandler = new TcpHandler(Settings.ServerAddress, Settings.TcpListeningPort);
            tcpHandler.BeginListening();
            signalTranslator = new SignalTranslator();
            signalTranslator.BeginTranslating();
            stopwatch.Stop();

            OnCommandExecuted += OnCommandExecutionInvoke;

            Console.WriteLine($"Done! Server took {stopwatch.Elapsed.TotalMilliseconds} milliseconds to start up.");
        }

        static void StopServer()
            command.Execute();
        }

        private static void StopServer()
        {
            signalTranslator?.StopTranslating();
            tcpHandler?.StopListening();
        }
    }
}