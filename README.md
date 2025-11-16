# MAUI - Bluetooth Printer Test

### scan previously connected bluetooth devices
```c#
#if ANDROID
            var enable = new Android.Content.Intent(Android.Bluetooth.BluetoothAdapter.ActionRequestEnable);
            enable.SetFlags(Android.Content.ActivityFlags.NewTask);

            var disable = new Android.Content.Intent(Android.Bluetooth.BluetoothAdapter.ActionRequestDiscoverable);
            disable.SetFlags(Android.Content.ActivityFlags.NewTask);

            var bluetoothManager = (Android.Bluetooth.BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
            var bluetoothAdapter = bluetoothManager.Adapter;

            var devices = bluetoothAdapter.BondedDevices;
            cboPrinters.Items.Clear();
            for (int i = 0; i < devices.Count; i++)
            {
                var device = $"{devices.ElementAt(i).Address} - {devices.ElementAt(i).Name}";
                if (device.ToLower().Contains("pos") || device.ToLower().Contains("print") || device.ToLower().Contains("epson"))
                {
                    cboPrinters.Items.Add(device);
                }
            }
#endif
```

### printing
```c#
#if ANDROID
            try
            {
                var bluetoothManager = (Android.Bluetooth.BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
                var bluetoothAdapter = bluetoothManager.Adapter;

                if (bluetoothAdapter == null || !bluetoothAdapter.IsEnabled)
                    throw new Exception("Bluetooth is not available or not enabled.");

                var device = bluetoothAdapter.GetRemoteDevice(macAddress);
                if (device == null)
                    throw new Exception("Device not found.");

                using var socket = device.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
                await socket.ConnectAsync();

                using var outputStream = socket.OutputStream;
                await outputStream.WriteAsync(data);
                await outputStream.FlushAsync();

                socket.Close();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BT PRINT ERROR] {ex}");
                return false;
            }
#endif
```


```c#
        public static async Task<bool> PrintTextAsync(string macAddress, string text)
        {
            // Append line feeds and cut command
            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(text + "\n\n"));
            // ESC/POS full cut
            bytes.AddRange(new byte[] { 0x1D, 0x56, 0x00 });
            return await PrintAsync(macAddress, bytes.ToArray());
        }
```
