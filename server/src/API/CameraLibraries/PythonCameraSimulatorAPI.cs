using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq.Expressions;
using ZPICommunicationModels.Messages;
using ZPICommunicationModels.Models;
using ZPIServer.Commands;

namespace ZPIServer.API.CameraLibraries;

public class PythonCameraSimulatorAPI : ICamera
{
    private const string PythonPrefix = "Python";
    private readonly Logger? _logger;
    private CameraDataMessage? _message;

    private static readonly string AbsoluteScriptsDirectory = Path.Combine(Environment.CurrentDirectory, "API", "CameraLibraries", "pythonScripts");
    private static readonly string InputFilename = "input.jpg";
    private static readonly string ScriptFilename = "communicator.py";
    private static readonly string OutputFilename = "output.json";

    private record ScriptResult(decimal HottestTemperature, decimal Percentage);

    public class PythonApiException : Exception
    {
        public PythonApiException(string message) : this(message, null)
        {
        }

        public PythonApiException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    public PythonCameraSimulatorAPI(Logger? logger = null)
    {
        _logger = logger;
    }

    public void DecodeReceivedBytes(byte[]? bytes)
    {
        //Can the current environment even run this method?
        if (!Settings.CanPythonCameraAPIScriptsRun)
        {
            _logger?.WriteLine($"During server startup, some required components were not detected! Ignoring received bytes.", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Warning);
            return;
        }

        //Are the received bytes ok to process?
        if (bytes is null || bytes.Length == 0)
            throw new PythonApiException("Received bytes were empty or null");

        //Gather the bytes and save/overwrite them as a raw file next to the script
        using (var writer = File.Create(Path.Combine(AbsoluteScriptsDirectory, InputFilename)))
        {
            writer.Write(bytes);
        }

        //Try to extract the embedded image - obsolete
        //var embeddedImage =
        //    ImageExtracter.GetEmbeddedImage(AbsoluteScriptsDirectory, InputFilename, _logger) ??
        //    throw new NullReferenceException("Failed to extract embedded image!");

        //Run thermalImageParser script and await its completion
        var startInfo = new ProcessStartInfo()
        {
            FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
            Arguments = $"/C python {ScriptFilename} --filename {InputFilename} --save {OutputFilename}",
            WorkingDirectory = AbsoluteScriptsDirectory,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        //Start the process and log any errors/events
        //If things go wrong, throw
        using (var process = Process.Start(startInfo))
        {
            using var standardReader = process!.StandardOutput;
            using var errorReader = process!.StandardError;

            Task standardOutput = Task.Run(() =>
            {
                while (!standardReader.EndOfStream)
                {
                    _logger?.WriteLine(standardReader.ReadLine(), PythonPrefix);
                }
            });
            Task<bool> errorOutput = Task.Run(() =>
            {
                bool wereAllErrorOutputsEmpty = true;
                while (!errorReader.EndOfStream)
                {
                    string? error = errorReader.ReadLine();
                    _logger?.WriteLine(error, PythonPrefix, Logger.MessageType.Error);
                    if (!string.IsNullOrEmpty(error))
                        wereAllErrorOutputsEmpty = false;
                    _logger?.WriteLine(errorReader.ReadLine(), PythonPrefix, Logger.MessageType.Error);
                }
                return wereAllErrorOutputsEmpty;
            });
            Task.WaitAll(standardOutput, errorOutput);
            if (!errorOutput.Result)
                throw new PythonApiException("Python script failed to extract data from provided message.");
        }

        //Look for the resulting JSON
        string json;
        try 
        {
            using var stream = File.OpenText(Path.Combine(AbsoluteScriptsDirectory, OutputFilename));
            json = stream.ReadToEnd().Replace(" ", string.Empty);
        }
        catch (IOException ex)
        {
            throw new PythonApiException("Could not locate the output Json file.", ex);
        }


        //Try to deserialize it into ScriptResult
        ScriptResult? scriptResult;
        try 
        {
            scriptResult =
                JsonConvert.DeserializeObject<ScriptResult>(json) ??
                throw new NullReferenceException("JsonConvert returned a null object.");
        }
        catch (Exception ex) when (ex is JsonSerializationException || ex is NullReferenceException)
        {
            throw new PythonApiException($"Failed to deserialize the resulting output into a {nameof(ScriptResult)} object.", ex);
        }

        //Build a CameraDataMessage object out of deserialized results
        _message = new CameraDataMessage()
        {
            LargestTemperature = scriptResult!.HottestTemperature,
            ImageVisibleDangerPercentage = scriptResult!.Percentage,
            Image = bytes,
            Status = HostDevice.DeviceStatus.OK
        };
    }

    public CameraDataMessage? GetDecodedMessage() => _message;

    public static bool CheckIfScriptsCanBeRun(Logger? logger = null)
    {
        return CheckPythonInstallation(logger) && CheckPythonPackagesInstallation(logger) && CheckPythonScripts(logger);// && CheckExiftool(logger);
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
            "requests",
            "Pillow==10.1.0",
            "numpy==1.26.2"
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
                    string? error = errorReader.ReadLine();
                    logger?.WriteLine(error, PythonPrefix, Logger.MessageType.Error);
                    if (!string.IsNullOrEmpty(error))
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
            areAllScriptsPresent = Path.Exists(Path.Combine(AbsoluteScriptsDirectory, filename));
            if (areAllScriptsPresent)
            {
                logger?.WriteLine($"Script {filename} is present in {AbsoluteScriptsDirectory}.", nameof(PythonCameraSimulatorAPI));
            }
            else
            {
                logger?.WriteLine($"Script {filename} was not found in {AbsoluteScriptsDirectory}! {nameof(PythonCameraSimulatorAPI)} will not be able to function properly!", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Error);
                break;
            }
        }
        return areAllScriptsPresent;
    }

    /// <summary>
    /// Sprawdza czy serwer ma dostęp do narzędzia "exiftool.exe" oraz czy folder, w którym się znajduje jest dopisany do zmiennej systemowej "PATH".
    /// </summary>
    /// <returns><c>true</c>, jeśli wszystko poszło OK.</returns>
    [Obsolete]
    private static bool CheckExiftool(Logger? logger = null)
    {
        string executablePath = Path.Combine(AbsoluteScriptsDirectory, "exiftool.exe");

        //Check if executable is where its meant to be
        bool executableIsPresent = Path.Exists(executablePath);
        if (executableIsPresent)
        {
            logger?.WriteLine($"exiftool.exe is present at {executablePath}.", nameof(PythonCameraSimulatorAPI));
        }
        else
        {
            logger?.WriteLine($"exiftool.exe could not be located at {executablePath}!", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Error);
            return false;
        }
        return executableIsPresent;
    }
}
