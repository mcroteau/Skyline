using System;

using Skyline;
using Skyline.Model;
using Skyline.Security;

namespace Skyline{
    public class SecurityAttributeResolver{
        String SECURITY_KEY = "cookie";
        SecurityAttributes securityAttributes;

        public Boolean resolve(NetworkRequest networkRequest, NetworkResponse networkResponse) {
            String securityAttributesElement = networkRequest.getHeaders().GetValueOrDefault(SECURITY_KEY, "");
            try {
                String[] securityAttributePartials = securityAttributesElement.Split(";");
                foreach(String securityAttributePartial in securityAttributePartials) {
                    
                    String[] securityAttributeParts = securityAttributePartial.Split("=", 2);

                    String securityAttributeKey = securityAttributeParts[0].Trim();
                    String securityAttributeValue = securityAttributeParts[1].Trim();

                    if (securityAttributes.getSecurityElement().Equals(securityAttributeKey)) {

                        String[] securityAttributeValueElements = securityAttributeValue.Split("\\.");
                        String securedElement = securityAttributeValueElements[0];

                        if(!securedElement.Equals(securityAttributes.getSecuredAttribute()))continue;

                        String securityElementPrincipalPre = securityAttributeValueElements[1];

                        String securityElementValue = securedElement + "." + securityElementPrincipalPre + "; path=/";
                        SecurityAttribute securityAttribute = new SecurityAttribute(securityAttributeKey, securityElementValue);

                        networkResponse.getSecurityAttributes().remove("skyline.security");
                        networkResponse.getSecurityAttributes().put("skyline.security", securityAttribute);

                        byte[] securityElementPrincipalBytes = Convert.FromBase64String(securityElementPrincipalPre);
                        String securityElementPrincipal = Encoding.UTF8.GetString(securityElementPrincipalBytes);
                        String s = new String(securityElementPrincipalBytes);

                        networkRequest.setSecurityAttributeInfo(securityAttributes.getSecuredAttribute());
                        networkRequest.setUserCredential(securityElementPrincipal);
                    }
                }
            }catch(Exception ex){}

            return true;
        }

        public SecurityAttributes getSecurityAttributes() {
            return this.securityAttributes;
        }

        public void setSecurityAttributes(SecurityAttributes securityAttributes) {
            this.securityAttributes = securityAttributes;
        }

        public int getCurrentTime(){
            String dateTime = DateTime.Now.ToString("yyyyMMddHHmmssSSS");
            return Int32.Parse(dateTime);
        }

    }
}

