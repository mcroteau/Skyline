using System;
using System.Net;
using System.Net.Sockets;
using System.IO.Pipes;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

using Skyline;
using Skyline.Model;
using Skyline.Specs;
using Skyline.Implement;
using Skyline.Security;

namespace Skyline{

    public class SkylineServer {
        
        String FAVICON = "/favicon.ico";
        String BREAK = "\r\n";
        String SPACE = " ";
        String DOUBLEBREAK = "\r\n\r\n";

        int REQUEST_METHOD = 0;
        int REQUEST_PATH = 1;
        int REQUEST_VERSION = 2;

        int numberOfPartitions = 3;
        int numberOfRequestExecutors = 10;

        int port;
        String sourcesPath;
        String PROPERTIES;

        Boolean persistentMode;

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

        HttpListener listener;

        String securedAttribute = "attribute";
        String securityElement = "default.security";

        public SkylineServer(){
            this.port = 4000;
            this.sourcesPath = "src";
            this.persistentMode = false;
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
            this.viewCache = new ViewCache();
            this.flashMessage = new FlashMessage();
        }

        public SkylineServer(int port){
            this.port = port;
            this.sourcesPath = "src";
            this.persistentMode = false;
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
            this.viewCache = new ViewCache();
            this.flashMessage = new FlashMessage();
        }

        public SkylineServer(int port, int numberOfRequestExecutors){
            this.port = port;
            this.sourcesPath = "src";
            this.persistentMode = false;
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
            this.viewCache = new ViewCache();
            this.flashMessage = new FlashMessage();
            this.numberOfRequestExecutors = numberOfRequestExecutors;
        }

        public void start(){
            try {

                // SpecTest specTest = new SpecTest();
                // specTest.Run();

                ResourceUtility skylineUtilities = new ResourceUtility();
                
                if (propertiesConfig == null) {
                    propertiesConfig = new PropertiesConfig();
                    propertiesConfig.setPropertiesFile(PROPERTIES);
                }
                
                if(applicationAttributes == null){
                    applicationAttributes = new ApplicationAttributes();
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


			    listener = new HttpListener();
			    listener.Prefixes.Add("http://*:" + port.ToString() + "/");

			    listener.Start();
                
                Console.WriteLine("Ready!");

                int requestCount = 0;
                while(requestCount < numberOfRequestExecutors){ 
                    PrepareNetworkRequest(); 
                    requestCount++;
                }


            }catch(Exception ex){
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
        
        void PrepareNetworkRequest(){
            Console.WriteLine("prep");
			var result = listener.BeginGetContext(ExecuteNetworkRequest, listener);
			result.AsyncWaitHandle.WaitOne();
		}

        void ExecuteNetworkRequest(IAsyncResult result){
			
            Console.WriteLine("1");

            Thread.Sleep(300);

			var context = listener.EndGetContext(result);

            var request = context.Request;
            Encoding encoding = request.ContentEncoding;

            MemoryStream memoryStream = new MemoryStream();
            int totalBytesReceived = 0;
            byte[] requestBytes = new byte[13];
            String completeRequestPayload = "";
            if(request.HttpMethod.ToUpper().Equals("POST")){          
                Stream requestStream = request.InputStream;  
                byte[] bytesBuffer = new byte[1024 * 12];
                int len = int.Parse(request.Headers["Content-Length"]);
                int bytesReceived;
                while(true){
                    bytesReceived = request.InputStream.Read(bytesBuffer, 0, bytesBuffer.Length);
                    totalBytesReceived += bytesReceived;
                    memoryStream.Write(bytesBuffer, 0, bytesReceived);
                    if(totalBytesReceived == len)break;
                }

                requestBytes = memoryStream.ToArray();

                char[] requestChars = new char[len];
                for(int xyz = 0; xyz < requestBytes.Length; xyz++){
                    requestChars[xyz] = Convert.ToChar(requestBytes[xyz]);
                }
                completeRequestPayload = new String(requestChars);

            }

            Console.WriteLine("2");

            var clientOut = context.Response.OutputStream;            
            ResourceUtility resourceUtility = new ResourceUtility();

            String portDelimeter = ":" + port.ToString();
            String[] requestParts = request.Url.ToString().Split(portDelimeter, 2);

            String requestPath = requestParts[1];

            if(request.QueryString.Equals(FAVICON)){
                PrepareNetworkRequest();
                return;
            }
            
            Console.WriteLine("3");
            RouteAttributes routeAttributesCopy = new RouteAttributes(routeAttributes);
            ApplicationAttributes applicationAttributesCopy = new ApplicationAttributes(applicationAttributes);
            SecurityAttributes securityAttributes = new SecurityAttributes(securityElement, securedAttribute);

            Console.WriteLine("4");
            NetworkRequest networkRequest = new NetworkRequest();
            networkRequest.setRequestAction(request.HttpMethod.ToLower());
            networkRequest.setRequestPath(requestPath);
            networkRequest.resolveRequestAttributes();
            networkRequest.setSecurityAttributes(securityAttributes);
            networkRequest.setContext(context);

            Console.WriteLine("5");
            NetworkResponse networkResponse = new NetworkResponse();
            networkResponse.setContext(context);

            RequestHeaderResolver requestHeaderResolver = new RequestHeaderResolver();
            requestHeaderResolver.setNetworkRequest(networkRequest);
            requestHeaderResolver.resolve();
            
            Console.WriteLine("6");
            RequestComponentResolver requestComponentResolver = new RequestComponentResolver();
            requestComponentResolver.setRequestPayload(completeRequestPayload);
            requestComponentResolver.setNetworkRequest(networkRequest);
            requestComponentResolver.setQueryString(requestPath);
            requestComponentResolver.setEncoding(encoding);
            requestComponentResolver.resolve();

            Console.WriteLine("7");
            RouteEndpointNegotiator routeEndpointNegotiator = new RouteEndpointNegotiator();
            routeEndpointNegotiator.setApplicationAttributes(applicationAttributesCopy);
            routeEndpointNegotiator.setSecurityAttributes(securityAttributes);
            routeEndpointNegotiator.setRouteAttributes(routeAttributes);
            routeEndpointNegotiator.setComponentsHolder(componentsHolder);

            Console.WriteLine("8");
            RouteAttributes routeAttributesFinal = routeEndpointNegotiator.getRouteAttributes();
            networkRequest.setRouteAttributes(routeAttributesFinal);
            
            DataTransferObject dto = new DataTransferObject(persistenceConfig);
            dto.setApplicationAttributes(applicationAttributesCopy);

            SecurityManager securityManager = null;
            if(securityAccessType != null){
                SecurityAccess securityAccessInstance = (SecurityAccess) Activator.CreateInstance(securityAccessType, new Object[]{dto});
                securityManager = new SecurityManager(securityAccessInstance, securityAttributes);
                if(securityManager != null) securityManager.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());
            }

            Console.WriteLine("9");
            SecurityAttributeResolver securityAttributeResolver = new SecurityAttributeResolver();
            securityAttributeResolver.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());
            securityAttributeResolver.resolve(networkRequest, networkResponse);

            StringBuilder sessionValues = new StringBuilder();
            foreach(var securityAttributeEntry in networkResponse.getSecurityAttributes()){
                SecurityAttribute securityAttribute = securityAttributeEntry.Value;
                sessionValues.Append(securityAttribute.getName()).Append("=").Append(securityAttribute.getValue());
            }

            Console.WriteLine("10");
            RouteResult routeResult = routeEndpointNegotiator.negotiate(persistentMode, viewConfig.getRenderingScheme(), viewConfig.getResourcesPath(), flashMessage, viewCache, viewConfig, networkRequest, networkResponse, securityAttributes, securityManager, viewBytesMap);
            StringBuilder responseOutput = new StringBuilder();

            // context.Response.StatusCode = int.Parse(routeResult.getResponseCode());
			// context.Response.StatusDescription = "OK";

            context.Response.ContentType = routeResult.getContentType();
        
            if(networkRequest.isRedirect()) {
                
                Console.WriteLine("." + networkRequest.getRedirectLocation());
                context.Response.RedirectLocation = networkRequest.getRedirectLocation();
                context.Response.Redirect(networkRequest.getRedirectLocation());
                responseOutput.Append(DOUBLEBREAK);

                context.Response.OutputStream.Write(encoding.GetBytes(responseOutput.ToString()), 0, responseOutput.ToString().Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
            
                viewCache = new ViewCache();
                flashMessage = new FlashMessage();

                PrepareNetworkRequest();
            }

            Console.WriteLine("11");
            String resultOut = encoding.GetString(routeResult.getResponseOutput());
            responseOutput.Append(resultOut);//hi
 
            // // byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
            // // clientOut.Write(resp);

            context.Response.OutputStream.Write(encoding.GetBytes(responseOutput.ToString()), 0, responseOutput.ToString().Length);
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();

            PrepareNetworkRequest();
        }    

        public void setPersistentMode(Boolean persistentMode){
            this.persistentMode = persistentMode;
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

        public void setNumberOfRequestNegotiators(int numberOfRequestExecutors){
            this.numberOfRequestExecutors = numberOfRequestExecutors;
        }

        public void setApplicationAttributes(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }
    }
}