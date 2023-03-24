﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vestervang.DotNetBlueZ;

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


        Console.WriteLine($"Disconnecting from device {DeviceToConnectTo}");

        try
        {
            await device1.DisconnectAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to disconnect device {DeviceToConnectTo}");
        }

        try
        {
            await adapter.ConnectDeviceAsync(dict);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        var device2 = await adapter.GetDeviceAsync(DeviceToConnectTo);

        try
        {
            if (!device2.HasConnectedHandler())
            {
                device2.Connected += ConnectedAsync;
            }

            if (!device2.HasDisconnectedHandler())
            {
                device2.Disconnected += DisconnectedAsync;
            }

            if (!device2.HasServicesResolvedHandler())
            {
                device2.ServicesResolved += ServicesResolvedAsync;
            }

            await device2.ConnectAsync();
            Console.WriteLine($"Connected to device {DeviceToConnectTo}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        var devicesAreSameInstance = device1.Equals(device2);

        Console.WriteLine($"Are devices the same instance? {devicesAreSameInstance}");

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
    }
}
