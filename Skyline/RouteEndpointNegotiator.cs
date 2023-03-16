using System;
using System.Text;

using Skyline.Model;
using Skyline.Security;

namespace Skyline{

    public class RouteEndpointNegotiator{

        RouteAttributes routeAttributes;
        ComponentsHolder componentsHolder;
        ApplicationAttributes applicationAttributes;
        SecurityAttributes securityAttributes;

        public RouteResult negotiate(String renderer, String resourcesDirectory, FlashMessage flashMessage, ViewCache viewCache, ViewConfig viewConfig, NetworkRequest networkRequest, NetworkResponse networkResponse, SecurityAttributes securityAttributes, SecurityManager securityManager, Dictionary<String, byte[]> viewBytesMap){

            var utf8 = new UTF8Encoding();
    //         String completePageRendered = "";
    //         String errorMessage = "";

    //         try {

    //             String routeEndpointPath = networkRequest.getRequestPath();
    //             String routeEndpointAction = networkRequest.getRequestAction().toLowerCase();

    //             if(routeEndpointPath.equals("/launcher.status")){
    //                 return new RouteResult("200 OK".getBytes(), "200 OK", "text/plain");
    //             }

    //             StargzrResources stargzrResources = new StargzrResources();
    //             ExperienceManager experienceManager = new ExperienceManager();

    //             RouteAttributes routeAttributes = networkRequest.getRouteAttributes();
    //             RouteEndpointHolder routeEndpointHolder = routeAttributes.getRouteEndpointHolder();

    //             if(routeEndpointPath.startsWith("/" + resourcesDirectory + "/")) {

    //                 MimeResolver mimeResolver = new MimeResolver(routeEndpointPath);

    //                 if (RENDERER.equals(RenderingScheme.CACHE_REQUESTS)) {

    //                     ByteArrayOutputStream outputStream = stargzrResources.getViewFileCopy(routeEndpointPath, viewBytesMap);
    //                     if (outputStream == null) {
    //                         return new RouteResult("404".getBytes(), "404", "text/html");
    //                     }
    //                     return new RouteResult(outputStream.toByteArray(), "200 OK", mimeResolver.resolve());

    //                 }else{

    //                     String assetsPath = Paths.get("src", "main", viewConfig.getViewsPath()).toString();
    //                     String filePath = assetsPath.concat(routeEndpointPath);
    //                     File staticResourcefile = new File(filePath);
    //                     InputStream fileInputStream = new FileInputStream(staticResourcefile);

    //                     if (fileInputStream != null && routeEndpointAction.equals("get")) {
    //                         ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
    //                         byte[] bytes = new byte[(int) staticResourcefile.length()];
    //                         int bytesRead;
    //                         try {
    //                             while ((bytesRead = fileInputStream.read(bytes, 0, bytes.length)) != -1) {
    //                                 outputStream.write(bytes, 0, bytesRead);
    //                             }
    //                             fileInputStream.close();
    //                             outputStream.flush();
    //                             outputStream.close();

    //                         } catch (IOException ex) {
    //                             ex.printStackTrace();
    //                         }
    //                         return new RouteResult(outputStream.toByteArray(), "200 OK", mimeResolver.resolve());
    //                     }
    //                 }

    //             }

    //             RouteEndpoint routeEndpoint = null;
    //             routeEndpointPath = routeEndpointPath.toLowerCase().trim();

    //             if(routeEndpointPath.equals("")){
    //                 routeEndpointPath = "/";
    //                 String routeKey = routeEndpointAction.toLowerCase() + routeEndpointPath.toLowerCase();
    //                 routeEndpoint = routeEndpointHolder.getRouteEndpoints().get(routeKey);
    //             }

    //             if(routeEndpoint == null) {
    //                 if (routeEndpointPath.length() > 1 && routeEndpointPath.endsWith("/")) {
    //                     int endIndex = routeEndpointPath.indexOf("/", routeEndpointPath.length() - 1);
    //                     routeEndpointPath = routeEndpointPath.substring(0, endIndex);
    //                 }

    // //                System.out.println("\n\n=====================");
    // //                for(Map.Entry<String, RouteEndpoint> routeEndpointEntry : routeEndpointHolder.getRouteEndpoints().entrySet()){
    // //                    System.out.println(routeEndpointEntry.getKey() + " ] ======== [ " + routeEndpointAction + ":" + routeEndpointPath);
    // //                }
    // //                System.out.println("=====================\n\n");

    //                 if (routeEndpointHolder.getRouteEndpoints().containsKey(routeEndpointAction + ":" + routeEndpointPath)) {
    //                     routeEndpoint = routeEndpointHolder.getRouteEndpoints().get(routeEndpointAction + ":" + routeEndpointPath);
    //                 }
    //             }

    //             if(routeEndpoint == null) {
    //                 for (Map.Entry<String, RouteEndpoint> routeEndpointEntry : routeEndpointHolder.getRouteEndpoints().entrySet()) {
    //                     RouteEndpoint activeRouteEndpoint = routeEndpointEntry.getValue();
    //                     Matcher routeEndpointMatcher = Pattern.compile(activeRouteEndpoint.getRegexRoutePath()).matcher(routeEndpointPath);
    // //                    System.out.println("m:" + routeEndpointMatcher.matches() + ":" +
    // //                            "rz:" + getRouteVariablesMatch(routeEndpointPath, activeRouteEndpoint) + ":" +
    // //                            "z:" + activeRouteEndpoint.isRegex() + ":" + activeRouteEndpoint.getRoutePath());
    //                     if (routeEndpointMatcher.matches() &&
    //                             getRouteVariablesMatch(routeEndpointPath, activeRouteEndpoint) &&
    //                             activeRouteEndpoint.isRegex()) {
    //                         routeEndpoint = activeRouteEndpoint;
    //                     }
    //                 }
    //             }


    //             if(routeEndpoint == null){
    //                 return new RouteResult("404".getBytes(), "404", "text/html");
    //             }

    //             //todo:
    // //            if(initialsRegistry.containsKey(routeEndpoint.getRouteMethod().getName()) &&
    // //                    initialsRegistry.get(routeEndpoint.getRouteMethod().getName()).getKlassName().equals(routeEndpoint.getKlass().getName())){
    // //                initialsRegistry.remove(routeEndpoint.getRouteMethod().getName());
    // //                return new RouteResult(true);
    // //            }

    //             MethodComponents methodComponents = getMethodAttributesComponents(routeEndpointPath, viewCache, flashMessage, networkRequest, networkResponse, securityManager, routeEndpoint);
    //             Method routeEndpointInstanceMethod = routeEndpoint.getRouteMethod();

    //             String title = null, keywords = null, description = null;
    //             if(routeEndpointInstanceMethod.isAnnotationPresent(Meta.class)){
    //                 Meta metaAnnotation = routeEndpointInstanceMethod.getAnnotation(Meta.class);
    //                 title = metaAnnotation.title();
    //                 keywords = metaAnnotation.keywords();
    //                 description = metaAnnotation.description();
    //             }

    //             routeEndpointInstanceMethod.setAccessible(true);
    //             Object routeInstance = routeEndpoint.getKlass().getConstructor().newInstance();

    //             Map<String, Object> routeEndpointInstances = new HashMap<>();
    //             PersistenceConfig persistenceConfig = routeAttributes.getPersistenceConfig();
    //             if(persistenceConfig != null) {
    //                 Dao routeDao = new Dao(persistenceConfig);

    //                 Field[] routeFields = routeInstance.getClass().getDeclaredFields();
    //                 for (Field routeField : routeFields) {
    //                     if (routeField.isAnnotationPresent(Bind.class)) {
    //                         String fieldKey = routeField.getName().toLowerCase();

    //                         if (componentsHolder.getServices().containsKey(fieldKey)) {
    //                             Class<?> serviceKlass = componentsHolder.getServices().get(fieldKey);
    //                             Constructor<?> serviceKlassConstructor = serviceKlass.getConstructor();
    //                             Object serviceInstance = serviceKlassConstructor.newInstance();

    //                             Field[] repoFields = serviceInstance.getClass().getDeclaredFields();
    //                             for (Field repoField : repoFields) {
    //                                 if (repoField.isAnnotationPresent(Bind.class)) {
    //                                     String repoFieldKey = repoField.getName().toLowerCase();

    //                                     if (componentsHolder.getRepositories().containsKey(repoFieldKey)) {
    //                                         Class<?> repositoryKlass = componentsHolder.getRepositories().get(repoFieldKey);
    //                                         Constructor<?> repositoryKlassConstructor = repositoryKlass.getConstructor(Dao.class);
    //                                         Object repositoryInstance = repositoryKlassConstructor.newInstance(routeDao);
    //                                         repoField.setAccessible(true);
    //                                         repoField.set(serviceInstance, repositoryInstance);
    //                                         routeEndpointInstances.put(repoFieldKey, repositoryInstance);
    //                                     }
    //                                 }
    //                             }

    //                             routeField.setAccessible(true);
    //                             routeField.set(routeInstance, serviceInstance);
    //                         }

    //                         if (componentsHolder.getRepositories().containsKey(fieldKey)) {
    //                             Class<?> componentKlass = componentsHolder.getRepositories().get(fieldKey);
    //                             Constructor<?> componentKlassConstructor = componentKlass.getConstructor(Dao.class);
    //                             Object componentInstance = componentKlassConstructor.newInstance(routeDao);
    //                             routeField.setAccessible(true);
    //                             routeField.set(routeInstance, componentInstance);
    //                             routeEndpointInstances.put(fieldKey, componentInstance);
    //                         }
    //                     }
    //                 }

    //                 try {
    //                     Method setPersistenceMethod = routeInstance.getClass().getMethod("setDao", Dao.class);
    //                     setPersistenceMethod.invoke(routeInstance, new Dao(persistenceConfig));
    //                 } catch (NoSuchMethodException nsme) { }

    //             }

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

    //                         String redirectRouteUri = stargzrResources.getRedirect(beforeResult.getRedirectUri());

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

    //             Object routeResponseObject = routeEndpointInstanceMethod.invoke(routeInstance, methodComponents.getRouteMethodAttributesList().toArray());
    //             String methodResponse = String.valueOf(routeResponseObject);
    //             if(methodResponse == null){
    //                 return new RouteResult("404".getBytes(), "404", "text/html");
    //             }

    //             if(methodResponse.startsWith("redirect:")) {
    //                 RedirectInfo redirectInfo = new RedirectInfo();
    //                 redirectInfo.setMethodName(routeEndpointInstanceMethod.getName());
    //                 redirectInfo.setKlassName(routeInstance.getClass().getName());
    //                 String redirectRouteUri = stargzrResources.getRedirect(methodResponse);
    //                 networkRequest.setRedirect(true);
    //                 networkRequest.setRedirectLocation(redirectRouteUri);
    //                 return new RouteResult("303".getBytes(), "303", "text/html");
    //             }

    //             if(routeEndpointInstanceMethod.isAnnotationPresent(JsonOutput.class)){
    //                 return new RouteResult(methodResponse.getBytes(), "200 OK", "application/json");
    //             }

    //             if(routeEndpointInstanceMethod.isAnnotationPresent(Text.class)){
    //                 return new RouteResult(methodResponse.getBytes(), "200 OK", "text/html");
    //             }

    //             if(RENDERER.equals("cache-request")) {

    //                 ByteArrayOutputStream unebaos = stargzrResources.getViewFileCopy(methodResponse, viewBytesMap);
    //                 if(unebaos == null){
    //                     return new RouteResult("404".getBytes(), "404", "text/html");
    //                 }
    //                 completePageRendered = unebaos.toString();

    //             }else{

    //                 Path webPath = Paths.get("src", "main", viewConfig.getViewsPath());
    //                 if (methodResponse.startsWith("/")) {
    //                     methodResponse = methodResponse.replaceFirst("/", "");
    //                 }

    //                 String htmlPath = webPath.toFile().getAbsolutePath().concat(File.separator + methodResponse);
    //                 File viewFile = new File(htmlPath);
    //                 ByteArrayOutputStream unebaos = new ByteArrayOutputStream();

    //                 InputStream pageInputStream = new FileInputStream(viewFile);
    //                 byte[] bytes = new byte[(int) viewFile.length()];
    //                 int pageBytesLength;
    //                 while ((pageBytesLength = pageInputStream.read(bytes)) != -1) {
    //                     unebaos.write(bytes, 0, pageBytesLength);
    //                     if(pageInputStream.available() == 0)break;
    //                 }
    //                 completePageRendered = unebaos.toString();//todo? ugly
    //             }


    //             viewCache.set("message", flashMessage.getMessage());

    //             String designUri = null;
    //             if(routeEndpointInstanceMethod.isAnnotationPresent(Design.class)){
    //                 Design annotation = routeEndpointInstanceMethod.getAnnotation(Design.class);
    //                 designUri = annotation.value();
    //             }
    //             if(designUri != null) {
    //                 String designContent;
    //                 if(RENDERER.equals("cache-request")) {

    //                     ByteArrayOutputStream baos = stargzrResources.getViewFileCopy(designUri, viewBytesMap);
    //                     designContent = baos.toString();

    //                 }else{

    //                     Path designPath = Paths.get("src", "main", "webapp", designUri);
    //                     File designFile = new File(designPath.toString());
    //                     InputStream designInputStream = new FileInputStream(designFile);

    //                     ByteArrayOutputStream baos = new ByteArrayOutputStream();

    //                     byte[] bytes = new byte[(int) designFile.length()];
    //                     int length;
    //                     while ((length = designInputStream.read(bytes)) != -1) {
    //                         baos.write(bytes, 0, length);
    //                         if(designInputStream.available() == 0)break;
    //                     }
    //                     designContent = baos.toString();

    //                 }

    //                 if(designContent == null){
    //                     return new RouteResult("design not found.".getBytes(), "200 OK", "text/html");
    //                 }

    //                 if(!designContent.contains("<stargzr:content/>")){
    //                     return new RouteResult("Your html template file is missing <stargzr:content/>".getBytes(), "200 OK", "text/html");
    //                 }

    //                 String[] bits = designContent.split("<stargzr:content/>");
    //                 String header = bits[0];
    //                 String bottom = "";
    //                 if(bits.length > 1) bottom = bits[1];

    //                 header = header + completePageRendered;
    //                 completePageRendered = header + bottom;

    //                 if(title != null) {
    //                     completePageRendered = completePageRendered.replace("${title}", title);
    //                 }
    //                 if(keywords != null) {
    //                     completePageRendered = completePageRendered.replace("${keywords}", keywords);
    //                 }
    //                 if(description != null){
    //                     completePageRendered = completePageRendered.replace("${description}", description);
    //                 }

    //                 completePageRendered = experienceManager.execute(completePageRendered, viewCache, networkRequest, securityAttributes, viewRenderers);
    //                 return new RouteResult(completePageRendered.getBytes(), "200 OK", "text/html");

    //             }else{
    //                 completePageRendered = experienceManager.execute(completePageRendered, viewCache, networkRequest, securityAttributes, viewRenderers);
    //                 return new RouteResult(completePageRendered.getBytes(), "200 OK", "text/html");
    //             }

    //         }catch (IllegalAccessException ex) {
    //             errorMessage = "<p style=\"border:solid 1px #ff0000; color:#ff0000;\">" + ex.getMessage() + "</p>";
    //             ex.printStackTrace();
    //         } catch (InvocationTargetException ex) {
    //             errorMessage = "<p style=\"border:solid 1px #ff0000; color:#ff0000;\">" + ex.getMessage() + "</p>";
    //             ex.printStackTrace();
    //         } catch (NoSuchFieldException ex) {
    //             errorMessage = "<p style=\"border:solid 1px #ff0000; color:#ff0000;\">" + ex.getMessage() + "</p>";
    //             ex.printStackTrace();
    //         } catch (NoSuchMethodException ex) {
    //             errorMessage = "<p style=\"border:solid 1px #ff0000; color:#ff0000;\">" + ex.getMessage() + "</p>";
    //             ex.printStackTrace();
    //         } catch (InstantiationException ex) {
    //             errorMessage = "<p style=\"border:solid 1px #ff0000; color:#ff0000;\">" + ex.getMessage() + "</p>";
    //             ex.printStackTrace();
    //         } catch (UnsupportedEncodingException ex) {
    //             errorMessage = "<p style=\"border:solid 1px #ff0000; color:#ff0000;\">" + ex.getMessage() + "</p>";
    //             ex.printStackTrace();
    //         } catch (IOException ex) {
    //             errorMessage = "<p style=\"border:solid 1px #ff0000; color:#ff0000;\">" + ex.getMessage() + "</p>";
    //             ex.printStackTrace();
    //         } catch (StargzrException ex) {
    //             errorMessage = "<p style=\"border:solid 1px #ff0000; color:#ff0000;\">" + ex.getMessage() + "</p>";
    //             ex.printStackTrace();
    //         }
    //         String erroredPageRendered = errorMessage + completePageRendered;
            return new RouteResult(utf8.GetBytes("issue."), "404", "text/plain");
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