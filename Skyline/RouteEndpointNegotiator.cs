using System;

using Skyline.Security;

namespace Skyline{

    public class RouteEndpointNegotiator{

        SecurityAttributes securityAttributes;

        public SecurityAttributes getSecurityAttributes() {
            return this.securityAttributes;
        }

        public void setSecurityAttributes(SecurityAttributes securityAttributes) {
            this.securityAttributes = securityAttributes;
        }

    }

}