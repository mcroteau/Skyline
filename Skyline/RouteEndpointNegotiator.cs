using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

namespace Skyline{

    public class RouteEndpointNegotiator{

        RouteAttributes routeAttributes;
        ComponentsHolder componentsHolder;
        ApplicationAttributes applicationAttributes;
        SecurityAttributes securityAttributes;

        public RouteResult negotiate(String renderer, String resourcesDirectory, FlashMessage flashMessage, ViewCache viewCache, ViewConfig viewConfig, NetworkRequest networkRequest, NetworkResponse networkResponse, SecurityAttributes securityAttributes, SecurityManager securityManager, Dictionary<String, byte[]> viewBytesMap){

            var utf8 = new UTF8Encoding();
            String completePageRendered = "";
            String errorMessage = "";

            try {

                RouteResult routeResult = new RouteResult();
                byte[] responseOutput = new byte[]{};

                String routeEndpointPath = networkRequest.getRequestPath();
                String routeEndpointAction = networkRequest.getRequestAction().ToLower();

                if(routeEndpointPath.Equals("/launcher.status")){
                    return new RouteResult(utf8.GetBytes("200 OK"), "200 OK", "text/plain");
                }

                ResourceUtility resourceUtility = new ResourceUtility();
                ApplicationExperienceResolver experienceResolver = new ApplicationExperienceResolver();

                RouteAttributes routeAttributes = networkRequest.getRouteAttributes();
                RouteEndpointHolder routeEndpointHolder = routeAttributes.getRouteEndpointHolder();

                if(routeEndpointPath.StartsWith("/" + resourcesDirectory + "/")) {

                    MimeTypeResolver mimeTypeResolver = new MimeTypeResolver();
                    mimeTypeResolver.setRouteEndpointPath(routeEndpointPath);
                    String mime = mimeTypeResolver.resolve();

                    if (renderer.Equals("cache-requests")) {
                        byte[] responseBytes = resourceUtility.getViewFileCopy(routeEndpointPath, viewBytesMap);
                        routeResult.setStatusCode(200);
                        routeResult.setResponseOutput(responseOutput);
                        routeResult.setContentType(mime);
                    }
                    if(renderer.Equals("reload-requests")){
                        FileToByteArrayConverter resourcesFileConverter = new FileToByteArrayConverter();
                        resourcesFileConverter.setFileName(routeEndpointPath);
                        resourcesFileConverter.setViewConfig(viewConfig);
                        responseOutput = resourcesFileConverter.convert();

                        routeResult.setStatusCode(200);
                        routeResult.setResponseOutput(responseOutput);
                        routeResult.setContentType(mime);
                        return routeResult;
                    }

                }

                foreach(var entry in routeEndpointHolder.getRouteEndpoints()){
                    Console.WriteLine("0 : '{0}'", entry.Key);
                }

                Console.WriteLine("0.1 : {0}", routeEndpointHolder.getRouteEndpoints().ContainsKey("/"));

                RouteEndpointNormalizer routeEndpointNormalizer = new RouteEndpointNormalizer();
                routeEndpointNormalizer.setRouteEndpointPath(routeEndpointPath);
                routeEndpointNormalizer.setRouteEndpointAction(routeEndpointAction);
                routeEndpointPath = routeEndpointNormalizer.normalize();//:looking for key
                Console.WriteLine("1 {0}", routeEndpointPath);

                RouteEndpointLocator routeEndpointLocator = new RouteEndpointLocator();
                routeEndpointLocator.setRouteEndpointHolder(routeEndpointHolder);
                routeEndpointLocator.setRouteEndpointAction(routeEndpointAction);
                routeEndpointLocator.setRouteEndpointPath(routeEndpointPath);
                RouteEndpoint routeEndpoint = routeEndpointLocator.locate();

                Console.WriteLine("2" + routeEndpoint);

                MethodComponents methodComponents = getMethodAttributesComponents(routeEndpointPath, viewCache, flashMessage, networkRequest, networkResponse, securityManager, routeEndpoint);
                MethodInfo routeEndpointInstanceMethod = routeEndpoint.getRouteMethod();
         
                String headline, keywords, description = "";
                Attributes attributes = (Attributes) Attribute.GetCustomAttribute(routeEndpointInstanceMethod.GetType(), typeof (Attributes));
                if(attributes != null){
                    headline = attributes.getHeadline();
                    keywords = attributes.getKeywords();
                    description = attributes.getDescription();
                }

                Console.WriteLine("2.1 {0}", routeEndpoint.getKlassReference());
                Object routeEndpointInstanceRef = Activator.CreateInstance(routeEndpoint.getKlassAssembly(), routeEndpoint.getKlassReference()).Unwrap();
                Object routeEndpointInstance = Activator.CreateInstance(routeEndpointInstanceRef.GetType(), new Object[]{applicationAttributes}, new Object[]{});
                
                Console.WriteLine("2.2 {0}", routeEndpointInstance);

                PersistenceConfig persistenceConfig = routeAttributes.getPersistenceConfig();
                if(persistenceConfig != null) {
                    DataTransferObject repoDto = new DataTransferObject(persistenceConfig);

                    FieldInfo[] routeFields = routeEndpointInstance.GetType().GetFields();
                    foreach(FieldInfo routeFieldInfo in routeFields) {

                        Bind bind = (Bind) Attribute.GetCustomAttribute(routeFieldInfo.GetType(), typeof (Bind));
                        if (bind != null) {
                            String fieldInfoKey = routeFieldInfo.Name.ToLower();
                            if (componentsHolder.getRepositories().ContainsKey(fieldInfoKey)) {
                                Type repositoryKlassType = componentsHolder.getRepositories()[fieldInfoKey];
                                Object repositoryKlassInstance = Activator.CreateInstance(repositoryKlassType, new Object[]{repoDto, applicationAttributes}, new Object[]{});
                                routeFieldInfo.SetValue(routeEndpointInstance, repositoryKlassInstance);
                            }
                        }
                    }
                }

                Console.WriteLine("3");

    //             if(routeEndpointInstanceMethod.isAnnotationPresent(Before.class)){
    //                 Before beforeAnnotation = routeEndpointInstanceMethod.getAnnotation(Before.class);

    //                 String routePrincipalVariablesElement = beforeAnnotation.variables();
    // //                System.out.println("routePrincipalVariablesElement: " + routePrincipalVariablesElement);
    //                 BeforeAttributes beforeAttributes = new BeforeAttributes();

    //                 String[] routePrincipalVariables = routePrincipalVariablesElement.split(",");
    //                 Integer routeVariableIndex = 0;
    //                 List<Object> routeAttributesVariableList = methodComponents.getRouteMethodAttributeVariablesList();
    //                 for(String routePrincipalVariableElement : routePrincipalVariables){
    //                     Object routePrincipalVariableValue = routeAttributesVariableList.get(routeVariableIndex);
    //                     String routePrincipalVariable = routePrincipalVariableElement.replace("{", "")
    //                             .replace("}", "").trim();
    //                     beforeAttributes.set(routePrincipalVariable, routePrincipalVariableValue);
    //                 }

    //                 for(Map.Entry<String, Object> routePrincipalInstance : routeEndpointInstances.entrySet()){
    //                     String routePrincipalInstanceKey = routePrincipalInstance.getKey().toLowerCase();
    // //                    System.out.println("key:" + routePrincipalInstanceKey + ":" + routePrincipalInstance.getValue());
    //                     beforeAttributes.set(routePrincipalInstanceKey, routePrincipalInstance.getValue());
    //                 }

    //                 BeforeResult beforeResult = null;
    //                 Class<?>[] routePrincipalKlasses = beforeAnnotation.value();
    //                 for(Class<?> routePrincipalKlass : routePrincipalKlasses) {
    //                     RouteEndpointBefore routePrincipal = (RouteEndpointBefore) routePrincipalKlass.getConstructor().newInstance();
    //                     beforeResult = routePrincipal.before(flashMessage, viewCache, networkRequest, networkResponse, securityManager, beforeAttributes);
    //                     if(!beforeResult.getRedirectUri().Equals("")){
    //                         RedirectInfo redirectInfo = new RedirectInfo();
    //                         redirectInfo.setMethodName(routeEndpointInstanceMethod.getName());
    //                         redirectInfo.setKlassName(routeInstance.getClass().getName());

    //                         if(beforeResult.getRedirectUri() == null || beforeResult.getRedirectUri().Equals("")){
    //                             throw new StargzrException("redirect uri is empty on " + routePrincipalKlass.getName());
    //                         }

    //                         String redirectRouteUri = resourceUtility.getRedirect(beforeResult.getRedirectUri());

    //                         if(!beforeResult.getMessage().Equals("")){
    //                             viewCache.set("message", beforeResult.getMessage());
    //                         }

    //                         networkRequest.setRedirect(true);
    //                         networkRequest.setRedirectLocation(redirectRouteUri);
    //                         break;
    //                     }
    //                 }

    //                 if(!beforeResult.getRedirectUri().Equals("")){
    //                     return new RouteResult(utf8.GetBytes("303"), "303", "text/html");
    //                 }
    //             }


                Console.WriteLine("4 {0}" + methodComponents.getRouteMethodAttributesList());

                Object[] methodAttributes = methodComponents.getRouteMethodAttributesList().ToArray();
                Object routeResponseObject = routeEndpointInstanceMethod.Invoke(routeEndpointInstance, methodAttributes);
                String methodResponse = routeResponseObject.ToString();
                if(methodResponse == null){
                    return new RouteResult(utf8.GetBytes("404"), "404", "text/html");
                }

            //     if(methodResponse.startsWith("redirect:")) {
            //         RedirectInfo redirectInfo = new RedirectInfo();
            //         redirectInfo.setMethodName(routeEndpointInstanceMethod.getName());
            //         redirectInfo.setKlassName(routeEndpointInstance.getClass().getName());
            //         String redirectRouteUri = resourceUtility.getRedirect(methodResponse);
            //         networkRequest.setRedirect(true);
            //         networkRequest.setRedirectLocation(redirectRouteUri);
            //         return new RouteResult(utf8.GetBytes("303"), "303", "text/html");
            //     }

            //     if(routeEndpointInstanceMethod.isAnnotationPresent(JsonOutput.class)){
            //         return new RouteResult(utf8.GetBytes(methodResponse), "200 OK", "application/json");
            //     }

            //     if(routeEndpointInstanceMethod.isAnnotationPresent(Text.class)){
            //         return new RouteResult(utf8.GetBytes(methodResponse), "200 OK", "text/html");
            //     }

            //     if(renderer.Equals("cache-request")) {

            //         ByteArrayOutputStream unebaos = resourceUtility.getViewFileCopy(methodResponse, viewBytesMap);
            //         if(unebaos == null){
            //             return new RouteResult(utf8.GetBytes("404"), "404", "text/html");
            //         }
            //         completePageRendered = unebaos.toString();

            //     }else{

            //         Path webPath = Paths.get("src", "main", viewConfig.getViewsPath());
            //         if (methodResponse.startsWith("/")) {
            //             methodResponse = methodResponse.replaceFirst("/", "");
            //         }

            //         String htmlPath = webPath.toFile().getAbsolutePath().concat(File.separator + methodResponse);
            //         File viewFile = new File(htmlPath);
            //         ByteArrayOutputStream unebaos = new ByteArrayOutputStream();

            //         InputStream pageInputStream = new FileInputStream(viewFile);
            //         byte[] bytes = new byte[(int) viewFile.length()];
            //         int pageBytesLength;
            //         while ((pageBytesLength = pageInputStream.read(bytes)) != -1) {
            //             unebaos.write(bytes, 0, pageBytesLength);
            //             if(pageInputStream.available() == 0)break;
            //         }
            //         completePageRendered = unebaos.toString();//todo? ugly
            //     }


            //     viewCache.set("message", flashMessage.getMessage());

            //     String designUri = null;
            //     if(routeEndpointInstanceMethod.isAnnotationPresent(Design.class)){
            //         Design annotation = routeEndpointInstanceMethod.getAnnotation(Design.class);
            //         designUri = annotation.value();
            //     }
            //     if(designUri != null) {
            //         String designContent;
            //         if(renderer.Equals("cache-request")) {

            //             ByteArrayOutputStream baos = resourceUtility.getViewFileCopy(designUri, viewBytesMap);
            //             designContent = baos.toString();

            //         }else{

            //             Path designPath = Paths.get("src", "main", "webapp", designUri);
            //             File designFile = new File(designPath.toString());
            //             InputStream designInputStream = new FileInputStream(designFile);

            //             ByteArrayOutputStream baos = new ByteArrayOutputStream();

            //             byte[] bytes = new byte[(int) designFile.length()];
            //             int length;
            //             while ((length = designInputStream.read(bytes)) != -1) {
            //                 baos.write(bytes, 0, length);
            //                 if(designInputStream.available() == 0)break;
            //             }
            //             designContent = baos.toString();

            //         }

            //         if(designContent == null){
            //             return new RouteResult(utf8.GetBytes("design not found."), "200 OK", "text/html");
            //         }

            //         if(!designContent.contains("<c:content/>")){
            //             return new RouteResult(utf8.GetBytes("Your template file is missing <c:content/>"), "200 OK", "text/html");
            //         }

            //         String[] bits = designContent.split("<c:content/>");
            //         String header = bits[0];
            //         String bottom = "";
            //         if(bits.length > 1) bottom = bits[1];

            //         header = header + completePageRendered;
            //         completePageRendered = header + bottom;

            //         if(title != null) {
            //             completePageRendered = completePageRendered.replace("${title}", title);
            //         }
            //         if(keywords != null) {
            //             completePageRendered = completePageRendered.replace("${keywords}", keywords);
            //         }
            //         if(description != null){
            //             completePageRendered = completePageRendered.replace("${description}", description);
            //         }

            //         completePageRendered = experienceManager.execute(completePageRendered, viewCache, networkRequest, securityAttributes, viewRenderers);
            //         return new RouteResult(utf8.GetBytes(completePageRendered), "200 OK", "text/html");

            //     }else{
            //         completePageRendered = experienceManager.execute(completePageRendered, viewCache, networkRequest, securityAttributes, viewRenderers);
            //         return new RouteResult(utf8.GetBytes(completePageRendered), "200 OK", "text/html");
            //     }

            }catch(Exception ex){
                Console.WriteLine(ex.Message);
                return new RouteResult(utf8.GetBytes("issue. " + ex.Message), "500", "text/plain");
            }

            return new RouteResult(utf8.GetBytes("issue. "), "500", "text/plain");
        }

        MethodComponents getMethodAttributesComponents(String routeEndpointPath, ViewCache viewCache, FlashMessage flashMessage, NetworkRequest networkRequest, NetworkResponse networkResponse, SecurityManager securityManager, RouteEndpoint routeEndpoint) {
            MethodComponents methodComponents = new MethodComponents();
            ParameterInfo[] endpointMethodAttributes = routeEndpoint.getRouteMethod().GetParameters();
            int index = 0;
            int pathVariableIndex = 0;
            int firstIndex = routeEndpointPath.IndexOf("/");
            int firstIndexWith = firstIndex + 1;
            String routeEndpointPathClean = routeEndpointPath.Substring(firstIndexWith);
            String[] routePathUriAttributes = routeEndpointPathClean.Split("/");
            foreach(ParameterInfo endpointMethodAttribute in endpointMethodAttributes){
                String methodAttributeKey = endpointMethodAttribute.Name.ToLower();
                String description = endpointMethodAttribute.Name.ToLower();

                RouteAttribute routeAttribute = routeEndpoint.getRouteAttributes()[methodAttributeKey];
                MethodAttribute methodAttribute = new MethodAttribute();
                methodAttribute.setDescription(description);

                Console.WriteLine("3.1 {0}" + endpointMethodAttribute.ParameterType.ToString());
                
                Console.WriteLine("3.10 {0}", pathVariableIndex);

                if(endpointMethodAttribute.ParameterType.ToString().Equals("Skyline.Security.SecurityManager")){
                    methodAttribute.setDescription("securitymanager");
                    methodAttribute.setAttribute(securityManager);
                    methodComponents.getRouteMethodAttributes().Add("securitymanager", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(securityManager);
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("Skyline.Model.NetworkRequest")){
                    methodAttribute.setDescription("networkrequest");
                    methodAttribute.setAttribute(networkRequest);
                    methodComponents.getRouteMethodAttributes().Add("networkrequest", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(networkRequest);
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("Skyline.Model.NetworkResponse")){
                    methodAttribute.setDescription("networkresponse");
                    methodAttribute.setAttribute(networkResponse);
                    methodComponents.getRouteMethodAttributes().Add("networkresponse", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(networkResponse);
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("Skyline.Model.FlashMessage")){
                    methodAttribute.setDescription("flashmessage");
                    methodAttribute.setAttribute(flashMessage);
                    methodComponents.getRouteMethodAttributes().Add("flashmessage", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(flashMessage);
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("Skyline.Model.ViewCache")){
                    methodAttribute.setDescription("viewcache");
                    methodAttribute.setAttribute(viewCache);
                    methodComponents.getRouteMethodAttributes().Add("viewcache", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(viewCache);
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.Int32")){
                    Int32 attributeValue = Int32.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                    pathVariableIndex++;
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.Int64")){
                    Int64 attributeValue = Int64.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                    pathVariableIndex++;
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.Int128")){
                    Console.WriteLine("3.20" + routePathUriAttributes[pathVariableIndex]);
                    Int128 attributeValue = Int128.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                    pathVariableIndex++;
                }
                Console.WriteLine("3.2:" + endpointMethodAttribute.ParameterType.ToString().Equals("System.Boolean"));
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.Boolean")){
                    Boolean attributeValue = Boolean.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                    pathVariableIndex++;
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.String")){
                    Console.WriteLine("3.21");
                    String attributeValue = new String(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                    pathVariableIndex++;
                }
            }

            Console.WriteLine("here.");
            foreach(Object obj in methodComponents.getRouteMethodAttributesList()){
                Console.WriteLine("3.10" + obj);
            }
            
            return methodComponents;
        }


        public SecurityAttributes getSecurityAttributes() {
            return this.securityAttributes;
        }

        public void setSecurityAttributes(SecurityAttributes securityAttributes) {
            this.securityAttributes = securityAttributes;
        }
        
        public ComponentsHolder getComponentsHolder() {
            return this.componentsHolder;
        }

        public void setComponentsHolder(ComponentsHolder componentsHolder) {
            this.componentsHolder = componentsHolder;
        }

        public RouteAttributes getRouteAttributes() {
            return this.routeAttributes;
        }

        public void setRouteAttributes(RouteAttributes routeAttributes) {
            this.routeAttributes = routeAttributes;
        }

        public ApplicationAttributes getApplicationAttributes() {
            return this.applicationAttributes;
        }

        public void setApplicationAttributes(ApplicationAttributes applicationAttributes) {
            this.applicationAttributes = applicationAttributes;
        }

    }

}