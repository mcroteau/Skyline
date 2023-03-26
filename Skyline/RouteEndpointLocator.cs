using System;
using System.Text.RegularExpressions;

using Skyline.Model;

namespace Skyline { 

    public class RouteEndpointLocator{

        String routeEndpointPath;
        String routeEndpointAction;
        RouteEndpointHolder routeEndpointHolder;

        public RouteEndpoint locate(){

            RouteEndpoint routeEndpoint = null;

            if (routeEndpointPath.Length > 1 && routeEndpointPath.EndsWith("/")) {
                int indexWith = routeEndpointPath.Length - 1;
                int endIndex = routeEndpointPath.IndexOf("/", indexWith);
                routeEndpointPath = routeEndpointPath.Substring(0, endIndex);
            }

            String compositeEndpointKey = routeEndpointAction + ":" + routeEndpointPath;

            if (routeEndpointHolder.getRouteEndpoints().ContainsKey(compositeEndpointKey)) {
                routeEndpoint = routeEndpointHolder.getRouteEndpoints()[compositeEndpointKey];
            }
            
            if(routeEndpoint == null) {
                foreach(var routeEndpointEntry in routeEndpointHolder.getRouteEndpoints()) {
                    RouteEndpoint activeRouteEndpoint = routeEndpointEntry.Value;

                    var regex = new Regex("/");
                    var routeEndpointRegex = regex.Replace(activeRouteEndpoint.getRegexRoutePath(), "", 1);

                    Match match = Regex.Match(routeEndpointPath, routeEndpointRegex);
                    if(!routeEndpointRegex.Equals("/") && 
                        match.Success && 
                            routeVariablesMatch(routeEndpointPath, activeRouteEndpoint) &&
                                activeRouteEndpoint.getRegex()){
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

        public void setRouteEndpointAction(String routeEndpointAction){
            this.routeEndpointAction = routeEndpointAction;
        }

        public void setRouteEndpointPath(String routeEndpointPath) {
            this.routeEndpointPath = routeEndpointPath;
        }

    }

}

