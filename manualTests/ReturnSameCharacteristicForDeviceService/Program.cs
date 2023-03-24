using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vestervang.DotNetBlueZ;
using Tmds.DBus;

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

        var device = await adapter.GetDeviceAsync(DeviceToConnectTo);

        try
        {
            device.Connected += ConnectedAsync;
            device.Disconnected += DisconnectedAsync;
            device.ServicesResolved += ServicesResolvedAsync;
            await device.ConnectAsync();
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

        var serviceUuid = BlueZManager.NormalizeUUID(GattConstants.HeartRateServiceUUID);
        var characteristicUuid =
            BlueZManager.NormalizeUUID(GattConstants.HeartRateCharacteristicHeartRateMeasurementUUID);

        IGattService1 service = await device.GetServiceAsync(serviceUuid);

        if (service == null)
        {
            Console.WriteLine($"No service found with this id: \"{serviceUuid}\"");
            return;
        }

        GattCharacteristic characteristic = await service.GetCharacteristicAsync(characteristicUuid);

        if (characteristic == null)
        {
            Console.WriteLine(
                $"We didn't find any characteristic with the id {characteristicUuid} in the service with id {serviceUuid}");
            return;
        }

        characteristic.Value += HeartRateHandler;
        GattCharacteristic characteristic2 = await service.GetCharacteristicAsync(characteristicUuid);

        var isSame = characteristic.Equals(characteristic2);

        await Task.Delay(3000);

        Console.WriteLine("Removing heart rate handler");
        characteristic2.Value -= HeartRateHandler;

        device.Dispose();

        Console.WriteLine($"Is same instance? {isSame}");
    }


    private static async Task HeartRateHandler(
        GattCharacteristic characteristic,
        GattCharacteristicValueEventArgs e
    )
    {
        try
        {
            var service = await characteristic.GetServiceAsync();
            var device = await service.GetDeviceAsync();
            var deviceAddress = await device.GetAddressAsync();

            Console.WriteLine($"[{DateTime.UtcNow}] Received heart rate from device {deviceAddress}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
}
