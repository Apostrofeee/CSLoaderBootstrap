using System.Diagnostics;

ProcessStartInfo startInfo = new ProcessStartInfo
{
    FileName = "open",
    Arguments = "/System/Applications/Calculator.app"
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