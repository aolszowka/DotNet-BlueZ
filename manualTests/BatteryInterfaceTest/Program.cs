using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProrepubliQ.DotNetBlueZ;

internal class Program
{
    private static readonly string DeviceToConnectTo = "E8:BF:FC:E0:05:FA";

    private static async Task Main(string[] args)
    {
        var adapters = await BlueZManager.GetAdaptersAsync();

        if (adapters.Count == 0)
        {
            throw new Exception("No Bluetooth adapters found.");
        }

        IAdapter1 adapter = adapters.First();

        var adapterPath = adapter.ObjectPath.ToString();
        var adapterName = adapterPath.Substring(adapterPath.LastIndexOf("/", StringComparison.Ordinal) + 1);
        Console.WriteLine($"Using Bluetooth adapter {adapterName}");

        var dict = new Dictionary<string, object>();
        dict.Add("Address", DeviceToConnectTo);
        dict.Add("AddressType", "random");

        try
        {
            await adapter.ConnectDeviceAsync(dict);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        var device1 = await adapter.GetDeviceAsync(DeviceToConnectTo);

        try
        {
            device1.Connected += ConnectedAsync;
            device1.Disconnected += DisconnectedAsync;
            device1.ServicesResolved += ServicesResolvedAsync;
            await device1.ConnectAsync();
            Console.WriteLine($"Connected to device {DeviceToConnectTo}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        
        await Task.Delay(10000);
    }

    private static async Task ConnectedAsync(Device device, BlueZEventArgs e)
    {
        Console.WriteLine("Device connected");
    }

    private static async Task DisconnectedAsync(Device device, BlueZEventArgs e)
    {
        Console.WriteLine("Device disconnected");
    }

    private static async Task ServicesResolvedAsync(Device device, BlueZEventArgs e)
    {
        Console.WriteLine("Services resolved");
        
        var battery = await device.GetBatteryAsync();
        var percentage = await battery.GetPercentageAsync();
        
        
        Console.WriteLine($"Battery percentage: {percentage}");
    }
}
