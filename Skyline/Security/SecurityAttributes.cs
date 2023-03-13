namespace Skyline.Security {
    public class SecurityAttributes {
        public SecurityAttributes(string securityElement, string securedAttribute) {
            this.securityElement = securityElement;
            this.securedAttribute = securedAttribute;
        }

        string securityElement;
        string defaultAttribute;
        string securedAttribute;

        public string getSecurityElement()
        {
            return this.securityElement;
        }

        public void setSecurityElement(string securityElement)
        {
            this.securityElement = securityElement;
        }

        public string getDefaultAttribute()
        {
            return this.defaultAttribute;
        }

        public void setDefaultAttribute(string defaultAttribute)
        {
            this.defaultAttribute = defaultAttribute;
        }

        public string getSecuredAttribute()
        {
            return this.securedAttribute;
        }

        public void setSecuredAttribute(string securedAttribute)
        {
            this.securedAttribute = securedAttribute;
        }
    }
}