namespace Zeus{
    public class NetworkRequest {
        public NetworkRequest() {
            this.headers = new Dictionary<String, String>();
            this.requestComponents = new Dictionary<String, RequestComponent>();
        }

        bool redirect;
        String redirectLocation;
        String requestPath;
        String requestAction;
        String requestBody;
        Dictionary<String, String> headers;
        Dictionary<String, RequestComponent> requestComponents;
        RouteAttributes routeAttributes;
        String securityAttributeInfo;
        SecurityAttributes securityAttributes;
        String userCredential;

        public bool isRedirect()
        {
            return this.redirect;
        }

        public void setRedirect(bool redirect)
        {
            this.redirect = redirect;
        }

        public String getRedirectLocation()
        {
            return this.redirectLocation;
        }

        public void setRedirectLocation(String redirectLocation)
        {
            this.redirectLocation = redirectLocation;
        }

        public String getRequestPath()
        {
            return this.requestPath;
        }

        public void setRequestPath(String requestPath)
        {
            this.requestPath = requestPath;
        }

        public String getRequestAction()
        {
            return this.requestAction;
        }

        public void setRequestAction(String requestAction)
        {
            this.requestAction = requestAction;
        }

        public String getRequestBody()
        {
            return this.requestBody;
        }

        public void setRequestBody(String requestBody)
        {
            this.requestBody = requestBody;
        }

        public Dictionary<String, String> getHeaders()
        {
            return this.headers;
        }

        public void setHeaders(Dictionary<String, String> headers)
        {
            this.headers = headers;
        }

        public Dictionary<String, RequestComponent> getRequestComponents()
        {
            return this.requestComponents;
        }

        public void setRequestComponents(Dictionary<String, RequestComponent> requestComponents)
        {
            this.requestComponents = requestComponents;
        }

        public RouteAttributes getRouteAttributes()
        {
            return this.routeAttributes;
        }

        public void setRouteAttributes(RouteAttributes routeAttributes)
        {
            this.routeAttributes = routeAttributes;
        }

        public String getSecurityAttributeInfo()
        {
            return this.securityAttributeInfo;
        }

        public void setSecurityAttributeInfo(String securityAttributeInfo)
        {
            this.securityAttributeInfo = securityAttributeInfo;
        }

        public SecurityAttributes getSecurityAttributes()
        {
            return this.securityAttributes;
        }

        public void setSecurityAttributes(SecurityAttributes securityAttributes)
        {
            this.securityAttributes = securityAttributes;
        }

        public String getUserCredential()
        {
            return this.userCredential;
        }

        public void setUserCredential(String userCredential)
        {
            this.userCredential = userCredential;
        }





    }
}