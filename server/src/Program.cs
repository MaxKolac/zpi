using System.Diagnostics;
using ZPIServer.API;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer
{
    public static class Program
    {
        static readonly CancellationTokenSource token = new();

        static TcpHandler? tcpHandler;
        static SignalTranslator? signalTranslator;

        public static event EventHandler<CommandEventArgs>? OnCommandExecuted;

        public static int Main(string[] args)
        {
            StartServer();
            while (!token.IsCancellationRequested)
            {
                //server lifetime loop
                Console.Write(">> ");
                string? consoleInput = Console.ReadLine();
                HandleCommand(consoleInput);
            }
            StopServer();
            return 0;
        }

        private static void OnCommandExecutionInvoke(object? sender, CommandEventArgs e)
        {

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

        private static void HandleCommand(string? line)
        {
            //Sanitize input line
            if (line is null)
            {
                Console.WriteLine("Command was null.");
                return;
            }
            List<string> words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            if (words.Count == 0)
            {
                Console.WriteLine("Command was empty.");
                return;
            }

            //Recognize command
            Command? command = words[0] switch
            {
                Command.HelpCommand => new HelpCommand(),
                Command.ShutdownCommand => new ShutdownCommand(),
                _ => null
            };

            if (command is null)
            {
                command = new HelpCommand();
                Console.WriteLine($"Command {words[0]} unrecognized.");
            }
            words.RemoveAt(0);
            command.SetArguments(words.ToArray());
            command.Execute();
        }

        private static void StopServer()
        {
            signalTranslator?.StopTranslating();
            tcpHandler?.StopListening();

            OnCommandExecuted -= OnCommandExecutionInvoke;
        }
    }
}