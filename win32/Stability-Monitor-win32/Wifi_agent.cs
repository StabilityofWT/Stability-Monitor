﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Stability_Monitor_win32
{
    class Wifi_agent : Agent
    {
        private TcpListener _tcplistener;
        private TcpClient _tcpclient;
        private NetworkStream _netstream;
        private Stream _stream;
        
        public Wifi_agent(String filepath, Agenttype agenttype, Callback_on_status_changed callback) : base(filepath, agenttype, callback)
        {
            
        }

        public override void send_file()
        {
            
        }

        public override void receive_file()
        {
            this.get_callback().on_file_received(get_filepath(), "subor uspesne prijaty");
        }
    }
}
