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

        Int32[] prefixes;
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
            this.sourcesPath = "src";
            this.persistentMode = false;
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
            this.viewCache = new ViewCache();
            this.flashMessage = new FlashMessage();
        }


        public void start(){
            try {

                SpecTest specTest = new SpecTest();
                specTest.Run();

                if(prefixes == null){
                    Console.WriteLine("Specify at least one prefix/port # example: new Int32[]{9000}");
                    throw new Exception();
                }

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
                foreach(int prefix in prefixes){
			        listener.Prefixes.Add("http://*:" + prefix.ToString() + "/");
                }

			    listener.Start();


                Console.WriteLine("   _____ __         ___");          
                Console.WriteLine("  / ___// /____  __/ (_)___  ___"); 
                Console.WriteLine("  \\__ \\/ //_/ / / / / / __ \\/ _ \\");
                Console.WriteLine(" ___/ / ,< / /_/ / / / / / /  __/");
                Console.WriteLine("/____/_/|_|\\__, /_/_/_/ /_/\\___/");
                Console.WriteLine("          /____/\n");            

                 
                foreach(int prefix in prefixes){
                    Console.WriteLine("http://localhost:" + prefix.ToString() + "/");
                }
                Console.WriteLine("\nRunning!");


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
			var result = listener.BeginGetContext(ExecuteNetworkRequest, listener);
			result.AsyncWaitHandle.WaitOne();
		}

        void ExecuteNetworkRequest(IAsyncResult result){
		
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


            var clientOut = context.Response.OutputStream;            
            ResourceUtility resourceUtility = new ResourceUtility();


            String portDelimeter = ":" + ((IPEndPoint)context.Request.LocalEndPoint).Port.ToString();
            String[] requestParts = request.Url.ToString().Split(portDelimeter, 2);

            String requestPath = requestParts[1];

            if(request.QueryString.Equals(FAVICON)){
                PrepareNetworkRequest();
                return;
            }
            
            RouteAttributes routeAttributesCopy = new RouteAttributes(routeAttributes);
            ApplicationAttributes applicationAttributesCopy = new ApplicationAttributes(applicationAttributes);
            SecurityAttributes securityAttributes = new SecurityAttributes(securityElement, securedAttribute);

            NetworkRequest networkRequest = new NetworkRequest();
            networkRequest.setRequestAction(request.HttpMethod.ToLower());
            networkRequest.setRequestPath(requestPath);
            networkRequest.resolveRequestAttributes();
            networkRequest.setSecurityAttributes(securityAttributes);
            networkRequest.setContext(context);

            NetworkResponse networkResponse = new NetworkResponse();
            networkResponse.setContext(context);

            RequestHeaderResolver requestHeaderResolver = new RequestHeaderResolver();
            requestHeaderResolver.setNetworkRequest(networkRequest);
            requestHeaderResolver.resolve();
            
            RequestComponentResolver requestComponentResolver = new RequestComponentResolver();
            requestComponentResolver.setRequestPayload(completeRequestPayload);
            requestComponentResolver.setNetworkRequest(networkRequest);
            requestComponentResolver.setQueryString(requestPath);
            requestComponentResolver.setEncoding(encoding);
            requestComponentResolver.resolve();

            RouteEndpointNegotiator routeEndpointNegotiator = new RouteEndpointNegotiator();
            routeEndpointNegotiator.setApplicationAttributes(applicationAttributesCopy);
            routeEndpointNegotiator.setSecurityAttributes(securityAttributes);
            routeEndpointNegotiator.setRouteAttributes(routeAttributes);
            routeEndpointNegotiator.setComponentsHolder(componentsHolder);

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

            SecurityAttributeResolver securityAttributeResolver = new SecurityAttributeResolver();
            securityAttributeResolver.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());
            securityAttributeResolver.resolve(networkRequest, networkResponse);

            StringBuilder sessionValues = new StringBuilder();
            foreach(var securityAttributeEntry in networkResponse.getSecurityAttributes()){
                SecurityAttribute securityAttribute = securityAttributeEntry.Value;
                sessionValues.Append(securityAttribute.getName()).Append("=").Append(securityAttribute.getValue());
            }

            RouteResult routeResult = routeEndpointNegotiator.negotiate(persistentMode, viewConfig.getRenderingScheme(), viewConfig.getResourcesPath(), flashMessage, viewCache, viewConfig, networkRequest, networkResponse, securityAttributes, securityManager, viewBytesMap);

            context.Response.ContentType = routeResult.getContentType();
            context.Response.ContentEncoding = Encoding.UTF8;
        
            if(networkRequest.isRedirect()) {
                context.Response.RedirectLocation = networkRequest.getRedirectLocation();
                context.Response.Redirect(networkRequest.getRedirectLocation());
                context.Response.OutputStream.Write(encoding.GetBytes(DOUBLEBREAK), 0, DOUBLEBREAK.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();

                PrepareNetworkRequest();
            }
            
            context.Response.OutputStream.Write(routeResult.getResponseOutput(), 0, routeResult.getResponseOutput().Length);
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();

            viewCache = new ViewCache();
            flashMessage = new FlashMessage();

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
        
        public void setPorts(Int32[] prefixes){
            this.prefixes = prefixes;
        }
    }
}
