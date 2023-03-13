using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using Skyline;
using Skyline.Model;

namespace Foo{
    class Launcher{
        public static int Main(String[] args){
            SkylineRunnable skyline = new SkylineRunnable(3000);
            skyline.SetNumberOfPartitions(300);
            skyline.SetNumberOfRequestExecutors(700);

            skyline.Start();
            return 0;
        }
    }
}

