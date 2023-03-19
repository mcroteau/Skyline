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
        PersistenceConfig persistenceConfig;
        RouteAttributes routeAttributes;
        ApplicationAttributes applicationAttributes;

        ViewCache viewCache;
        FlashMessage flashMessage;
        Dictionary<String, byte[]> viewBytesMap;
        
        ComponentsHolder componentsHolder;
        RouteEndpointHolder routeEndpointHolder;

        int numberOfPartitions = 3;
        int numberOfRequestExecutors = 7;
        String securityAccessKlass;

        Socket listener;

        String securedAttribute = "attribute";
        String securityElement = "s.k.y.l.i.n.e";

        public SkylineRunnable(){
            this.port = 1301;
            this.sourcesPath = "Source";
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
            this.viewCache = new ViewCache();
            this.flashMessage = new FlashMessage();
        }

        public SkylineRunnable(int port){
            this.port = port;
            this.sourcesPath = "Source";
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
            this.viewCache = new ViewCache();
            this.flashMessage = new FlashMessage();
        }

        public void Start(){
            try {

                SpecTest specTest = new SpecTest();
                specTest.Run();

                ResourceUtility skylineUtilities = new ResourceUtility();
                
                if (propertiesConfig == null) {
                    propertiesConfig = new PropertiesConfig();
                    propertiesConfig.setPropertiesFile(PROPERTIES);
                }

                RouteAttributesResolver routeAttributesResolver = new RouteAttributesResolver();
                routeAttributesResolver.setPropertiesConfig(propertiesConfig);
                routeAttributes = routeAttributesResolver.resolve();
                routeAttributes.setPersistenceConfig(persistenceConfig);

                foreach(var propertyEntry in routeAttributes.getAttributes()){
                    Console.WriteLine(propertyEntry.Key + ":" + propertyEntry.Value);
                }

                String resourcesDirectory = viewConfig.getResourcesPath();
                viewBytesMap = skylineUtilities.getViewBytesMap(viewConfig);

                ResourceUtility resourceUtility = new ResourceUtility();

                RouteEndpointResolver routeEndpointResolver = new RouteEndpointResolver(new RouteEndpointHolder());
                routeEndpointResolver.setApplicationAttributes(applicationAttributes);
                routeEndpointHolder = routeEndpointResolver.resolve();

                routeAttributes.setRouteEndpointHolder(routeEndpointHolder);

                ComponentAnnotationResolver componentAnnotationResolver = new ComponentAnnotationResolver(new ComponentsHolder());
                componentAnnotationResolver.setApplicationAttributes(applicationAttributes);
                componentsHolder = componentAnnotationResolver.resolve();

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
            Socket networkRequestHandler = listener.Accept();

            String data = null;
            byte[] bytes = null;

            var utf8 = new UTF8Encoding();
            String completeRequestContent = new String("");
            while (true){
                bytes = new byte[1024 * 3];
                int bytesRec = networkRequestHandler.Receive(bytes);
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

            NetworkRequest networkRequest = new NetworkRequest();
            networkRequest.setRequestAction(networkRequestAction);
            networkRequest.setRequestPath(networkRequestPath);
            networkRequest.setAttributes();

            NetworkResponse networkResponse = new NetworkResponse();

            RequestHeaderResolver requestHeaderResolver = new RequestHeaderResolver();
            requestHeaderResolver.setNetworkRequestHeaderElement(requestHeaderElement);
            requestHeaderResolver.setNetworkRequest(networkRequest);
            requestHeaderResolver.resolve();

            SecurityAttributes securityAttributes = new SecurityAttributes(securityElement, securedAttribute);

            RouteEndpointNegotiator routeEndpointNegotiator = new RouteEndpointNegotiator();
            routeEndpointNegotiator.setApplicationAttributes(applicationAttributes);
            routeEndpointNegotiator.setSecurityAttributes(securityAttributes);
            routeEndpointNegotiator.setRouteAttributes(routeAttributes);
            routeEndpointNegotiator.setComponentsHolder(componentsHolder);

            RouteAttributes routeAttributesFinal = routeEndpointNegotiator.getRouteAttributes();
            networkRequest.setRouteAttributes(routeAttributesFinal);
            SecurityManager securityManager = routeAttributesFinal.getSecurityManager();
            if(securityManager != null) securityManager.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());

            RouteResult routeResult = routeEndpointNegotiator.negotiate("reload-request", viewConfig.getResourcesPath(), flashMessage, viewCache, viewConfig, networkRequest, networkResponse, securityAttributes, securityManager, viewBytesMap);

            SecurityAttributeResolver securityAttributeResolver = new SecurityAttributeResolver();
            securityAttributeResolver.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());
            securityAttributeResolver.resolve(networkRequest, networkResponse);

            // networkRequestHandler.Send((requestVersion + " ").getBytes());
            // networkRequestHandler.Send(routeResult.getResponseCode().getBytes());
            // networkRequestHandler.Send(BREAK.getBytes());

            // if(networkRequest.isRedirect()) {
            //     networkRequestHandler.Send("Content-Type:text/html".getBytes());
            //     networkRequestHandler.Send(BREAK.getBytes());
            //     networkRequestHandler.Send("Set-Cookie:".getBytes());
            //     networkRequestHandler.Send(sessionValues.toString().getBytes());
            //     networkRequestHandler.Send(BREAK.getBytes());
            //     networkRequestHandler.Send(("Location: " +  networkRequest.getRedirectLocation() + SPACE).getBytes());
            //     networkRequestHandler.Send(BREAK.getBytes());
            //     networkRequestHandler.Send("Content-Length: -1".getBytes());
            //     networkRequestHandler.Send(DOUBLEBREAK.getBytes());

            //     networkRequestHandler.close();
            //
            //     viewCache = new ViewCache();
            //     flashMessage = new FlashMessage();
            //     ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteNetworkRequest));
            //     return;
            // }


            // networkRequestHandler.Send("Content-Type:".getBytes());
            // networkRequestHandler.Send(routeResult.getContentType().getBytes());
            // networkRequestHandler.Send(BREAK.getBytes());

            // networkRequestHandler.Send("Set-Cookie:".getBytes());
            // networkRequestHandler.Send(sessionValues.toString().getBytes());
            // networkRequestHandler.Send(DOUBLEBREAK.getBytes());
            // networkRequestHandler.Send(routeResult.getResponseBytes());

            byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
            networkRequestHandler.Send(resp);
            networkRequestHandler.Close();

            ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteNetworkRequest));
        }    

        static String GetBytesToStringConverted(byte[] bytes){
            MemoryStream stream = new MemoryStream(bytes);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        public void setPersistenceConfig(PersistenceConfig persistenceConfig){
            this.persistenceConfig = persistenceConfig;
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