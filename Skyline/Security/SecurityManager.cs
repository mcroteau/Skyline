using System;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using Skyline;
using Skyline.Model;
using Skyline.Implement;

namespace Skyline.Security{
    public class SecurityManager {
        public SecurityManager(SecurityAccess securityAccess) {
            this.securityAccess = securityAccess;
        }

        public SecurityManager(SecurityAccess securityAccess, SecurityAttributes securityAttributes){
            this.securityAccess = securityAccess;
            this.securityAttributes = securityAttributes;
        }

        SecurityAccess securityAccess;
        SecurityAttributes securityAttributes;

        public SecurityAttributes getSecurityAttributes() {
            return securityAttributes;
        }

        public void setSecurityAttributes(SecurityAttributes securityAttributes) {
            this.securityAttributes = securityAttributes;
        }

        public SecurityAccess getSecurityAccess() {
            return securityAccess;
        }

        public void setSecurityAccess(SecurityAccess securityAccess) {
            this.securityAccess = securityAccess;
        }

        public bool hasRole(String role, NetworkRequest networkRequest){
            String user = getUser(networkRequest);
            if(user != null) {
                HashSet<String> roles = securityAccess.getRoles(user);
                if(roles.Contains(role)){
                    return true;
                }
            }
            return false;
        }

        public bool hasPermission(String permission, NetworkRequest networkRequest){
            String user = networkRequest.getUserCredential();
            if(user != null) {
                HashSet<String> permissions = securityAccess.getPermissions(user);
                if(permissions.Contains(permission)){
                    return true;
                }
            }
            return false;
        }

        public String getUser(NetworkRequest networkRequest){
            return networkRequest.getUserCredential();
        }

        public Boolean signin(String username, String sentPassword, NetworkRequest networkRequest, NetworkResponse networkResponse) {
            String password = securityAccess.getPassword(username).Trim();
            
            try{
                if (!isAuthenticated(networkRequest) &&
                        password == sentPassword) {
                    String securityAttributePrincipal = Convert.ToBase64String(Encoding.UTF8.GetBytes(username));
                    String securityAttributeValue = securityAttributes.getSecuredAttribute() + "." + securityAttributePrincipal;

                    Cookie securityAttribute = new Cookie();
                    securityAttribute.Path = "/";
                    securityAttribute.Name = "default.security";
                    securityAttribute.Value = securityAttributeValue;
                    networkResponse.getContext().Response.Cookies.Add(securityAttribute);

                    networkRequest.setSecurityAttributeInfo(securityAttributes.getSecuredAttribute());
                    return true;
                }

            }catch(Exception ex){
                Console.WriteLine(ex.ToString());
            }

            return false;
        }

        public bool signout(NetworkRequest networkRequest, NetworkResponse networkResponse){
            networkRequest.setSecurityAttributeInfo("");
            networkResponse.getContext().Response.Headers.Add("Set-Cookie", "default.security=; expires=Thu, 09-Mar-1960 00:00:00 GMT; path=/");
            return true;
        }

        public bool isAuthenticated(NetworkRequest networkRequest){
            if(!networkRequest.getUserCredential().Equals("")){
                return true;
            }
            return false;
        }
    }
}