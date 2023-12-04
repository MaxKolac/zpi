using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ZPIServer.API;
internal class ImageExtracter
{
    static String exiftoolLocation = "exiftool.exe";
    public static Task<Image?> getTrueImage(String filename)
    {
        byte[]? ImageBytes = getImageBytes(filename);
        if (ImageBytes == null)
        {
            return Task.FromResult<Image?>(null);
        }
        using (MemoryStream ms = new MemoryStream(ImageBytes))
        {
            return Task.FromResult<Image?>(Image.FromStream(ms));
        }
    }
    public static byte[]? getImageBytes(String filename)
    {
        String errData;
        StreamReader outData;
        String[] flags = {
            "s3",
            "b",
            "EmbeddedImage"
        };
        (outData, errData) = RunExiftool(flags, filename);
        if (IsError(errData))
        {
            return null;
        }
        byte[] imageBytes;
        FileStream? baseStream = outData.BaseStream as FileStream;
        if (baseStream is null)
        {
            return null;
        }
        int lastRead;
        using (MemoryStream ms = new MemoryStream())
        {
            byte[] buffer = new byte[4096];
            do
            {
                lastRead = baseStream.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, lastRead);
            } while (lastRead > 0);
            imageBytes = ms.ToArray();
        }
        return imageBytes;
    }

    static bool IsError(string errData)
    {
        String[] lines = errData.Split('\n');
        foreach (String line in lines)
        {
            if (!line.StartsWith("Warning: ") && !(line.Length == 0))
            {
                return true;
            }
        }
        return false;
    }

    static (StreamReader, string) RunExiftool(String[] flags, String filename)
    {
        Process cmd = new Process();
        StringBuilder errData = new StringBuilder();
        String command = exiftoolLocation;
        foreach (String flag in flags)
        {
            command += " -" + flag;
        }
        command += " " + filename;
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.Arguments = "/C " + command;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.RedirectStandardError = true;
        cmd.ErrorDataReceived += (sender, args) => errData.Append(args.Data ?? String.Empty);
        cmd.Start();
        cmd.BeginErrorReadLine();
        return (cmd.StandardOutput, errData.ToString());
    }
}
