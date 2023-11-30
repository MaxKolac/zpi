using System.Diagnostics;
using ZPICommunicationModels.Messages;
using ZPIServer.Commands;

namespace ZPIServer.API.CameraLibraries;

public class PythonCameraSimulatorAPI : ICamera
{
    private const string PythonPrefix = "Python";
    private readonly Logger? _logger;
    private CameraDataMessage? _message;

    public PythonCameraSimulatorAPI(Logger? logger = null)
    {
        _logger = logger;
    }

    public void DecodeReceivedBytes(byte[]? bytes)
    {
        if (!Settings.IsPythonDetected)
        {
            _logger?.WriteLine($"Server was started without detecting a Python installation! Ignoring received bytes.", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Warning);
            return;
        }

        if (bytes is null || bytes.Length == 0)
            throw new ArgumentException("Received bytes were empty or null");

        //do stuff
    }

    public CameraDataMessage? GetDecodedMessage() => _message;

    /// <summary>
    /// Sprawdza czy obecny system ma zainstalowanego Python'a.
    /// </summary>
    /// <returns>
    /// true, jeśli wykryto instalację Python'a. W przeciwnym wypadku, false.
    /// </returns>
    public static bool CheckPythonInstallation(Logger? logger = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = @"python.exe",
            Arguments = "--version",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        string result = string.Empty;
        using (var process = Process.Start(startInfo))
        {
            using var reader = process!.StandardOutput;
            result = reader.ReadToEnd();
        }

        bool isPythonDetected;
        string trimmedResult = result.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedResult) && trimmedResult.Contains("Python"))
        {
            logger?.WriteLine($"{trimmedResult} installation found.", nameof(PythonCameraSimulatorAPI));
            isPythonDetected = true;
        }
        else
        {
            logger?.WriteLine("Could not find 'python.exe' executable.", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Error);
            isPythonDetected = false;
        }
        return isPythonDetected;
    }

    /// <summary>
    /// Sprawdza obecność tych zależności skryptu w Python'ie. W razie potrzeby, instaluje je.<br/>
    /// Pełna lista zależności wraz z wymaganymi wersjami znajduje się wewnątrz metody, w tabeli <c>dependencies</c>.
    /// </summary>
    public static void CheckPythonPackagesInstallation(Logger? logger = null)
    {
        var dependencies = new string[]
        {
            "Pillow==10.0.0",
            "numpy==1.26.2",
            "opencv-contrib-python==4.8.1.78", //requires numpy >= 1.21.2
            "python-dotenv==1.0.0"
        };

        //Setup cmd.exe processes which will install those dependencies
        var startInfos = new ProcessStartInfo[dependencies.Length];
        for (int i = 0; i < startInfos.Length; i++)
        {
            startInfos[i] = new ProcessStartInfo()
            {
                FileName = @"python.exe",
                Arguments = "-m pip install " + dependencies[i],
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        }

        //Attempt to install them and redirect all the output into ZPIServer's Logger
        logger?.WriteLine("Attempting to install required dependencies.", nameof(PythonCameraSimulatorAPI));
        foreach (var startInfo in startInfos)
        {
            using var process = Process.Start(startInfo);
            using var standardReader = process!.StandardOutput;
            using var errorReader = process!.StandardError;
            Task standardOutput = Task.Run(() =>
            {
                while (!standardReader.EndOfStream)
                {
                    logger?.WriteLine(standardReader.ReadLine(), PythonPrefix);
                }
            });
            Task errorOutput = Task.Run(() =>
            {
                while (!errorReader.EndOfStream)
                {
                    logger?.WriteLine(errorReader.ReadLine(), PythonPrefix, Logger.MessageType.Error);
                }
            });
            Task.WaitAll(standardOutput, errorOutput);
        }
    }
}
