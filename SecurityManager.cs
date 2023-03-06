using System;
using System.Text;
using System.Security.Cryptography;
using Zeus;
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
            Set<String> roles = securityAccess.getRoles(user);
            if(roles.contains(role)){
                return true;
            }
        }
        return false;
    }

    public bool hasPermission(String permission, NetworkRequest networkRequest){
        String user = networkRequest.getUserCredential();
        if(user != null) {
            Set<String> permissions = securityAccess.getPermissions(user);
            if(permissions.contains(permission)){
                return true;
            }
        }
        return false;
    }

    public String getUser(NetworkRequest networkRequest){
        return networkRequest.getUserCredential();
    }

    public Boolean signin(String username, String passwordUntouched, NetworkRequest networkRequest, NetworkResponse networkResponse) {
        String hashed = hash(passwordUntouched);
        String password = securityAccess.getPassword(username);

        try{
            if (!isAuthenticated(networkRequest) &&
                    password.equals(hashed)) {

                String securityAttributePrincipal = Base64.getEncoder().encodeToString(username.getBytes());
                networkRequest.setSecurityAttributeInfo(securityAttributes.getSecuredAttribute());
                String securityAttributeValue = securityAttributes.getSecuredAttribute() + "." + securityAttributePrincipal + "; path=/;";
                SecurityAttribute securityAttribute = new SecurityAttribute(securityAttributes.getSecurityElement(), securityAttributeValue);
                networkResponse.getSecurityAttributes().Add("plsar.security", securityAttribute);
                return true;
            }

        }catch(Exception ex){
            ex.StackTrace();
        }

        return false;
    }

    public bool signout(NetworkRequest networkRequest, NetworkResponse networkResponse){
        networkRequest.setSecurityAttributeInfo("");
        String securityAttributeValue = ";Expires/MaxAge=-1;Expires=-1;MaxAge=-1;";
        SecurityAttribute securityAttribute = new SecurityAttribute(securityAttributes.getSecurityElement(), securityAttributeValue);
        networkResponse.getSecurityAttributes().remove("plsar.security");
        networkResponse.getSecurityAttributes().Add("plsar.security", securityAttribute);
        return true;
    }

    public bool isAuthenticated(NetworkRequest networkRequest){
        if(!networkRequest.getUserCredential().equals("")){
            return true;
        }
        return false;
    }

    public String hash(String passwd){
        StringBuilder Sb = new StringBuilder();
        var hash = SHA256.Create();

        Encoding enc = Encoding.UTF8;
        byte[] result = hash.ComputeHash(enc.GetBytes(passwd));

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