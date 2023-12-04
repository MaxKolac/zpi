using System.Diagnostics;
using System.Text;
using System.Drawing;

namespace ZPIServer.API;

public static class ImageExtracter
{
    private static readonly string exiftoolLocation = "exiftool.exe";

    public static Image? GetTrueImage(string filename)
    {
#pragma warning disable CA1416
        byte[]? imageBytes = GetImageBytes(filename);
        if (imageBytes == null)
        {
            return null;
        }
        using var memoryStream = new MemoryStream(imageBytes);
        return Image.FromStream(memoryStream);
#pragma warning restore CA1416
    }

    public static byte[]? GetImageBytes(string filename)
    {
        string errData;
        StreamReader outData;
        string[] flags =
        {
            "s3",
            "b",
            "EmbeddedImage"
        };

        (outData, errData) = RunExiftool(flags, filename);
        if (IsError(errData) || outData.BaseStream is not FileStream baseStream)
        {
            return null;
        }

        byte[] imageBytes;
        int lastRead;
        using (var memoryStream = new MemoryStream())
        {
            byte[] buffer = new byte[4096];
            do
            {
                lastRead = baseStream.Read(buffer, 0, buffer.Length);
                memoryStream.Write(buffer, 0, lastRead);
            }
            while (lastRead > 0);
            imageBytes = memoryStream.ToArray();
        }
        return imageBytes;
    }

    static bool IsError(string errData)
    {
        string[] lines = errData.Split('\n');
        foreach (string line in lines)
        {
            if (!line.StartsWith("Warning: ") && !(line.Length == 0))
            {
                return true;
            }
        }
        return false;
    }

    static (StreamReader, string) RunExiftool(string[] flags, string filename)
    {
        var process = new Process();
        var errData = new StringBuilder();

        string command = exiftoolLocation;
        foreach (string flag in flags)
        {
            command += " -" + flag;
        }
        command += " " + filename;

        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/C " + command;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (sender, args) => errData.Append(args.Data ?? string.Empty);
        process.Start();
        process.BeginErrorReadLine();

        return (process.StandardOutput, errData.ToString());
    }
}
