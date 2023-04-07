using System;
using System.Collections;
using System.Reflection;

using Skyline.Model;
using Skyline.Annotation;

namespace Skyline{
    
    public class RouteEndpointResolver{

        RouteEndpointHolder routeEndpointHolder;
        ApplicationAttributes applicationAttributes;

        public RouteEndpointResolver(RouteEndpointHolder routeEndpointHolder){
            this.routeEndpointHolder = routeEndpointHolder;
        }

        public RouteEndpointHolder resolve(){
            String sourcesDirectory = Directory.GetCurrentDirectory() + 
            Path.DirectorySeparatorChar.ToString() + "src" + 
            Path.DirectorySeparatorChar.ToString();

            InspectFilePath(sourcesDirectory, sourcesDirectory);
            return routeEndpointHolder;
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
                    String assemblyCopy = klassPathBefore;
                    String assembly = assemblyCopy.Substring(0, separatorIndex);
                    String klassPath = klassPathBefore.Replace(".cs", "");

                    if(filePath.EndsWith(".cs")){
                        Object klassInstanceValidate = Activator.CreateInstance(assembly, klassPath).Unwrap();
                        Type klassType = klassInstanceValidate.GetType();
                        Object[] attrs = klassType.GetCustomAttributes(typeof(NetworkController), true);
                        if(attrs.Length > 0) {
                            MethodInfo[] routeMethods = klassType.GetMethods();
                            String routeKey = new String("");
                            String routePath = new String("");
                            foreach(MethodInfo routeMethod in routeMethods){
                                
                                Object[] gets = routeMethod.GetCustomAttributes(typeof (Get), true);
                                if(gets.Length > 0){
                                    Get get = (Get) gets[0];
                                    routePath = get.getRoute();
                                    RouteEndpoint routeEndpoint = getCompleteRouteEndpoint("get", routePath, routeMethod, assembly, klassPath);
                                    routeKey = routeEndpoint.getRouteVerb() + ":" + routeEndpoint.getRoutePath().ToLower();
                                    routeEndpointHolder.getRouteEndpoints().Add(routeKey, routeEndpoint);
                                }

                                Object[] posts = routeMethod.GetCustomAttributes(typeof (Post), true);
                                if(posts.Length > 0){
                                    Post post = (Post) posts[0];
                                    routePath = post.getRoute();
                                    RouteEndpoint routeEndpoint = getCompleteRouteEndpoint("post", routePath, routeMethod, assembly, klassPath);
                                    routeKey = routeEndpoint.getRouteVerb() + ":" + routeEndpoint.getRoutePath();
                                    routeEndpointHolder.getRouteEndpoints().Add(routeKey, routeEndpoint);
                                }

                                Object[] deletes = routeMethod.GetCustomAttributes(typeof (Delete), true);
                                if(deletes.Length > 0){
                                    Delete delete = (Delete) deletes[0];
                                    routePath = delete.getRoute();
                                    RouteEndpoint routeEndpoint = getCompleteRouteEndpoint("delete", routePath, routeMethod, assembly, klassPath);
                                    routeKey = routeEndpoint.getRouteVerb() + ":" + routeEndpoint.getRoutePath();
                                    routeEndpointHolder.getRouteEndpoints().Add(routeKey, routeEndpoint);
                                }
                            }
                        }
                    }

                    foreach(var r in routeEndpointHolder.getRouteEndpoints()){
                        Console.WriteLine(r.Key + ":" + r.Value);
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

        RouteEndpoint getCompleteRouteEndpoint(String routeVerb, String routePath, MethodInfo routeMethod, String klassAssembly, String klassReference) {
            RouteEndpoint routeEndpoint = new RouteEndpoint();
            routeEndpoint.setRouteVerb(routeVerb);
            routeEndpoint.setRoutePath(routePath.ToLower());
            routeEndpoint.setRouteMethod(routeMethod);
            routeEndpoint.setKlassAssembly(klassAssembly);
            routeEndpoint.setKlassReference(klassReference);

            String routeRegex = new String("");
            String[] routeParts = routePath.Split("/");
            foreach(String routePart in routeParts){
                routeRegex += "/";
                if(routePart.Contains("{") && routePart.Contains("}")){
                    routeRegex += "[a-zA-Z0-9-_]*";
                }else{
                    routeRegex += routePart;
                }
            }

            if(routeRegex.Equals(""))routeRegex += "/";

            routeEndpoint.setRegexRoutePath(routeRegex);

            if(routeEndpoint.getRegexRoutePath().Contains("["))routeEndpoint.setRegex(true);
            if(!routeEndpoint.getRegexRoutePath().Contains("["))routeEndpoint.setRegex(false);

            ParameterInfo[] variableParametersList = routeMethod.GetParameters();

            int index = 0;
            foreach(ParameterInfo variableParameterAttribute in variableParametersList){
                RouteAttribute routeAttribute = new RouteAttribute();
                String variableAttributeKey = variableParameterAttribute.Name.ToLower();
                routeAttribute.setRoutePosition(index);
                routeAttribute.setTypeKlass(variableParameterAttribute.GetType().ToString());
                routeAttribute.setQualifiedName(variableParameterAttribute.Name.ToLower());
                if(!variableParameterAttribute.GetType().ToString().Contains("Skyline")){
                    routeEndpoint.setRegex(true);
                    routeAttribute.setRouteVariable(true);
                }else{
                    routeAttribute.setRouteVariable(false);
                }
                routeEndpoint.getRouteAttributes().Add(variableAttributeKey, routeAttribute);
                index++;
            }

            Dictionary<String, RouteAttribute> routeAttributesFinal = new Dictionary<String, RouteAttribute>();
            Dictionary<int, Boolean> processed = new Dictionary<int, Boolean>();
            foreach(var routeAttributeEntry in routeEndpoint.getRouteAttributes()){
                int pathIndex = 0;
                foreach(String routePart in routeParts){
                    if(routePart.Equals(""))continue;
                    RouteAttribute routeAttribute = (RouteAttribute) routeAttributeEntry.Value;
                    if(routePart.Contains("{") && routePart.Contains("}") && routeAttribute.getRouteVariable()){
                        if(!processed.ContainsKey(routeAttribute.getRoutePosition())){
                            processed.Add(routeAttribute.getRoutePosition(), true);
                            routeAttribute.setRoutePosition(pathIndex);
                            routeEndpoint.getRouteAttributes()[routeAttributeEntry.Key] = routeAttribute;
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