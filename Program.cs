using System.Diagnostics;

string gamePath = "/System/Applications/Calculator.app";

ProcessStartInfo startInfo = new ProcessStartInfo
{
    FileName = "open",
    Arguments = gamePath
};

using Process? process = Process.Start(startInfo);
if (process is null)
{
    Console.WriteLine("Не удалось запустить процесс.");
}
else
{
    Console.WriteLine($"Процесс запущен, PID={process.Id}");
}