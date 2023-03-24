namespace vestervang.DotNetBlueZ
{
    public static class BluezConstants
    {
        public const string DbusService = "org.bluez";
        public const string AdapterInterface = "org.bluez.Adapter1";
        public const string DeviceInterface = "org.bluez.Device1";
        public const string GattServiceInterface = "org.bluez.GattService1";
        public const string GattCharacteristicInterface = "org.bluez.GattCharacteristic1";
        public const string BatteryInterface = "org.bluez.Battery1";
    }

    // https://www.bluetooth.com/specifications/gatt/

    public static class GattConstants
    {
        // Device Information
        public const string DeviceInformationServiceUUID = "0000180a-0000-1000-8000-00805f9b34fb";
        public const string ModelNameCharacteristicUUID = "00002a24-0000-1000-8000-00805f9b34fb";
        public const string ManufacturerNameCharacteristicUUID = "00002a29-0000-1000-8000-00805f9b34fb";

        // Current Time
        public const string CurrentTimeServiceUUID = "00001805-0000-1000-8000-00805f9b34fb";
        public const string CurrentTimeCharacteristicUUID = "00002a2b-0000-1000-8000-00805f9b34fb";

        // Service types
        public const string HeartRateServiceUUID = "0000180d-0000-1000-8000-00805f9b34fb";
        public const string HeartRateCharacteristicHeartRateMeasurementUUID = "00002A37-0000-1000-8000-00805f9b34fb";


        // Battery Service
        // BlueZ presents this service a separate interface, Battery1.
        // public const string BatteryServiceUUID = "0000180f-0000-1000-8000-00805f9b34fb";
        // public const string BatteryLevelCharacteristicUUID = "00002a19-0000-1000-8000-00805f9b34fb";

        // Apple Notification Center Service (ANCS)
        // https://developer.apple.com/library/ios/documentation/CoreBluetooth/Reference/AppleNotificationCenterServiceSpecification/Introduction/Introduction.html
        public const string ANCServiceUUID = "7905f431-b5ce-4e99-a40f-4b1e122d00d0";
        public const string ANCSNotificationSourceUUID = "9fbf120d-6301-42d9-8c58-25e699a21dbd";
        public const string ANCSControlPointUUID = "69d1d8f3-45e1-49a8-9821-9bbdfdaad9d9";
        public const string ANCSDataSourceUUID = "22eac6e9-24d6-4bb5-be44-b36ace7c7bfb";


    }
}
