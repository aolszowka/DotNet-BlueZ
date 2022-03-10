using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace ProrepubliQ.DotNetBlueZ
{
    public delegate Task DeviceEventHandlerAsync(Device sender, BlueZEventArgs eventArgs);

    /// <summary>
    ///     Adds events to IDevice1.
    /// </summary>
    public class Device : IDevice1, IDisposable
    {
        private IDisposable m_propertyWatcher;

        private IDevice1 m_proxy;

        private static readonly IDictionary<string, Device> DeviceCache = new ConcurrentDictionary<string, Device>();

        public ObjectPath ObjectPath => m_proxy.ObjectPath;

        public Task CancelPairingAsync()
        {
            return m_proxy.CancelPairingAsync();
        }

        public Task ConnectAsync()
        {
            return m_proxy.ConnectAsync();
        }

        public Task ConnectProfileAsync(string UUID)
        {
            return m_proxy.ConnectProfileAsync(UUID);
        }

        public Task DisconnectAsync()
        {
            return m_proxy.DisconnectAsync();
        }

        public Task DisconnectProfileAsync(string UUID)
        {
            return m_proxy.DisconnectProfileAsync(UUID);
        }

        public Task<Device1Properties> GetAllAsync()
        {
            return m_proxy.GetAllAsync();
        }

        public Task<T> GetAsync<T>(string prop)
        {
            return m_proxy.GetAsync<T>(prop);
        }

        public Task PairAsync()
        {
            return m_proxy.PairAsync();
        }

        public Task SetAsync(string prop, object val)
        {
            return m_proxy.SetAsync(prop, val);
        }

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return m_proxy.WatchPropertiesAsync(handler);
        }

        public void Dispose()
        {
            m_propertyWatcher?.Dispose();
            m_propertyWatcher = null;

            string devicePath = m_proxy.ObjectPath.ToString();
            if (DeviceCache.ContainsKey(devicePath))
            {
                DeviceCache.Remove(devicePath);
            }

            GC.SuppressFinalize(this);
        }

        ~Device()
        {
            Dispose();
        }

        internal static async Task<Device> CreateAsync(IDevice1 proxy)
        {
            string devicePath = proxy.ObjectPath.ToString();
            if (DeviceCache.ContainsKey(devicePath))
            {
                return DeviceCache[devicePath];
            }

            var device = new Device
            {
                m_proxy = proxy
            };
            device.m_propertyWatcher = await proxy.WatchPropertiesAsync(device.OnPropertyChanges);

            DeviceCache.Add(devicePath, device);

            return device;
        }

        public event DeviceEventHandlerAsync Connected
        {
            add
            {
                m_connected += value;
                FireEventIfPropertyAlreadyTrueAsync(m_connected, "Connected");
            }
            remove => m_connected -= value;
        }

        public event DeviceEventHandlerAsync Disconnected
        {
            add
            {
                m_disconnected += value;
                FireEventIfPropertyAlreadyTrueAsync(m_resolved, "Connected");
            }
            remove => m_disconnected -= value;
        }

        public event DeviceEventHandlerAsync ServicesResolved
        {
            add
            {
                m_resolved += value;
                FireEventIfPropertyAlreadyTrueAsync(m_resolved, "ServicesResolved");
            }
            remove => m_resolved -= value;
        }

        public bool HasServicesResolvedHandler()
        {
            if (m_resolved == null)
            {
                return false;
            }

            return m_resolved.GetInvocationList().Length != 0;
        }

        public bool HasConnectedHandler()
        {
            if (m_connected == null)
            {
                return false;
            }

            return m_connected.GetInvocationList().Length != 0;
        }

        public bool HasDisconnectedHandler()
        {
            if (m_disconnected == null)
            {
                return false;
            }

            return m_disconnected.GetInvocationList().Length != 0;
        }

        private async void FireEventIfPropertyAlreadyTrueAsync(DeviceEventHandlerAsync handler, string prop)
        {
            try
            {
                var value = await m_proxy.GetAsync<bool>(prop);
                if (value)
                    // TODO: Suppress duplicate event from OnPropertyChanges.
                    handler?.Invoke(this, new BlueZEventArgs(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if '{prop}' is already true: {ex}");
            }
        }

        private void OnPropertyChanges(PropertyChanges changes)
        {
            foreach (var pair in changes.Changed)
                switch (pair.Key)
                {
                    case "Connected":
                        if (true.Equals(pair.Value))
                            m_connected?.Invoke(this, new BlueZEventArgs());
                        else
                            m_disconnected?.Invoke(this, new BlueZEventArgs());
                        break;

                    case "ServicesResolved":
                        if (true.Equals(pair.Value)) m_resolved?.Invoke(this, new BlueZEventArgs());
                        break;
                }
        }

        private event DeviceEventHandlerAsync m_disconnected;
        private event DeviceEventHandlerAsync m_connected;
        private event DeviceEventHandlerAsync m_resolved;
    }
}
