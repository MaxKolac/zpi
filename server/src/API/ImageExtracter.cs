using System.Diagnostics;
using System.Drawing;
using System.Text;
using ZPIServer.Commands;

namespace ZPIServer.API;

public static class ImageExtracter
{
    public static Image? GetEmbeddedImage(string workingDirectory, string inputFile, Logger? logger = null)
    {
        //Build the command string
        string[] flags =
        {
            "s3",
            "b",
            "EmbeddedImage"
        };
        string command = Path.Combine(workingDirectory, "exiftool.exe");
        foreach (string flag in flags)
        {
            command += " -" + flag;
        }
        command += " " + Path.Combine(workingDirectory, inputFile);

        //Build process info
        var startInfo = new ProcessStartInfo()
        {
            FileName = @"cmd.exe",
            Arguments = "/C " + command,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        //Start exiftool
        //The EmbeddedImage will be streamed into the standardOutput as bytes
        using var process = Process.Start(startInfo);
        using var standardReader = process!.StandardOutput;
        using var errorReader = process!.StandardError;

        var builder = new StringBuilder();
        Task<byte[]> standardOutput = Task.Run(() =>
        {
            //Collect, sanitize and assemble bytes from exiftool.exe
            using var memoryStream = new MemoryStream();
            List<byte> imageBytes = new();
            byte[] buffer = new byte[4096];
            while (standardReader.BaseStream.Read(buffer, 0, buffer.Length) != 0)
            {
                memoryStream.Write(buffer, 0, buffer.Length);
                imageBytes.AddRange(buffer);
                buffer = new byte[4096];
            }

            //Przy ostatniej iteracji pętli while, buffer na 99% będzie miał jakieś zerowe bajty na końcu.
            //Trzeba je wyciąć.
            int emptyBytesPosition = 0;
            for (int i = imageBytes.Count - 1; i >= 0; i--)
            {
                if (imageBytes[i] != 0)
                {
                    emptyBytesPosition = i;
                    break;
                }
            }

            return imageBytes.ToArray()[0..emptyBytesPosition];
        });
        Task errorOutput = Task.Run(() =>
        {
            while (!errorReader.EndOfStream)
            {
                logger?.WriteLine(errorReader.ReadLine());
            }
        });
        Task.WaitAll(standardOutput, errorOutput);

#pragma warning disable CA1416
        return Image.FromStream(new MemoryStream(standardOutput.Result));
#pragma warning restore CA1416
    }
}
