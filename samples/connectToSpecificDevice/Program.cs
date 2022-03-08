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

        var devices = await adapter.GetDevicesAsync();

        foreach (var device in devices)
        {
            var address = await device.GetAddressAsync();

            if (!string.Equals(address, DeviceToConnectTo))
            {
                continue;
            }

            try
            {
                await device.ConnectAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        foreach (var device in devices)
        {
            var address = await device.GetAddressAsync();
            var name = await device.GetAliasAsync();
            var connected = await device.GetConnectedAsync();

            Console.WriteLine($"{name} with address: {address} in adapter, connected: {connected}");
        }

    }
}
