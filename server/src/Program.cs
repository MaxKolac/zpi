using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using ZPICommunicationModels.Models;
using ZPIServer.API;
using ZPIServer.API.CameraLibraries;
using ZPIServer.Commands;

namespace ZPIServer
{
    public static class Program
    {
        const string ServerPrefix = "SERVER";

        static readonly CancellationTokenSource serverLifetimeToken = new();
        static readonly Logger logger = new();

        static TcpReceiver? tcpReceiver;
        static TcpSender? tcpSender;
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

        private static void OnCommandExecuted(object? sender, System.EventArgs e)
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
                    logger.WriteLine($"Server will listen on IP address {args[0]}.", ServerPrefix);
                }
                catch (FormatException)
                {
                    logger.WriteLine($"Failed to format argument 0 to IPAddress. Server will launch on default value of 127.0.0.1.", ServerPrefix, Logger.MessageType.Warning);
                }
            }

            //Getting Python and dependencies for PythonCameraSimulatorApi
            Settings.CanPythonCameraAPIScriptsRun = PythonCameraSimulatorAPI.CheckIfScriptsCanBeRun(logger);
            if (Settings.CanPythonCameraAPIScriptsRun)
            {
                logger.WriteLine($"{nameof(PythonCameraSimulatorAPI)} did not report any missing dependent components - server will enable it.", ServerPrefix);
            }
            else
            {
                logger.WriteLine($"Until all necessary components are properly installed and server is restarted, server will ignore messages from {HostDevice.HostType.PythonCameraSimulator} devices!", ServerPrefix, Logger.MessageType.Warning);
            }

            using (var context = new DatabaseContext())
            {
                context.Database.Migrate();
            }

            //Components initialization
            tcpReceiver = new TcpReceiver(Settings.ServerAddress, Settings.TcpReceiverPorts, logger);
            tcpReceiver.Enable();
            tcpSender = new TcpSender(logger);
            tcpSender.Enable();
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
            tcpSender?.Disable();
            tcpReceiver?.Disable();
            logger?.Stop();
        }
    }
}