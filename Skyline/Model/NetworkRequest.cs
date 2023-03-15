using System;
using System.Collections.Generic;

using Skyline.Security;

namespace Skyline.Model {
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

        public void setValues(String parameters) {
            String[] keyValues = parameters.Split("&");
            foreach(String keyValue in keyValues){
                String[] parts = keyValue.Split("=");
                if(parts.Length > 1){
                    String key = parts[0];
                    String value = parts[1];
                    RequestComponent requestComponent = new RequestComponent();
                    requestComponent.setName(key);
                    requestComponent.setValue(value);
                    requestComponents.Add(key, requestComponent);
                }
            }
        }

        public void setAttributes(){
            int attributesIndex = this.requestPath.IndexOf("?");
            if(attributesIndex != -1) {
                int attributesIndexWith = attributesIndex + 1;
                String attributesElement = this.requestPath.Substring(attributesIndexWith);
                this.requestPath = this.requestPath.Substring(0, attributesIndex);
                setValues(attributesElement);
            }
        }
    }
}