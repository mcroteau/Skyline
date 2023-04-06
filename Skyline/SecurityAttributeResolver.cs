using System;
using System.Net;
using System.Text;

using Skyline;
using Skyline.Model;
using Skyline.Security;

namespace Skyline{
    public class SecurityAttributeResolver{
        String SECURITY_KEY = "cookie";
        SecurityAttributes securityAttributes;

        public Boolean resolve(NetworkRequest networkRequest, NetworkResponse networkResponse) {
            try{
                Cookie securityAttributeValue = networkRequest.getContext().Request.Cookies["default.security"];
                Console.WriteLine("c:" + securityAttributeValue.ToString());
                String[] securityAttributeValueElements = securityAttributeValue.Value.Split(".");
                String securityElementPrincipalPre = securityAttributeValueElements[1];
        
                byte[] securityElementPrincipalBytes = Convert.FromBase64String(securityElementPrincipalPre);
                String securityElementPrincipal = Encoding.UTF8.GetString(securityElementPrincipalBytes);

                networkRequest.setSecurityAttributeInfo(securityAttributes.getSecuredAttribute());
                networkRequest.setUserCredential(securityElementPrincipal);

            }catch(Exception ex){
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public Boolean resolveGeneric(NetworkRequest networkRequest, NetworkResponse networkResponse) {
            try {
                if(networkRequest.getHeaders().ContainsKey(SECURITY_KEY)){

                    String securityAttributesElement = networkRequest.getHeaders()[SECURITY_KEY];
                    Console.WriteLine("cookie:" + securityAttributesElement);
                    String[] securityAttributePartials = securityAttributesElement.Split(";");
                    foreach(String securityAttributePartial in securityAttributePartials) {
                        
                        String[] securityAttributeParts = securityAttributePartial.Split("=", 2);

                        String securityAttributeKey = securityAttributeParts[0].Trim();
                        String securityAttributeKeyClean = new string(securityAttributeKey.Where(c => !char.IsControl(c)).ToArray());

                        String securityAttributeValue = securityAttributeParts[1].Trim();

                        if (securityAttributes.getSecurityElement().Equals(securityAttributeKeyClean)) {

                            String[] securityAttributeValueElements = securityAttributeValue.Split(".");
                            String securedElement = securityAttributeValueElements[0];
                            String securedElementClean = new string(securedElement.Where(c => !char.IsControl(c)).ToArray());

                            if(!securedElementClean.Equals(securityAttributes.getSecuredAttribute()))continue;

                            String securityElementPrincipalPre = securityAttributeValueElements[1];

                            String securityElementValue = securedElement + "." + securityElementPrincipalPre + "; path=/";
                            SecurityAttribute securityAttribute = new SecurityAttribute(securityAttributeKey, securityElementValue);

                            networkResponse.getSecurityAttributes()["default.security"] = securityAttribute;

                            byte[] securityElementPrincipalBytes = Convert.FromBase64String(securityElementPrincipalPre);
                            String securityElementPrincipal = Encoding.UTF8.GetString(securityElementPrincipalBytes);
    
                            networkRequest.setSecurityAttributeInfo(securityAttributes.getSecuredAttribute());
                            networkRequest.setUserCredential(securityElementPrincipal);
                        }
                    }
                }
            }catch(Exception ex){
                Console.WriteLine(ex.ToString());
            }

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

