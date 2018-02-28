using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Java.Util;
using System.Linq;
using System.Text;

namespace CheckTag
{
    [Activity(Label = "CheckTag", MainLauncher = true, Icon = "@drawable/icon",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        // Local Bluetooth adapter
        private BluetoothAdapter bluetoothAdapter = null;
        private BluetoothDevice device = null;
        private BluetoothSocket socket = null;

        private TagsAdapter tagsadapter = null;
        private ListView tagsview = null;

        // Line received from the RFID reader
        StringBuilder line = new StringBuilder();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            tagsadapter = new TagsAdapter(this);
            tagsview = FindViewById<ListView>(Resource.Id.TagsListView);
            tagsview.Adapter = tagsadapter;

            tagsadapter.AddMsgTag("\n\n\nNote:\nThis app only works with the reader IP30 s/n ...506");

            Button button = FindViewById<Button>(Resource.Id.buttonRead);
            button.Click += delegate {
                if (ConnectToIP30())
                {
                    tagsadapter.AddMsgTag("Connected\nUse trigger to read");
                    GetData();
                    button.Enabled = false;
                }
                else
                {
                    tagsadapter.AddMsgTag("Connection failed");
                    tagsadapter.AddMsgTag("Check if IP30 s/n...506 is nearby and active");
                }
            };

            //-----------------------------------------------------------
            // Get local Bluetooth adapter
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // If the adapter is null, then Bluetooth is not supported
            if (bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Bluetooth is not available", ToastLength.Long).Show();
                Finish();
                return;
            }

            //-----------------------------------------------------------
            // Check if Bluetooth is enabled
            if (!bluetoothAdapter.IsEnabled)
            {
                Toast.MakeText(this, "Bluetooth is not enabled", ToastLength.Long).Show();
                Finish();
                return;
            }
            else
            {
                //AddTxt("Bluetooth is enabled\n");
            }

            //-----------------------------------------------------------
            // Listing paired devices
            var pairedDevices = bluetoothAdapter.BondedDevices;
            //AddTxt("Paired devices:");
            bool found = false;
            foreach (var d in pairedDevices)
            {
                //AddTxt("  - " + d.Name + " : " + d.Address);
                if (d.Name == "IP30-03611359506")
                {
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                Toast.MakeText(this, "IP30-03611359506 is not paired", ToastLength.Long).Show();
                Finish();
                return;
            }
        }


        protected bool ConnectToIP30()
        {
            //-----------------------------------------------------------
            // Connecting to the specific IP30 we want to use
            //AddTxt("Connecting to IP30-03611359506...");
            device = (from bd in bluetoothAdapter.BondedDevices
                      where bd.Name == "IP30-03611359506"
                      select bd).FirstOrDefault();

            if (device == null)
            {
                //AddTxt("...not found");
                return false;
            }
            else
            {
                //AddTxt("...connected");
            }

            //-----------------------------------------------------------
            // Get the socket to communicate with IP30
            try
            {
                // Connect the device through the socket. This will block until it succeeds or throws an exception
                // The UUID is the well-known SPP UUID for Bluetooth serial boards
                socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                socket.Connect();
                //AddTxt("Got the socket");
            }
            catch (Java.IO.IOException)
            {
                //AddTxt("Failed to get the socket");
                socket.Close();
                socket = null;
                return false;
            }
            return true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            tagsadapter.AddMsgTag("OnDestroy");

            try
            {
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
            }
            catch (Java.IO.IOException)
            {
                tagsadapter.AddMsgTag("close() of connect socket failed");
            }
        }

        protected async void GetData()
        {
            byte[] buffer = new byte[1024];
            int c;

            do
            {
                c = await socket.InputStream.ReadAsync(buffer, 0, buffer.Length);
                //AddTxt("buffer: " + c.ToString() + " " + System.Text.Encoding.UTF8.GetString(buffer));

                int i;
                for (i = 0; i < c; i++)
                {
                    if (buffer[i] != '\r')
                        line.Append((char)buffer[i]);

                    if (buffer[i] == '\n')
                    {
                        string msg = line.ToString();

                        if (msg.Contains("OK>"))
                        {
                            //AddTxt("\n");
                        }
                        else if (!msg.Contains("EVT:"))
                        {
                            //AddTxt(msg);
                            // Add a new tag
                            tagsadapter.AddNewTag(msg);
                        }

                        if (msg.ToUpper().Contains("TRIGPULL"))
                        {
                            tagsadapter.ClearTags();
                            byte[] rd_command = Encoding.ASCII.GetBytes("READ HEX(2:0,8) HEX(3:0,4)\r\n");
                            await socket.OutputStream.WriteAsync(rd_command, 0, rd_command.Length);
                        }
                        line.Clear();
                    }
                }
            }
            while (c != 0);
        }
    }
}

