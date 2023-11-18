namespace ZPIServer.Commands;

/// <summary>
/// Klasa rozszerzająca <see cref="Console"/> o kilka usprawnień organizacyjnych. Przyjmuje i rozpoznaje polecenia użytkownika oraz wyświetla komunikaty wysłane przez inne klasy podczas egzekucji serwera.
/// </summary>
public class Logger
{
    private readonly CancellationTokenSource _token;
    private readonly SemaphoreSlim _accessSemaphore;
    private readonly Task _loggerTask;

    public Logger()
    {
        _token = new();
        _accessSemaphore = new(1, 1);
        _loggerTask = new Task(() =>
        {
            while (!_token.IsCancellationRequested)
            {
            }
        });
    }

    /// <summary>
    /// Przyjmuje linijke z <see cref="Console.ReadLine"/>, rozpoznaje w niej polecenie z argumentami i jeśli polecenie zostanie rozpoznane, wykonuje je.
    /// </summary>
    /// <param name="line">Linijka, w której znajduje się polecenie.</param>
    public void HandleCommand(string? line)
    {
        //Sanitize input line
        if (line is null)
            return;
        List<string> words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (words.Count == 0)
            return;

        //Recognize command
        Command? command = words[0] switch
        {
            Command.Help => new HelpCommand(this),
            Command.Shutdown => new ShutdownCommand(this),
            Command.Status => new StatusCommand(this),
            _ => null
        };
        if (command is null)
        {
            WriteLine($"Command '{words[0]}' unrecognized. Type '{Command.Help}' to get all available commands.");
        }
        words.RemoveAt(0);

        //Execute command
        command?.SetArguments(words.ToArray());
        command?.Execute();
    }

    public void Start() => _loggerTask.Start();

    public void Stop() => _token.Cancel();

    /// <summary>
    /// Wypisuje nową linijkę z podanym komunikatem w konsoli.
    /// </summary>
    /// <param name="message">Linijka do przekazania.</param>
    /// <param name="callingClass">Nazwa klasy wywołującej metodę. Pojawi się jako prefiks w nawiasach kwadratowych. Podanie wartości <c>null</c> pominie dodawanie prefiksu i wyświetli jedynie <paramref name="message"/>.</param>
    public void WriteLine(string? message, string? callingClass = null)
    {
        _accessSemaphore.Wait();

        //Remember where the cursor is so its position can be restored later
        int cursorOffset = Console.GetCursorPosition().Left;
        //Create a new empty line to copy currently entered user input into
        Console.WriteLine();
        //Move user input into new line
        var cursorPosition = Console.GetCursorPosition();
        if (OperatingSystem.IsWindows()) //supresses CA1416 
        {
            Console.MoveBufferArea(0, cursorPosition.Top - 1, Console.BufferWidth, 1, 0, cursorPosition.Top);
        }
        //Move cursor to the now empty line above user's input
        Console.SetCursorPosition(0, cursorPosition.Top - 1);
        //Clear character that used to be user's input
        for (int j = 0; j < cursorOffset; j++)
            Console.Write(' ');
        Console.SetCursorPosition(0, cursorPosition.Top - 1);
        //Print the output line  (What will happen if line is so long it would spill into user input???)
        Console.Write(callingClass is null ? $"{message}" : $"[{callingClass}] {message}");
        //Move cursor back at the beginning of user's input
        cursorPosition = Console.GetCursorPosition();
        Console.SetCursorPosition(cursorOffset, cursorPosition.Top + 1);

        _accessSemaphore.Release();
    }
}
