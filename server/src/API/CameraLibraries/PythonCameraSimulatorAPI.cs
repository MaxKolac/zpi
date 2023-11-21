﻿using System.Diagnostics;
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
        if (bytes is null || bytes.Length == 0)
            throw new ArgumentException("Received bytes were empty or null");

        if (CheckPythonInstallation() == -1)
            return;
    }

    public CameraDataMessage? GetDecodedMessage() => _message;

    /// <summary>
    /// Sprawdza czy obecny system ma zainstalowanego Python'a.
    /// </summary>
    /// <returns>
    /// 1, jeśli zainstalowany jest Python wersji 3 lub późniejszej.<br/>
    /// 0, jeśli zainstalowany jest Python w wersji starszej od 3.<br/>
    /// -1, jeśli nie wykryto żadnej instalacji Python'a.
    /// </returns>
    private int CheckPythonInstallation()
    {
        int returnCode = 1;
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

        if (!string.IsNullOrWhiteSpace(result))
        {
            string trimmedResult = result.Trim();
            if (result.Contains("Python 3"))
            {
                _logger?.WriteLine($"{trimmedResult} installation found.", nameof(PythonCameraSimulatorAPI));
            }
            else if (result.Contains("Python"))
            {
                _logger?.WriteLine($"Outdated {trimmedResult} installation found. Things might break!", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Warning);
                returnCode = 0;
            }
        }
        else
        {
            _logger?.WriteLine("Could not find 'python.exe' executable.", nameof(PythonCameraSimulatorAPI), Logger.MessageType.Error);
            returnCode = -1;
        }
        return returnCode;
    }

    /// <summary>
    /// Sprawdza obecność tych zależności w danych wersjach:
    /// <code>
    /// Pillow 10.0.0
    /// numpy 1.25.0
    /// opencv-contrib-python 4.8.1.78
    /// python-dotenv 1.0.0
    /// </code>
    /// </summary>
    private void CheckPythonPackagesInstallation()
    {
        var dependencies = new string[]
        {
            "Pillow==10.0.0",
            "numpy==1.25.0",
            "opencv-contrib-python==4.8.1.78",
            "python-dotenv==1.0.0"
        };

        var startInfos = new ProcessStartInfo[dependencies.Length];
        for (int i = 0; i < startInfos.Length; i++)
        {
            startInfos[i] = new ProcessStartInfo()
            {
                FileName = @"python.exe",
                Arguments = "-m pip install " + dependencies[i],
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
        }
        
        foreach (var startInfo in startInfos) 
        {
            using var process = Process.Start(startInfo);
            using var standardReader = process!.StandardOutput;
            using var errorReader = process!.StandardError;
            Task standardOutput = Task.Run(() =>
            {
                while (!standardReader.EndOfStream)
                {
                    _logger?.WriteLine(standardReader.ReadLine(), PythonPrefix);
                }
            });
            Task errorOutput = Task.Run(() =>
            {
                while (!errorReader.EndOfStream)
                {
                    _logger?.WriteLine(errorReader.ReadLine(), PythonPrefix, Logger.MessageType.Error);
                }
            });
            Task.WaitAll(standardOutput, errorOutput);
        }
    }
}
