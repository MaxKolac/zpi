using ZPIServer.API;

namespace ZPIServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            StartServer();
        }

        static void StartServer()
        {
            Task.Run(TcpHandler.BeginListening);
            Task.Run(SignalTranslator.BeginTranslating);
        }

        static void StopServer()
        {
            Task.Run(SignalTranslator.StopTranslating);
            Task.Run(TcpHandler.StopListening);
        }
    }
}