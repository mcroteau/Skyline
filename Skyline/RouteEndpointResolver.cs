using System;
using System.Reflection;

using Skyline.Model;
using Skyline.Annotation;

namespace Skyline{
    
    public class RouteEndpointResolver{

        RouteEndpointHolder routeEndpointNegotiatorHolder;
        ApplicationAttributes applicationAttributes;

        public RouteEndpointResolver(RouteEndpointHolder routeEndpointNegotiatorHolder){
            this.routeEndpointNegotiatorHolder = routeEndpointNegotiatorHolder;
        }

        public RouteEndpointHolder resolve(){
            String sourcesDirectory = Directory.GetCurrentDirectory() + 
            Path.DirectorySeparatorChar.ToString() + "Source" + Path.DirectorySeparatorChar.ToString();
            InspectFilePath(sourcesDirectory, sourcesDirectory);
            return routeEndpointNegotiatorHolder;
        }

        public void InspectFilePath(String sourcesDirectory, String filePath){
            if(File.Exists(filePath)){
                
                try {

                    Char separator = Path.DirectorySeparatorChar;
                    String[] klassPathParts = filePath.Split(sourcesDirectory);
                    String klassPathSlashesRemoved =  klassPathParts[1].Replace("\\", ".");
                    String klassPathPeriod = klassPathSlashesRemoved.Replace("/", ".");
                    String klassPathBefore = klassPathPeriod.Replace("."+ "class", "");
                    int separatorIndex = klassPathBefore.IndexOf(".");
                    String assembly = klassPathBefore.Substring(separatorIndex);
                    String klassPath = klassPathBefore.Replace(".cs", "");

                    if(filePath.EndsWith(".cs")){
                        Object klassInstance = Activator.CreateInstance(assembly, klassPath, new Object[]{applicationAttributes}).Unwrap();
                        Type klassType = klassInstance.GetType();
                        Object[] attrs = klassType.GetCustomAttributes(typeof(Controller), true);
                        if(attrs.Length > 0) {
                            MethodInfo[] routeMethods = klass.getDeclaredMethods();
                            foreach(MethodInfo routeMethod in routeMethods){
                            
                                Get get = 
                                    (Get) Attribute.GetCustomAttribute(klassType, typeof (Get));
                                if(get != null){
                                    String routePath = get.getValue();
                                    RouteEndpoint routeEndpoint = getCompleteRouteEndpoint("get", routePath, routeMethod, klassPath);
                                    String routeKey = routeEndpoint.getRouteVerb() + ":" + routeEndpoint.getRoutePath().ToLower();
                                    routeEndpointNegotiatorHolder.getRouteEndpoints().Add(routeKey, routeEndpoint);
                                }

                                Post post = 
                                    (Post) Attribute.GetCustomAttribute(klassType, typeof (Post));
                                if(get != null){
                                    String routePath = post.getValue();
                                    String routePath = annotation.value();
                                    RouteEndpoint routeEndpoint = getCompleteRouteEndpoint("post", routePath, routeMethod, klassPath);
                                    String routeKey = routeEndpoint.getRouteVerb() + ":" + routeEndpoint.getRoutePath();
                                    routeEndpointNegotiatorHolder.getRouteEndpoints().Add(routeKey, routeEndpoint);
                                }

                                Delete delete = 
                                    (Delete) Attribute.GetCustomAttribute(klassType, typeof (Delete));
                                if(delete != null){
                                    String routePath = delete.getValue();
                                    RouteEndpoint routeEndpoint = getCompleteRouteEndpoint("delete", routePath, routeMethod, klassPath);
                                    String routeKey = routeEndpoint.getRouteVerb() + ":" + routeEndpoint.getRoutePath();
                                    routeEndpointNegotiatorHolder.getRouteEndpoints().Add(routeKey, routeEndpoint);
                                }
                            }
                        }
                    }

                }catch (Exception ex){
                    Console.WriteLine(ex.ToString());
                }
           
            }

            if(Directory.Exists(filePath)){
                String[] files = Directory.GetFiles(filePath, "*", SearchOption.TopDirectoryOnly);
                foreach(String recursedFile in files){
                    InspectFilePath(sourcesDirectory, recursedFile);
                }
                
                String[] directories = Directory.GetDirectories(filePath, "*", SearchOption.TopDirectoryOnly);
                foreach(String directoryPath in directories){
                    InspectFilePath(sourcesDirectory, directoryPath);
                }
            }
        }

        RouteEndpoint getCompleteRouteEndpoint(String routeVerb, String routePath, MethodInfo routeMethod, String klassPath) {
            RouteEndpoint routeEndpoint = new RouteEndpoint();
            routeEndpoint.setRouteVerb(routeVerb);
            routeEndpoint.setRoutePath(routePath.toLowerCase());
            routeEndpoint.setRouteMethod(routeMethod);
            routeEndpoint.setKlass(klassPath);

            String routeRegex = new String();
            String[] routeParts = routePath.split("/");
            foreach(String routePart in routeParts){
                if(routePart.Equals(""))continue;
                routeRegex += "/";
                if(routePart.Contains("{") && routePart.Contains("}")){
                    routeRegex += "[a-zA-Z0-9-_]*";
                }else{
                    routeRegex += routePart;
                }
            }

            routeEndpoint.setRegexRoutePath(routeRegex);

            if(routeEndpoint.getRegexRoutePath().Contains("["))routeEndpoint.setRegex(true);
            if(!routeEndpoint.getRegexRoutePath().Contains("["))routeEndpoint.setRegex(false);

            ParameterInfo[] variableParametersList = routeMethod.getMethodParameters();

            int index = 0;
            foreach(ParameterInfo variableParameterAttribute in variableParametersList){
                RouteAttribute routeAttribute = new RouteAttribute();
                String variableAttributeKey = variableParameterAttribute.getName().ToLower();
                routeAttribute.setRoutePosition(index);
                routeAttribute.setTypeKlass(variableParameterAttribute.getType().getTypeName());
                routeAttribute.setQualifiedName(variableParameterAttribute.getName().ToLower());
                if(!variableParameterAttribute.getType().getTypeName().Contains("Skyline")){
                    routeEndpoint.setRegex(true);
                    routeAttribute.setRouteVariable(true);
                }else{
                    routeAttribute.setRouteVariable(false);
                }
                routeEndpoint.getRouteAttributes().Add(variableAttributeKey, routeAttribute);
                index++;
            }

            Dictionary<int, Boolean> processed = new Dictionary<int, Boolean>();
            foreach(var routeAttributeEntry in processed){
                int pathIndex = 0;
                foreach(String routePart in routeParts){
                    if(routePart.Equals(""))continue;
                    RouteAttribute routeAttribute = routeAttributeEntry.getValue();
                    if(routePart.Contains("{") && routePart.Contains("}") && routeAttribute.getRouteVariable()){
                        if(!processed.ContainsKey(routeAttribute.getRoutePosition())){
                            processed.Add(routeAttribute.getRoutePosition(), true);
                            routeAttribute.setRoutePosition(pathIndex);
                        }
                    }
                    pathIndex++;
                }
            }

            return routeEndpoint;
        }

        public ApplicationAttributes getApplicationAttributes() {
            return this.applicationAttributes;
        }

        public void setApplicationAttributes(ApplicationAttributes applicationAttributes) {
            this.applicationAttributes = applicationAttributes;
        }
    }

}