using System;
using System.Reflection;

using Skyline;
using Skyline.Model;
using Skyline.Security;


namespace Skyline{
    public class RouteNegotiatorFactory {

        ResourceUtility resourceUtility;
        RouteAttributes routeAttributes;
        ApplicationAttributes applicationAttributes;
        SecurityAttributes securityAttributes;

        public RouteEndpointNegotiator create() {

            RouteEndpointNegotiator routeEndpointNegotiator = new RouteEndpointNegotiator();

            try {
                
                ResourceUtility resourceUtility = new ResourceUtility();

                String securedAttribute = "attribute";
                String securityElement = "s.k.y.l.i.n.e";
                this.securityAttributes = new SecurityAttributes(securityElement, securedAttribute);

                RouteEndpointResolver routeEndpointResolver = new RouteEndpointResolver(new RouteEndpointHolder());
                routeEndpointResolver.setApplicationAttributes(applicationAttributes);
                RouteEndpointHolder routeEndpointHolder = routeEndpointResolver.resolve();
                routeAttributes.setRouteEndpointHolder(routeEndpointHolder);

                ComponentAnnotationResolver componentAnnotationResolver = new ComponentAnnotationResolver(new ComponentsHolder());
                componentAnnotationResolver.setApplicationAttributes(applicationAttributes);
                ComponentsHolder componentsHolder = componentAnnotationResolver.resolve();

                routeEndpointNegotiator.setApplicationAttributes(applicationAttributes);
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

        public ApplicationAttributes getApplicationAttributes() {
            return this.applicationAttributes;
        }

        public void setApplicationAttributes(ApplicationAttributes applicationAttributes) {
            this.applicationAttributes = applicationAttributes;
        }
    }
}