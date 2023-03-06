using System;
using System.Collections.Generic;

namespace Zeus{

    public class NetworkResponse {
        public NetworkResponse(){
            this.securityAttributes = new HashMap<>();
        }

        Boolean redirect;
        String contentType;
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