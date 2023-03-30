using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vestervang.DotNetBlueZ;

namespace DotNetBlueZTest1;

internal class Program
{
    private static readonly TimeSpan timeout = TimeSpan.FromSeconds(15);

    private static async Task Main(string[] args)
    {
        var scanSeconds = 15;
        var adapters = await BlueZManager.GetAdaptersAsync();
        var adapter = adapters.First();

        if (adapter is null)
        {
            Console.WriteLine("No bluetooth adapters found");
            return;
        }

        Console.WriteLine($"Scanning for {scanSeconds} seconds...");

        adapter.DeviceFound += DeviceFound;

        var props = new Dictionary<string, object>();
        props.Add("Transport", "le");
        props.Add("DuplicateData", false);
        await adapter.SetDiscoveryFilterAsync(props);

        await adapter.StartDiscoveryAsync();
        await Task.Delay(TimeSpan.FromSeconds(scanSeconds));
        await adapter.StopDiscoveryAsync();

        var devices = await adapter.GetDevicesAsync();
        Console.WriteLine($"{devices.Count} device(s) found.");
    }

    private static async Task DeviceFound(Adapter sender, DeviceFoundEventArgs eventArgs)
    {
        var deviceProperties = await eventArgs.Device.GetAllAsync();

        Console.WriteLine(
            $"[{DateTime.UtcNow}] Found device {deviceProperties.Name} with address {deviceProperties.Address}");
    }
}