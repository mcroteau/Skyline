namespace Zeus{
    public class NetworkRequest {
        public NetworkRequest() {
            this.headers = new Dictionary<string, string>();
            this.requestComponents = new Dictionary<string, RequestComponent>();
        }

        bool redirect;
        string redirectLocation;
        string requestPath;
        string requestAction;
        string requestBody;
        Dictionary<string, string> headers;
        Dictionary<string, RequestComponent> requestComponents;
        RouteAttributes routeAttributes;
        string securityAttributeInfo;
        SecurityAttributes securityAttributes;
        string userCredential;

        public bool isRedirect()
        {
            return this.redirect;
        }

        public void setRedirect(bool redirect)
        {
            this.redirect = redirect;
        }

        public string getRedirectLocation()
        {
            return this.redirectLocation;
        }

        public void setRedirectLocation(string redirectLocation)
        {
            this.redirectLocation = redirectLocation;
        }

        public string getRequestPath()
        {
            return this.requestPath;
        }

        public void setRequestPath(string requestPath)
        {
            this.requestPath = requestPath;
        }

        public string getRequestAction()
        {
            return this.requestAction;
        }

        public void setRequestAction(string requestAction)
        {
            this.requestAction = requestAction;
        }

        public string getRequestBody()
        {
            return this.requestBody;
        }

        public void setRequestBody(string requestBody)
        {
            this.requestBody = requestBody;
        }

        public string> getHeaders()
        {
            return this.headers;
        }

        public void setHeaders(string> headers)
        {
            this.headers = headers;
        }

        public RequestComponent> getRequestComponents()
        {
            return this.requestComponents;
        }

        public void setRequestComponents(RequestComponent> requestComponents)
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

        public string getSecurityAttributeInfo()
        {
            return this.securityAttributeInfo;
        }

        public void setSecurityAttributeInfo(string securityAttributeInfo)
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

        public string getUserCredential()
        {
            return this.userCredential;
        }

        public void setUserCredential(string userCredential)
        {
            this.userCredential = userCredential;
        }





    }
}