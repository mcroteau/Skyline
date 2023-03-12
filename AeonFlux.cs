using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;

using AeonFlux;
using AeonFlux.Specs;

namespace AeonFlux{

    public class AeonFlux {

        int port;
        String PROPERTIES;

        // ViewConfig viewConfig;
        // PropertiesConfig propertiesConfig;
        int numberOfPartitions = 3;
        int numberOfRequestExecutors = 7;
        String securityAccessKlass;

        Socket listener;

        public AeonFlux(){
            this.port = 1301;
            this.PROPERTIES = "system.properties";
            // this.viewConfig = new ViewConfig();
        }

        public AeonFlux(int port){
            this.port = port;
            this.PROPERTIES = "system.properties";
            // this.viewConfig = new ViewConfig();
        }

        public void Start(){
            try {

                runViewValidationSpecs();

                // StartupAnnotationInspector startupAnnotationInspector = new StartupAnnotationInspector(new ComponentsHolder());
                // startupAnnotationInspector.inspect();
                // ComponentsHolder componentsHolder = startupAnnotationInspector.getComponentsHolder();

                // if (propertiesConfig == null) {
                //     propertiesConfig = new PropertiesConfig();
                //     propertiesConfig.setPropertiesFile(PROPERTIES);
                // }

                // String propertiesFile = propertiesConfig.getPropertiesFile();
                // RouteAttributesResolver routeAttributesResolver = new RouteAttributesResolver(propertiesFile);
                // RouteAttributes routeAttributes = routeAttributesResolver.resolve();
                // AnnotationComponent serverStartup = componentsHolder.getServerStartup();

                // AeonHelper aeonHelper = new AeonHelper();

                // String resourcesDirectory = viewConfig.getResourcesPath();
                // Dictionary<String, byte[]> viewBytesMap = aeonHelper.getViewBytesMap(viewConfig);

                // Log.info("Running startup routine, please wait...");
                // if (serverStartup != null) {
                //     Method startupMethod = serverStartup.GetType().getMethod("startup");
                //     Object startupObject = serverStartup.GetType().getConstructor().newInstance();
                //     startupMethod.Invoke(startupObject);
                // }

                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(1);
                
                ThreadPool.SetMinThreads(numberOfRequestExecutors, numberOfRequestExecutors);
                Console.WriteLine("Registering network request negotiators, please wait...");
                
                Thread thread = null;
                for(int partitions = 0; partitions < numberOfPartitions; partitions++){
                    thread = new Thread(() => {
                        Console.Write("Registered {0} network request negotiators \r", partitions * numberOfRequestExecutors);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteRequest));
                    });
                    thread.Start();
                }               
                thread.Join();

                Console.WriteLine("\n\nReady!");

            }catch(Exception ex){
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        public void ExecuteRequest(Object stateInfo){
            Socket handler = listener.Accept();

            string data = null;
            byte[] bytes = null;

            var utf8 = new UTF8Encoding();
            
            while (true){
                bytes = new byte[1024 * 3];
                int bytesRec = handler.Receive(bytes);
                string info = GetBytesToStringConverted(bytes);
                if(bytesRec < bytes.Length)break;
            }

            byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
            handler.Send(resp);
            handler.Close();

            ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteRequest));
        }    

        static String GetBytesToStringConverted(byte[] bytes){
            MemoryStream stream = new MemoryStream(bytes);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        public void runViewValidationSpecs(){
            SpecTest specTest = new SpecTest();
            specTest.A();specTest.B();specTest.C();specTest.D();specTest.E();specTest.F();specTest.G();
        }

        // public void setPropertiesConfig(PropertiesConfig propertiesConfig){
        //     this.propertiesConfig = propertiesConfig;
        // }

        // public void setViewConfig(ViewConfig viewConfig) {
        //     this.viewConfig = viewConfig;
        // }

        public void setSecurityAccess(String securityAccessKlass) {
            this.securityAccessKlass = securityAccessKlass;
        }

        public void setNumberOfPartitions(int numberOfPartitions){
            this.numberOfPartitions = numberOfPartitions;
        }

        public void setNumberOfRequestExecutors(int numberOfRequestExecutors){
            this.numberOfRequestExecutors = numberOfRequestExecutors;
        }
    }
}