using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using System.Reflection;

using Skyline;
using Skyline.Model;
using Skyline.Specs;
using Skyline.Security;

namespace Skyline{

    public class SkylineRunnable {
        
        String FAVICON = "/favicon.ico";
        String BREAK = "\r\n";
        String SPACE = " ";
        String DOUBLEBREAK = "\r\n\r\n";

        int REQUEST_METHOD = 0;
        int REQUEST_PATH = 1;
        int REQUEST_VERSION = 2;

        int port;
        String sourcesPath;
        String PROPERTIES;

        ViewConfig viewConfig;
        PropertiesConfig propertiesConfig;
        RouteAttributes routeAttributes;
        ApplicationAttributes applicationAttributes;

        int numberOfPartitions = 3;
        int numberOfRequestExecutors = 7;
        String securityAccessKlass;

        Socket listener;

        public SkylineRunnable(){
            this.port = 1301;
            this.sourcesPath = "Source";
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
        }

        public SkylineRunnable(int port){
            this.port = port;
            this.sourcesPath = "Source";
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
        }

        public void Start(){
            try {

                SpecTest specTest = new SpecTest();
                specTest.Run();

                ResourceUtility skylineUtilities = new ResourceUtility();
                StartupAnnotationInspector startupAnnotationInspector = new StartupAnnotationInspector(new ComponentsHolder());
                ComponentsHolder componentsHolder = startupAnnotationInspector.Inspect();

                if (propertiesConfig == null) {
                    propertiesConfig = new PropertiesConfig();
                    propertiesConfig.setPropertiesFile(PROPERTIES);
                }

                RouteAttributesResolver routeAttributesResolver = new RouteAttributesResolver(propertiesConfig);
                routeAttributes = routeAttributesResolver.resolve();

                foreach(var propertyEntry in routeAttributes.getAttributes()){
                    Console.WriteLine(propertyEntry.Key + ":" + propertyEntry.Value);
                }

                String resourcesDirectory = viewConfig.getResourcesPath();
                Dictionary<String, byte[]> viewBytesMap = skylineUtilities.getViewBytesMap(viewConfig);

                Object serverStartupInstance = componentsHolder.getServerStartup();
                if (serverStartupInstance != null) {
                    MethodInfo startupMethod = serverStartupInstance.GetType().GetMethod("Start");
                    startupMethod.Invoke(serverStartupInstance, new Type[]{}); 
                }

                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(1);
                
                Console.WriteLine("Registering network request negotiators, please wait...");
                
                Thread thread = null;
                for(int partitions = 0; partitions < numberOfPartitions; partitions++){
                    thread = new Thread(() => {
                        Console.Write("Registered {0} network request negotiators \r", partitions * numberOfRequestExecutors);
                        ThreadPool.SetMinThreads(numberOfRequestExecutors, numberOfRequestExecutors);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteNetworkRequest));
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

        public void ExecuteNetworkRequest(Object stateInfo){
            Socket requestHandler = listener.Accept();

            String data = null;
            byte[] bytes = null;

            var utf8 = new UTF8Encoding();
            String completeRequestContent = new String("");
            while (true){
                bytes = new byte[1024 * 3];
                int bytesRec = requestHandler.Receive(bytes);
                Thread.Sleep(19);
                completeRequestContent = GetBytesToStringConverted(bytes);
                if(bytesRec < bytes.Length)break;
            }

            ResourceUtility resourceUtility = new ResourceUtility();
            String[] requestBlocks = completeRequestContent.Split(DOUBLEBREAK, 2);

            String requestHeaderElement = requestBlocks[0];
            String[] methodPathComponentsLookup = requestHeaderElement.Split(BREAK);
            String methodPathComponent = methodPathComponentsLookup[0];

            String[] methodPathVersionComponents = methodPathComponent.Split(SPACE);

            String networkRequestAction = methodPathVersionComponents[REQUEST_METHOD];
            String networkRequestPath = methodPathVersionComponents[REQUEST_PATH];
            String networkRequestVersion = methodPathVersionComponents[REQUEST_VERSION];

            if(networkRequestPath.Equals(FAVICON)){
                ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteNetworkRequest));
                return;
            }

            RouteAttributes routeAttributesCopy = new RouteAttributes(routeAttributes);
            ApplicationAttributes applicationAttributesCopy = new ApplicationAttributes(applicationAttributes);

            RouteNegotiatorFactory routeNegotiatorFactory = new RouteNegotiatorFactory();
            routeNegotiatorFactory.setRouteAttributes(routeAttributesCopy);
            RouteEndpointNegotiator routeEndpointNegotiator = routeNegotiatorFactory.create();
            SecurityAttributes securityAttributes = routeNegotiatorFactory.getSecurityAttributes();

            NetworkRequest networkRequest = new NetworkRequest();
            networkRequest.setRequestAction(networkRequestAction);
            networkRequest.setRequestPath(networkRequestPath);
            networkRequest.setAttributes();

            NetworkResponse networkResponse = new NetworkResponse();

            NetworkRequestHeaderResolver networkRequestHeaderResolver = new NetworkRequestHeaderResolver();
            networkRequestHeaderResolver.setNetworkRequestHeaderElement(requestHeaderElement);
            networkRequestHeaderResolver.setNetworkRequest(networkRequest);
            networkRequestHeaderResolver.resolve();
        
            
            
            SecurityAttributeResolver securityAttributeResolver = new SecurityAttributeResolver();
            securityAttributeResolver.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());
            securityAttributeResolver.resolve(networkRequest, networkResponse);



            // requestHandler.Send((requestVersion + " ").getBytes());
            // requestHandler.Send(routeResult.getResponseCode().getBytes());
            // requestHandler.Send(BREAK.getBytes());

            // if(networkRequest.isRedirect()) {
            //     requestHandler.Send("Content-Type:text/html".getBytes());
            //     requestHandler.Send(BREAK.getBytes());
            //     requestHandler.Send("Set-Cookie:".getBytes());
            //     requestHandler.Send(sessionValues.toString().getBytes());
            //     requestHandler.Send(BREAK.getBytes());
            //     requestHandler.Send(("Location: " +  networkRequest.getRedirectLocation() + SPACE).getBytes());
            //     requestHandler.Send(BREAK.getBytes());
            //     requestHandler.Send("Content-Length: -1".getBytes());
            //     requestHandler.Send(DOUBLEBREAK.getBytes());

            //     requestHandler.close();
            //     socketClient.close();

            //     networkRequestFactory.execute();
            //     return;
            // }


            // requestHandler.Send("Content-Type:".getBytes());
            // requestHandler.Send(routeResult.getContentType().getBytes());
            // requestHandler.Send(BREAK.getBytes());

            // requestHandler.Send("Set-Cookie:".getBytes());
            // requestHandler.Send(sessionValues.toString().getBytes());
            // requestHandler.Send(DOUBLEBREAK.getBytes());
            // requestHandler.Send(routeResult.getResponseBytes());

            byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
            requestHandler.Send(resp);
            requestHandler.Close();

            ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteNetworkRequest));
        }    

        static String GetBytesToStringConverted(byte[] bytes){
            MemoryStream stream = new MemoryStream(bytes);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        public void setPropertiesConfig(PropertiesConfig propertiesConfig){
            this.propertiesConfig = propertiesConfig;
        }

        public void setViewConfig(ViewConfig viewConfig) {
            this.viewConfig = viewConfig;
        }

        public void SetSourcesPath(String sourcesPath){
            this.sourcesPath = sourcesPath;
        }

        public void SetSecurityAccess(String securityAccessKlass) {
            this.securityAccessKlass = securityAccessKlass;
        }

        public void SetNumberOfPartitions(int numberOfPartitions){
            this.numberOfPartitions = numberOfPartitions;
        }

        public void SetNumberOfRequestExecutors(int numberOfRequestExecutors){
            this.numberOfRequestExecutors = numberOfRequestExecutors;
        }

        public void SetApplicationAttributes(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }
    }
}