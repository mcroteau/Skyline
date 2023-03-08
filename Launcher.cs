using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using Zeus;

namespace Zeus
{
    class Launcher
    {
        public static int Main(String[] args)
        {
            ExperienceResolver experienceResolver = new ExperienceResolver();
            StringBuilder sb = new StringBuilder();
            sb.Append("${name}\n");
            sb.Append("<c:if spec=\"${todos.Count > 0}\">\n");
            sb.Append("<c:foreach items=\"${todos}\" var=\"todo\">\n");
            sb.Append("${todo}\n");
            sb.Append("</c:foreach>\n");
            sb.Append("</c:if>\n");


            ArrayList todos = new ArrayList();
            todos.Add("Zebra House");
            todos.Add("Zebra Bra");
            todos.Add("Zebra Love");

            ViewCache cache = new ViewCache();
            cache.set("name", "Dirk");
            cache.set("todos", todos);

            String output = experienceResolver.resolve(sb.ToString(), cache, null, null, null);
            Console.WriteLine(output);

            //StartServer();
            return 0;
        }

        public static void StartServer()
        {
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8900);

            try {

                // Create a Socket that will use Tcp protocol
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will listen 10 requests at a time
                listener.Listen(1);

                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();

                // Incoming data from the client.
                string data = null;
                byte[] bytes = null;

                var utf8 = new UTF8Encoding();
                
                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    Console.WriteLine(bytesRec);
                    string info = GetBytesToStringConverted(bytes);
                    Console.WriteLine("{0}", info);
                    if(bytesRec < bytes.Length)break;
                }
                
                DataPartial dataPartial = new DataPartial();
                dataPartial.setEntry("spock");
                Console.WriteLine(dataPartial.getEntry());

                byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
                handler.Send(resp);
                
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                Console.WriteLine(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }
        
        static string GetBytesToStringConverted(byte[] bytes){
            MemoryStream stream = new MemoryStream(bytes);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}

