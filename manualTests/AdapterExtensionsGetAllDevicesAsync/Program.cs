﻿// See https://github.com/vestervang/DotNet-BlueZ/issues/8

using vestervang.DotNetBlueZ;

namespace AdapterExtensionsGetAllDevicesAsync
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault();
            if (adapter == null)
            {
                Console.Error.WriteLine("Failed to get the Adapter; Exiting");
                Environment.ExitCode = -1;
                return;
            }

            try
            {
                var props = new Dictionary<string, object>();
                props.Add("Transport", "le");
                props.Add("DuplicateData", false);
                await adapter.SetDiscoveryFilterAsync(props);
                await adapter.StartDiscoveryAsync();
                Console.WriteLine("Waiting for devices. Use Control-C to quit.");
                while (true)
                {
                    var devices = await adapter.GetDevicesAsync();
                    foreach (var device in devices)
                    {
                        var deviceProperties = await device.GetAllAsync();
                        Console.WriteLine($"{deviceProperties.Name} ({deviceProperties.Address}) was the device.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("Stopping Discovery");
                await adapter.StopDiscoveryAsync();
            }
        }
    }
}
