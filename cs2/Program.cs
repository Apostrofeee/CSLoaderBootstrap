using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

const string AppRegistryKey = @"Software\CSLoaderBootstrap";
const string LegacyAppRegistryKey = @"Software\GamesenseReloadedFix";
const string SavedSteamPathName = "SteamPath";
const string SteamRegistryKey = @"Software\Valve\Steam";
const string SteamExeValueName = "SteamExe";
const string SteamExeFileName = "steam.exe";
const string CsgoLegacyAppId = "4465480";
const string SteamFastLaunchArguments = "-silent -noverifyfiles -skipstreamingdrivers -vrdisable -nocrashmonitor -nofriendsui -nointro";

bool hasSteamArgument = HasSteamPathArgument(args);
string? steamPathFromArgs = ReadSteamPathFromArgs(args);
string? steamPath = FindSteamPath(steamPathFromArgs, hasSteamArgument, allowPrompt: false);

if (steamPath is not null)
    return LaunchGameBySteamExe(steamPath);

if (!hasSteamArgument && TryLaunchGameBySteamProtocol())
    return 0;

steamPath = FindSteamPath(steamPathFromArgs, hasSteamArgument, allowPrompt: true);

if (steamPath is null)
{
    EnsureConsole();
    Console.WriteLine("Путь к Steam не найден.");
    Console.WriteLine("Запустите программу с аргументом: --steam \"C:\\Program Files (x86)\\Steam\\steam.exe\"");
    WaitForKey();
    return 1;
}

return LaunchGameBySteamExe(steamPath);

static string? LoadSavedSteamPath()
{
    string? savedPath = LoadSavedSteamPathFromKey(AppRegistryKey);
    if (savedPath is not null)
        return savedPath;

    string? legacySavedPath = LoadSavedSteamPathFromKey(LegacyAppRegistryKey);
    if (legacySavedPath is not null)
        SaveSteamPath(legacySavedPath);

    return legacySavedPath;
}

static string? LoadSavedSteamPathFromKey(string registryKeyPath)
{
    using RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryKeyPath);

    object? value = key?.GetValue(SavedSteamPathName);
    string? path = NormalizePath(value?.ToString());

    return IsSteamExecutable(path) ? path : null;
}

static void SaveSteamPath(string path)
{
    using RegistryKey? key = Registry.CurrentUser.CreateSubKey(AppRegistryKey);
    key?.SetValue(SavedSteamPathName, path);
}

static string? FindSteamPath(string? pathFromArgs, bool hasSteamArgument, bool allowPrompt)
{
    if (pathFromArgs is not null && IsSteamExecutable(pathFromArgs))
    {
        SaveSteamPath(pathFromArgs);
        return pathFromArgs;
    }

    if (hasSteamArgument && allowPrompt)
    {
        EnsureConsole();
        Console.WriteLine("Переданный через --steam путь некорректен.");
    }

    string? savedPath = LoadSavedSteamPath();
    if (savedPath is not null)
        return savedPath;

    string? steamPath = FindSteamPathInRegistry();
    if (steamPath is not null)
    {
        SaveSteamPath(steamPath);
        return steamPath;
    }

    string? commonPath = FindSteamPathInCommonLocations();
    if (commonPath is not null)
    {
        SaveSteamPath(commonPath);
        return commonPath;
    }

    return allowPrompt ? AskForSteamPath() : null;
}

static string? FindSteamPathInRegistry()
{
    using RegistryKey? steamKey = Registry.CurrentUser.OpenSubKey(SteamRegistryKey);

    string? steamPath = NormalizePath(steamKey?.GetValue(SteamExeValueName)?.ToString());

    return IsSteamExecutable(steamPath) ? steamPath : null;
}

static string? FindSteamPathInCommonLocations()
{
    string[] candidates =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", SteamExeFileName),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", SteamExeFileName)
    ];

    return candidates.FirstOrDefault(IsSteamExecutable);
}

static string? AskForSteamPath()
{
    EnsureConsole();
    Console.WriteLine("Введите путь к steam.exe:");
    string? input = NormalizePath(Console.ReadLine());

    if (input is null || !IsSteamExecutable(input))
    {
        Console.WriteLine("Неверный путь к steam.exe.");
        return null;
    }

    Console.WriteLine("Сохранить этот путь для будущих запусков? (y/n)");
    string? answer = Console.ReadLine();

    if (IsYes(answer))
        SaveSteamPath(input);

    return input;
}

static bool TryLaunchGameBySteamProtocol()
{
    string commandProtocol = $"steam:\"{SteamFastLaunchArguments} -applaunch {CsgoLegacyAppId}\"";

    return TryStartShellCommand(commandProtocol)
        || TryStartShellCommand($"steam://rungameid/{CsgoLegacyAppId}");
}

static bool TryStartShellCommand(string fileName)
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = true
        })?.Dispose();

        return true;
    }
    catch
    {
        return false;
    }
}

static int LaunchGameBySteamExe(string steamPath)
{
    ProcessStartInfo startInfo = new()
    {
        FileName = steamPath,
        Arguments = $"{SteamFastLaunchArguments} -applaunch {CsgoLegacyAppId}",
        UseShellExecute = false
    };

    try
    {
        using Process? process = Process.Start(startInfo);
        if (process is not null)
            return 0;

        EnsureConsole();
        Console.WriteLine("Не удалось запустить Steam.");
        WaitForKey();
        return 2;
    }
    catch (Exception ex)
    {
        EnsureConsole();
        Console.WriteLine($"Ошибка запуска: {ex.Message}");
        WaitForKey();
        return 3;
    }
}

static bool HasSteamPathArgument(string[] args)
{
    return args.Any(arg => string.Equals(arg, "--steam", StringComparison.OrdinalIgnoreCase));
}

static string? ReadSteamPathFromArgs(string[] args)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], "--steam", StringComparison.OrdinalIgnoreCase))
            return NormalizePath(args[i + 1]);
    }

    return null;
}

static string? NormalizePath(string? path)
{
    if (string.IsNullOrWhiteSpace(path))
        return null;

    string expandedPath = Environment.ExpandEnvironmentVariables(path.Trim().Trim('"'));

    return Path.GetFullPath(expandedPath);
}

static bool IsSteamExecutable(string? path)
{
    return path is not null
        && File.Exists(path)
        && string.Equals(Path.GetFileName(path), SteamExeFileName, StringComparison.OrdinalIgnoreCase);
}

static bool IsYes(string? value)
{
    return value?.Trim().ToLowerInvariant() is "y" or "yes" or "д" or "да";
}

static void EnsureConsole()
{
    NativeMethods.AttachConsole(NativeMethods.AttachParentProcess);
    NativeMethods.AllocConsole();

    Console.InputEncoding = System.Text.Encoding.UTF8;
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    Console.SetIn(new StreamReader(Console.OpenStandardInput(), Console.InputEncoding));
    Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding) { AutoFlush = true });
    Console.SetError(new StreamWriter(Console.OpenStandardError(), Console.OutputEncoding) { AutoFlush = true });
}

static void WaitForKey()
{
    Console.WriteLine();
    Console.WriteLine("Нажмите любую клавишу для выхода...");
    Console.ReadKey(intercept: true);
}

internal static class NativeMethods
{
    internal const uint AttachParentProcess = 0xFFFFFFFF;

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool AllocConsole();
}
