using System.Diagnostics;
using ZPICommunicationModels.Messages;
using ZPIServer.Commands;

namespace ZPIServer.API.CameraLibraries;

public class PythonCameraSimulatorAPI : ICamera
{
    private const string PythonPrefix = "Python";
    private readonly Logger? _logger;
    private CameraDataMessage? _message;

    private record ScriptResult(decimal HottestTemperature, decimal Percentage);

    public PythonCameraSimulatorAPI(Logger? logger = null)
    {
        _logger = logger;
    }

    public void DecodeReceivedBytes(byte[]? bytes)
    {
        if (!Settings.CanPythonCameraAPIScriptsRun)
        {
            _logger?.WriteLine($"Server was started without detecting a Python installation! Ignoring received bytes.", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Warning);
            return;
        }

        if (bytes is null || bytes.Length == 0)
            throw new ArgumentException("Received bytes were empty or null");

        //DONE: Migrate DB to acommodate for new property Percentage
        //DONE: Change CameraDataMessage to acommodate for Percentage (will break ZPIClient)
        //TODO: Add toggling between simulating Camera and PythonCamera in ZPICameraSimulator
        //DONE: Make compiler copy the python script and exiftool.exe to this folder
        //TODO: Add Filip's script to start exiftool to extract the RJPG image (can be run independently)

        //On server startup:
        //DONE: Check PATH sys variable contains folder pythonScripts whic has the exiftool.exe
        //DONE: Check all required python scripts are present

        //Decypher the bytes
        //Save/overwrite the received bytes as raw file photo next to the script <- script from Filip
        //Run script and await its completion
        //Look for the resulting JSON
        //Try to deserialize it into ScriptResult
        //Build a CameraDataMessage object out of deserialized results
    }

    public CameraDataMessage? GetDecodedMessage() => _message;

    public static bool CheckIfScriptsCanBeRun(Logger? logger = null)
    {
        return CheckPythonInstallation(logger) && CheckPythonPackagesInstallation(logger) && CheckPythonScripts(logger) && CheckExiftool(logger);
    }

    /// <summary>
    /// Sprawdza czy obecny system ma zainstalowanego Python'a.
    /// </summary>
    /// <returns>
    /// true, jeśli wykryto instalację Python'a. W przeciwnym wypadku, false.
    /// </returns>
    private static bool CheckPythonInstallation(Logger? logger = null)
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
    private static bool CheckPythonPackagesInstallation(Logger? logger = null)
    {
        var dependencies = new string[]
        {
            "Pillow==10.1.0",
            "numpy==1.26.2",
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

        bool wereAllErrorOutputsEmpty = true;
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
                    if (errorReader.BaseStream.Length >= 0)
                        wereAllErrorOutputsEmpty = false;
                }
            });
            Task.WaitAll(standardOutput, errorOutput);
        }
        return wereAllErrorOutputsEmpty;
    }

    /// <summary>
    /// Sprawdza czy serwer posiada wszystkie potrzebne skrypty w języku Python gdyby komuś się zachciało aby jeden usunąć tak dla śmiechu.
    /// </summary
    private static bool CheckPythonScripts(Logger? logger = null)
    {
        string relativePath = Path.Combine(Environment.CurrentDirectory, "API", "CameraLibraries", "pythonScripts");

        string[] scriptFilenames =
        {
            "communicator.py",
            "constants.py",
            "oldParser.py",
            "show_working_areas.py",
            "thermalImageParser.py",
            "utils.py"
        };

        bool areAllScriptsPresent = false;
        foreach (var filename in scriptFilenames)
        {
            areAllScriptsPresent = Path.Exists(Path.Combine(relativePath, filename));
            if (areAllScriptsPresent)
            {
                logger?.WriteLine($"Script {filename} is present in {relativePath}.", nameof(PythonCameraSimulatorAPI));
            }
            else
            {
                logger?.WriteLine($"Script {filename} was not found in {relativePath}! {nameof(PythonCameraSimulatorAPI)} will not be able to function properly!", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Error);
                break;
            }
        }
        return areAllScriptsPresent;
    }

    /// <summary>
    /// Sprawdza czy serwer ma dostęp do narzędzia "exiftool.exe" oraz czy folder, w którym się znajduje jest dopisany do zmiennej systemowej "PATH".
    /// </summary>
    /// <returns><c>true</c>, jeśli wszystko poszło OK.</returns>
    private static bool CheckExiftool(Logger? logger = null)
    {
        string executablePath = Path.Combine(Environment.CurrentDirectory, "API", "CameraLibraries", "pythonScripts", "exiftool.exe");

        //Check if executable is where its meant to be
        bool executableIsPresent = Path.Exists(executablePath);
        if (executableIsPresent)
        {
            logger?.WriteLine($"exiftool.exe found at {executablePath}.", nameof(PythonCameraSimulatorAPI));
        }
        else
        {
            logger?.WriteLine($"exiftool.exe could not be located at {executablePath}!", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Error);
            return false;
        }

        //Check if PATH variable contains a folder to it
        bool sysVarPointsToExecutable = false;
        if (!sysVarPointsToExecutable)
        {
            Environment.SetEnvironmentVariable("PATH", Path.GetFullPath(executablePath));
            sysVarPointsToExecutable = Environment.GetEnvironmentVariable("PATH")?.Contains(executablePath) ?? false;
        }
        
        if (sysVarPointsToExecutable)
        {
            logger?.WriteLine($"Successfully added exiftool.exe path to PATH environment variable.", nameof(PythonCameraSimulatorAPI));
        }
        else
        {
            logger?.WriteLine($"Failed to add exiftool.exe path to PATH environment variable.", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Error);
        }

        return executableIsPresent && sysVarPointsToExecutable;
    }
}
