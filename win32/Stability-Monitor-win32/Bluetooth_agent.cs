﻿using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Stability_Monitor_win32
{
    class Bluetooth_agent : Agent
    {
        private BluetoothClient _bluetooth_client;
        private BluetoothAddress _bluetooth_address;
        private NetworkStream _netstream;
        private Guid _bluetooth_guid;
        private Stream _filestream;
        private BluetoothListener _bluetooth_listener;

        private System.Timers.Timer _timer_ts = new System.Timers.Timer();
        private byte[] _buffer;
        private int _count_received_bytes = 0;
        private Stopwatch _stopwatch = new Stopwatch();
        private String _message;
        private bool _writing = false;

        public Bluetooth_agent(String filepath, Agenttype agenttype, Callback_on_status_changed callback, Results results, Main_view mv) : base(filepath, agenttype, callback, results, mv) { }

        public override void send_file(String devicename, String bluid, int not)
        {
            try
            {
                _stopwatch.Start();

                _bluetooth_client = new BluetoothClient();

                BluetoothDeviceInfo[] devinfos = _bluetooth_client.DiscoverDevices();

                foreach (var device in devinfos)
                {
                    if (device.DeviceName == devicename)
                    {
                        _bluetooth_address = device.DeviceAddress;
                        break;
                    }
                }

                _bluetooth_guid = Guid.Parse(bluid);

                _bluetooth_client.Connect(_bluetooth_address, _bluetooth_guid);

                _netstream = _bluetooth_client.GetStream();

                byte[] dataToSend = File.ReadAllBytes(this.filepath);

                _netstream.Write(dataToSend, 0, dataToSend.Length);
                _netstream.Flush();

                _netstream.Dispose();
                _netstream.Close();

                _bluetooth_client.Dispose();
                _bluetooth_client.Close();

                _message = format_message(_stopwatch.Elapsed, "File Transfer", "OK", this.filepath);
                this.callback.on_file_received(_message, this.results);
                this.main_view.text_to_logs(_message);

                _stopwatch.Stop();
            }
            catch (Exception e)
            {
                append_error_tolog(e, _stopwatch.Elapsed, devicename);
            }
        }

        public override void receive_file(String devicename, String bluid, int not)
        {
            try
            {
                _stopwatch.Start();
                scan_transfer_speed();                

                _bluetooth_guid = Guid.Parse(bluid);
                _bluetooth_listener = new BluetoothListener(_bluetooth_guid);
                _bluetooth_listener.Start();

                _bluetooth_client = _bluetooth_listener.AcceptBluetoothClient();
                _netstream = _bluetooth_client.GetStream();

                _filestream = new FileStream(this.filepath, FileMode.Create, FileAccess.ReadWrite);

                int length;
                _buffer = new byte[65000];
                
                while ((length = _netstream.Read(_buffer, 0, _buffer.Length)) != 0)
                {
                    while (_writing) { }

                    _count_received_bytes += length;

                    _filestream.Write(_buffer, 0, length);
                }

                _timer_ts.Close();
                _stopwatch.Stop();

                int _transferspeed = _count_received_bytes / 1024;
                _message = format_message(_stopwatch.Elapsed, "Transferspeed", _transferspeed.ToString(), "kB/s");
                this.callback.on_transfer_speed_change(_message, this.results);
                this.main_view.text_to_logs(_message);

                _filestream.Dispose();
                _filestream.Close();

                _netstream.Dispose();
                _netstream.Close();

                _bluetooth_client.Dispose();
                _bluetooth_client.Close();

                _bluetooth_listener.Stop();

                _message = format_message(_stopwatch.Elapsed, "File Transfer", "OK", this.filepath);
                this.callback.on_file_received(_message, this.results);
                this.main_view.text_to_logs(_message);
            }
            catch (Exception e)
            {
                append_error_tolog(e, _stopwatch.Elapsed, devicename);
            }
        }               

        private void check_transfer_speed(object sender, EventArgs e)
        {
            try
            {
                _writing = true;

                if (_netstream != null)
                {
                    byte[] ack = BitConverter.GetBytes(_count_received_bytes);
                    _netstream.Write(ack, 0, ack.Length);
                }             

                int _transferspeed = _count_received_bytes / 1024;
                _message = format_message(_stopwatch.Elapsed, "Transferspeed", _transferspeed.ToString(), "kB/s");
                this.callback.on_transfer_speed_change(_message, this.results);
                this.main_view.text_to_logs(_message);

                _count_received_bytes = 0;

                _writing = false;
            }
            catch (Exception e2)
            {
                append_error_tolog(e2, _stopwatch.Elapsed, "");
            }
        }

        private void scan_transfer_speed()
        {
            _timer_ts.Interval = 1000;
            _timer_ts.Elapsed += new ElapsedEventHandler(check_transfer_speed);
            _timer_ts.Start();
        }

        public void stop_scanning()
        {
            if (_timer_ts.Enabled) _timer_ts.Stop();
            _stopwatch.Stop();
        }
    }
}
