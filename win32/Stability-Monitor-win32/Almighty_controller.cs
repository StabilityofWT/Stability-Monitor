﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stability_Monitor_win32
{
    class Almighty_controller
    {
        
        public void shedule_test()
        {
            Test test1 = new Test(Testtype.Test_1);
            test1.start_test();
        }
        
    }
}