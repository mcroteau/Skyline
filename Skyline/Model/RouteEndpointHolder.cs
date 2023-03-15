using System;
using System.Collections.Generic;

namespace Skyline.Model{
    public class RouteEndpointHolder {

        Dictionary<String, RouteEndpoint> routeEndpoints;
        
        public RouteEndpointHolder(){
            this.routeEndpoints = new Dictionary<String, RouteEndpoint>();
        }

        public Dictionary<String, RouteEndpoint> getRouteEndpoints() {
            return this.routeEndpoints;
        }

        public void setRouteEndpoints(Dictionary<String, RouteEndpoint> routeEndpoints) {
            this.routeEndpoints = routeEndpoints;
        }
    }
}