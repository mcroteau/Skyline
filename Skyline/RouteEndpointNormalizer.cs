using System;

namespace Skyline{

    public class RouteEndpointNormalizer{
        String routeEndpointPath;
        String routeEndpointAction;

        public String normalize(){
            routeEndpointPath = routeEndpointPath.ToLower().Trim();
            if(routeEndpointPath.Equals("")){
                routeEndpointPath = "/";
                String routeKey = routeEndpointAction.ToLower() + routeEndpointPath.ToLower();            
            }
            return routeEndpointPath;
        }
        
        public void setRouteEndpointPath(String routeEndpointPath) {
            this.routeEndpointPath = routeEndpointPath;
        }
        
        public void setRouteEndpointAction(String routeEndpointAction) {
            this.routeEndpointAction = routeEndpointAction;
        }

    }
}