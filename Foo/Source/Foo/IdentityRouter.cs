using System;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

namespace Foo{


    [NetworkController]
    public class IdentityRouter{

        public IdentityRouter(){}

        ApplicationAttributes applicationAttributes;
        public IdentityRouter(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        [Layout(file="/Pages/Default.asp")]        
        [Get(route="/signin")]
        public String signin(){
            return "/Pages/Signin.asp";
        }

        [Post(route="/signin")]
        public String signin(NetworkRequest req, NetworkResponse resp, SecurityManager manager, ViewCache cache){
            String email = req.getValue("email");
            String password = req.getValue("password");
            String hashed = manager.hash(password);
            
            cache.set("message", "");

            if(manager.signin(email, hashed, req, resp)){
                Console.WriteLine("email:" + email + " password:" + password + " hashed:" + hashed + " resp:" + resp);
                return "redirect:/secured";
            }
            cache.set("message","fail.");
            return "redirect:/signin";
        }
    }
}