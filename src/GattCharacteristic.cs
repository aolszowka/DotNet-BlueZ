using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace ProrepubliQ.DotNetBlueZ
{
    public delegate Task GattCharacteristicEventHandlerAsync(GattCharacteristic sender,
        GattCharacteristicValueEventArgs eventArgs);

    /// <summary>
    ///     Adds events to IGattCharacteristic1.
    /// </summary>
    public class GattCharacteristic : IGattCharacteristic1, IDisposable
    {
        private IDisposable m_propertyWatcher;

        private IGattCharacteristic1 m_proxy;

        private static readonly IDictionary<ObjectPath, GattCharacteristic> CharacteristicCache =
            new ConcurrentDictionary<ObjectPath, GattCharacteristic>();

        public void Dispose()
        {
            m_propertyWatcher?.Dispose();
            m_propertyWatcher = null;

            GC.SuppressFinalize(this);
        }

        public ObjectPath ObjectPath => m_proxy.ObjectPath;

        public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
        {
            return m_proxy.ReadValueAsync(Options);
        }

        public Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
        {
            return m_proxy.WriteValueAsync(Value, Options);
        }

        public Task<(CloseSafeHandle fd, ushort mtu)> AcquireWriteAsync(IDictionary<string, object> Options)
        {
            return m_proxy.AcquireWriteAsync(Options);
        }

        public Task<(CloseSafeHandle fd, ushort mtu)> AcquireNotifyAsync(IDictionary<string, object> Options)
        {
            return m_proxy.AcquireNotifyAsync(Options);
        }

        public Task StartNotifyAsync()
        {
            return m_proxy.StartNotifyAsync();
        }

        public Task StopNotifyAsync()
        {
            return m_proxy.StopNotifyAsync();
        }

        public Task<T> GetAsync<T>(string prop)
        {
            return m_proxy.GetAsync<T>(prop);
        }

        public Task<GattCharacteristic1Properties> GetAllAsync()
        {
            return m_proxy.GetAllAsync();
        }

        public Task SetAsync(string prop, object val)
        {
            return m_proxy.SetAsync(prop, val);
        }

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return m_proxy.WatchPropertiesAsync(handler);
        }

        ~GattCharacteristic()
        {
            Dispose();
        }

        internal static async Task<GattCharacteristic> CreateAsync(IGattCharacteristic1 proxy)
        {
            if (CharacteristicCache.ContainsKey(proxy.ObjectPath))
            {
                return CharacteristicCache[proxy.ObjectPath];
            }

            var characteristic = new GattCharacteristic
            {
                m_proxy = proxy
            };

            characteristic.m_propertyWatcher = await proxy.WatchPropertiesAsync(characteristic.OnPropertyChanges);

            CharacteristicCache.Add(proxy.ObjectPath, characteristic);
            return characteristic;
        }

        internal static void RemoveCharacteristicsFromCache(ObjectPath devicePath)
        {
            var entries = CharacteristicCache.Where(c => c.Key.ToString().Contains(devicePath.ToString()));

            foreach (var entry in entries)
            {
                var characteristic = entry.Value;
                characteristic.m_value = null;
                CharacteristicCache.Remove(entry.Key);
                characteristic.Dispose();
            }
        }

        public event GattCharacteristicEventHandlerAsync Value
        {
            add
            {
                m_value += value;

                // Subscribe here instead of CreateAsync, because not all GATT characteristics are notifable.
                Subscribe();
            }
            remove => m_value -= value;
        }

        private async void Subscribe()
        {
            try
            {
                await m_proxy.StartNotifyAsync();

                // Is there a way to check if a characteristic supports Read?
                // Reading the current value will trigger OnPropertyChanges.
                // var options = new Dictionary<string, object>();
                // var value = await m_proxy.ReadValueAsync(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error subscribing to characteristic value: {ex}");
            }
        }

        private void OnPropertyChanges(PropertyChanges changes)
        {
            // Console.WriteLine("OnPropertyChanges called.");
            var test = m_value;
            foreach (var pair in changes.Changed)
                switch (pair.Key)
                {
                    case "Value":
                        m_value?.Invoke(this, new GattCharacteristicValueEventArgs((byte[])pair.Value));
                        break;
                }
        }

        private event GattCharacteristicEventHandlerAsync m_value;
    }
}
