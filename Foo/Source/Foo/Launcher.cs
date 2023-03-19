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
            PersistenceConfig persistenceConfig = new PersistenceConfig();
            skyline.setPersistenceConfig(persistenceConfig);

            ApplicationAttributes applicationAttributes = new ApplicationAttributes();
            applicationAttributes.getAttributes().Add("abc", "123");
            skyline.SetApplicationAttributes(applicationAttributes);

            skyline.Start();
            return 0;
        }
    }
}

