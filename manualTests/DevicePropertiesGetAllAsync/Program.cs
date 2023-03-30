// See https://github.com/vestervang/DotNet-BlueZ/issues/8

using vestervang.DotNetBlueZ;

namespace DevicePropertiesGetAllAsync
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault())
            {
                try
                {
                    if (adapter != null)
                    {
                        adapter.DeviceFound += adapter_DeviceFoundAsync;

                        var props = new Dictionary<string, object>();
                        props.Add("Transport", "le");
                        props.Add("DuplicateData", false);
                        await adapter.SetDiscoveryFilterAsync(props);
                        await adapter.StartDiscoveryAsync();
                        Console.WriteLine("Waiting for events. Use Control-C to quit.");
                        await Task.Delay(-1);
                    }
                    else
                    {
                        Console.Error.WriteLine("Failed to get the Adapter; Exiting");
                        Environment.ExitCode = -1;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                finally
                {
                    Console.WriteLine("Stopping Discovery");
                    if (adapter != null)
                    {
                        await adapter.StopDiscoveryAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Called when a new Device is Found ("Discovered"). This is the
        /// initial connection made during the discovery phase.
        /// </summary>
        /// <param name="sender">
        /// The Bluetooth Adapter that found the device.
        /// </param>
        /// <param name="eventArgs">The DeviceFound Event Arguments</param>
        /// <returns> Nothing.</returns>
        private static async Task adapter_DeviceFoundAsync(Adapter sender, DeviceFoundEventArgs eventArgs)
        {
            Console.WriteLine("A device was found!");
            var deviceProperties = await eventArgs.Device.GetAllAsync();
            Console.WriteLine($"{deviceProperties.Name} ({deviceProperties.Address}) was the device.");
        }
    }
}

