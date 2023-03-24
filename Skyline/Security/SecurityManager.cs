using System;
using System.Text;
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

        public Boolean signin(String username, String hashedPassword, NetworkRequest networkRequest, NetworkResponse networkResponse) {
            String password = securityAccess.getPassword(username);

            Console.WriteLine("cz:" + password + ":" + hashedPassword);
            try{
                if (!isAuthenticated(networkRequest) &&
                        password.Equals(hashedPassword)) {
                    Console.WriteLine("00");
                    String securityAttributePrincipal = Convert.ToBase64String(Encoding.UTF8.GetBytes(username));
                    Console.WriteLine("00");
                    networkRequest.setSecurityAttributeInfo(securityAttributes.getSecuredAttribute());
                    Console.WriteLine("00");
                    String securityAttributeValue = securityAttributes.getSecuredAttribute() + "." + securityAttributePrincipal + "; path=/;";
                    Console.WriteLine("00");
                    SecurityAttribute securityAttribute = new SecurityAttribute(securityAttributes.getSecurityElement(), securityAttributeValue);
                    Console.WriteLine("00");
                    networkResponse.getSecurityAttributes()["default.security"] = securityAttribute;
                    return true;
                }

            }catch(Exception ex){
                Console.WriteLine(ex.ToString());
            }

            return false;
        }

        public bool signout(NetworkRequest networkRequest, NetworkResponse networkResponse){
            networkRequest.setSecurityAttributeInfo("");
            String securityAttributeValue = ";Expires/MaxAge=-1;Expires=-1;MaxAge=-1;";
            SecurityAttribute securityAttribute = new SecurityAttribute(securityAttributes.getSecurityElement(), securityAttributeValue);
            networkResponse.getSecurityAttributes()["default.security"] = securityAttribute;
            return true;
        }

        public bool isAuthenticated(NetworkRequest networkRequest){
            if(!networkRequest.getUserCredential().Equals("")){
                return true;
            }
            return false;
        }

        public String hash(String password){
            StringBuilder Sb = new StringBuilder();
            var hash = SHA256.Create();

            Encoding enc = Encoding.UTF8;
            byte[] result = hash.ComputeHash(enc.GetBytes(password));

            foreach (byte b in result){
                Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        public static String dirty(String passwd){
            StringBuilder Sb = new StringBuilder();
            var hash = SHA256.Create();

            Encoding enc = Encoding.UTF8;
            byte[] result = hash.ComputeHash(enc.GetBytes(passwd));

            foreach (byte b in result){
                Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}