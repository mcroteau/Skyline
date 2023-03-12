using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using Skyline;
using Skyline.Model;
using Skyline;

namespace Skyline{
    class Launcher{
        public static int Main(String[] args){
            Skyline skyline = new Skyline(1301);
            skyline.SetNumberOfPartitions(300);
            skyline.SetNumberOfRequestExecutors(700);

            skyline.SetSourcesPath("Foo");

            skyline.Start();
            return 0;
        }
    }
}

