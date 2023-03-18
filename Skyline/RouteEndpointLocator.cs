using System;

using Skyline.Model;

namespace Skyline { 

    public class RouteEndpointLocator{

        String routeEndpointPath;
        RouteEndpoint routeEndpoint;
        RouteEndpointHolder routeEndpointHolder;

        public RouteEndpoint locate(){
            if(routeEndpoint == null) {
                if (routeEndpointPath.Length > 1 && routeEndpointPath.EndsWith("/")) {
                    int indexWith = routeEndpointPath.Length - 1;
                    int endIndex = routeEndpointPath.IndexOf("/", indexWith);
                    routeEndpointPath = routeEndpointPath.Substring(0, endIndex);
                }
                if (routeEndpointHolder.getRouteEndpoints().ContainsKey(routeEndpointAction + ":" + routeEndpointPath)) {
                    routeEndpoint = routeEndpointHolder.getRouteEndpoints().GetOrDefault(routeEndpointAction + ":" + routeEndpointPath, null);
                }
            }

            if(routeEndpoint == null) {
                foreach(var routeEndpointEntry in routeEndpointHolder.getRouteEndpoints()) {
                    RouteEndpoint activeRouteEndpoint = routeEndpointEntry.value;

                    Regex regexLocator = new Regex(activeRouteEndpoint.getRegexRoutePath(), RegexOptions.IgnoreCase);
                    Match matcher = regexLocator.Match(routeEndpointPath);
                    if(matcher.Success &&
                            routeVariablesMatch(routeEndpointPath, activeRouteEndpoint) &&
                                activeRouteEndpoint.isRegex()){
                        return activeRouteEndpoint;
                    }
                }
            }
            return routeEndpoint;
        }

        Boolean routeVariablesMatch(String routeEndpointPath, RouteEndpoint routeEndpoint) {
            String[] routeUriParts = routeEndpointPath.Split("/");
            String[] routeEndpointParts = routeEndpoint.getRoutePath().Split("/");
            if(routeUriParts.Length != routeEndpointParts.Length)return false;
            return true;
        }

        public void setRouteEndpointHolder(RouteEndpointHolder routeEndpointHolder) {
            this.routeEndpointHolder = routeEndpointHolder;
        }

        public void setRouteEndpointPath(String routeEndpointPath) {
            this.routeEndpointPath = routeEndpointPath;
        }

        public void setRouteEndpoint(RouteEndpoint routeEndpoint) {
            this.routeEndpoint = routeEndpoint;
        }



    }

}

