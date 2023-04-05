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

        HttpListener listener;

        String securedAttribute = "attribute";
        String securityElement = "default.security";

        public SkylineRunnable(){
            this.port = 1301;
            this.sourcesPath = "Src";
            this.PROPERTIES = "System.Properties";
            this.viewConfig = new ViewConfig();
            this.viewCache = new ViewCache();
            this.flashMessage = new FlashMessage();
        }

        public SkylineRunnable(int port){
            this.port = port;
            this.sourcesPath = "Src";
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


			    listener = new HttpListener();
			    listener.Prefixes.Add("http://*:" + port.ToString() + "/");

			    listener.Start();
                Console.WriteLine("Registering request handlers, please wait...");
                
                int requestCount = 0;
                while(requestCount < numberOfRequestExecutors){ 
                    PrepareNetworkRequest(); 
                    requestCount++;
                }

                Console.WriteLine("Ready!");

            }catch(Exception ex){
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
        
        void PrepareNetworkRequest(){
			var result = listener.BeginGetContext(ExecuteNetworkRequest, listener);
			var startNew = Stopwatch.StartNew();
			result.AsyncWaitHandle.WaitOne();
			startNew.Stop();
			Console.WriteLine("ElapsedMilliseconds "+ startNew.ElapsedMilliseconds);
		}

        void ExecuteNetworkRequest(IAsyncResult result){
			
            Thread.Sleep(1000);

			var context = listener.EndGetContext(result);

            var request = context.Request;
            Encoding encoding = request.ContentEncoding;
    
            ArrayList requestBytesArray = new ArrayList();
            MemoryStream memoryStream = new MemoryStream();
            int totalBytesRecieved = 0;
            String completeRequestPayload = "";
            if(request.HttpMethod.ToUpper().Equals("POST")){          
                Stream requestStream = request.InputStream;  
                // Console.WriteLine("l:" + requestStream.Length);
                // byte[] bytesBuffer = new byte[1024 * 12];
                // int bytesRecieved;
                // while(true){
                //     bytesRecieved = requestStream.Read(bytesBuffer, 0, bytesBuffer.Length);
                //     totalBytesRecieved += bytesRecieved;
                //     memoryStream.Write(bytesBuffer, 0, bytesRecieved);
                //     if(totalBytesRecieved == requestStream.Length)break;
                //     bytesBuffer = new byte[1024 * 12];
                // }
                StreamReader reader = new StreamReader(requestStream, encoding); 
                completeRequestPayload = reader.ReadToEnd();
            }

            byte[] requestBytes = encoding.GetBytes(completeRequestPayload);
            // for(int xyz = 0; xyz < requestBytesArray.Count; xyz++){
            //     requestBytes[xyz] = (byte)requestBytesArray[xyz];
            // }

            var clientOut = context.Response.OutputStream;

            Console.WriteLine("completeRequestPayload:" + completeRequestPayload);
            
            ResourceUtility resourceUtility = new ResourceUtility();


            Console.WriteLine(request.Url.OriginalString);
            String portDelimeter = ":" + port.ToString();
            String[] requestParts = request.Url.ToString().Split(portDelimeter, 2);
            Console.WriteLine(requestParts.Length);
            String requestAction = requestParts[1];

            Console.WriteLine(requestAction);
            
            if(request.QueryString.Equals(FAVICON)){
                PrepareNetworkRequest();
                return;
            }
            
            RouteAttributes routeAttributesCopy = new RouteAttributes(routeAttributes);
            ApplicationAttributes applicationAttributesCopy = new ApplicationAttributes(applicationAttributes);
            SecurityAttributes securityAttributes = new SecurityAttributes(securityElement, securedAttribute);

            NetworkRequest networkRequest = new NetworkRequest();
            NetworkResponse networkResponse = new NetworkResponse();
            networkRequest.setRequestAction(request.HttpMethod.ToLower());
            networkRequest.setRequestPath(requestAction);
            networkRequest.resolveRequestAttributes();
            networkRequest.setSecurityAttributes(securityAttributes);

            foreach (String key in request.Headers.AllKeys){
                String[] values = request.Headers.GetValues(key);
                if(values.Length > 0){
                    StringBuilder Sb = new StringBuilder();
                    foreach (String value in values){
                        Sb.Append(value + ";");
                    }
                    networkRequest.getHeaders()[key.ToLower()] = Sb.ToString();
                }
            }

            // RequestHeaderResolver requestHeaderResolver = new RequestHeaderResolver();
            // requestHeaderResolver.setNetworkRequestHeaderElement(requestHeaderElement);
            // requestHeaderResolver.setNetworkRequest(networkRequest);
            // requestHeaderResolver.resolve();
            
            RequestComponentResolver requestComponentResolver = new RequestComponentResolver();
            requestComponentResolver.setRequestPayload(completeRequestPayload);
            requestComponentResolver.setNetworkRequest(networkRequest);
            requestComponentResolver.setEncoding(encoding);
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
            
            SecurityAttributeResolver securityAttributeResolver = new SecurityAttributeResolver();
            securityAttributeResolver.setSecurityAttributes(routeEndpointNegotiator.getSecurityAttributes());
            securityAttributeResolver.resolve(networkRequest, networkResponse);

            StringBuilder sessionValues = new StringBuilder();
            foreach(var securityAttributeEntry in networkResponse.getSecurityAttributes()){
                SecurityAttribute securityAttribute = securityAttributeEntry.Value;
                sessionValues.Append(securityAttribute.getName()).Append("=").Append(securityAttribute.getValue());
            }

            RouteResult routeResult = routeEndpointNegotiator.negotiate(viewConfig.getRenderingScheme(), viewConfig.getResourcesPath(), flashMessage, viewCache, viewConfig, networkRequest, networkResponse, securityAttributes, securityManager, viewBytesMap);

            StringBuilder responseOutput = new StringBuilder();

            Console.WriteLine("r..." + routeResult.getResponseCode());

            // context.Response.StatusCode = int.Parse(routeResult.getResponseCode());
			// context.Response.StatusDescription = "OK";

            context.Response.ContentType = routeResult.getContentType();
            Console.WriteLine("set...");
            responseOutput.Append("HTTP/1.1 " + routeResult.getResponseCode());
            responseOutput.Append(BREAK);


            if(networkRequest.isRedirect()) {

                responseOutput.Append("Content-Type:text/plain");
                responseOutput.Append(BREAK);

                responseOutput.Append("Set-Cookie:" + sessionValues.ToString());
                responseOutput.Append(BREAK);

                responseOutput.Append("Location: " +  networkRequest.getRedirectLocation() + SPACE);
                responseOutput.Append(BREAK);

                responseOutput.Append("Content-Length: -1");
                responseOutput.Append(DOUBLEBREAK);

                clientOut.Write(encoding.GetBytes(responseOutput.ToString()), 0, responseOutput.ToString().Length);

                clientOut.Close();
            
                viewCache = new ViewCache();
                flashMessage = new FlashMessage();

                PrepareNetworkRequest();
            }

            responseOutput.Append("Content-Type:" + routeResult.getContentType());                
            responseOutput.Append(BREAK);

            responseOutput.Append("Set-Cookie:" + sessionValues.ToString());
            responseOutput.Append(DOUBLEBREAK);

            String resultOut = encoding.GetString(routeResult.getResponseOutput());
            responseOutput.Append(resultOut);//hi
 
            // // byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
            // // clientOut.Write(resp);

            clientOut.Write(encoding.GetBytes(responseOutput.ToString()), 0, responseOutput.ToString().Length);
            clientOut.Close();

            PrepareNetworkRequest();
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