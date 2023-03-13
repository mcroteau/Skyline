using System;
using System.Reflection;

using Skyline;
using Skyline.Model;
using Skyline.Security;


namespace Skyline{
    public class RouteNegotiatorFactory {

        ResourceUtility resourceUtility;
        RouteAttributes routeAttributes;
        SecurityAttributes securityAttributes;

        public RouteEndpointNegotiator create() {

            RouteEndpointNegotiator routeEndpointNegotiator = new RouteEndpointNegotiator();

            try {
                
                ResourceUtility resourceUtility = new ResourceUtility();

                String securedAttribute = "attribute";
                String securityElement = "s.k.y.l.i.n.e";
                this.securityAttributes = new SecurityAttributes(securityElement, securedAttribute);

                RouteEndpointResolver routeEndpointResolver = new RouteEndpointResolver(resourceUtility);
                RouteEndpointHolder routeEndpointHolder = routeEndpointResolver.resolve();
                routeAttributes.setRouteEndpointHolder(routeEndpointHolder);

                ComponentAnnotationInspector componentAnnotationInspector = new ComponentAnnotationInspector(new ComponentsHolder());
                componentAnnotationInspector.inspect();
                ComponentsHolder componentsHolder = componentAnnotationInspector.getComponentsHolder();

                routeEndpointNegotiator.setSecurityAttributes(securityAttributes);
                routeEndpointNegotiator.setRouteAttributes(routeAttributes);
                routeEndpointNegotiator.setComponentsHolder(componentsHolder);

            } catch (Exception ex){
                Console.WriteLine(ex.Message);
            }

            return routeEndpointNegotiator;
        }

        public SecurityAttributes getSecurityAttributes() {
            return securityAttributes;
        }

        public void setSecurityAttributes(SecurityAttributes securityAttributes) {
            this.securityAttributes = securityAttributes;
        }

        public RouteAttributes getRouteAttributes() {
            return routeAttributes;
        }

        public void setRouteAttributes(RouteAttributes routeAttributes) {
            this.routeAttributes = routeAttributes;
        }
    }
}