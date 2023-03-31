## AdapterExtensionsGetAllDevicesAsync
This toy program was created to validate issues discovered on
https://github.com/vestervang/DotNet-BlueZ/issues/8.

This toy program attempts to put the adapter in Discovery Mode and then print
out each time a device is discovered along with its Device Address and Name (if
present).

This version of the program uses the Adapter Extension Methods.

This application was tested working on 2023/03/29 on a Raspberry Pi 4B running
Ubuntu 22.04 Jammy Jellyfish.

`uname -a` - `Linux rbpi4b 5.15.0-1025-raspi #27-Ubuntu SMP PREEMPT Thu Feb 16 17:09:55 UTC 2023 aarch64 aarch64 aarch64 GNU/Linux`
`bluetoothctl -v` - `bluetoothctl: 5.64`
