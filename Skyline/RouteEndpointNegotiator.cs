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

        public RouteResult negotiate(Boolean persistentMode, String renderer, String resourcesDirectory, FlashMessage flashMessage, ViewCache viewCache, ViewConfig viewConfig, NetworkRequest networkRequest, NetworkResponse networkResponse, SecurityAttributes securityAttributes, SecurityManager securityManager, Dictionary<String, byte[]> viewBytesMap){

            var utf8 = new UTF8Encoding();
            String completePageRendered = "";
            String errorMessage = "";

            try {

                RouteResult routeResult = new RouteResult();
                byte[] responseOutput = new byte[]{};

                String routeEndpointPath = networkRequest.getRequestPath();
                String routeEndpointAction = networkRequest.getRequestAction().ToLower();

                ResourceUtility resourceUtility = new ResourceUtility();
                ApplicationExperienceResolver experienceResolver = new ApplicationExperienceResolver();

                RouteAttributes routeAttributes = networkRequest.getRouteAttributes();
                RouteEndpointHolder routeEndpointHolder = routeAttributes.getRouteEndpointHolder();

                if(routeEndpointPath.Contains(resourcesDirectory + "/")) {

                    MimeResolver mimeResolver = new MimeResolver();
                    mimeResolver.setRouteEndpointPath(routeEndpointPath);
                    String mime = mimeResolver.resolve();

                    if (renderer.Equals("cache-requests")) {
                        byte[] responseBytes = resourceUtility.getViewFileCopy(routeEndpointPath, viewBytesMap);
                        routeResult.setStatusCode(200);
                        routeResult.setResponseOutput(responseOutput);
                        routeResult.setContentType(mime);
                        return routeResult;
                    }
                    if(renderer.Equals("reload-requests")){
                        ResourceFileConverter resourcesFileConverter = new ResourceFileConverter();
                        resourcesFileConverter.setFile(routeEndpointPath);
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
                routeEndpointPath = routeEndpointNormalizer.normalize();//:looking for key

                RouteEndpointLocator routeEndpointLocator = new RouteEndpointLocator();
                routeEndpointLocator.setRouteEndpointHolder(routeEndpointHolder);
                routeEndpointLocator.setRouteEndpointAction(routeEndpointAction);
                routeEndpointLocator.setRouteEndpointPath(routeEndpointPath);
                RouteEndpoint routeEndpoint = routeEndpointLocator.locate();

                if(routeEndpoint == null){
                    routeResult.setStatusCode(404);
                    routeResult.setResponseOutput(utf8.GetBytes("404 Not Found!"));
                    routeResult.setContentType("text/plain");
                    return routeResult;
                }

                MethodComponents methodComponents = getMethodAttributesComponents(routeEndpointPath, viewCache, flashMessage, networkRequest, networkResponse, securityManager, routeEndpoint);
                MethodInfo routeEndpointInstanceMethod = routeEndpoint.getRouteMethod();
         
                String headline = "", keywords = "", description = "";
                Attributes attributes = (Attributes) Attribute.GetCustomAttribute(routeEndpointInstanceMethod.GetType(), typeof (Attributes));
                if(attributes != null){
                    headline = attributes.getHeadline();
                    keywords = attributes.getKeywords();
                    description = attributes.getDescription();
                }

                Object routeEndpointInstanceRef = Activator.CreateInstance(routeEndpoint.getKlassAssembly(), routeEndpoint.getKlassReference()).Unwrap();
                Object routeEndpointInstance = Activator.CreateInstance(routeEndpointInstanceRef.GetType(), new Object[]{applicationAttributes}, new Object[]{});
                
                PersistenceConfig persistenceConfig = routeAttributes.getPersistenceConfig();

                if(persistentMode || persistenceConfig != null) {
                    if(persistenceConfig == null) persistenceConfig = new PersistenceConfig();
                    if(applicationAttributes == null) applicationAttributes = new ApplicationAttributes();

                    DataTransferObject dto = new DataTransferObject(persistenceConfig);
                    dto.setApplicationAttributes(applicationAttributes);
                    
                    FieldInfo[] routeFields = routeEndpointInstance.GetType().GetFields();
                    foreach(FieldInfo routeFieldInfo in routeFields) {
                        Object[] binds = routeFieldInfo.GetCustomAttributes(typeof (Bind), true);
                        if(binds.Length > 0){
                            String fieldInfoKey = routeFieldInfo.Name.ToLower();
                            if (componentsHolder.getRepositories().ContainsKey(fieldInfoKey)) {
                                Type repositoryKlassType = componentsHolder.getRepositories()[fieldInfoKey];
                                Object repositoryKlassInstance = Activator.CreateInstance(repositoryKlassType, new Object[]{dto}, new Object[]{});
                                routeFieldInfo.SetValue(routeEndpointInstance, repositoryKlassInstance);
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


                Object[] methodAttributes = methodComponents.getRouteMethodAttributesList().ToArray();
                Object routeResponseObject = routeEndpointInstanceMethod.Invoke(routeEndpointInstance, methodAttributes);
                String methodResponse = routeResponseObject.ToString();
                
                if(methodResponse == null){
                    return new RouteResult(utf8.GetBytes("404"), "404", "text/html");
                }

                if(methodResponse.Contains("redirect:")) {
                    RedirectInfo redirectInfo = new RedirectInfo();
                    redirectInfo.setMethodName(routeEndpointInstanceMethod.Name);
                    redirectInfo.setKlassName(routeEndpointInstance.GetType().Name);
                    String redirectRouteUri = resourceUtility.getRedirect(methodResponse);
                    networkRequest.setRedirect(true);
                    networkRequest.setRedirectLocation(redirectRouteUri);
                
                    routeResult.setStatusCode(303);
                    routeResult.setResponseOutput(utf8.GetBytes("303"));
                    routeResult.setContentType("text/plain");
                    return routeResult;
                }


                Object[] jsons = routeEndpointInstanceMethod.GetCustomAttributes(typeof (Json), true);
                if(jsons.Length > 0){
                    routeResult.setStatusCode(200);
                    routeResult.setResponseOutput(utf8.GetBytes(methodResponse));
                    routeResult.setContentType("application/json");
                    return routeResult;
                }

                Object[] texts = routeEndpointInstanceMethod.GetCustomAttributes(typeof (Text), true);
                if(texts.Length > 0){
                    routeResult.setStatusCode(200);
                    routeResult.setResponseOutput(utf8.GetBytes(methodResponse));
                    routeResult.setContentType("text/plain");
                    return routeResult;
                }

                if(renderer.Equals("cache-requests")) {
                    byte[] viewbytes = resourceUtility.getViewFileCopy(methodResponse, viewBytesMap);
                    if(viewbytes == null){
                        routeResult.setStatusCode(404);
                        routeResult.setResponseOutput(utf8.GetBytes("404 Not Found!"));
                        routeResult.setContentType("text/plain");
                        return routeResult;
                    }
                    completePageRendered = utf8.GetString(viewbytes);
                }


                if(renderer.Equals("reload-requests")){
                    ViewFileConverter viewConverter = new ViewFileConverter();
                    viewConverter.setFile(methodResponse);
                    byte[] viewBytes = viewConverter.convert();
                    completePageRendered = utf8.GetString(viewBytes);
                }


                Object[] layouts = routeEndpointInstanceMethod.GetCustomAttributes(typeof (Layout), true);
                if(layouts.Length > 0){
                    Layout layout = (Layout)layouts[0];
                    String designUri = layout.getFile();
                    byte[] designBytes = new byte[]{};
                    if(renderer.Equals("cache-requests")) {
                        designBytes = resourceUtility.getViewFileCopy(designUri, viewBytesMap);
                    }

                    if(renderer.Equals("reload-requests")){
                        ViewFileConverter layoutConverter = new ViewFileConverter();
                        layoutConverter.setFile(designUri);
                        designBytes = layoutConverter.convert(); 
                    }

                    String designTemplate = utf8.GetString(designBytes);
                    if(designTemplate == null){
                        return new RouteResult(utf8.GetBytes("design not found."), "200 OK", "text/html");
                    }

                    if(!designTemplate.Contains("<c:render/>")){
                        return new RouteResult(utf8.GetBytes("Template is missing <c:render/>"), "200 OK", "text/html");
                    }

                    String[] designPartials = designTemplate.Split("<c:render/>");
                    String headerPartial = designPartials[0];
                    String footerPartial = "";
                    if(designPartials.Length > 1) footerPartial = designPartials[1];

                    headerPartial = headerPartial + completePageRendered;
                    completePageRendered = headerPartial + footerPartial;

                    if(headline != null) {
                        completePageRendered = completePageRendered.Replace("${headline}", headline);
                    }
                    if(keywords != null) {
                        completePageRendered = completePageRendered.Replace("${keywords}", keywords);
                    }
                    if(description != null){
                        completePageRendered = completePageRendered.Replace("${description}", description);
                    }

                    completePageRendered = experienceResolver.resolve(completePageRendered, viewCache, networkRequest, securityAttributes, null);
                    
                    return new RouteResult(utf8.GetBytes(completePageRendered), "200 OK", "text/html");
                }

                if(layouts.Length == 0){
                    completePageRendered = experienceResolver.resolve(completePageRendered, viewCache, networkRequest, securityAttributes, null);
                    return new RouteResult(utf8.GetBytes(completePageRendered), "200 OK", "text/html");
                }

            }catch(Exception ex){
                Console.WriteLine(ex.ToString());
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

                if(routeAttribute.getRoutePosition() != null){
                    pathVariableIndex = routeAttribute.getRoutePosition();
                }

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
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.Int64")){
                    Int64 attributeValue = Int64.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.Int128")){
                    Int128 attributeValue = Int128.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.Boolean")){
                    Boolean attributeValue = Boolean.Parse(routePathUriAttributes[pathVariableIndex]);
                    methodAttribute.setAttribute(attributeValue);
                    methodComponents.getRouteMethodAttributes().Add(methodAttribute.getDescription().ToLower(), methodAttribute);
                    methodComponents.getRouteMethodAttributesList().Add(attributeValue);
                    methodComponents.getRouteMethodAttributeVariablesList().Add(attributeValue);
                }
                if(endpointMethodAttribute.ParameterType.ToString().Equals("System.String")){
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