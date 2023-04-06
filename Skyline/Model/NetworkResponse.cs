using System;
using System.Net;
using System.Collections.Generic;

using Skyline.Security;

namespace Skyline.Model {

    public class NetworkResponse {
        public NetworkResponse(){
            this.securityAttributes = new Dictionary<String, SecurityAttribute>();
        }

        Boolean redirect;
        String contentType;
        HttpListenerContext context;
        Dictionary<String, SecurityAttribute> securityAttributes;

        public Boolean getRedirect()
        {
            return this.redirect;
        }

        public void setRedirect(Boolean redirect)
        {
            this.redirect = redirect;
        }

        public String getContentType()
        {
            return this.contentType;
        }

        public void setContentType(String contentType)
        {
            this.contentType = contentType;
        }

        public void setContext(HttpListenerContext context){
            this.context = context;
        }

        public HttpListenerContext getContext(){
            return this.context;
        }

        public Dictionary<String, SecurityAttribute> getSecurityAttributes()
        {
            return this.securityAttributes;
        }

        public void setSecurityAttributes(Dictionary<String, SecurityAttribute> securityAttributes)
        {
            this.securityAttributes = securityAttributes;
        }

    }
}