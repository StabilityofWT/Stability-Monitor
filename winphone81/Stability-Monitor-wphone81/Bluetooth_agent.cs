﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stability_Monitor_wphone81
{
    class Bluetooth_agent : Agent
    {


        public Bluetooth_agent(String filepath, Agenttype agenttype, Callback_on_status_changed callback, Results results) : base(filepath, agenttype, callback, results) { }

        public override void send_file(String bluid, int not)
        {

        }

        public override void receive_file(String bluid, int not)
        {

        }
    }
}