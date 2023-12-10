using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace ZPICommunicationModels;

public static class ImageExtracter
{
    /// <summary>
    /// Wyciąga zwykłe zdjęcie ze zdjęcia termicznego. Wymaga programu 'exiftool.exe'. Program 'exiftool.exe' i samo zdjęcie muszą znajdować się w tym samym katalogu.<br/>
    /// Autor: Genocider7 (Filip)
    /// </summary>
    /// <param name="workingDirectory">Ściezka do folderu, w którym znajduje się 'exiftool.exe'. Użyj kombinacji <c>Path.Combine(Environment.CurrentDirectory, "podfolder1", "podfolder2", ...);</c>.</param>
    /// <param name="inputFile"></param>
    /// <returns>Zwraca obiekt <see cref="Image"/> utworzony z danych wyjściowych 'exiftool.exe'.</returns>
    public static Image? GetEmbeddedImage(string workingDirectory, string inputFile)
    {
        //Build the command string
        //When passing paths to files/folders as args for CMD with /C, both whole argument list needs to be in quotation marks
        //and each individual path, so like this:
        /*
         * cmd.exe /C ""D:\\GitHub Projects\\zpi\\client\\src\\bin\\Debug\\net7.0-windows10.0.17763.0\\exiftool.exe" -s3 -b -EmbeddedImage "D:\\GitHub Projects\\zpi\\client\\src\\bin\\Debug\\net7.0-windows10.0.17763.0\\temp.jpg"" 
         */
        string[] flags =
        {
            "s3",
            "b",
            "EmbeddedImage"
        };
        string command = "\"\"" + Path.Combine(workingDirectory, "exiftool.exe") + "\"";
        foreach (string flag in flags)
        {
            command += " -" + flag;
        }
        command += " \"" + Path.Combine(workingDirectory, inputFile) + "\"\"";

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
            int lastNonZeroBytePosition = imageBytes.Count - 1;
            while (lastNonZeroBytePosition >= 0 && imageBytes[lastNonZeroBytePosition] == 0)
            {               
                lastNonZeroBytePosition--;
            }

            return imageBytes.ToArray()[0..(lastNonZeroBytePosition + 1)];
        });
        Task errorOutput = Task.Run(() =>
        {
            while (!errorReader.EndOfStream)
            {
                string? s = errorReader.ReadLine();
                Console.WriteLine(s);
            }
        });
        Task.WaitAll(standardOutput, errorOutput);

#pragma warning disable CA1416
        return Image.FromStream(new MemoryStream(standardOutput.Result));
#pragma warning restore CA1416
    }
}
