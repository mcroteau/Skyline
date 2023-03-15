using System;

using Skyline.Model;
using Skyline.Security;

namespace Skyline{

    public class RouteEndpointNegotiator{

        RouteAttributes routeAttributes;
        ComponentsHolder componentsHolder;
        ApplicationAttributes applicationAttributes;
        SecurityAttributes securityAttributes;


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