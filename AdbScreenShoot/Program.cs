using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using TqkLibrary.AdbDotNet;
using TqkLibrary.WinApi.FindWindowHelper;

//scan exist adb/ldplayer process
var process = Process.GetProcessesByName("adb").FirstOrDefault();
if (process is null)
    process = Process.GetProcessesByName("dnplayer").FirstOrDefault();
if (process is not null)
{
    ProcessHelper processHelper = new ProcessHelper(process.Id);
    var win32_process = processHelper.Query_Win32_Process();
    if (!string.IsNullOrWhiteSpace(win32_process?.ExecutablePath))
    {
        if ("adb.exe".Equals(win32_process.Name))
        {
            Adb.AdbPath = win32_process.ExecutablePath;
        }
        else
        {
            var dirInfo = Directory.GetParent(win32_process.ExecutablePath);
            var adbPath = Directory.GetFiles(dirInfo!.FullName, "adb.exe").FirstOrDefault();
            if (File.Exists(adbPath))
                Adb.AdbPath = adbPath;
        }
    }
}


var devices = (await Adb.DevicesAsync()).Where(x => x.DeviceState == DeviceState.Device).ToList();
for (int i = 0; i < devices.Count; i++)
{
    Console.WriteLine($"{i}: {devices[i].DeviceId}");
}
int deviceIndex = -1;
while (deviceIndex < 0 || deviceIndex >= devices.Count)
{
    Console.Write($"Chose device:");
    string? choice = Console.ReadLine();
    int.TryParse(choice, out deviceIndex);
}
var device = devices[deviceIndex];
Adb adb = new Adb(device.DeviceId);
Console.WriteLine($"Enter to capture");
while (true)
{
    Console.ReadLine();
    Bitmap? bitmap = null;
    try
    {
        try
        {
            bitmap = await adb.ScreenShotExecOutAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.GetType().FullName}: {ex.Message}\r\n{ex.StackTrace}");
        }

        if (bitmap is null)
        {
            try
            {
                bitmap = await adb.ScreenShotPullAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().FullName}: {ex.Message}\r\n{ex.StackTrace}");
            }
        }

        if (bitmap is not null)
        {
            try
            {
                string fileName = $"{DateTime.Now:yyyy-MM-dd HH-mm-ss.fffff}.png";
                bitmap.Save(fileName, ImageFormat.Png);
                Console.WriteLine($"Saved {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().FullName}: {ex.Message}\r\n{ex.StackTrace}");
            }
        }
    }
    finally
    {
        bitmap?.Dispose();
    }
}