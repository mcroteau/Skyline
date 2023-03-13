using System;

using Skyline.Model;
using Skyline.Security;

namespace Skyline{

    public class RouteEndpointNegotiator{

        SecurityAttributes securityAttributes;
        ComponentsHolder componentsHolder;
        RouteAttributes routeAttributes;

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

    }

}