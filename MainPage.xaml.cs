using System;
using System.Text;

namespace MauiApp3
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            cboPrinters.Items.Add("Hello!");
            cboPrinters.Items.Add("World!");
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void cboPrinters_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnScan_Clicked(object sender, EventArgs e)
        {
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

            //if (bluetoothAdapter.IsEnabled == true)
            //{
            //    Android.App.Application.Context.StartActivity(disable);
            //    // Disable the Bluetooth;
            //}
            //else
            //{
            //    // Enable the Bluetooth
            //    Android.App.Application.Context.StartActivity(enable);
            //}
#endif
        }

        private async void btnPrint_Clicked(object sender, EventArgs e)
        {
            if (cboPrinters.SelectedItem == null)
            {
                await DisplayAlert("Printer", "Please select a printer.", "OK");
                return;
            }

            var selected = cboPrinters.SelectedItem.ToString();
            var mac = selected.Split('-')[0].Trim();

            var success = await PrintTextAsync(mac, "Hello from .NET MAUI!");
            await DisplayAlert("Print", success ? "Printed successfully!" : "Printing failed.", "OK");
        }

        public static async Task<bool> PrintAsync(string macAddress, byte[] data)
        {
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
        }

        public static async Task<bool> PrintTextAsync(string macAddress, string text)
        {
            // Append line feeds and cut command
            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(text + "\n\n"));
            // ESC/POS full cut
            bytes.AddRange(new byte[] { 0x1D, 0x56, 0x00 });
            return await PrintAsync(macAddress, bytes.ToArray());
        }


    }
}
