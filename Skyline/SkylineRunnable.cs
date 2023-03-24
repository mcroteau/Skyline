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
using Skyline.Implement;
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

        int numberOfPartitions = 3;
        int numberOfRequestExecutors = 7;

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

        NetworkRequest networkRequest;
        NetworkResponse networkResponse;

        Type securityAccessType;

        Socket listener;

        String securedAttribute = "attribute";
        String securityElement = "default.security";

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

        public void start(){
            try {

                SpecTest specTest = new SpecTest();
                specTest.Run();

                ResourceUtility skylineUtilities = new ResourceUtility();
                
                if (propertiesConfig == null) {
                    propertiesConfig = new PropertiesConfig();
                    propertiesConfig.setPropertiesFile(PROPERTIES);
                }

                networkRequest = new NetworkRequest();
                networkResponse = new NetworkResponse();

                RouteAttributesResolver routeAttributesResolver = new RouteAttributesResolver();
                routeAttributesResolver.setPropertiesConfig(propertiesConfig);
                routeAttributes = routeAttributesResolver.resolve();
                routeAttributes.setPersistenceConfig(persistenceConfig);

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

            byte[] requestByteArray = utf8.GetBytes(completeRequestContent);
        
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
            SecurityAttributes securityAttributes = new SecurityAttributes(securityElement, securedAttribute);

            networkRequest.setRequestAction(networkRequestAction);
            networkRequest.setRequestPath(networkRequestPath);
            networkRequest.setSecurityAttributes(securityAttributes);
            networkRequest.resolveRequestAttributes();

            RequestHeaderResolver requestHeaderResolver = new RequestHeaderResolver();
            requestHeaderResolver.setNetworkRequestHeaderElement(requestHeaderElement);
            requestHeaderResolver.setNetworkRequest(networkRequest);
            requestHeaderResolver.resolve();

            RequestComponentResolver requestComponentResolver = new RequestComponentResolver();
            requestComponentResolver.setRequestBytes(requestByteArray);
            requestComponentResolver.setNetworkRequest(networkRequest);
            requestComponentResolver.resolve();

            RouteEndpointNegotiator routeEndpointNegotiator = new RouteEndpointNegotiator();
            routeEndpointNegotiator.setApplicationAttributes(applicationAttributes);
            routeEndpointNegotiator.setSecurityAttributes(securityAttributes);
            routeEndpointNegotiator.setRouteAttributes(routeAttributes);
            routeEndpointNegotiator.setComponentsHolder(componentsHolder);

            RouteAttributes routeAttributesFinal = routeEndpointNegotiator.getRouteAttributes();
            networkRequest.setRouteAttributes(routeAttributesFinal);
            DataTransferObject dto = new DataTransferObject(persistenceConfig);
            dto.setApplicationAttributes(applicationAttributesCopy);
            SecurityAccess securityAccessInstance = (SecurityAccess) Activator.CreateInstance(securityAccessType, new Object[]{dto});
            SecurityManager securityManager = new SecurityManager(securityAccessInstance, securityAttributes);
            if(securityManager != null) securityManager.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());
            
            RouteResult routeResult = routeEndpointNegotiator.negotiate(viewConfig.getRenderingScheme(), viewConfig.getResourcesPath(), flashMessage, viewCache, viewConfig, networkRequest, networkResponse, securityAttributes, securityManager, viewBytesMap);

            SecurityAttributeResolver securityAttributeResolver = new SecurityAttributeResolver();
            securityAttributeResolver.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());
            securityAttributeResolver.resolve(networkRequest, networkResponse);

            StringBuilder sessionValues = new StringBuilder();
            foreach(var securityAttributeEntry in networkResponse.getSecurityAttributes()){
                Console.WriteLine("zc:" + securityAttributeEntry.Value);
                SecurityAttribute securityAttribute = securityAttributeEntry.Value;
                sessionValues.Append(securityAttribute.getName()).Append("=").Append(securityAttribute.getValue());
            }

            Console.WriteLine("se:" + sessionValues.ToString());

            networkRequestHandler.Send(utf8.GetBytes((networkRequestVersion + " ")));
            networkRequestHandler.Send(utf8.GetBytes(routeResult.getResponseCode()));
            networkRequestHandler.Send(utf8.GetBytes(BREAK));

            Console.WriteLine("\nbefore:" + networkRequest.isRedirect() + ":" + networkRequest.getRedirectLocation());
            if(networkRequest.isRedirect()) {
                networkRequestHandler.Send(utf8.GetBytes("Content-Type:text/plain"));
                networkRequestHandler.Send(utf8.GetBytes(BREAK));
                networkRequestHandler.Send(utf8.GetBytes("Set-Cookie:"));
                networkRequestHandler.Send(utf8.GetBytes(sessionValues.ToString()));
                networkRequestHandler.Send(utf8.GetBytes(BREAK));
                networkRequestHandler.Send(utf8.GetBytes(("Location: " +  networkRequest.getRedirectLocation() + SPACE)));
                networkRequestHandler.Send(utf8.GetBytes(BREAK));
                networkRequestHandler.Send(utf8.GetBytes("Content-Length: -1"));
                networkRequestHandler.Send(utf8.GetBytes(DOUBLEBREAK));

                networkRequestHandler.Close();
            
                viewCache = new ViewCache();
                flashMessage = new FlashMessage();
                ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteNetworkRequest));
                return;
            }


            networkRequestHandler.Send(utf8.GetBytes("Content-Type:"));
            networkRequestHandler.Send(utf8.GetBytes(routeResult.getContentType()));
            networkRequestHandler.Send(utf8.GetBytes(BREAK));

            networkRequestHandler.Send(utf8.GetBytes("Set-Cookie:"));
            networkRequestHandler.Send(utf8.GetBytes(sessionValues.ToString()));
            networkRequestHandler.Send(utf8.GetBytes(DOUBLEBREAK));
            networkRequestHandler.Send(routeResult.getResponseOutput());

            // byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
            // networkRequestHandler.Send(resp);
            networkRequestHandler.Close();

            networkRequest = new NetworkRequest();
            networkResponse = new NetworkResponse();

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

        public void setSourcesPath(String sourcesPath){
            this.sourcesPath = sourcesPath;
        }

        public void setSecurityAccessType(Type securityAccessType) {
            this.securityAccessType = securityAccessType;
        }

        public void setNumberOfPartitions(int numberOfPartitions){
            this.numberOfPartitions = numberOfPartitions;
        }

        public void setNumberOfRequestExecutors(int numberOfRequestExecutors){
            this.numberOfRequestExecutors = numberOfRequestExecutors;
        }

        public void setApplicationAttributes(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }
    }
}