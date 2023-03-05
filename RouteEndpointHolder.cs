using System.String;
using System.Collections.Generic;

namespace Zeus{
    public class RouteEndpointHolder {
        Dictionary<String, RouteEndpoint> routeEndpoints;

        public Dictionary<String, RouteEndpoint> getRouteEndpoints() {
            return routeEndpoints;
        }

        public void setRouteEndpoints(Dictionary<String, RouteEndpoint> routeEndpoints) {
            this.routeEndpoints = routeEndpoints;
        }

        public RouteEndpointHolder(){
            this.routeEndpoints = new Dictionary<string, RouteEndpoint>();
        }
    }
}