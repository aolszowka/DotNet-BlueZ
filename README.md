# DotNet-BlueZ

A quick and dirty library for BlueZ's D-Bus APIs. Primary focus is Bluetooth Low Energy.

[![.NET Build, Test, and Publish Nuget Package](https://github.com/vestervang/DotNet-BlueZ/actions/workflows/main.yml/badge.svg)](https://github.com/vestervang/DotNet-BlueZ/actions/workflows/main.yml)

Uses [Tmds.DBus](https://github.com/tmds/Tmds.DBus) to access D-Bus. Tmds.DBus.Tool was used to generate the D-Bus object interfaces. D-Bus is the preferred interface for Bluetooth in userspace. The [Doing Bluetooth Low Energy on Linux](https://elinux.org/images/3/32/Doing_Bluetooth_Low_Energy_on_Linux.pdf) presentation says "Use D-Bus API (documentation in [doc/](<(https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc)>)) whenever possible".

# Requirements

- Linux
- A recent release of BlueZ. This package was tested with BlueZ 5.53 with the `-E` flag. You can check which version you're using with `bluetoothd -v`.

# Installation

```bash
dotnet add package vestervang.DotNetBlueZ
```

# Enabling Experimental features

To use all the features of this library you'll have to enable experimental features.

## Ubuntu

run `systemctl status bluetooth` this will give in information about the bluetooth process.

there should be a line saying something like this:

Loaded: loaded (/lib/systemd/system/bluetooth.service; enabled; vendor preset: enabled)

The `/lib/systemd/system/bluetooth.service` is the path to your service file, we will have to edit this.

Run `sudo vim /lib/systemd/system/bluetooth.service`

Add `-E` at the end of the `ExecStart` line.

Now reboot and it should be enabled

# Events

C# events are available for several properties. Events are useful for properly handling disconnects and reconnects.

# Usage

## Get a Bluetooth adapter

```C#
using vestervang.DotNetBlueZ;
...

IAdapter1 adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault();
```

or get a particular adapter:

```C#
IAdapter1 adapter = await BlueZManager.GetAdapterAsync(adapterName: "hci0");
```

## Scan for Bluetooth devices

```C#
adapter.DeviceFound += adapter_DeviceFoundAsync;

await adapter.StartDiscoveryAsync();
...
await adapter.StopDiscoveryAsync();
```

## Get Devices

`adapter.DeviceFound` (above) will be called immediately for existing devices, and as new devices show up during scanning; `eventArgs.IsStateChange` can be used to distinguish between existing and new devices. Alternatively you can can use `GetDevicesAsync`:

```C#
IReadOnlyList<Device> devices = await adapter.GetDevicesAsync();
```

## Connect to a Device

```C#
device.Connected += device_ConnectedAsync;
device.Disconnected += device_DisconnectedAsync;
device.ServicesResolved += device_ServicesResolvedAsync;

await device.ConnectAsync();
```

Alternatively you can wait for "Connected" and "ServicesResolved" to equal true:

```C#
TimeSpan timeout = TimeSpan.FromSeconds(15);

await device.ConnectAsync();
await device.WaitForPropertyValueAsync("Connected", value: true, timeout);
await device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);

```

## Retrieve a GATT Service and Characteristic

Prerequisite: You must be connected to a device and services must be resolved. You may need to pair with the device in order to use some services.

Example using GATT Device Information Service UUIDs.

```C#
string serviceUUID = "0000180a-0000-1000-8000-00805f9b34fb";
string characteristicUUID = "00002a24-0000-1000-8000-00805f9b34fb";

IGattService1 service = await device.GetServiceAsync(serviceUUID);
IGattCharacteristic1 characteristic = await service.GetCharacteristicAsync(characteristicUUID);
```

## Read a GATT Characteristic value

```C#
byte[] value = await characteristic.ReadValueAsync(timeout);

string modelName = Encoding.UTF8.GetString(value);
```

## Subscribe to GATT Characteristic Notifications

```C#
characteristic.Value += characteristic_Value;
...

private static async Task characteristic_Value(GattCharacteristic characteristic, GattCharacteristicValueEventArgs e)
{
  try
  {
    Console.WriteLine($"Characteristic value (hex): {BitConverter.ToString(e.Value)}");

    Console.WriteLine($"Characteristic value (UTF-8): \"{Encoding.UTF8.GetString(e.Value)}\"");
  }
  catch (Exception ex)
  {
    Console.Error.WriteLine(ex);
  }
}

```

# Tips

It may be necessary to pair with a device for a GATT service to be visible or for reading GATT characteristics to work. To pair, one option is to run `bluetoothctl` (or `sudo bluetoothctl`)
and then run `default agent` and `agent on` within `bluetoothctl`. Watch `bluetoothctl` for pairing requests.

Running the following code with an unpaired device

```csharp
var dict = new Dictionary<string, object>();
dict.Add("Address", DeviceToConnectTo);
dict.Add("AddressType", "random");

await adapter.ConnectDeviceAsync(dict);
```

might result in the following error:

> org.freedesktop.DBus.Error.UnknownMethod: Method "ConnectDevice" with signature "a{sv}" on interface "org.bluez.Adapter1" doesn't exist

Connecting directly to a device that was obtained by discovery might succeed without pairing.

See [Ubuntu's Introduction to Pairing](https://ubuntu.com/core/docs/bluez/reference/pairing/introduction).

# Contributing

See [Contributing](./github/CONTRIBUTING.md).

# Reference

- [BlueZ D-Bus API docs](https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc)
- [Install BlueZ on the Raspberry PI](https://learn.adafruit.com/install-bluez-on-the-raspberry-pi/overview)
