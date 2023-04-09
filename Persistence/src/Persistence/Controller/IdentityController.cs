using System;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

namespace Persistence.Controller{

    [NetworkController]
    public class IdentityController{

        public IdentityController(){}

        ApplicationAttributes applicationAttributes;
        
        public IdentityController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        [Layout(file="pages/Default.asp")]        
        [Get(route="/signin")]
        public String signin(NetworkRequest req, ViewCache cache){           
            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);

            return "pages/Signin.asp";
        }

        [Post(route="/signin")]
        public String signin(NetworkRequest req, 
                            NetworkResponse resp, 
                            SecurityManager manager, 
                            ViewCache cache){            
            String email = req.getValue("email");
            String password = req.getValue("password");
            
            manager.signout(req, resp);

            if(manager.signin(email, password, req, resp)){
                return "redirect:/secured";
            }
            cache.set("message", "fail.");
            return "redirect:/signin";
        }

        [Get(route="/signout")]
        public String signout(NetworkRequest req, NetworkResponse resp, SecurityManager manager){
            manager.signout(req, resp);
            return "redirect:/signin";
        }
    }
}