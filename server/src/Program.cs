using System.Diagnostics;
using System.Net;
using ZPICommunicationModels.Models;
using ZPIServer.API;
using ZPIServer.API.CameraLibraries;
using ZPIServer.Commands;
using ZPIServer.EventArgs;

namespace ZPIServer
{
    public static class Program
    {
        const string ServerPrefix = "SERVER";

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
                    logger.WriteLine($"Server will listen on IP address {args[0]}.", ServerPrefix);
                }
                catch (FormatException)
                {
                    logger.WriteLine($"Failed to format argument 0 to IPAddress. Server will launch on default value of 127.0.0.1.", ServerPrefix, Logger.MessageType.Warning);
                }
            }

            //Getting Python and dependencies for PythonCameraSimulatorApi
            Settings.PythonInstallationStatus = PythonCameraSimulatorAPI.CheckPythonInstallation(logger);
            switch (Settings.PythonInstallationStatus)
            {
                case -1:
                    logger.WriteLine($"Until Python is properly installed and server restarted, server will ignore messages from {HostDevice.HostType.PythonCameraSimulator} devices!", ServerPrefix, Logger.MessageType.Warning);
                    break;
                case 0:
                    logger.WriteLine($"Outdated Python installation detected, server will attempt to parse messages from {HostDevice.HostType.PythonCameraSimulator} devices with no guarantee of success!", ServerPrefix, Logger.MessageType.Warning);
                    PythonCameraSimulatorAPI.CheckPythonPackagesInstallation(logger);
                    break;
                case 1:
                    logger.WriteLine($"Python installation detected.", ServerPrefix);
                    PythonCameraSimulatorAPI.CheckPythonPackagesInstallation(logger);
                    break;
            }

            //Components initialization
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