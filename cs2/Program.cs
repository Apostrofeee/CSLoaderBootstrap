using System.Diagnostics;
using Microsoft.Win32;

const string AppRegistryKey = @"Software\GamesenseReloadedFix";
const string SavedSteamPathName = "SteamPath";

static string? LoadSavedSteamPath()
{
    using RegistryKey? key = Registry.CurrentUser.OpenSubKey(AppRegistryKey);

    object? value = key?.GetValue(SavedSteamPathName);
    string? path = value?.ToString();

    return File.Exists(path) ? path : null;
}

static void SaveSteamPath(string path)
{
    using RegistryKey? key = Registry.CurrentUser.CreateSubKey(AppRegistryKey);
    key?.SetValue(SavedSteamPathName, path);
}

static string? FindSteamPath()
{
    string? savedPath = LoadSavedSteamPath();
    if (savedPath is not null)
        return savedPath;

    using RegistryKey? steamKey =
        Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

    string? steamPath = steamKey?.GetValue("SteamExe")?.ToString();

    if (File.Exists(steamPath))
        return steamPath;

    Console.WriteLine("Enter path to steam.exe:");
    string? input = Console.ReadLine();

    if (input is null || !File.Exists(input))
    {
        Console.WriteLine("Invalid path.");
        return null;
    }

    Console.WriteLine("Save this path for future use? (y/n)");
    string? answer = Console.ReadLine();

    if (answer?.ToLower() == "y")
        SaveSteamPath(input);

    return input;
}

string? steamPath = FindSteamPath();

if (steamPath is null)
{
    Console.WriteLine("Steam path not found.");
    Console.ReadKey();
    return;
}

ProcessStartInfo startInfo = new ProcessStartInfo
{
    // You can manually change the Steam path here if the program fails to find it in the registry.
    // Example: "C:\Program Files (x86)\Steam\steam.exe"
    FileName = steamPath,
    Arguments = "-silent -applaunch 4465480",
};

try
{
    using Process? process = Process.Start(startInfo);
    Console.WriteLine(process is null
        ? "Не удалось запустить процесс."
        : $"Процесс запущен, PID={process.Id}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}