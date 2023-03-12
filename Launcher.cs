using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using AeonFlux;
using AeonFlux.Model;
using AeonFlux;

namespace AeonFlux{
    class Launcher{
        public static int Main(String[] args){

            AeonFlux aeonFlux = new AeonFlux(1301);
            aeonFlux.setNumberOfPartitions(300);
            aeonFlux.setNumberOfRequestExecutors(700);
            aeonFlux.Start();
            
            return 0;
        }

        // static Socket listener;

        // public static void StartServer()
        // {
        //     // Get Host IP Address that is used to establish a connection
        //     // In this case, we get one IP address of localhost that is IP : 127.0.0.1
        //     // If a host has multiple addresses, you will get a list of addresses
        //     IPHostEntry host = Dns.GetHostEntry("localhost");
        //     IPAddress ipAddress = host.AddressList[0];
        //     IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8900);

        //     try {

        //         // Create a Socket that will use Tcp protocol
        //         listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        //         // A Socket must be associated with an endpoint using the Bind method
        //         listener.Bind(localEndPoint);
        //         // Specify how many requests a Socket can listen before it gives Server busy response.
        //         // We will listen 10 requests at a time
        //         listener.Listen(1);

        //         Console.WriteLine("Waiting for a connection...");
                
        //         ThreadPool.SetMinThreads(1000, 1000);
        //         ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteRequest));
                
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.ToString());
        //     }

        //     Console.WriteLine("\n Press any key to continue...");
        //     Console.ReadKey();
        // }

        // static void ExecuteRequest(Object stateInfo){
            
        //     Console.WriteLine("Hello from the thread pool.");
        //     Socket handler = listener.Accept();

        //     // Incoming data from the client.
        //     string data = null;
        //     byte[] bytes = null;

        //     var utf8 = new UTF8Encoding();
            
        //     while (true){
        //         bytes = new byte[1024];
        //         int bytesRec = handler.Receive(bytes);
        //         Console.WriteLine(bytesRec);
        //         string info = GetBytesToStringConverted(bytes);
        //         Console.WriteLine("{0}", info);
        //         if(bytesRec < bytes.Length)break;
        //     }

        //     byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
        //     handler.Send(resp);
        //     handler.Close();

        //     ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteRequest));
        // }    
    
        // static string GetBytesToStringConverted(byte[] bytes){
        //     MemoryStream stream = new MemoryStream(bytes);
        //     StreamReader streamReader = new StreamReader(stream);
        //     return streamReader.ReadToEnd();
        // }
    }
}

