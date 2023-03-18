using System;
using System.Text;
using System.IO;
using System.Reflection;

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
                String routeEndpointAction = networkRequest.getRequestAction().toLowerCase();

                if(routeEndpointPath.equals("/launcher.status")){
                    return new RouteResult("200 OK".getBytes(), "200 OK", "text/plain");
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

                RouteEndpointNormalizer routeEndpointNormalizer = new RouteEndpointNormalizer();
                routeEndpointNormalizer.setRouteEndpointPath(routeEndpointPath);
                routeEndpointNormalizer.setRouteEndpointAction(routeEndpointAction);
                routeEndpointPath = routeEndpointNormalizer.normalize();
                RouteEndpoint routeEndpoint = routeEndpointHolder.getRouteEndpoints().get(routeKey);

                RouteEndpointLocator routeEndpointLocator = new RouteEndpointLocator();
                routeEndpointLocator.setRouteEndpointHolder(routeEndpointHolder);
                routeEndpointLocator.setRouteEndpointPath(routeEndpointPath);
                routeEndpointLocator.setRouteEndpoint(routeEndpoint);
                routeEndpoint = routeEndpointLocator.locate();

                MethodComponents methodComponents = getMethodAttributesComponents(routeEndpointPath, viewCache, flashMessage, networkRequest, networkResponse, securityManager, routeEndpoint);
                MethodInfo routeEndpointInstanceMethod = routeEndpoint.getRouteMethod();

                String title, keywords, description = "";
                Attributes attributes = (Attributes) Attribute.GetCustomAttribute(routeEndpointInstanceMethod.GetType(), typeof (Attributes));
                if(attributes != null){
                    title = attributes.getTitle();
                    keywords = attributes.getKeywords();
                    description = attributes.getDescription();
                }

                Object routeEndpointInstance = Activator.CreateInstance(routeEndpoint.getKlassInstance().GetType(), new Object[]{applicationAttributes}, new Object[]{});
                PersistenceConfig persistenceConfig = routeAttributes.getPersistenceConfig();
                if(persistenceConfig != null) {
                    DataTransferObject repoDto = new DataTransferObject(persistenceConfig);

                    FieldInfo[] routeFields = routeEndpointInstance.GetType().GetFields();
                    foreach(FieldInfo routeFieldInfo in routeFields) {

                        Bind bind = (Bind) Attribute.GetCustomAttribute(routeFieldInfo.GetType(), typeof (Bind));
                        if (bind != null) {
                            String fieldInfoKey = routeFieldInfo.getName().ToLower();
                            if (componentsHolder.getRepositories().ContainsKey(fieldInfoKey)) {
                                Type repositoryKlassType = componentsHolder.getRepositories().GetOrDefault(fieldInfoKey, null);
                                Object repositoryKlassInstance = Activator.CreateInstance(repositoryKlassType, new Object[]{repoDto, applicationAttributes}, new Object[]{});
                                routeFieldInfo.Set(routeEndpointInstance, repositoryKlassInstance);
                            }
                        }
                    }
                }

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
    //                     if(!beforeResult.getRedirectUri().equals("")){
    //                         RedirectInfo redirectInfo = new RedirectInfo();
    //                         redirectInfo.setMethodName(routeEndpointInstanceMethod.getName());
    //                         redirectInfo.setKlassName(routeInstance.getClass().getName());

    //                         if(beforeResult.getRedirectUri() == null || beforeResult.getRedirectUri().equals("")){
    //                             throw new StargzrException("redirect uri is empty on " + routePrincipalKlass.getName());
    //                         }

    //                         String redirectRouteUri = resourceUtility.getRedirect(beforeResult.getRedirectUri());

    //                         if(!beforeResult.getMessage().equals("")){
    //                             viewCache.set("message", beforeResult.getMessage());
    //                         }

    //                         networkRequest.setRedirect(true);
    //                         networkRequest.setRedirectLocation(redirectRouteUri);
    //                         break;
    //                     }
    //                 }

    //                 if(!beforeResult.getRedirectUri().equals("")){
    //                     return new RouteResult("303".getBytes(), "303", "text/html");
    //                 }
    //             }

                Object routeResponseObject = routeEndpointInstanceMethod.Invoke(routeInstance, methodComponents.getRouteMethodAttributesList().ToArray());
                String methodResponse = routeResponseObject.ToString();
                if(methodResponse == null){
                    return new RouteResult(utf8.GetByte("404"), "404", "text/html");
                }

            //     if(methodResponse.startsWith("redirect:")) {
            //         RedirectInfo redirectInfo = new RedirectInfo();
            //         redirectInfo.setMethodName(routeEndpointInstanceMethod.getName());
            //         redirectInfo.setKlassName(routeInstance.getClass().getName());
            //         String redirectRouteUri = resourceUtility.getRedirect(methodResponse);
            //         networkRequest.setRedirect(true);
            //         networkRequest.setRedirectLocation(redirectRouteUri);
            //         return new RouteResult("303".getBytes(), "303", "text/html");
            //     }

            //     if(routeEndpointInstanceMethod.isAnnotationPresent(JsonOutput.class)){
            //         return new RouteResult(methodResponse.getBytes(), "200 OK", "application/json");
            //     }

            //     if(routeEndpointInstanceMethod.isAnnotationPresent(Text.class)){
            //         return new RouteResult(methodResponse.getBytes(), "200 OK", "text/html");
            //     }

            //     if(renderer.equals("cache-request")) {

            //         ByteArrayOutputStream unebaos = resourceUtility.getViewFileCopy(methodResponse, viewBytesMap);
            //         if(unebaos == null){
            //             return new RouteResult("404".getBytes(), "404", "text/html");
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
            //         if(renderer.equals("cache-request")) {

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
            //             return new RouteResult("design not found.".getBytes(), "200 OK", "text/html");
            //         }

            //         if(!designContent.contains("<c:content/>")){
            //             return new RouteResult("Your html template file is missing <c:content/>".getBytes(), "200 OK", "text/html");
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
            //         return new RouteResult(completePageRendered.getBytes(), "200 OK", "text/html");

            //     }else{
            //         completePageRendered = experienceManager.execute(completePageRendered, viewCache, networkRequest, securityAttributes, viewRenderers);
            //         return new RouteResult(completePageRendered.getBytes(), "200 OK", "text/html");
            //     }

            }catch(Exception ex){
                return new RouteResult(utf8.GetBytes("issue. " + ex.Message), "500", "text/plain");
            }

            return new RouteResult(utf8.GetBytes("issue. "), "500", "text/plain");
        }

        MethodComponents getMethodAttributesComponents(String routeEndpointPath, ViewCache viewCache, FlashMessage flashMessage, NetworkRequest networkRequest, NetworkResponse networkResponse, SecurityManager securityManager, RouteEndpoint routeEndpoint) {
            MethodComponents methodComponents = new MethodComponents();
            ParameterInfo[] endpointMethodAttributes = routeEndpoint.getRouteMethod().GetParameters();
            Integer index = 0;
            Integer pathVariableIndex = 0;
            String routeEndpointPathClean = routeEndpointPath.ReplaceFirst("/", "");
            String[] routePathUriAttributes = routeEndpointPathClean.Split("/");
            foreach(ParameterInfo endpointMethodAttribute in endpointMethodAttributes){
                String methodAttributeKey = endpointMethodAttribute.getName().ToLower();
                String description = endpointMethodAttribute.getDeclaringExecutable().getName().toLowerCase();

                RouteAttribute routeAttribute = routeEndpoint.getRouteAttributes().GetOrDefault(methodAttributeKey, null);
                MethodAttribute methodAttribute = new MethodAttribute();
                methodAttribute.setDescription(description);

                pathVariableIndex = routeAttribute.getRoutePosition() != null ? routeAttribute.getRoutePosition() : 0;
                if(endpointMethodAttribute.GetType().Name.Equals("Skyline.Security.SecurityManager")){
                    methodAttribute.setDescription("securitymanager");
                    methodAttribute.setAttribute(securityManager);
                    methodComponents.getRouteMethodAttributes().Add("securitymanager", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(securityManager);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("Skyline.Model.NetworkRequest")){
                    methodAttribute.setDescription("networkrequest");
                    methodAttribute.setAttribute(networkRequest);
                    methodComponents.getRouteMethodAttributes().Add("networkrequest", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(networkRequest);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("Skyline.Model.NetworkResponse")){
                    methodAttribute.setDescription("networkresponse");
                    methodAttribute.setAttribute(networkResponse);
                    methodComponents.getRouteMethodAttributes().Add("networkresponse", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(networkResponse);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("Skyline.Model.FlashMessage")){
                    methodAttribute.setDescription("flashmessage");
                    methodAttribute.setAttribute(flashMessage);
                    methodComponents.getRouteMethodAttributes().Add("flashmessage", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(flashMessage);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("Skyline.Model.ViewCache")){
                    methodAttribute.setDescription("viewcache");
                    methodAttribute.setAttribute(viewCache);
                    methodComponents.getRouteMethodAttributes().Add("viewcache", methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(viewCache);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("System.Int32")){
                    Int32 attributeValue = Int32.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("System.Int64")){
                    Int64 attributeValue = Int64.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("System.Int128")){
                    Int128 attributeValue = Int128.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("java.lang.Boolean")){
                    Boolean attributeValue = Boolean.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                }
                if(endpointMethodAttribute.GetType().Name.Equals("java.lang.String")){
                    String attributeValue = new String(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                }
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