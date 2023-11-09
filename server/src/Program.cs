using ZPIServer.API;

namespace ZPIServer
{
    public class Program
    {
        static TcpHandler? tcpHandler;
        static SignalTranslator? signalTranslator;

        static void Main(string[] args)
        {
            StartServer();
            while (true)
            {
                //
            }
        }

        static void StartServer()
        {
            tcpHandler = new TcpHandler(Settings.ServerAddress, Settings.TcpListeningPort);
            tcpHandler.BeginListening();
            signalTranslator = new SignalTranslator();
            signalTranslator.BeginTranslating();
        }

        static void StopServer()
        {
            signalTranslator?.StopTranslating();
            tcpHandler?.StopListening();
        }
    }
}